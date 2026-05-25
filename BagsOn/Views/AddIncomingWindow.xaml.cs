using BagsOn.Models;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace BagsOn.Views
{
    public partial class AddIncomingWindow : Window
    {
        private readonly StockItem _stockItem;

        public int Quantity { get; private set; }

        public string CommentText { get; private set; } = string.Empty;

        public AddIncomingWindow(StockItem stockItem)
        {
            InitializeComponent();

            _stockItem = stockItem;

            FillInfo();
        }

        private void FillInfo()
        {
            ProductInfoTextBlock.Text =
                $"{_stockItem.BrandName} {_stockItem.ModelName} | Колір: {_stockItem.ColorName}";

            CurrentQuantityTextBlock.Text =
                $"{_stockItem.TotalQuantity} шт.  |  Доступно: {_stockItem.AvailableQuantity} шт.";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(QuantityTextBox.Text.Trim(), out int quantity))
            {
                MessageBox.Show(
                    "Введіть коректну кількість.",
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (quantity <= 0)
            {
                MessageBox.Show(
                    "Кількість має бути більшою за 0.",
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            Quantity = quantity;
            CommentText = CommentTextBox.Text.Trim();

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void OnlyDigits_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsDigitsOnly(e.Text);
        }

        private void OnlyDigits_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = e.DataObject.GetData(typeof(string)) as string ?? string.Empty;

                if (!IsDigitsOnly(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private bool IsDigitsOnly(string text)
        {
            return Regex.IsMatch(text, @"^[0-9]+$");
        }
    }
}