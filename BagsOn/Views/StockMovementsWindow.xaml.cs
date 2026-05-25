using BagsOn.Models;
using BagsOn.Repositories;
using System;
using System.Windows;

namespace BagsOn.Views
{
    public partial class StockMovementsWindow : Window
    {
        private readonly StockItem _stockItem;
        private readonly StockRepository _stockRepository;

        public StockMovementsWindow(StockItem stockItem)
        {
            InitializeComponent();

            _stockItem = stockItem;
            _stockRepository = new StockRepository();

            Loaded += StockMovementsWindow_Loaded;
        }

        private async void StockMovementsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ProductInfoTextBlock.Text =
                    $"{_stockItem.BrandName} {_stockItem.ModelName} | Колір: {_stockItem.ColorName}";

                var movements = await _stockRepository.GetStockMovementsAsync(_stockItem.VariantId);

                MovementsDataGrid.ItemsSource = movements;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Помилка при завантаженні історії руху товару:\n\n" + ex.Message,
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}