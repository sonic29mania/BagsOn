using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
// Клас ReportsService відповідає за експорт і друк універсальних табличних звітів.
namespace BagsOn.Services
{
    public static class ReportsService
    {

        // Метод ExportReportToPdf експортує табличний звіт у PDF-файл.
        public static void ExportReportToPdf(
            DataTable table,
            string reportTitle,
            string filePath,
            DateTime dateFrom,
            DateTime dateTo)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);

                    page.DefaultTextStyle(text =>
                        text.FontFamily("Arial").FontSize(7)
                    );

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .Text(reportTitle)
                                .FontSize(20)
                                .Bold()
                                .FontColor("#111827");

                            column.Item()
                                .Text($"Період: {dateFrom:dd.MM.yyyy} - {dateTo:dd.MM.yyyy}")
                                .FontSize(10)
                                .FontColor("#6B7280");

                            column.Item()
                                .Text($"Дата формування: {DateTime.Now:dd.MM.yyyy HH:mm}")
                                .FontSize(10)
                                .FontColor("#6B7280");
                        });

                    page.Content()
                        .PaddingTop(15)
                        .Table(tableDescriptor =>
                        {
                            tableDescriptor.ColumnsDefinition(columns =>
                            {
                                foreach (DataColumn column in table.Columns)
                                {
                                    columns.RelativeColumn();
                                }
                            });

                            tableDescriptor.Header(header =>
                            {
                                foreach (DataColumn column in table.Columns)
                                {
                                    AddPdfHeaderCell(header, column.ColumnName);
                                }
                            });

                            foreach (DataRow row in table.Rows)
                            {
                                foreach (DataColumn column in table.Columns)
                                {
                                    AddPdfCell(tableDescriptor, FormatValue(row[column]));
                                }
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
        // Метод PrintReport відкриває діалог друку і друкує табличний звіт.
        // Якщо користувач не підтвердив друк, метод не виконує жодних дій.
        public static void PrintReport(
            DataTable table,
            string reportTitle,
            DateTime dateFrom,
            DateTime dateTo)
        {
            PrintDialog printDialog = new PrintDialog();

            bool? result = printDialog.ShowDialog();

            if (result != true)
            {
                return;
            }

            FlowDocument document = CreatePrintDocument(
                table,
                reportTitle,
                dateFrom,
                dateTo
            );

            document.PageWidth = printDialog.PrintableAreaWidth;
            document.PageHeight = printDialog.PrintableAreaHeight;
            document.PagePadding = new Thickness(30);
            document.ColumnWidth = printDialog.PrintableAreaWidth;

            IDocumentPaginatorSource paginatorSource = document;

            printDialog.PrintDocument(
                paginatorSource.DocumentPaginator,
                reportTitle
            );
        }
        // Метод CreatePrintDocument створює друкований документ FlowDocument на основі DataTable.
        private static FlowDocument CreatePrintDocument(
            DataTable table,
            string reportTitle,
            DateTime dateFrom,
            DateTime dateTo)
        {
            FlowDocument document = new FlowDocument();

            Paragraph title = new Paragraph(new Run(reportTitle))
            {
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(17, 24, 39)),
                Margin = new Thickness(0, 0, 0, 6)
            };

            Paragraph period = new Paragraph(
                new Run($"Період: {dateFrom:dd.MM.yyyy} - {dateTo:dd.MM.yyyy}")
            )
            {
                FontSize = 12,
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(107, 114, 128)),
                Margin = new Thickness(0, 0, 0, 4)
            };

            Paragraph createdAt = new Paragraph(
                new Run($"Дата формування: {DateTime.Now:dd.MM.yyyy HH:mm}")
            )
            {
                FontSize = 12,
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(107, 114, 128)),
                Margin = new Thickness(0, 0, 0, 18)
            };

            document.Blocks.Add(title);
            document.Blocks.Add(period);
            document.Blocks.Add(createdAt);

            Table printTable = new Table
            {
                CellSpacing = 0
            };

            foreach (DataColumn column in table.Columns)
            {
                printTable.Columns.Add(new TableColumn());
            }

            TableRowGroup headerGroup = new TableRowGroup();
            TableRow headerRow = new TableRow();

            foreach (DataColumn column in table.Columns)
            {
                headerRow.Cells.Add(CreatePrintCell(column.ColumnName, true));
            }

            headerGroup.Rows.Add(headerRow);
            printTable.RowGroups.Add(headerGroup);

            TableRowGroup bodyGroup = new TableRowGroup();

            foreach (DataRow row in table.Rows)
            {
                TableRow tableRow = new TableRow();

                foreach (DataColumn column in table.Columns)
                {
                    tableRow.Cells.Add(CreatePrintCell(FormatValue(row[column]), false));
                }

                bodyGroup.Rows.Add(tableRow);
            }

            printTable.RowGroups.Add(bodyGroup);

            document.Blocks.Add(printTable);

            return document;
        }
        // Метод CreatePrintCell створює комірку таблиці для друкованого звіту.
        private static TableCell CreatePrintCell(string text, bool isHeader)
        {
            TableCell cell = new TableCell(new Paragraph(new Run(text)))
            {
                Padding = new Thickness(5),
                BorderThickness = new Thickness(0.5),
                BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(229, 231, 235))
            };

            if (isHeader)
            {
                cell.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(238, 242, 255));
                cell.FontWeight = FontWeights.Bold;
            }

            return cell;
        }
        // Метод AddPdfHeaderCell додає комірку заголовка до PDF-таблиці.
        private static void AddPdfHeaderCell(TableCellDescriptor header, string text)
        {
            header.Cell()
                .Background("#EEF2FF")
                .Border(1)
                .BorderColor("#E5E7EB")
                .Padding(4)
                .Text(text)
                .Bold()
                .FontSize(7);
        }
        // Метод AddPdfCell додає звичайну комірку до PDF-таблиці.
        private static void AddPdfCell(TableDescriptor table, string text)
        {
            table.Cell()
                .Border(1)
                .BorderColor("#E5E7EB")
                .Padding(4)
                .Text(text)
                .FontSize(7);
        }
        
        // Метод FormatValue перетворює значення з таблиці у текст для звіту.

        private static string FormatValue(object value)
        {
            if (value == DBNull.Value || value == null)
            {
                return "";
            }

            if (value is DateTime dateTime)
            {
                return dateTime.ToString("dd.MM.yyyy HH:mm");
            }

            if (value is decimal decimalValue)
            {
                return decimalValue.ToString("N2");
            }

            return value.ToString() ?? "";
        }
    }
}