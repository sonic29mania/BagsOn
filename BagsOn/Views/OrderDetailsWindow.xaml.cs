using BagsOn.Models;
using BagsOn.Repositories;
using System;
using System.Collections.Generic;
using System.Windows;
using BagsOn.Services;
using Microsoft.Win32;

namespace BagsOn.Views
{
    public partial class OrderDetailsWindow : Window
    {
        private readonly Order _order;
        private readonly OrderRepository _orderRepository;

        public OrderDetailsWindow(Order order)
        {
            InitializeComponent();

            _order = order;
            _orderRepository = new OrderRepository();

            Loaded += OrderDetailsWindow_Loaded;
        }
        private List<OrderItemDetail> _items = new List<OrderItemDetail>();

        private async void OrderDetailsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            FillOrderInfo();

            _items = await _orderRepository.GetOrderItemsAsync(_order.OrderId);

            OrderItemsDataGrid.ItemsSource = _items;
        }


        private void FillOrderInfo()
        {
            OrderTitleTextBlock.Text = $"Замовлення №{_order.OrderId}";

            OrderDateTextBlock.Text =
                $"Дата: {_order.OrderDate:dd.MM.yyyy}  |  Час: {_order.OrderTime:hh\\:mm}";

            StatusTextBlock.Text = _order.StatusName;

            CustomerNameTextBlock.Text = GetText(_order.CustomerName);
            PhoneTextBlock.Text = GetText(_order.Phone);
            EmailTextBlock.Text = GetText(_order.Email);
            CityTextBlock.Text = GetText(_order.City);

            DeliveryTypeTextBlock.Text = GetText(_order.DeliveryTypeName);

            DeliveryDateTextBlock.Text = _order.DeliveryDate == null
                ? "Не вказано"
                : _order.DeliveryDate.Value.ToString("dd.MM.yyyy");

            DeliveryTimeTextBlock.Text = _order.DeliveryTime == null
                ? "Не вказано"
                : _order.DeliveryTime.Value.ToString(@"hh\:mm");

            DeliveryAddressTextBlock.Text = GetText(_order.DeliveryAddress);

            CommentTextBlock.Text = GetText(_order.Comment);

            TotalAmountTextBlock.Text = $"{_order.TotalAmount:N2} грн";

            FillArchiveInfo();
        }
        private void FillArchiveInfo()
        {
            bool isArchiveOrder =
                _order.IsArchived ||
                _order.StatusName == "Виконано" ||
                _order.StatusName == "Скасовано";

            if (!isArchiveOrder)
            {
                ArchiveInfoBorder.Visibility = Visibility.Collapsed;
                return;
            }

            ArchiveInfoBorder.Visibility = Visibility.Visible;

            ArchiveTypeTextBlock.Text = GetArchiveTypeText();

            ArchiveDateTextBlock.Text = _order.ArchivedAt == null
                ? "Не вказано"
                : _order.ArchivedAt.Value.ToString("dd.MM.yyyy HH:mm");

            ArchiveReasonTextBlock.Text = GetArchiveReasonText();
        }

        private string GetArchiveTypeText()
        {
            if (_order.StatusName == "Виконано")
            {
                return "Виконане";
            }

            if (_order.StatusName == "Скасовано")
            {
                return "Скасоване";
            }

            if (_order.IsArchived)
            {
                return "Ручно архівоване";
            }

            return "Архівне";
        }

        private string GetArchiveReasonText()
        {
            if (!string.IsNullOrWhiteSpace(_order.ArchiveReason))
            {
                return _order.ArchiveReason;
            }

            if (_order.StatusName == "Виконано")
            {
                return "Замовлення успішно виконано";
            }

            if (_order.StatusName == "Скасовано")
            {
                return "Замовлення скасовано";
            }

            if (_order.IsArchived)
            {
                return "Замовлення перенесено в архів";
            }

            return "Не вказано";
        }
        private string GetText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Не вказано";
            }

            return value;
        }

        private void PrintReceiptButton_Click(object sender, RoutedEventArgs e)
        {
            if (_items.Count == 0)
            {
                MessageBox.Show("У замовленні немає товарів для друку.");
                return;
            }

            ReceiptService.PrintReceipt(_order, _items);
        }


        private void ExportPdfButton_Click(object sender, RoutedEventArgs e)
        {
            if (_items.Count == 0)
            {
                MessageBox.Show("У замовленні немає товарів для експорту.");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Зберегти чек у PDF",
                Filter = "PDF файл (*.pdf)|*.pdf",
                FileName = $"Чек_замовлення_{_order.OrderId}.pdf"
            };

            bool? result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                try
                {
                    ReceiptService.ExportReceiptToPdf(_order, _items, saveFileDialog.FileName);

                    MessageBox.Show("PDF-чек успішно збережено.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка при експорті PDF:\n" + ex.Message);
                }
            }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}