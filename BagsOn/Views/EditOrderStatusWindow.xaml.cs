using BagsOn.Models;
using BagsOn.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace BagsOn.Views
{
    public partial class EditOrderStatusWindow : Window
    {
        private readonly Order _order;
        private readonly OrderRepository _orderRepository;

        private List<ReferenceItem> _allStatuses = new List<ReferenceItem>();

        public EditOrderStatusWindow(Order order)
        {
            InitializeComponent();

            _order = order;
            _orderRepository = new OrderRepository();

            Loaded += EditOrderStatusWindow_Loaded;
        }


        private async void EditOrderStatusWindow_Loaded(object sender, RoutedEventArgs e)
        {
            FillOrderInfo();
            FillCustomerInfo();

            _allStatuses = await _orderRepository.GetOrderStatusesAsync();

            LoadAllowedStatuses();
        }


        private void FillOrderInfo()
        {
            OrderInfoTextBlock.Text =
                $"№{_order.OrderId} від {_order.OrderDate:dd.MM.yyyy}";

            CurrentStatusTextBlock.Text = _order.StatusName;
        }


        private void FillCustomerInfo()
        {
            CustomerNameTextBox.Text = _order.CustomerName;
            PhoneTextBox.Text = _order.Phone;
            EmailTextBox.Text = _order.Email;
            CityTextBox.Text = _order.City;
            CustomerAddressTextBox.Text = _order.CustomerAddress;

            CustomerCommentTextBox.Text = _order.CustomerComment;
            ManagerCommentTextBox.Text = _order.ManagerComment;
        }


        private void LoadAllowedStatuses()
        {
            List<string> allowedStatusNames = GetAllowedStatusNames(_order.StatusName);

            List<ReferenceItem> allowedStatuses = _allStatuses
                .Where(status => allowedStatusNames.Contains(status.Name))
                .ToList();

            StatusComboBox.ItemsSource = allowedStatuses;

            ReferenceItem? currentStatus = allowedStatuses
                .FirstOrDefault(status => status.Id == _order.StatusId);

            if (currentStatus != null)
            {
                StatusComboBox.SelectedValue = currentStatus.Id;
            }
            else if (allowedStatuses.Count > 0)
            {
                StatusComboBox.SelectedValue = allowedStatuses[0].Id;
            }

            if (_order.StatusName == "Виконано" || _order.StatusName == "Скасовано")
            {
                StatusComboBox.IsEnabled = false;
                StatusHintTextBlock.Text =
                    "Цей статус вже фінальний. Його не можна змінити назад.";
            }
            else
            {
                StatusComboBox.IsEnabled = true;
                StatusHintTextBlock.Text =
                    "У списку показані тільки дозволені переходи статусу.";
            }
        }


        private List<string> GetAllowedStatusNames(string currentStatus)
        {
            if (currentStatus == "Нове")
            {
                return new List<string>
                {
                    "Нове",
                    "В обробці",
                    "Скасовано"
                };
            }

            if (currentStatus == "В обробці")
            {
                return new List<string>
                {
                    "В обробці",
                    "Очікує оплати",
                    "Оплачено",
                    "Скасовано"
                };
            }

            if (currentStatus == "Очікує оплати")
            {
                return new List<string>
                {
                    "Очікує оплати",
                    "Оплачено",
                    "Скасовано"
                };
            }

            if (currentStatus == "Оплачено")
            {
                return new List<string>
                {
                    "Оплачено",
                    "Відправлено",
                    "Скасовано"
                };
            }

            if (currentStatus == "Відправлено")
            {
                return new List<string>
                {
                    "Відправлено",
                    "Виконано"
                };
            }

            if (currentStatus == "Виконано")
            {
                return new List<string>
                {
                    "Виконано"
                };
            }

            if (currentStatus == "Скасовано")
            {
                return new List<string>
                {
                    "Скасовано"
                };
            }

            return new List<string>
            {
                currentStatus
            };
        }


        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CustomerNameTextBox.Text))
            {
                MessageBox.Show("Введіть ім’я клієнта.");
                return;
            }

            if (string.IsNullOrWhiteSpace(PhoneTextBox.Text))
            {
                MessageBox.Show("Введіть телефон клієнта.");
                return;
            }

            if (StatusComboBox.SelectedValue == null)
            {
                MessageBox.Show("Оберіть статус замовлення.");
                return;
            }

            int selectedStatusId = (int)StatusComboBox.SelectedValue;

            try
            {
                await _orderRepository.UpdateOrderAsync(
                    _order.OrderId,
                    _order.CustomerId,
                    CustomerNameTextBox.Text.Trim(),
                    PhoneTextBox.Text.Trim(),
                    EmailTextBox.Text.Trim(),
                    CityTextBox.Text.Trim(),
                    CustomerAddressTextBox.Text.Trim(),
                    selectedStatusId,
                    CustomerCommentTextBox.Text.Trim(),
                    ManagerCommentTextBox.Text.Trim()
                );

                MessageBox.Show("Замовлення успішно оновлено.");

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при редагуванні замовлення:\n" + ex.Message);
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}