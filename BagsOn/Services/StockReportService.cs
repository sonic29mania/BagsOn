using BagsOn.Models;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
// Клас StockReportService відповідає за створення деталізованих звітів по складу.
namespace BagsOn.Services
{
    public static class StockReportService
    {
        // Метод ExportToExcel формує Excel-звіт по складу.
        public static void ExportToExcel(
             List<StockItem> items,
             Dictionary<int, List<StockMovement>> movementsByVariant,
             string filePath)
        {
            using XLWorkbook workbook = new XLWorkbook();

            var stockSheet = workbook.Worksheets.Add("Залишки");

            stockSheet.Cell(1, 1).Value = "Звіт по складу";
            stockSheet.Cell(1, 1).Style.Font.Bold = true;
            stockSheet.Cell(1, 1).Style.Font.FontSize = 18;

            stockSheet.Cell(2, 1).Value = $"Дата формування: {DateTime.Now:dd.MM.yyyy HH:mm}";
            stockSheet.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;

            int headerRow = 4;

            stockSheet.Cell(headerRow, 1).Value = "№";
            stockSheet.Cell(headerRow, 2).Value = "Модель";
            stockSheet.Cell(headerRow, 3).Value = "Бренд";
            stockSheet.Cell(headerRow, 4).Value = "Тип";
            stockSheet.Cell(headerRow, 5).Value = "Категорії";
            stockSheet.Cell(headerRow, 6).Value = "Колір";
            stockSheet.Cell(headerRow, 7).Value = "Ціна";
            stockSheet.Cell(headerRow, 8).Value = "Всього";
            stockSheet.Cell(headerRow, 9).Value = "Резерв";
            stockSheet.Cell(headerRow, 10).Value = "Доступно";
            stockSheet.Cell(headerRow, 11).Value = "Мін.";
            stockSheet.Cell(headerRow, 12).Value = "Статус";
            stockSheet.Cell(headerRow, 13).Value = "Попередження";
            stockSheet.Cell(headerRow, 14).Value = "Місце";
            stockSheet.Cell(headerRow, 15).Value = "Вартість складу";

            var headerRange = stockSheet.Range(headerRow, 1, headerRow, 15);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#EEF2FF");
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int row = headerRow + 1;
            int number = 1;

            foreach (var item in items)
            {
                stockSheet.Cell(row, 1).Value = number;
                stockSheet.Cell(row, 2).Value = item.ModelName;
                stockSheet.Cell(row, 3).Value = item.BrandName;
                stockSheet.Cell(row, 4).Value = item.TypeName;
                stockSheet.Cell(row, 5).Value = item.CategoryName;
                stockSheet.Cell(row, 6).Value = item.ColorName;
                stockSheet.Cell(row, 7).Value = item.Price;
                stockSheet.Cell(row, 8).Value = item.TotalQuantity;
                stockSheet.Cell(row, 9).Value = item.ReservedQuantity;
                stockSheet.Cell(row, 10).Value = item.AvailableQuantity;
                stockSheet.Cell(row, 11).Value = item.MinQuantity;
                stockSheet.Cell(row, 12).Value = item.StockStatus;
                stockSheet.Cell(row, 13).Value = item.StockWarning;
                stockSheet.Cell(row, 14).Value = item.Location;
                stockSheet.Cell(row, 15).Value = item.TotalValue;

                if (item.StockStatus == "Немає")
                {
                    stockSheet.Range(row, 1, row, 15).Style.Fill.BackgroundColor = XLColor.FromHtml("#FEE2E2");
                }
                else if (item.StockStatus == "Мало")
                {
                    stockSheet.Range(row, 1, row, 15).Style.Fill.BackgroundColor = XLColor.FromHtml("#FEF3C7");
                }
                else if (item.StockStatus == "Є резерв")
                {
                    stockSheet.Range(row, 1, row, 15).Style.Fill.BackgroundColor = XLColor.FromHtml("#EDE9FE");
                }

                row++;
                number++;
            }

            stockSheet.Column(7).Style.NumberFormat.Format = "#,##0.00 \"грн\"";
            stockSheet.Column(15).Style.NumberFormat.Format = "#,##0.00 \"грн\"";

            stockSheet.Columns().AdjustToContents();

            var movementsSheet = workbook.Worksheets.Add("Рух товарів");

            movementsSheet.Cell(1, 1).Value = "Рух товарів по складу";
            movementsSheet.Cell(1, 1).Style.Font.Bold = true;
            movementsSheet.Cell(1, 1).Style.Font.FontSize = 18;

            movementsSheet.Cell(2, 1).Value = $"Дата формування: {DateTime.Now:dd.MM.yyyy HH:mm}";
            movementsSheet.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;

            int movementHeaderRow = 4;

            movementsSheet.Cell(movementHeaderRow, 1).Value = "Модель";
            movementsSheet.Cell(movementHeaderRow, 2).Value = "Бренд";
            movementsSheet.Cell(movementHeaderRow, 3).Value = "Колір";
            movementsSheet.Cell(movementHeaderRow, 4).Value = "Дата";
            movementsSheet.Cell(movementHeaderRow, 5).Value = "Операція";
            movementsSheet.Cell(movementHeaderRow, 6).Value = "Кількість";
            movementsSheet.Cell(movementHeaderRow, 7).Value = "Було";
            movementsSheet.Cell(movementHeaderRow, 8).Value = "Стало";
            movementsSheet.Cell(movementHeaderRow, 9).Value = "Замовлення";
            movementsSheet.Cell(movementHeaderRow, 10).Value = "Коментар";

            var movementHeaderRange = movementsSheet.Range(movementHeaderRow, 1, movementHeaderRow, 10);
            movementHeaderRange.Style.Font.Bold = true;
            movementHeaderRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#EEF2FF");
            movementHeaderRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int movementRow = movementHeaderRow + 1;

            foreach (var item in items)
            {
                if (!movementsByVariant.ContainsKey(item.VariantId) ||
                    movementsByVariant[item.VariantId].Count == 0)
                {
                    movementsSheet.Cell(movementRow, 1).Value = item.ModelName;
                    movementsSheet.Cell(movementRow, 2).Value = item.BrandName;
                    movementsSheet.Cell(movementRow, 3).Value = item.ColorName;
                    movementsSheet.Cell(movementRow, 4).Value = "—";
                    movementsSheet.Cell(movementRow, 5).Value = "Рухів ще немає";
                    movementsSheet.Cell(movementRow, 6).Value = "";
                    movementsSheet.Cell(movementRow, 7).Value = "";
                    movementsSheet.Cell(movementRow, 8).Value = "";
                    movementsSheet.Cell(movementRow, 9).Value = "";
                    movementsSheet.Cell(movementRow, 10).Value = "";

                    movementRow++;
                    continue;
                }

                foreach (var movement in movementsByVariant[item.VariantId])
                {
                    movementsSheet.Cell(movementRow, 1).Value = item.ModelName;
                    movementsSheet.Cell(movementRow, 2).Value = item.BrandName;
                    movementsSheet.Cell(movementRow, 3).Value = item.ColorName;
                    movementsSheet.Cell(movementRow, 4).Value = movement.CreatedAt;
                    movementsSheet.Cell(movementRow, 5).Value = movement.MovementType;
                    movementsSheet.Cell(movementRow, 6).Value = movement.QuantityChange;
                    movementsSheet.Cell(movementRow, 7).Value = movement.QuantityBefore;
                    movementsSheet.Cell(movementRow, 8).Value = movement.QuantityAfter;
                    movementsSheet.Cell(movementRow, 9).Value = movement.OrderText;
                    movementsSheet.Cell(movementRow, 10).Value = movement.Comment;

                    if (movement.MovementType == "Надходження")
                    {
                        movementsSheet.Range(movementRow, 1, movementRow, 10).Style.Fill.BackgroundColor = XLColor.FromHtml("#DCFCE7");
                    }
                    else if (movement.MovementType == "Списання")
                    {
                        movementsSheet.Range(movementRow, 1, movementRow, 10).Style.Fill.BackgroundColor = XLColor.FromHtml("#FEE2E2");
                    }
                    else if (movement.MovementType == "Коригування")
                    {
                        movementsSheet.Range(movementRow, 1, movementRow, 10).Style.Fill.BackgroundColor = XLColor.FromHtml("#FEF3C7");
                    }

                    movementRow++;
                }
            }

            movementsSheet.Column(4).Style.DateFormat.Format = "dd.MM.yyyy HH:mm";
            movementsSheet.Columns().AdjustToContents();

            workbook.SaveAs(filePath);
        }
        // Метод ExportToPdf формує деталізований PDF-звіт по складу.
        // Для кожного товару він показує залишки, резерв, доступну кількість, статус
        // і таблицю руху товарів по відповідному варіанту.
        public static void ExportToPdf(
            List<StockItem> items,
            Dictionary<int, List<StockMovement>> movementsByVariant,
            string filePath)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);
                    page.DefaultTextStyle(text => text.FontFamily("Arial").FontSize(8));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .Text("Деталізований звіт по складу")
                                .FontSize(20)
                                .Bold()
                                .FontColor("#111827");

