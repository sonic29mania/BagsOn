using BagsOn.Models;
using BagsOn.Repositories;
using BagsOn.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BagsOn.ViewModels
{
    public class AnalyticsVM : BaseViewModel
    {
        private readonly AnalyticsRepository _analyticsRepository;

        public ObservableCollection<AnalyticsTopProduct> TopProducts { get; set; }

        public ObservableCollection<AnalyticsReplenishmentItem> ReplenishmentItems { get; set; }
        private readonly ReportsRepository _reportsRepository;

        private AnalyticsSummary _summary;

        public AnalyticsSummary Summary
        {
            get => _summary;
            set
            {
                _summary = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _dateFrom;

        public DateTime? DateFrom
        {
            get => _dateFrom;
            set
            {
                _dateFrom = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _dateTo;

        public DateTime? DateTo
        {
            get => _dateTo;
            set
            {
                _dateTo = value;
                OnPropertyChanged();
            }
        }

        public AnalyticsVM()
        {
            _analyticsRepository = new AnalyticsRepository();
            _reportsRepository = new ReportsRepository();

            ReportTypes = new ObservableCollection<ReportTypeOption>
{
    new ReportTypeOption { Name = "Звіт по продажах", Code = "sales" },
    new ReportTypeOption { Name = "Звіт по складу", Code = "stock" },
    new ReportTypeOption { Name = "Звіт по руху товарів", Code = "movements" },
    new ReportTypeOption { Name = "Товари з низьким залишком", Code = "low_stock" }
};

            SelectedReportType = ReportTypes[0];
            ReportTitle = "Звіт по продажах";
            ReportStatusText = "Оберіть тип звіту та натисніть «Сформувати».";
            TopProducts = new ObservableCollection<AnalyticsTopProduct>();
            ReplenishmentItems = new ObservableCollection<AnalyticsReplenishmentItem>();
            StockMovements = new ObservableCollection<AnalyticsStockMovement>();

            MovementTypes = new ObservableCollection<string>
    {
        "Усі",
        "Надходження",
        "Списання",
        "Продаж",
        "Повернення",
        "Коригування"
    };

            StockMovementsView = CollectionViewSource.GetDefaultView(StockMovements);
            StockMovementsView.Filter = FilterStockMovements;

            _summary = new AnalyticsSummary();

            DateFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTo = DateTime.Now;

            _ = LoadAnalyticsAsync();
        }

        public async Task LoadAnalyticsAsync()
        {
            DateTime from = DateFrom ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime to = DateTo ?? DateTime.Now;

            Summary = await _analyticsRepository.GetSummaryAsync(from, to);

            var topProducts = await _analyticsRepository.GetTopProductsAsync(from, to);

            TopProducts.Clear();

            foreach (var product in topProducts)
            {
                TopProducts.Add(product);
            }

            var replenishmentItems = await _analyticsRepository.GetReplenishmentItemsAsync();
            var movements = await _analyticsRepository.GetStockMovementsForAnalyticsAsync(from, to);

            StockMovements.Clear();

            foreach (var movement in movements)
            {
                StockMovements.Add(movement);
            }

            StockMovementsView.Refresh();

            RefreshMovementStatistics();
            ReplenishmentItems.Clear();

            foreach (var item in replenishmentItems)
            {
                ReplenishmentItems.Add(item);
            }
        }
        private bool _exportAllMovementTypes = true;

        public bool ExportAllMovementTypes
        {
            get => _exportAllMovementTypes;
            set
            {
                _exportAllMovementTypes = value;
                OnPropertyChanged();
            }
        }

        private bool _exportIncoming;

        public bool ExportIncoming
        {
            get => _exportIncoming;
            set
            {
                _exportIncoming = value;
                OnPropertyChanged();
            }
        }

        private bool _exportWriteOff;

        public bool ExportWriteOff
        {
            get => _exportWriteOff;
            set
            {
                _exportWriteOff = value;
                OnPropertyChanged();
            }
        }

        private bool _exportSales;

        public bool ExportSales
        {
            get => _exportSales;
            set
            {
                _exportSales = value;
                OnPropertyChanged();
            }
        }

        private bool _exportReturns;

        public bool ExportReturns
        {
            get => _exportReturns;
            set
            {
                _exportReturns = value;
                OnPropertyChanged();
            }
        }

        private bool _exportAdjustments;

        public bool ExportAdjustments
        {
            get => _exportAdjustments;
            set
            {
                _exportAdjustments = value;
                OnPropertyChanged();
            }
        }
        public List<AnalyticsStockMovement> GetMovementsForExport()
        {
            IEnumerable<AnalyticsStockMovement> movements = StockMovements;

            if (!ExportAllMovementTypes)
            {
                List<string> selectedTypes = new List<string>();

                if (ExportIncoming)
                {
                    selectedTypes.Add("Надходження");
                }

                if (ExportWriteOff)
                {
                    selectedTypes.Add("Списання");
                }

                if (ExportSales)
                {
                    selectedTypes.Add("Продаж");
                }

                if (ExportReturns)
                {
                    selectedTypes.Add("Повернення");
                }

                if (ExportAdjustments)
                {
                    selectedTypes.Add("Коригування");
                }

                movements = movements.Where(movement => selectedTypes.Contains(movement.MovementType));
            }

            if (!string.IsNullOrWhiteSpace(MovementSearchText))
            {
                string search = MovementSearchText.Trim().ToLower();

                movements = movements.Where(movement =>
                    movement.ModelName.ToLower().Contains(search) ||
                    movement.BrandName.ToLower().Contains(search) ||
                    movement.ColorName.ToLower().Contains(search) ||
                    movement.MovementType.ToLower().Contains(search) ||
                    movement.Comment.ToLower().Contains(search)
                );
            }

            return movements.ToList();
        }
        public ObservableCollection<ReportTypeOption> ReportTypes { get; set; }

        private ReportTypeOption? _selectedReportType;

        public ReportTypeOption? SelectedReportType
        {
            get => _selectedReportType;
            set
            {
                _selectedReportType = value;
                OnPropertyChanged();

                if (_selectedReportType != null)
                {
                    ReportTitle = _selectedReportType.Name;
                }
            }
        }

        private DataView? _reportView;

        public DataView? ReportView
        {
            get => _reportView;
            set
            {
                _reportView = value;
                OnPropertyChanged();
            }
        }

        private DataTable? _currentReportTable;

        public DataTable? CurrentReportTable
        {
            get => _currentReportTable;
            set
            {
                _currentReportTable = value;
                OnPropertyChanged();
            }
        }

        private string _reportTitle = string.Empty;

        public string ReportTitle
        {
            get => _reportTitle;
            set
            {
                _reportTitle = value;
                OnPropertyChanged();
            }
        }

        private string _reportStatusText = string.Empty;

        public string ReportStatusText
        {
            get => _reportStatusText;
            set
            {
                _reportStatusText = value;
                OnPropertyChanged();
            }
        }
        public async Task GenerateReportAsync()
        {
            if (SelectedReportType == null)
            {
                ReportStatusText = "Оберіть тип звіту.";
                return;
            }

            DateTime from = DateFrom ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime to = DateTo ?? DateTime.Now;

            DataTable table;

            if (SelectedReportType.Code == "sales")
            {
                table = await _reportsRepository.GetSalesReportAsync(from, to);
            }
            else if (SelectedReportType.Code == "stock")
            {
                table = await _reportsRepository.GetStockReportAsync();
            }
            else if (SelectedReportType.Code == "movements")
            {
                table = await _reportsRepository.GetStockMovementsReportAsync(from, to);
            }
            else if (SelectedReportType.Code == "low_stock")
            {
                table = await _reportsRepository.GetLowStockReportAsync();
            }
            else
            {
                ReportStatusText = "Невідомий тип звіту.";
                return;
            }

            CurrentReportTable = table;
            ReportView = table.DefaultView;

            ReportTitle = SelectedReportType.Name;

            ReportStatusText =
                $"Звіт сформовано. Рядків: {table.Rows.Count}. Період: {from:dd.MM.yyyy} - {to:dd.MM.yyyy}.";
        }
        private bool FilterStockMovements(object obj)
        {
            if (obj is not AnalyticsStockMovement movement)
            {
                return false;
            }

            if (SelectedMovementType != "Усі" &&
                movement.MovementType != SelectedMovementType)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(MovementSearchText))
            {
                string search = MovementSearchText.Trim().ToLower();

                bool containsSearch =
                    movement.ModelName.ToLower().Contains(search) ||
                    movement.BrandName.ToLower().Contains(search) ||
                    movement.ColorName.ToLower().Contains(search) ||
                    movement.MovementType.ToLower().Contains(search) ||
                    movement.Comment.ToLower().Contains(search);

                if (!containsSearch)
                {
                    return false;
                }
            }

            return true;
        }

        private void RefreshMovementStatistics()
        {
            OnPropertyChanged(nameof(IncomingQuantity));
            OnPropertyChanged(nameof(WriteOffQuantity));
            OnPropertyChanged(nameof(SalesQuantity));
            OnPropertyChanged(nameof(ReturnQuantity));
        }
        public void SetThisMonth()
        {
            DateFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTo = DateTime.Now;
        }

        public void SetLast7Days()
        {
            DateFrom = DateTime.Now.AddDays(-7);
            DateTo = DateTime.Now;
        }

        public void SetToday()
        {
            DateFrom = DateTime.Now.Date;
            DateTo = DateTime.Now.Date;
        }
        public ObservableCollection<AnalyticsStockMovement> StockMovements { get; set; }

        public ICollectionView StockMovementsView { get; set; }

        public ObservableCollection<string> MovementTypes { get; set; }

        private string _selectedMovementType = "Усі";

        public string SelectedMovementType
        {
            get => _selectedMovementType;
            set
            {
                _selectedMovementType = value;
                OnPropertyChanged();
                StockMovementsView.Refresh();
            }
        }

        private string _movementSearchText = string.Empty;

        public string MovementSearchText
        {
            get => _movementSearchText;
            set
            {
                _movementSearchText = value;
                OnPropertyChanged();
                StockMovementsView.Refresh();
            }
        }

        public int IncomingQuantity
        {
            get
            {
                int total = 0;

                foreach (var movement in StockMovements)
                {
                    if (movement.MovementType == "Надходження")
                    {
                        total += movement.QuantityChange;
                    }
                }

                return total;
            }
        }

        public int WriteOffQuantity
        {
            get
            {
                int total = 0;

                foreach (var movement in StockMovements)
                {
                    if (movement.MovementType == "Списання")
                    {
                        total += Math.Abs(movement.QuantityChange);
                    }
                }

                return total;
            }
        }

        public int SalesQuantity
        {
            get
            {
                int total = 0;

                foreach (var movement in StockMovements)
                {
                    if (movement.MovementType == "Продаж")
                    {
                        total += Math.Abs(movement.QuantityChange);
                    }
                }

                return total;
            }
        }

        public int ReturnQuantity
        {
            get
            {
                int total = 0;

                foreach (var movement in StockMovements)
                {
                    if (movement.MovementType == "Повернення")
                    {
                        total += movement.QuantityChange;
                    }
                }

                return total;
            }
        }
    }
}