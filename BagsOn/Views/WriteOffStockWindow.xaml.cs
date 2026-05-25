using BagsOn.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace BagsOn.Views
{
    public partial class WriteOffStockWindow : Window
    {
        private readonly StockItem _stockItem;

        public int Quantity { get; private set; }

        public string Reason { get; private set; } = string.Empty;

        public string CommentText { get; private set; } = string.Empty;

        public WriteOffStockWindow(StockItem stockItem)
        {
            InitializeComponent();

            _stockItem = stockItem;

            FillReasons();
            FillInfo();
        }

        private void FillReasons()
        {
            ReasonComboBox.ItemsSource = new List<string>
            {
                "Пошкодження",
                "Брак",
                "Втрачено",
                "Інше"
            };

            ReasonComboBox.SelectedIndex = 0;
        }

        private void FillInfo()
        {
            ProductInfoTextBlock.Text =
                $"{_stockItem.BrandName} {_stockItem.ModelName} | Колір: {_stockItem.ColorName}";

            CurrentQuantityTextBlock.Text =
                $"Всього: {_stockItem.TotalQuantity} шт.  |  " +
                $"Зарезервовано: {_stockItem.ReservedQuantity} шт.  |  " +
                $"Доступно: {_stockItem.AvailableQuantity} шт.";
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

            if (quantity > _stockItem.AvailableQuantity)
            {
                MessageBox.Show(
                    $"Не можна списати {quantity} шт., тому що доступно тільки {_stockItem.AvailableQuantity} шт.\n\n" +
                    $"Зарезервований товар списувати не можна.",
                    "Недостатньо товару",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (ReasonComboBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "Оберіть причину списання.",
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            Quantity = quantity;
            Reason = ReasonComboBox.SelectedItem.ToString() ?? "Інше";
            CommentText = CommentTextBox.Text.Trim();

            DialogResult = true;
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
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}