                            column.Item()
                                .Text($"Дата формування: {DateTime.Now:dd.MM.yyyy HH:mm}")
                                .FontSize(10)
                                .FontColor("#6B7280");
                        });

                    page.Content()
                        .PaddingTop(15)
                        .Column(column =>
                        {
                            foreach (var item in items)
                            {
                                column.Item()
                                    .PaddingBottom(10)
                                    .Border(1)
                                    .BorderColor("#E5E7EB")
                                    .Padding(8)
                                    .Column(productColumn =>
                                    {
                                        productColumn.Item()
                                            .Text($"{item.BrandName} {item.ModelName} | Колір: {item.ColorName}")
                                            .FontSize(12)
                                            .Bold()
                                            .FontColor("#111827");

                                        productColumn.Item()
                                            .Text($"Всього: {item.TotalQuantity} | Резерв: {item.ReservedQuantity} | Доступно: {item.AvailableQuantity} | Статус: {item.StockStatus} | Вартість: {item.TotalValue:N2} грн")
                                            .FontSize(8)
                                            .FontColor("#6B7280");

                                        productColumn.Item()
                                            .PaddingTop(6)
                                            .Table(table =>
                                            {
                                                table.ColumnsDefinition(columns =>
                                                {
                                                    columns.RelativeColumn(1.2f);
                                                    columns.RelativeColumn(1.2f);
                                                    columns.RelativeColumn(0.8f);
                                                    columns.RelativeColumn(0.8f);
                                                    columns.RelativeColumn(0.8f);
                                                    columns.RelativeColumn(1f);
                                                    columns.RelativeColumn(2.2f);
                                                });

                                                table.Header(header =>
                                                {
                                                    AddPdfHeaderCell(header, "Дата");
                                                    AddPdfHeaderCell(header, "Операція");
                                                    AddPdfHeaderCell(header, "К-сть");
                                                    AddPdfHeaderCell(header, "Було");
                                                    AddPdfHeaderCell(header, "Стало");
                                                    AddPdfHeaderCell(header, "Замовл.");
                                                    AddPdfHeaderCell(header, "Коментар");
                                                });

                                                if (movementsByVariant.ContainsKey(item.VariantId) &&
                                                    movementsByVariant[item.VariantId].Count > 0)
                                                {
                                                    foreach (var movement in movementsByVariant[item.VariantId])
                                                    {
                                                        AddPdfCell(table, movement.CreatedAt.ToString("dd.MM.yyyy HH:mm"));
                                                        AddPdfCell(table, movement.MovementType);
                                                        AddPdfCell(table, movement.QuantityChangeText);
                                                        AddPdfCell(table, movement.QuantityBefore.ToString());
                                                        AddPdfCell(table, movement.QuantityAfter.ToString());
                                                        AddPdfCell(table, movement.OrderText);
                                                        AddPdfCell(table, movement.Comment);
                                                    }
                                                }
                                                else
                                                {
                                                    AddPdfCell(table, "—");
                                                    AddPdfCell(table, "Рухів ще немає");
                                                    AddPdfCell(table, "");
                                                    AddPdfCell(table, "");
                                                    AddPdfCell(table, "");
                                                    AddPdfCell(table, "");
                                                    AddPdfCell(table, "");
                                                }
                                            });
                                    });
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Сторінка ");
                            text.CurrentPageNumber();
                            text.Span(" з ");
                            text.TotalPages();
                        });
                });
            })
            .GeneratePdf(filePath);
        }
        // Метод PrintStockReport відкриває діалог друку і друкує деталізований звіт по складу.
        public static void PrintStockReport(
            List<StockItem> items,
            Dictionary<int, List<StockMovement>> movementsByVariant)
        {
            PrintDialog printDialog = new PrintDialog();

            bool? result = printDialog.ShowDialog();

            if (result != true)
            {
                return;
            }

            FlowDocument document = CreatePrintDocument(items, movementsByVariant);

            document.PageWidth = printDialog.PrintableAreaWidth;
            document.PageHeight = printDialog.PrintableAreaHeight;
            document.PagePadding = new Thickness(30);
            document.ColumnWidth = printDialog.PrintableAreaWidth;

            IDocumentPaginatorSource paginatorSource = document;

            printDialog.PrintDocument(
                paginatorSource.DocumentPaginator,
                "Деталізований звіт по складу"
            );
        }

        // Метод CreatePrintDocument створює FlowDocument для друку складського звіту.
        private static FlowDocument CreatePrintDocument(
            List<StockItem> items,
            Dictionary<int, List<StockMovement>> movementsByVariant)
        {
            FlowDocument document = new FlowDocument();

            Paragraph title = new Paragraph(new Run("Деталізований звіт по складу"))
            {
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(17, 24, 39)),
                Margin = new Thickness(0, 0, 0, 6)
            };

            Paragraph date = new Paragraph(new Run($"Дата формування: {DateTime.Now:dd.MM.yyyy HH:mm}"))
            {
                FontSize = 12,
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(107, 114, 128)),
                Margin = new Thickness(0, 0, 0, 18)
            };

            document.Blocks.Add(title);
            document.Blocks.Add(date);

            foreach (var item in items)
            {
                Paragraph productTitle = new Paragraph(
                    new Run($"{item.BrandName} {item.ModelName} | Колір: {item.ColorName}")
                )
                {
                    FontSize = 15,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 12, 0, 4)
                };

                Paragraph productInfo = new Paragraph(
                    new Run(
                        $"Всього: {item.TotalQuantity} шт. | " +
                        $"Резерв: {item.ReservedQuantity} шт. | " +
                        $"Доступно: {item.AvailableQuantity} шт. | " +
                        $"Статус: {item.StockStatus} | " +
                        $"Вартість: {item.TotalValue:N2} грн"
                    )
                )
                {
                    FontSize = 11,
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(107, 114, 128)),
                    Margin = new Thickness(0, 0, 0, 6)
                };

                document.Blocks.Add(productTitle);
                document.Blocks.Add(productInfo);

                Table table = new Table
                {
                    CellSpacing = 0,
                    Margin = new Thickness(0, 0, 0, 10)
                };

                table.Columns.Add(new TableColumn { Width = new GridLength(120) });
                table.Columns.Add(new TableColumn { Width = new GridLength(110) });
                table.Columns.Add(new TableColumn { Width = new GridLength(70) });
                table.Columns.Add(new TableColumn { Width = new GridLength(70) });
                table.Columns.Add(new TableColumn { Width = new GridLength(70) });
                table.Columns.Add(new TableColumn { Width = new GridLength(90) });
                table.Columns.Add(new TableColumn { Width = new GridLength(230) });

                TableRowGroup headerGroup = new TableRowGroup();
                TableRow headerRow = new TableRow();

                headerRow.Cells.Add(CreatePrintCell("Дата", true));
                headerRow.Cells.Add(CreatePrintCell("Операція", true));
                headerRow.Cells.Add(CreatePrintCell("К-сть", true));
                headerRow.Cells.Add(CreatePrintCell("Було", true));
                headerRow.Cells.Add(CreatePrintCell("Стало", true));
                headerRow.Cells.Add(CreatePrintCell("Замовл.", true));
                headerRow.Cells.Add(CreatePrintCell("Коментар", true));

                headerGroup.Rows.Add(headerRow);
                table.RowGroups.Add(headerGroup);

                TableRowGroup bodyGroup = new TableRowGroup();

                if (movementsByVariant.ContainsKey(item.VariantId) &&
                    movementsByVariant[item.VariantId].Count > 0)
                {
                    foreach (var movement in movementsByVariant[item.VariantId])
                    {
                        TableRow row = new TableRow();

                        row.Cells.Add(CreatePrintCell(movement.CreatedAt.ToString("dd.MM.yyyy HH:mm"), false));
                        row.Cells.Add(CreatePrintCell(movement.MovementType, false));
                        row.Cells.Add(CreatePrintCell(movement.QuantityChangeText, false));
                        row.Cells.Add(CreatePrintCell(movement.QuantityBefore.ToString(), false));
                        row.Cells.Add(CreatePrintCell(movement.QuantityAfter.ToString(), false));
                        row.Cells.Add(CreatePrintCell(movement.OrderText, false));
                        row.Cells.Add(CreatePrintCell(movement.Comment, false));

                        bodyGroup.Rows.Add(row);
                    }
                }
                else
                {
                    TableRow row = new TableRow();

                    row.Cells.Add(CreatePrintCell("—", false));
                    row.Cells.Add(CreatePrintCell("Рухів ще немає", false));
                    row.Cells.Add(CreatePrintCell("", false));
                    row.Cells.Add(CreatePrintCell("", false));
                    row.Cells.Add(CreatePrintCell("", false));
                    row.Cells.Add(CreatePrintCell("", false));
                    row.Cells.Add(CreatePrintCell("", false));

                    bodyGroup.Rows.Add(row);
                }

                table.RowGroups.Add(bodyGroup);

                document.Blocks.Add(table);
            }

            return document;
        }
        // Метод CreatePrintCell створює комірку таблиці для друкованого складського звіту.
        private static TableCell CreatePrintCell(string text, bool isHeader)
        {
            TableCell cell = new TableCell(new Paragraph(new Run(text)))
            {
                Padding = new Thickness(6),
                BorderThickness = new Thickness(0.5),
                BorderBrush = new SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(229, 231, 235)
                )
            };

            if (isHeader)
            {
                cell.Background = new SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(238, 242, 255)
                );

                cell.FontWeight = FontWeights.Bold;
            }

            return cell;
        }

        private static void AddPdfHeaderCell(TableCellDescriptor header, string text)
        {
            header.Cell()
                .Background("#EEF2FF")
                .Border(1)
                .BorderColor("#E5E7EB")
                .Padding(4)
                .Text(text)
                .Bold()
                .FontSize(8);
        }

        private static void AddPdfCell(TableDescriptor table, string text)
        {
            table.Cell()
                .Border(1)
                .BorderColor("#E5E7EB")
                .Padding(4)
                .Text(text)
                .FontSize(8);
        }

       
    }
}