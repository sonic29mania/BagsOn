using BagsOn.Models;
using BagsOn.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BagsOn.Views
{
    public partial class ArchiveOrderWindow : Window
    {
        private readonly Order _order;
        private readonly OrderRepository _orderRepository;

        private List<ReferenceItem> _archiveReasons = new List<ReferenceItem>();

        public int ArchiveReasonId { get; private set; }

        public string ArchiveReason { get; private set; } = string.Empty;

        public string ArchiveComment { get; private set; } = string.Empty;

        public ArchiveOrderWindow(Order order)
        {
            InitializeComponent();

            _order = order;
            _orderRepository = new OrderRepository();

            Loaded += ArchiveOrderWindow_Loaded;
        }

        private async void ArchiveOrderWindow_Loaded(object sender, RoutedEventArgs e)
        {
            OrderInfoTextBlock.Text =
                $"№{_order.OrderId} від {_order.OrderDate:dd.MM.yyyy}";

            CustomerInfoTextBlock.Text =
                $"{_order.CustomerName} | {_order.Phone}";

            _archiveReasons = await _orderRepository.GetArchiveReasonsAsync();

            ReasonComboBox.ItemsSource = _archiveReasons;
        }

        private void ReasonComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReasonComboBox.SelectedItem is not ReferenceItem selectedReason)
            {
                return;
            }

            bool needComment =
                selectedReason.Name == "Інше" ||
                selectedReason.Name == "Помилка менеджера" ||
                selectedReason.Name == "Некоректні дані клієнта";

            CommentLabelTextBlock.Visibility = needComment
                ? Visibility.Visible
                : Visibility.Collapsed;

            CommentTextBox.Visibility = needComment
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (!needComment)
            {
                CommentTextBox.Text = string.Empty;
            }
        }

        private void ArchiveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ReasonComboBox.SelectedItem is not ReferenceItem selectedReason)
            {
                MessageBox.Show("Оберіть причину перенесення замовлення в архів.");
                return;
            }

            bool needComment =
                selectedReason.Name == "Інше" ||
                selectedReason.Name == "Помилка менеджера" ||
                selectedReason.Name == "Некоректні дані клієнта";

            if (needComment && string.IsNullOrWhiteSpace(CommentTextBox.Text))
            {
                MessageBox.Show("Для цієї причини потрібно написати коментар.");
                return;
            }

            ArchiveReasonId = selectedReason.Id;
            ArchiveReason = selectedReason.Name;
            ArchiveComment = CommentTextBox.Text.Trim();

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}