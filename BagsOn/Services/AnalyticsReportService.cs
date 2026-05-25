using BagsOn.Models;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;

namespace BagsOn.Services
{
    public static class AnalyticsReportService
    {
        public static void ExportStockMovementsToExcel(
            List<AnalyticsStockMovement> movements,
            string filePath)
        {
            using XLWorkbook workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Рух товарів");

            worksheet.Cell(1, 1).Value = "Звіт по руху товарів";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 18;

            worksheet.Cell(2, 1).Value = $"Дата формування: {DateTime.Now:dd.MM.yyyy HH:mm}";
            worksheet.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;

            int headerRow = 4;

            worksheet.Cell(headerRow, 1).Value = "№";
            worksheet.Cell(headerRow, 2).Value = "Дата руху";
            worksheet.Cell(headerRow, 3).Value = "Тип руху";
            worksheet.Cell(headerRow, 4).Value = "Модель";
            worksheet.Cell(headerRow, 5).Value = "Бренд";
            worksheet.Cell(headerRow, 6).Value = "Колір";
            worksheet.Cell(headerRow, 7).Value = "Кількість";
            worksheet.Cell(headerRow, 8).Value = "Було";
            worksheet.Cell(headerRow, 9).Value = "Стало";
            worksheet.Cell(headerRow, 10).Value = "Останній рух";
            worksheet.Cell(headerRow, 11).Value = "Коментар";

            var headerRange = worksheet.Range(headerRow, 1, headerRow, 11);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#EEF2FF");
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int row = headerRow + 1;
            int number = 1;

            foreach (var movement in movements)
            {
                worksheet.Cell(row, 1).Value = number;
                worksheet.Cell(row, 2).Value = movement.CreatedAt;
                worksheet.Cell(row, 3).Value = movement.MovementType;
                worksheet.Cell(row, 4).Value = movement.ModelName;
                worksheet.Cell(row, 5).Value = movement.BrandName;
                worksheet.Cell(row, 6).Value = movement.ColorName;
                worksheet.Cell(row, 7).Value = movement.QuantityChange;
                worksheet.Cell(row, 8).Value = movement.QuantityBefore;
                worksheet.Cell(row, 9).Value = movement.QuantityAfter;
                worksheet.Cell(row, 10).Value = movement.LastMovementText;
                worksheet.Cell(row, 11).Value = movement.Comment;

                if (movement.MovementType == "Надходження")
                {
                    worksheet.Range(row, 1, row, 11).Style.Fill.BackgroundColor = XLColor.FromHtml("#DCFCE7");
                }
                else if (movement.MovementType == "Списання")
                {
                    worksheet.Range(row, 1, row, 11).Style.Fill.BackgroundColor = XLColor.FromHtml("#FEE2E2");
                }
                else if (movement.MovementType == "Продаж")
                {
                    worksheet.Range(row, 1, row, 11).Style.Fill.BackgroundColor = XLColor.FromHtml("#DBEAFE");
                }
                else if (movement.MovementType == "Повернення")
                {
                    worksheet.Range(row, 1, row, 11).Style.Fill.BackgroundColor = XLColor.FromHtml("#EDE9FE");
                }
                else if (movement.MovementType == "Коригування")
                {
                    worksheet.Range(row, 1, row, 11).Style.Fill.BackgroundColor = XLColor.FromHtml("#FEF3C7");
                }

                row++;
                number++;
            }

            worksheet.Column(2).Style.DateFormat.Format = "dd.MM.yyyy HH:mm";

            var tableRange = worksheet.Range(headerRow, 1, row - 1, 11);
            tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            tableRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#E5E7EB");
            tableRange.Style.Border.InsideBorderColor = XLColor.FromHtml("#E5E7EB");

            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(filePath);
        }

        public static void ExportStockMovementsToPdf(
            List<AnalyticsStockMovement> movements,
            string filePath)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);

                    page.DefaultTextStyle(text =>
                        text.FontFamily("Arial").FontSize(8)
                    );

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .Text("Звіт по руху товарів")
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
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.1f);
                                columns.RelativeColumn(1.7f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1f);
                                columns.RelativeColumn(0.8f);
                                columns.RelativeColumn(0.8f);
                                columns.RelativeColumn(0.8f);
                                columns.RelativeColumn(1.1f);
                                columns.RelativeColumn(2f);
                            });

                            table.Header(header =>
                            {
                                AddPdfHeaderCell(header, "№");
                                AddPdfHeaderCell(header, "Дата");
                                AddPdfHeaderCell(header, "Тип");
                                AddPdfHeaderCell(header, "Модель");
                                AddPdfHeaderCell(header, "Бренд");
                                AddPdfHeaderCell(header, "Колір");
                                AddPdfHeaderCell(header, "К-сть");
                                AddPdfHeaderCell(header, "Було");
                                AddPdfHeaderCell(header, "Стало");
                                AddPdfHeaderCell(header, "Останній");
                                AddPdfHeaderCell(header, "Коментар");
                            });

                            int number = 1;

                            foreach (var movement in movements)
                            {
                                AddPdfCell(table, number.ToString());
                                AddPdfCell(table, movement.CreatedAt.ToString("dd.MM.yyyy HH:mm"));
                                AddPdfCell(table, movement.MovementType);
                                AddPdfCell(table, movement.ModelName);
                                AddPdfCell(table, movement.BrandName);
                                AddPdfCell(table, movement.ColorName);
                                AddPdfCell(table, movement.QuantityChangeText);
                                AddPdfCell(table, movement.QuantityBefore.ToString());
                                AddPdfCell(table, movement.QuantityAfter.ToString());
                                AddPdfCell(table, movement.LastMovementText);
                                AddPdfCell(table, movement.Comment);

                                number++;
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