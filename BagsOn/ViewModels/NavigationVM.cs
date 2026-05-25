using BagsOn.Views;
using System.Windows.Controls;
using System.Windows.Input;

namespace BagsOn.ViewModels
{
    public class NavigationVM : BaseViewModel
    {
        private UserControl _currentView;

        public UserControl CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        private string _currentPageTitle;

        public string CurrentPageTitle
        {
            get => _currentPageTitle;
            set
            {
                _currentPageTitle = value;
                OnPropertyChanged();
            }
        }

        public ICommand ProductsCommand { get; set; }
        public ICommand OrdersCommand { get; set; }
        public ICommand StockCommand { get; set; }
        public ICommand AnalyticsCommand { get; set; }

        public NavigationVM()
        {
            ProductsCommand = new RelayCommand(OpenProducts);
            OrdersCommand = new RelayCommand(OpenOrders);
            StockCommand = new RelayCommand(OpenStock);
            AnalyticsCommand = new RelayCommand(OpenAnalytics);

            _currentView = new ProductsView();
            _currentPageTitle = "Асортимент";
        }

        private void OpenProducts(object? parameter)
        {
            CurrentView = new ProductsView();
            CurrentPageTitle = "Асортимент";
        }

        private void OpenOrders(object? parameter)
        {
            CurrentView = new OrdersView();
            CurrentPageTitle = "Замовлення";
        }

        private void OpenStock(object? parameter)
        {
            CurrentView = new StockView();
            CurrentPageTitle = "Склад";
        }

        private void OpenAnalytics(object? parameter)
        {
            CurrentView = new AnalyticsView();
            CurrentPageTitle = "Аналітика";
        }
    }
}