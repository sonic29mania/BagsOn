using BagsOn.Models;
using BagsOn.Repositories;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
// Клас StockVM є ViewModel для сторінки складу.
namespace BagsOn.ViewModels
{
    public class StockVM : BaseViewModel
    {
        private readonly StockRepository _stockRepository;

        public ObservableCollection<StockItem> StockItems { get; set; }

        public ICollectionView StockItemsView { get; set; }

        public ObservableCollection<string> StockStatuses { get; set; }

        private string _searchText = string.Empty;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                StockItemsView.Refresh();
            }
        }

        private string _selectedStockStatus = "Усі";

        public string SelectedStockStatus
        {
            get => _selectedStockStatus;
            set
            {
                _selectedStockStatus = value;
                OnPropertyChanged();
                StockItemsView.Refresh();
            }
        }

        public int TotalPositionsCount
        {
            get
            {
                return StockItems.Count;
            }
        }

        public int LowStockCount
        {
            get
            {
                int count = 0;

                foreach (var item in StockItems)
                {
                    if (item.StockStatus == "Мало")
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public int OutOfStockCount
        {
            get
            {
                int count = 0;

                foreach (var item in StockItems)
                {
                    if (item.StockStatus == "Немає")
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public int ReservedCount
        {
            get
            {
                int count = 0;

                foreach (var item in StockItems)
                {
                    if (item.ReservedQuantity > 0)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public decimal TotalStockValue
        {
            get
            {
                decimal value = 0;

                foreach (var item in StockItems)
                {
                    value += item.TotalValue;
                }

                return value;
            }
        }
        // Конструктор StockVM створює репозиторій складу, колекції товарів і статусів,
        // налаштовує фільтрацію StockItemsView та запускає початкове завантаження залишків.
        public StockVM()
        {
            _stockRepository = new StockRepository();

            StockItems = new ObservableCollection<StockItem>();

            StockStatuses = new ObservableCollection<string>
            {
                "Усі",
                "В наявності",
                "Мало",
                "Немає",
                "Є резерв"
            };

            StockItemsView = CollectionViewSource.GetDefaultView(StockItems);
            StockItemsView.Filter = FilterStockItems;

            _ = LoadStockAsync();
        }
        // Метод асинхронно завантажує інформацію про залишки товарів зі складу.
        public async Task LoadStockAsync()
        {
            var items = await _stockRepository.GetStockItemsAsync();

            StockItems.Clear();

            foreach (var item in items)
            {
                StockItems.Add(item);
            }

            RefreshStatistics();

            StockItemsView.Refresh();
        }
        // Метод визначає, чи потрібно показувати товар у таблиці складу.
        private bool FilterStockItems(object obj)
        {
            if (obj is not StockItem item)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string search = SearchText.Trim().ToLower();

                bool containsSearch =
                    item.ModelName.ToLower().Contains(search) ||
                    item.BrandName.ToLower().Contains(search) ||
                    item.TypeName.ToLower().Contains(search) ||
                    item.CategoryName.ToLower().Contains(search) ||
                    item.MaterialName.ToLower().Contains(search) ||
                    item.ColorName.ToLower().Contains(search) ||
                    item.StockStatus.ToLower().Contains(search) ||
                    item.StockWarning.ToLower().Contains(search);

                if (!containsSearch)
                {
                    return false;
                }
            }

            if (SelectedStockStatus != "Усі" &&
                item.StockStatus != SelectedStockStatus)
            {
                return false;
            }

            return true;
        }
        // Метод очищає пошук і фільтр за статусом складу.
        public void ClearFilters()
        {
            SearchText = string.Empty;
            SelectedStockStatus = "Усі";

            StockItemsView.Refresh();
        }
        // Метод повідомляє інтерфейс про зміну підсумкових показників складу.
        private void RefreshStatistics()
        {
            OnPropertyChanged(nameof(TotalPositionsCount));
            OnPropertyChanged(nameof(LowStockCount));
            OnPropertyChanged(nameof(OutOfStockCount));
            OnPropertyChanged(nameof(ReservedCount));
            OnPropertyChanged(nameof(TotalStockValue));
        }
    }
}