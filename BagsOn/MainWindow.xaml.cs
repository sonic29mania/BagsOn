using System.Windows;
using System.Windows.Controls;

namespace BagsOn
{
    public partial class MainWindow : Window
    {
        private bool _isMenuOpen = true;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BurgerButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isMenuOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }

        private void CloseMenu()
        {
            _isMenuOpen = false;

            MenuColumn.Width = new GridLength(112);

            LeftMenuBorder.Padding = new Thickness(14);

            LogoPanel.Visibility = Visibility.Collapsed;
            UserBlock.Visibility = Visibility.Collapsed;

            ProductsMenuButton.Content = "👜";
            OrdersMenuButton.Content = "🧾";
            StockMenuButton.Content = "📦";
            AnalyticsMenuButton.Content = "📊";

            ProductsMenuButton.ToolTip = "Асортимент";
            OrdersMenuButton.ToolTip = "Замовлення";
            StockMenuButton.ToolTip = "Склад";
            AnalyticsMenuButton.ToolTip = "Аналітика";

            SetClosedMenuButton(ProductsMenuButton);
            SetClosedMenuButton(OrdersMenuButton);
            SetClosedMenuButton(StockMenuButton);
            SetClosedMenuButton(AnalyticsMenuButton);

            BurgerButton.Content = "☰";
            BurgerButton.ToolTip = "Відкрити меню";
        }

        private void OpenMenu()
        {
            _isMenuOpen = true;

            MenuColumn.Width = new GridLength(260);

            LeftMenuBorder.Padding = new Thickness(22);

            LogoPanel.Visibility = Visibility.Visible;
            UserBlock.Visibility = Visibility.Visible;

            ProductsMenuButton.Content = "👜  Асортимент";
            OrdersMenuButton.Content = "🧾  Замовлення";
            StockMenuButton.Content = "📦  Склад";
            AnalyticsMenuButton.Content = "📊  Аналітика";

            ProductsMenuButton.ToolTip = null;
            OrdersMenuButton.ToolTip = null;
            StockMenuButton.ToolTip = null;
            AnalyticsMenuButton.ToolTip = null;

            SetOpenMenuButton(ProductsMenuButton);
            SetOpenMenuButton(OrdersMenuButton);
            SetOpenMenuButton(StockMenuButton);
            SetOpenMenuButton(AnalyticsMenuButton);

            BurgerButton.Content = "☰";
            BurgerButton.ToolTip = "Закрити меню";
        }

        private void SetClosedMenuButton(Button button)
        {
            button.Width = 56;
            button.Height = 52;
            button.FontSize = 22;
            button.Margin = new Thickness(0, 0, 0, 14);
            button.Padding = new Thickness(0);
            button.HorizontalAlignment = HorizontalAlignment.Center;
            button.HorizontalContentAlignment = HorizontalAlignment.Center;
        }

        private void SetOpenMenuButton(Button button)
        {
            button.Width = double.NaN;
            button.Height = 48;
            button.FontSize = 15;
            button.Margin = new Thickness(0, 0, 0, 10);
            button.Padding = new Thickness(18, 0, 0, 0);
            button.HorizontalAlignment = HorizontalAlignment.Stretch;
            button.HorizontalContentAlignment = HorizontalAlignment.Left;
        }
    }
}