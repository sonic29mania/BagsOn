using BagsOn.Models;
using BagsOn.Services;
using BagsOn.ViewModels;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BagsOn.Views
{
    public partial class AnalyticsView : UserControl
    {
        public AnalyticsView()
        {
            InitializeComponent();

            Loaded += AnalyticsView_Loaded;
        }

        private async void AnalyticsView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is AnalyticsVM viewModel)
                {
                    await viewModel.LoadAnalyticsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Помилка при завантаженні аналітики:\n\n" + ex.Message,
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await ReloadAnalyticsAsync();
        }

        private async void ApplyPeriodButton_Click(object sender, RoutedEventArgs e)
        {
            await ReloadAnalyticsAsync();
        }

        private async void TodayButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AnalyticsVM viewModel)
            {
                viewModel.SetToday();
                await viewModel.LoadAnalyticsAsync();
            }
        }

        private async void Last7DaysButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AnalyticsVM viewModel)
            {
                viewModel.SetLast7Days();
                await viewModel.LoadAnalyticsAsync();
            }
        }
        private void OpenModelMovementHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not AnalyticsStockMovement movement)
            {
                return;
            }

            StockItem stockItem = new StockItem
            {
                VariantId = movement.VariantId,
                ModelName = movement.ModelName,
                BrandName = movement.BrandName,
                ColorName = movement.ColorName
            };

            StockMovementsWindow window = new StockMovementsWindow(stockItem);
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();
        }
        private void ExportMovementsExcelButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not AnalyticsVM viewModel)
            {
                return;
            }

            List<AnalyticsStockMovement> movements = viewModel.GetMovementsForExport();

            if (movements.Count == 0)
            {
                MessageBox.Show(
                    "Немає рухів товару для експорту.\n\nОберіть хоча б один тип руху або змініть фільтр пошуку.",
                    "Експорт",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Зберегти рух товарів в Excel",
                Filter = "Excel файл (*.xlsx)|*.xlsx",
                FileName = $"Рух_товарів_{DateTime.Now:yyyy_MM_dd_HH_mm}.xlsx"
            };

            bool? result = saveFileDialog.ShowDialog();

            if (result != true)
            {
                return;
            }

            try
            {
                AnalyticsReportService.ExportStockMovementsToExcel(
                    movements,
                    saveFileDialog.FileName
                );

                MessageBox.Show(
                    "Excel-звіт по руху товарів успішно збережено.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Помилка при експорті руху товарів в Excel:\n\n" + ex.Message,
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void ExportMovementsPdfButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not AnalyticsVM viewModel)
            {
                return;
            }

            List<AnalyticsStockMovement> movements = viewModel.GetMovementsForExport();

            if (movements.Count == 0)
            {
                MessageBox.Show(
                    "Немає рухів товару для експорту.\n\nОберіть хоча б один тип руху або змініть фільтр пошуку.",
                    "Експорт",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Зберегти рух товарів в PDF",
                Filter = "PDF файл (*.pdf)|*.pdf",
                FileName = $"Рух_товарів_{DateTime.Now:yyyy_MM_dd_HH_mm}.pdf"
            };

            bool? result = saveFileDialog.ShowDialog();

            if (result != true)
            {
                return;
            }

            try
            {
                AnalyticsReportService.ExportStockMovementsToPdf(
                    movements,
                    saveFileDialog.FileName
                );

                MessageBox.Show(
                    "PDF-звіт по руху товарів успішно збережено.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Помилка при експорті руху товарів у PDF:\n\n" + ex.Message,
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private async void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is AnalyticsVM viewModel)
                {
                    await viewModel.GenerateReportAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Помилка при формуванні звіту:\n\n" + ex.Message,
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void PrintReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not AnalyticsVM viewModel ||
                viewModel.CurrentReportTable == null ||
                viewModel.CurrentReportTable.Rows.Count == 0)
            {
                MessageBox.Show(
                    "Спочатку сформуйте звіт.",
                    "Друк",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                return;
            }

            try
            {
                DateTime from = viewModel.DateFrom ?? DateTime.Now;
                DateTime to = viewModel.DateTo ?? DateTime.Now;

                ReportsService.PrintReport(
                    viewModel.CurrentReportTable,
                    viewModel.ReportTitle,
                    from,
                    to
                );
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

        private void ExportReportPdfButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not AnalyticsVM viewModel ||
                viewModel.CurrentReportTable == null ||
                viewModel.CurrentReportTable.Rows.Count == 0)
            {
                MessageBox.Show(
                    "Спочатку сформуйте звіт.",
                    "Експорт PDF",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Зберегти звіт у PDF",
                Filter = "PDF файл (*.pdf)|*.pdf",
                FileName = $"{viewModel.ReportTitle}_{DateTime.Now:yyyy_MM_dd_HH_mm}.pdf"
            };

            bool? result = saveFileDialog.ShowDialog();

            if (result != true)
            {
                return;
            }

            try
            {
                DateTime from = viewModel.DateFrom ?? DateTime.Now;
                DateTime to = viewModel.DateTo ?? DateTime.Now;

                ReportsService.ExportReportToPdf(
                    viewModel.CurrentReportTable,
                    viewModel.ReportTitle,
                    saveFileDialog.FileName,
                    from,
                    to
                );

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
                    "Помилка при експорті звіту в PDF:\n\n" + ex.Message,
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
        private async Task ReloadAnalyticsAsync()
        {
            try
            {
                if (DataContext is AnalyticsVM viewModel)
                {
                    await viewModel.LoadAnalyticsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Помилка при оновленні аналітики:\n\n" + ex.Message,
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}