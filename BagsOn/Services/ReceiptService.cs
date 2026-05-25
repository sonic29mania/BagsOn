using BagsOn.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace BagsOn.Services
{
    public static class ReceiptService
    {
        public static void PrintReceipt(Order order, List<OrderItemDetail> items)
        {
            FlowDocument document = CreatePrintDocument(order, items);

            PrintDialog printDialog = new PrintDialog();

            bool? result = printDialog.ShowDialog();

            if (result == true)
            {
                printDialog.PrintDocument(
                    ((IDocumentPaginatorSource)document).DocumentPaginator,
                    $"Чек замовлення №{order.OrderId}"
                );
            }
        }


        public static void ExportReceiptToPdf(Order order, List<OrderItemDetail> items, string filePath)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(35);

                    page.DefaultTextStyle(text =>
                        text.FontFamily("Arial").FontSize(11)
                    );

                    page.Header().Column(column =>
                    {
                        column.Item().Text("BagsOn")
                            .FontSize(26)
                            .Bold();

                        column.Item().Text($"Чек замовлення №{order.OrderId}")
                            .FontSize(18)
                            .SemiBold();

                        column.Item().Text($"Дата: {order.OrderDate:dd.MM.yyyy}  Час: {order.OrderTime:hh\\:mm}")
                            .FontSize(11)
                            .FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().PaddingTop(20).Column(column =>
                    {
                        column.Spacing(14);

                        column.Item().Text("Інформація про клієнта")
                            .FontSize(15)
                            .Bold();

                        column.Item().Text($"Клієнт: {GetText(order.CustomerName)}");
                        column.Item().Text($"Телефон: {GetText(order.Phone)}");
                        column.Item().Text($"Email: {GetText(order.Email)}");
                        column.Item().Text($"Місто: {GetText(order.City)}");

                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        column.Item().Text("Інформація про замовлення")
                            .FontSize(15)
                            .Bold();

                        column.Item().Text($"Статус: {GetText(order.StatusName)}");
                        column.Item().Text($"Доставка: {GetText(order.DeliveryTypeName)}");
                        column.Item().Text($"Адреса доставки: {GetText(order.DeliveryAddress)}");

                        string deliveryDate = order.DeliveryDate == null
                            ? "Не вказано"
                            : order.DeliveryDate.Value.ToString("dd.MM.yyyy");

                        string deliveryTime = order.DeliveryTime == null
                            ? "Не вказано"
                            : order.DeliveryTime.Value.ToString(@"hh\:mm");

                        column.Item().Text($"Дата доставки: {deliveryDate}");
                        column.Item().Text($"Час доставки: {deliveryTime}");

                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        column.Item().Text("Товари в замовленні")
                            .FontSize(15)
                            .Bold();

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                            });

                            table.Cell().Element(HeaderCell).Text("Товар");
                            table.Cell().Element(HeaderCell).Text("Бренд");
                            table.Cell().Element(HeaderCell).Text("Колір");
                            table.Cell().Element(HeaderCell).Text("К-сть");
                            table.Cell().Element(HeaderCell).Text("Ціна");
                            table.Cell().Element(HeaderCell).Text("Знижка");
                            table.Cell().Element(HeaderCell).Text("Сума");

                            foreach (OrderItemDetail item in items)
                            {
                                table.Cell().Element(Cell).Text(item.ModelName);
                                table.Cell().Element(Cell).Text(item.BrandName);
                                table.Cell().Element(Cell).Text(GetText(item.ColorName));
                                table.Cell().Element(Cell).Text(item.Quantity.ToString());
                                table.Cell().Element(Cell).Text($"{item.UnitPrice:N2}");
                                table.Cell().Element(Cell).Text($"{item.DiscountPercent:N0}%");
                                table.Cell().Element(Cell).Text($"{item.LineTotal:N2}");
                            }
                        });

                        column.Item().AlignRight().Text($"Разом: {order.TotalAmount:N2} грн")
                            .FontSize(18)
                            .Bold()
                            .FontColor(Colors.Green.Darken2);

                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        column.Item().Text("Коментар")
                            .FontSize(15)
                            .Bold();

                        column.Item().Text(GetText(order.Comment));
                    });

                    page.Footer().AlignCenter().Text("Дякуємо за покупку в BagsOn!")
                        .FontSize(11)
                        .FontColor(Colors.Grey.Darken1);
                });
            })
            .GeneratePdf(filePath);
        }


        private static FlowDocument CreatePrintDocument(Order order, List<OrderItemDetail> items)
        {
            FlowDocument document = new FlowDocument();

            document.PagePadding = new Thickness(35);
            document.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            document.FontSize = 13;
            document.ColumnWidth = double.PositiveInfinity;

            Paragraph title = new Paragraph(new Run("BagsOn"))
            {
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5)
            };

            document.Blocks.Add(title);

            Paragraph subtitle = new Paragraph(new Run($"Чек замовлення №{order.OrderId}"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            document.Blocks.Add(subtitle);

            document.Blocks.Add(CreateParagraph($"Дата: {order.OrderDate:dd.MM.yyyy}  Час: {order.OrderTime:hh\\:mm}"));
            document.Blocks.Add(CreateParagraph($"Клієнт: {GetText(order.CustomerName)}"));
            document.Blocks.Add(CreateParagraph($"Телефон: {GetText(order.Phone)}"));
            document.Blocks.Add(CreateParagraph($"Статус: {GetText(order.StatusName)}"));
            document.Blocks.Add(CreateParagraph($"Доставка: {GetText(order.DeliveryTypeName)}"));
            document.Blocks.Add(CreateParagraph($"Адреса: {GetText(order.DeliveryAddress)}"));

            Paragraph productsTitle = new Paragraph(new Run("Товари:"))
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 20, 0, 10)
            };

            document.Blocks.Add(productsTitle);

            Table table = new Table();
            table.CellSpacing = 0;

            table.Columns.Add(new TableColumn { Width = new GridLength(220) });
            table.Columns.Add(new TableColumn { Width = new GridLength(80) });
            table.Columns.Add(new TableColumn { Width = new GridLength(80) });
            table.Columns.Add(new TableColumn { Width = new GridLength(100) });
            table.Columns.Add(new TableColumn { Width = new GridLength(110) });

            TableRowGroup group = new TableRowGroup();

            TableRow header = new TableRow();
            header.Cells.Add(CreateTableCell("Товар", true));
            header.Cells.Add(CreateTableCell("Колір", true));
            header.Cells.Add(CreateTableCell("К-сть", true));
            header.Cells.Add(CreateTableCell("Ціна", true));
            header.Cells.Add(CreateTableCell("Сума", true));

            group.Rows.Add(header);

            foreach (OrderItemDetail item in items)
            {
                TableRow row = new TableRow();

                row.Cells.Add(CreateTableCell(item.ModelName, false));
                row.Cells.Add(CreateTableCell(GetText(item.ColorName), false));
                row.Cells.Add(CreateTableCell(item.Quantity.ToString(), false));
                row.Cells.Add(CreateTableCell($"{item.UnitPrice:N2}", false));
                row.Cells.Add(CreateTableCell($"{item.LineTotal:N2}", false));

                group.Rows.Add(row);
            }

            table.RowGroups.Add(group);
            document.Blocks.Add(table);

            Paragraph total = new Paragraph(new Run($"Разом: {order.TotalAmount:N2} грн"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Right,
                Margin = new Thickness(0, 20, 0, 10)
            };

            document.Blocks.Add(total);

            Paragraph thanks = new Paragraph(new Run("Дякуємо за покупку в BagsOn!"))
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            document.Blocks.Add(thanks);

            return document;
        }


        private static Paragraph CreateParagraph(string text)
        {
            return new Paragraph(new Run(text))
            {
                Margin = new Thickness(0, 0, 0, 5)
            };
        }


        private static TableCell CreateTableCell(string text, bool isHeader)
        {
            TableCell cell = new TableCell(new Paragraph(new Run(text)))
            {
                Padding = new Thickness(6),
                BorderThickness = new Thickness(0, 0, 0, 1),
                BorderBrush = System.Windows.Media.Brushes.LightGray
            };

            if (isHeader)
            {
                cell.FontWeight = FontWeights.Bold;
                cell.Background = System.Windows.Media.Brushes.AliceBlue;
            }

            return cell;
        }


        private static IContainer HeaderCell(IContainer container)
        {
            return container
                .Background(Colors.Grey.Lighten3)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(6);
        }


        private static IContainer Cell(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten3)
                .Padding(6);
        }


        private static string GetText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Не вказано";
            }

            return value;
        }
    }
}