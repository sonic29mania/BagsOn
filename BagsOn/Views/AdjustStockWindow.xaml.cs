using BagsOn.Models;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace BagsOn.Views
{
    public partial class AdjustStockWindow : Window
    {
        private readonly StockItem _stockItem;

        public int NewTotalQuantity { get; private set; }

        public string CommentText { get; private set; } = string.Empty;

        public AdjustStockWindow(StockItem stockItem)
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
                $"Всього: {_stockItem.TotalQuantity} шт.  |  " +
                $"Зарезервовано: {_stockItem.ReservedQuantity} шт.  |  " +
                $"Доступно: {_stockItem.AvailableQuantity} шт.";

            NewQuantityTextBox.Text = _stockItem.TotalQuantity.ToString();
            CommentTextBox.Text = "Коригування після інвентаризації";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(NewQuantityTextBox.Text.Trim(), out int newQuantity))
            {
                MessageBox.Show(
                    "Введіть коректну кількість.",
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (newQuantity < 0)
            {
                MessageBox.Show(
                    "Кількість не може бути меншою за 0.",
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (newQuantity < _stockItem.ReservedQuantity)
            {
                MessageBox.Show(
                    $"Фактична кількість не може бути меншою за резерв.\n\n" +
                    $"Зарезервовано: {_stockItem.ReservedQuantity} шт.",
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (newQuantity == _stockItem.TotalQuantity)
            {
                MessageBox.Show(
                    "Кількість не змінилася.",
                    "Коригування не потрібне",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                return;
            }

            NewTotalQuantity = newQuantity;
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