using BagsOn.Models;
using BagsOn.Repositories;
using BagsOn.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BagsOn.Views
{
    public partial class ProductsView : UserControl
    {
        public ProductsView()
        {
            InitializeComponent();
        }

        private async void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            AddProductWindow window = new AddProductWindow();

            window.Owner = Window.GetWindow(this);

            bool? result = window.ShowDialog();

            if (result == true && DataContext is ProductsVM viewModel)
            {
                await viewModel.LoadProductsAsync();
            }
        }

        private async void RowAddVariantButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not Product product)
            {
                return;
            }

            AddVariantWindow window = new AddVariantWindow(product.ProductId);

            window.Owner = Window.GetWindow(this);

            bool? result = window.ShowDialog();

            if (result == true && DataContext is ProductsVM viewModel)
            {
                await viewModel.LoadProductsAsync();
            }
        }

        private async void RowEditProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not Product product)
            {
                return;
            }

            EditProductWindow window = new EditProductWindow(
                product.ProductId,
                product.VariantId
            );

            window.Owner = Window.GetWindow(this);

            bool? result = window.ShowDialog();

            if (result == true && DataContext is ProductsVM viewModel)
            {
                await viewModel.LoadProductsAsync();
            }
        }

        private async void RowDeleteProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not Product product)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Ви дійсно хочете видалити цей варіант товару?\n\n" +
                $"Модель: {product.ModelName}\n" +
                $"Колір: {product.ColorName}\n" +
                $"Кількість: {product.Quantity} шт.",
                "Підтвердження видалення",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                ProductRepository productRepository = new ProductRepository();

                await productRepository.DeleteVariantAsync(
                    product.ProductId,
                    product.VariantId
                );

                if (DataContext is ProductsVM viewModel)
                {
                    await viewModel.LoadProductsAsync();
                }

                MessageBox.Show(
                    "Товар успішно видалено.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Помилка при видаленні товару:\n" + ex.Message,
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private async void AddVariantButton_Click(object sender, RoutedEventArgs e)
        {
            AddVariantWindow window = new AddVariantWindow();

            window.Owner = Window.GetWindow(this);

            bool? result = window.ShowDialog();

            if (result == true && DataContext is ProductsVM viewModel)
            {
                await viewModel.LoadProductsAsync();
            }
        }

        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProductsVM viewModel)
            {
                viewModel.ClearFilters();
            }
        }

        private void RemoveFilterChipButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.Tag is FilterChip chip &&
                DataContext is ProductsVM viewModel)
            {
                viewModel.RemoveFilterChip(chip);
            }
        }

        private void ProductImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Product product)
            {
                ImageViewerWindow window = new ImageViewerWindow(
                    product.VariantId,
                    product.ModelName
                );

                window.Owner = Window.GetWindow(this);
                window.ShowDialog();
            }
        }
    }
}