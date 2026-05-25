using BagsOn.Models;
using BagsOn.Repositories;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BagsOn.ViewModels
{
    public class OrdersVM : BaseViewModel
    {
        private readonly OrderRepository _orderRepository;

        public ObservableCollection<Order> Orders { get; set; }

        public ObservableCollection<Order> ArchivedOrders { get; set; }

        public ICollectionView OrdersView { get; set; }

        public ICollectionView ArchivedOrdersView { get; set; }

        public ObservableCollection<ReferenceItem> Statuses { get; set; }

        public ObservableCollection<ReferenceItem> DeliveryTypes { get; set; }

        public string ActiveTabTitle
        {
            get
            {
                return $"Активні замовлення ({Orders.Count})";
            }
        }

        public string ArchiveTabTitle
        {
            get
            {
                return $"Архів ({ArchivedOrders.Count})";
            }
        }

        // ============================
        // ФІЛЬТРИ ДЛЯ АКТИВНИХ ЗАМОВЛЕНЬ
        // ============================

        private string _activeSearchText = string.Empty;

        public string ActiveSearchText
        {
            get => _activeSearchText;
            set
            {
                _activeSearchText = value;
                OnPropertyChanged();
                RefreshActiveView();
            }
        }

        private ReferenceItem? _activeSelectedStatus;

        public ReferenceItem? ActiveSelectedStatus
        {
            get => _activeSelectedStatus;
            set
            {
                _activeSelectedStatus = value;
                OnPropertyChanged();
                RefreshActiveView();
            }
        }

        private ReferenceItem? _activeSelectedDeliveryType;

        public ReferenceItem? ActiveSelectedDeliveryType
        {
            get => _activeSelectedDeliveryType;
            set
            {
                _activeSelectedDeliveryType = value;
                OnPropertyChanged();
                RefreshActiveView();
            }
        }

        private DateTime? _activeDateFrom;

        public DateTime? ActiveDateFrom
        {
            get => _activeDateFrom;
            set
            {
                _activeDateFrom = value;
                OnPropertyChanged();
                RefreshActiveView();
            }
        }

        private DateTime? _activeDateTo;

        public DateTime? ActiveDateTo
        {
            get => _activeDateTo;
            set
            {
                _activeDateTo = value;
                OnPropertyChanged();
                RefreshActiveView();
            }
        }

        // ============================
        // ФІЛЬТРИ ДЛЯ АРХІВУ
        // ============================

        private string _archiveSearchText = string.Empty;

        public string ArchiveSearchText
        {
            get => _archiveSearchText;
            set
            {
                _archiveSearchText = value;
                OnPropertyChanged();
                RefreshArchiveView();
            }
        }

        private ReferenceItem? _archiveSelectedStatus;

        public ReferenceItem? ArchiveSelectedStatus
        {
            get => _archiveSelectedStatus;
            set
            {
                _archiveSelectedStatus = value;
                OnPropertyChanged();
                RefreshArchiveView();
            }
        }

        private string _archiveReasonText = string.Empty;

        public string ArchiveReasonText
        {
            get => _archiveReasonText;
            set
            {
                _archiveReasonText = value;
                OnPropertyChanged();
                RefreshArchiveView();
            }
        }

        private DateTime? _archiveDateFrom;

        public DateTime? ArchiveDateFrom
        {
            get => _archiveDateFrom;
            set
            {
                _archiveDateFrom = value;
                OnPropertyChanged();
                RefreshArchiveView();
            }
        }

        private DateTime? _archiveDateTo;

        public DateTime? ArchiveDateTo
        {
            get => _archiveDateTo;
            set
            {
                _archiveDateTo = value;
                OnPropertyChanged();
                RefreshArchiveView();
            }
        }

        public OrdersVM()
        {
            _orderRepository = new OrderRepository();

            Orders = new ObservableCollection<Order>();
            ArchivedOrders = new ObservableCollection<Order>();

            Statuses = new ObservableCollection<ReferenceItem>();
            DeliveryTypes = new ObservableCollection<ReferenceItem>();

            OrdersView = CollectionViewSource.GetDefaultView(Orders);
            OrdersView.Filter = FilterActiveOrders;

            ArchivedOrdersView = CollectionViewSource.GetDefaultView(ArchivedOrders);
            ArchivedOrdersView.Filter = FilterArchivedOrders;

            _ = LoadOrdersAsync();
            _ = LoadFiltersAsync();
        }

        public async Task LoadOrdersAsync()
        {
            var activeOrders = await _orderRepository.GetAllOrdersAsync();
            var archiveOrders = await _orderRepository.GetArchivedOrdersAsync();

            Orders.Clear();

            foreach (var order in activeOrders)
            {
                Orders.Add(order);
            }

            ArchivedOrders.Clear();

            foreach (var order in archiveOrders)
            {
                ArchivedOrders.Add(order);
            }

            UpdateTabTitles();
            RefreshAllViews();
        }

        private async Task LoadFiltersAsync()
        {
            Statuses.Clear();
            DeliveryTypes.Clear();

            foreach (var status in await _orderRepository.GetOrderStatusesAsync())
            {
                Statuses.Add(status);
            }

            foreach (var deliveryType in await _orderRepository.GetDeliveryTypesAsync())
            {
                DeliveryTypes.Add(deliveryType);
            }
        }

        private bool FilterActiveOrders(object obj)
        {
            if (obj is not Order order)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(ActiveSearchText))
            {
                string search = ActiveSearchText.Trim().ToLower();

                bool containsSearch =
                    order.OrderId.ToString().Contains(search) ||
                    order.CustomerName.ToLower().Contains(search) ||
                    order.Phone.ToLower().Contains(search) ||
                    order.City.ToLower().Contains(search) ||
                    order.StatusName.ToLower().Contains(search) ||
                    order.DeliveryTypeName.ToLower().Contains(search);

                if (!containsSearch)
                {
                    return false;
                }
            }

            if (ActiveSelectedStatus != null &&
                order.StatusName != ActiveSelectedStatus.Name)
            {
                return false;
            }

            if (ActiveSelectedDeliveryType != null &&
                order.DeliveryTypeName != ActiveSelectedDeliveryType.Name)
            {
                return false;
            }

            if (ActiveDateFrom != null &&
                order.OrderDate.Date < ActiveDateFrom.Value.Date)
            {
                return false;
            }

            if (ActiveDateTo != null &&
                order.OrderDate.Date > ActiveDateTo.Value.Date)
            {
                return false;
            }

            return true;
        }

        private bool FilterArchivedOrders(object obj)
        {
            if (obj is not Order order)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(ArchiveSearchText))
            {
                string search = ArchiveSearchText.Trim().ToLower();

                bool containsSearch =
     order.OrderId.ToString().Contains(search) ||
     order.CustomerName.ToLower().Contains(search) ||
     order.Phone.ToLower().Contains(search) ||
     order.City.ToLower().Contains(search) ||
     order.StatusName.ToLower().Contains(search) ||
     order.DeliveryTypeName.ToLower().Contains(search) ||
     order.UrgencyText.ToLower().Contains(search);

                if (!containsSearch)
                {
                    return false;
                }
            }

            if (ArchiveSelectedStatus != null &&
                order.StatusName != ArchiveSelectedStatus.Name)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(ArchiveReasonText))
            {
                string reasonSearch = ArchiveReasonText.Trim().ToLower();

                if (!order.ArchiveReason.ToLower().Contains(reasonSearch))
                {
                    return false;
                }
            }

            DateTime archiveDate = order.ArchivedAt?.Date ?? order.OrderDate.Date;

            if (ArchiveDateFrom != null &&
                archiveDate < ArchiveDateFrom.Value.Date)
            {
                return false;
            }

            if (ArchiveDateTo != null &&
                archiveDate > ArchiveDateTo.Value.Date)
            {
                return false;
            }

            return true;
        }

        private void RefreshActiveView()
        {
            OrdersView.Refresh();
        }

        private void RefreshArchiveView()
        {
            ArchivedOrdersView.Refresh();
        }

        private void RefreshAllViews()
        {
            OrdersView.Refresh();
            ArchivedOrdersView.Refresh();
        }

        private void UpdateTabTitles()
        {
            OnPropertyChanged(nameof(ActiveTabTitle));
            OnPropertyChanged(nameof(ArchiveTabTitle));
        }

        public void ClearActiveFilters()
        {
            ActiveSearchText = string.Empty;
            ActiveSelectedStatus = null;
            ActiveSelectedDeliveryType = null;
            ActiveDateFrom = null;
            ActiveDateTo = null;

            RefreshActiveView();
        }

        public void ClearArchiveFilters()
        {
            ArchiveSearchText = string.Empty;
            ArchiveSelectedStatus = null;
            ArchiveReasonText = string.Empty;
            ArchiveDateFrom = null;
            ArchiveDateTo = null;

            RefreshArchiveView();
        }

        public void ClearFilters()
        {
            ClearActiveFilters();
            ClearArchiveFilters();
        }
    }
}