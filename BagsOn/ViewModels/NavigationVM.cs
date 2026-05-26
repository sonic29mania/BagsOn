using BagsOn.Views;
using System.Windows.Controls;
using System.Windows.Input;

// Клас NavigationVM відповідає за навігацію між сторінками програми.
// Він зберігає поточний UserControl, назву активної
//сторінки та команди для переходу до розділів асортименту, замовлень, складу й аналітики.

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


        // Конструктор NavigationVM створює команди навігації та задає початкову сторінку програми.
        // За замовчуванням відкривається сторінка асортименту.
        public NavigationVM()
        {
            ProductsCommand = new RelayCommand(OpenProducts);
            OrdersCommand = new RelayCommand(OpenOrders);
            StockCommand = new RelayCommand(OpenStock);
            AnalyticsCommand = new RelayCommand(OpenAnalytics);

            _currentView = new ProductsView();
            _currentPageTitle = "Асортимент";
        }
        // Метод відкриває сторінку асортименту товарів.
        private void OpenProducts(object? parameter)
        {
            CurrentView = new ProductsView();
            CurrentPageTitle = "Асортимент";
        }
        // Метод відкриває сторінку замовлень.
        private void OpenOrders(object? parameter)
        {
            CurrentView = new OrdersView();
            CurrentPageTitle = "Замовлення";
        }
        // Метод  відкриває сторінку складу.
        private void OpenStock(object? parameter)
        {
            CurrentView = new StockView();
            CurrentPageTitle = "Склад";
        }
        // Метод  відкриває сторінку аналітики.
        private void OpenAnalytics(object? parameter)
        {
            CurrentView = new AnalyticsView();
            CurrentPageTitle = "Аналітика";
        }
    }
}