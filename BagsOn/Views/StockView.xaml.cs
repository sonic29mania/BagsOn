using BagsOn.Models;
using BagsOn.Repositories;
using BagsOn.Services;
using BagsOn.ViewModels;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace BagsOn.Views
{
    public partial class StockView : UserControl
    {
        public StockView()
        {
            InitializeComponent();

            Loaded += StockView_Loaded;
        }

        private async void StockView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is StockVM viewModel)
                {
                    await viewModel.LoadStockAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Помилка при завантаженні складу:\n\n" + ex.Message,
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is StockVM viewModel)
                {
                    await viewModel.LoadStockAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Помилка при оновленні складу:\n\n" + ex.Message,
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is StockVM viewModel)
            {
                viewModel.ClearFilters();
            }
        }

        private void ViewMovementsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is StockItem stockItem)
            {
                StockMovementsWindow window = new StockMovementsWindow(stockItem);
                window.Owner = Window.GetWindow(this);
                window.ShowDialog();
            }
        }

        private async void AddIncomingButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not StockItem stockItem)
            {
                return;
            }

            AddIncomingWindow window = new AddIncomingWindow(stockItem);
            window.Owner = Window.GetWindow(this);

            bool? result = window.ShowDialog();

            if (result != true)
            {
                return;
            }

            try
            {
                StockRepository repository = new StockRepository();

                await repository.AddIncomingAsync(
                    stockItem.VariantId,
                    window.Quantity,
                    window.CommentText
                );

                if (DataContext is StockVM viewModel)
                {
                    await viewModel.LoadStockAsync();
                }

                MessageBox.Show(
                    "Надходження товару успішно додано.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Помилка при додаванні надходження:\n\n" + ex.Message,
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
        private async void WriteOffButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not StockItem stockItem)
            {
                return;
            }

            WriteOffStockWindow window = new WriteOffStockWindow(stockItem);
            window.Owner = Window.GetWindow(this);

            bool? result = window.ShowDialog();

            if (result != true)
            {
                return;
            }

            MessageBoxResult confirmResult = MessageBox.Show(
                $"Списати товар зі складу?\n\n" +
                $"Товар: {stockItem.BrandName} {stockItem.ModelName}\n" +
                $"Колір: {stockItem.ColorName}\n" +
                $"Кількість: {window.Quantity} шт.\n" +
                $"Причина: {window.Reason}",
                "Підтвердження списання",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (confirmResult != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                StockRepository repository = new StockRepository();

                await repository.WriteOffStockAsync(
                    stockItem.VariantId,
                    window.Quantity,
                    window.Reason,
                    window.CommentText
                );

                if (DataContext is StockVM viewModel)
                {
                    await viewModel.LoadStockAsync();
                }

                MessageBox.Show(
                    "Товар успішно списано зі складу.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Помилка при списанні товару:\n\n" + ex.Message,
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
        private async void AdjustStockButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not StockItem stockItem)
            {
                return;
            }

            AdjustStockWindow window = new AdjustStockWindow(stockItem);
            window.Owner = Window.GetWindow(this);

            bool? result = window.ShowDialog();

            if (result != true)
            {
                return;
            }

            MessageBoxResult confirmResult = MessageBox.Show(
                $"Змінити залишок товару після інвентаризації?\n\n" +
                $"Товар: {stockItem.BrandName} {stockItem.ModelName}\n" +
                $"Колір: {stockItem.ColorName}\n\n" +
                $"Було в програмі: {stockItem.TotalQuantity} шт.\n" +
                $"Буде після коригування: {window.NewTotalQuantity} шт.",
                "Підтвердження коригування",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (confirmResult != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                StockRepository repository = new StockRepository();

                await repository.AdjustStockAsync(
                    stockItem.VariantId,
                    window.NewTotalQuantity,
                    window.CommentText
                );

                if (DataContext is StockVM viewModel)
                {
                    await viewModel.LoadStockAsync();
                }

                MessageBox.Show(
                    "Залишок товару успішно скориговано.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Помилка при коригуванні залишку:\n\n" + ex.Message,
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
        private List<StockItem> GetCurrentStockItems()
        {
            if (DataContext is not StockVM viewModel)
            {
                return new List<StockItem>();
            }

            return viewModel.StockItemsView
                .Cast<StockItem>()
                .ToList();
        }

        private async void ExportExcelButton_Click(object sender, RoutedEventArgs e)
        {
            List<StockItem> items = GetCurrentStockItems();

            if (items.Count == 0)
            {
                MessageBox.Show(
                    "Немає даних для експорту.",
                    "Експорт",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Зберегти звіт складу в Excel",
                Filter = "Excel файл (*.xlsx)|*.xlsx",
                FileName = $"Склад_{DateTime.Now:yyyy_MM_dd_HH_mm}.xlsx"
            };

            bool? result = saveFileDialog.ShowDialog();

            if (result != true)
            {
                return;
            }

            try
            {
                var movementsByVariant = await GetMovementsByVariantAsync(items);

                StockReportService.ExportToExcel(items, movementsByVariant, saveFileDialog.FileName);

                MessageBox.Show(
                    "Excel-звіт успішно збережено.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Помилка при експорті в Excel:\n\n" + ex.Message,
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private async void ExportPdfButton_Click(object sender, RoutedEventArgs e)
        {
            List<StockItem> items = GetCurrentStockItems();

            if (items.Count == 0)
            {
                MessageBox.Show(
                    "Немає даних для експорту.",
                    "Експорт",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Зберегти звіт складу в PDF",
                Filter = "PDF файл (*.pdf)|*.pdf",
                FileName = $"Склад_{DateTime.Now:yyyy_MM_dd_HH_mm}.pdf"
            };

            bool? result = saveFileDialog.ShowDialog();

            if (result != true)
            {
                return;
            }

            try
            {
                var movementsByVariant = await GetMovementsByVariantAsync(items);

                StockReportService.ExportToPdf(items, movementsByVariant, saveFileDialog.FileName);

                MessageBox.Show(
                    "PDF-звіт успішно збережено.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Помилка при експорті в PDF:\n\n" + ex.Message,
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private async void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            List<StockItem> items = GetCurrentStockItems();

            if (items.Count == 0)
            {
                MessageBox.Show(
                    "Немає даних для друку.",
                    "Друк",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                return;
            }

            try
            {
                var movementsByVariant = await GetMovementsByVariantAsync(items);

                StockReportService.PrintStockReport(items, movementsByVariant);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Помилка при друці звіту:\n\n" + ex.Message,
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
        private async Task<Dictionary<int, List<StockMovement>>> GetMovementsByVariantAsync(List<StockItem> items)
        {
            StockRepository repository = new StockRepository();

            Dictionary<int, List<StockMovement>> movementsByVariant = new Dictionary<int, List<StockMovement>>();

            foreach (var item in items)
            {
                var movements = await repository.GetStockMovementsAsync(item.VariantId);

                movementsByVariant[item.VariantId] = movements;
            }

            return movementsByVariant;
        }
    }
}