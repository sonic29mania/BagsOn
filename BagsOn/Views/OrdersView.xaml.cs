using BagsOn.Models;
using BagsOn.Repositories;
using BagsOn.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BagsOn.Views
{
    public partial class OrdersView : UserControl
    {
        public OrdersView()
        {
            InitializeComponent();

            Loaded += OrdersView_Loaded;
        }


        private async void OrdersView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is OrdersVM viewModel)
                {
                    await viewModel.LoadOrdersAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Помилка при завантаженні замовлень:\n\n" + ex.Message,
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }


        private void AddOrderButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Тут буде вікно додавання замовлення.");
        }


        private void ViewOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Order order)
            {
                OrderDetailsWindow window = new OrderDetailsWindow(order);

                window.Owner = Window.GetWindow(this);

                window.ShowDialog();
            }
        }


        private async void EditOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not Order order)
            {
                return;
            }

            EditOrderStatusWindow window = new EditOrderStatusWindow(order);

            window.Owner = Window.GetWindow(this);

            bool? result = window.ShowDialog();

            if (result == true && DataContext is OrdersVM viewModel)
            {
                await viewModel.LoadOrdersAsync();
            }
        }


        private async void CancelOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not Order order)
            {
                return;
            }

            ArchiveOrderWindow archiveWindow = new ArchiveOrderWindow(order);

            archiveWindow.Owner = Window.GetWindow(this);

            bool? archiveResult = archiveWindow.ShowDialog();

            if (archiveResult != true)
            {
                return;
            }

            string reasonText = archiveWindow.ArchiveReason;

            if (!string.IsNullOrWhiteSpace(archiveWindow.ArchiveComment))
            {
                reasonText += ": " + archiveWindow.ArchiveComment;
            }

            MessageBoxResult confirmResult = MessageBox.Show(
                $"Перенести замовлення №{order.OrderId} в архів?\n\n" +
                $"Клієнт: {order.CustomerName}\n" +
                $"Сума: {order.TotalAmount:N2} грн\n\n" +
                $"Причина: {reasonText}",
                "Підтвердження архівації",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (confirmResult != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                OrderRepository repository = new OrderRepository();

                await repository.ArchiveOrderAsync(
                    order.OrderId,
                    archiveWindow.ArchiveReasonId,
                    archiveWindow.ArchiveReason,
                    archiveWindow.ArchiveComment
                );

                if (DataContext is OrdersVM viewModel)
                {
                    await viewModel.LoadOrdersAsync();
                }

                MessageBox.Show("Замовлення перенесено в архів.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при перенесенні замовлення в архів:\n" + ex.Message);
            }
        }


        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is OrdersVM viewModel)
            {
                viewModel.ClearFilters();
            }
        }
        private async void RestoreOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not Order order)
            {
                return;
            }

            if (order.StatusName == "Виконано")
            {
                MessageBox.Show(
                    "Це замовлення вже виконане.\n\nЙого не можна повернути з архіву напряму, тому що воно завершене.",
                    "Повернення неможливе",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                return;
            }

            if (order.StatusName == "Скасовано")
            {
                MessageBox.Show(
                    "Це замовлення скасоване.\n\nЙого не можна повернути з архіву напряму. Якщо потрібно відновити його, треба змінити статус замовлення.",
                    "Повернення неможливе",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Повернути замовлення №{order.OrderId} з архіву?\n\n" +
                $"Клієнт: {order.CustomerName}\n" +
                $"Сума: {order.TotalAmount:N2} грн",
                "Повернення з архіву",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                OrderRepository repository = new OrderRepository();

                bool restored = await repository.RestoreOrderFromArchiveAsync(order.OrderId);

                if (!restored)
                {
                    MessageBox.Show(
                        "Замовлення не вдалося повернути.\n\nМожливо, воно вже виконане або скасоване.",
                        "Повернення неможливе",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );

                    return;
                }

                if (DataContext is OrdersVM viewModel)
                {
                    await viewModel.LoadOrdersAsync();
                }

                MessageBox.Show("Замовлення повернено до активних.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при поверненні замовлення з архіву:\n" + ex.Message);
            }
        }
        private void ClearActiveFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is OrdersVM viewModel)
            {
                viewModel.ClearActiveFilters();
            }
        }

        private void ClearArchiveFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is OrdersVM viewModel)
            {
                viewModel.ClearArchiveFilters();
            }
        }
    }
}