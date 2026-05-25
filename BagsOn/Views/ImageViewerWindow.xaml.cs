using BagsOn.Models;
using BagsOn.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BagsOn.Views
{
    public partial class ImageViewerWindow : Window
    {
        private readonly ProductRepository _productRepository;

        private readonly int _variantId;
        private readonly string _productName;

        private List<ProductImage> _images;

        private int _currentIndex;

        public ImageViewerWindow(int variantId, string productName)
        {
            InitializeComponent();

            _productRepository = new ProductRepository();

            _variantId = variantId;
            _productName = productName;

            _images = new List<ProductImage>();
            _currentIndex = 0;

            Loaded += ImageViewerWindow_Loaded;
        }

        private async void ImageViewerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            TitleTextBlock.Text = _productName;

            _images = await _productRepository.GetImagesByVariantIdAsync(_variantId);

            if (_images.Count == 0)
            {
                CounterTextBlock.Text = "Фото відсутні";
                MessageBox.Show("Для цього товару ще немає фото.");
                return;
            }

            ShowCurrentImage();
        }

        private void ShowCurrentImage()
        {
            if (_images.Count == 0)
            {
                return;
            }

            ProductImage image = _images[_currentIndex];

            string fullPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                image.ImagePath.Replace("/", "\\")
            );

            if (!File.Exists(fullPath))
            {
                ProductImage.Source = null;
                CounterTextBlock.Text = "Файл не знайдено";
                return;
            }

            BitmapImage bitmap = new BitmapImage();

            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
            bitmap.EndInit();

            ProductImage.Source = bitmap;

            CounterTextBlock.Text = $"{_currentIndex + 1} / {_images.Count}";
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (_images.Count == 0)
            {
                return;
            }

            _currentIndex--;

            if (_currentIndex < 0)
            {
                _currentIndex = _images.Count - 1;
            }

            ShowCurrentImage();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_images.Count == 0)
            {
                return;
            }

            _currentIndex++;

            if (_currentIndex >= _images.Count)
            {
                _currentIndex = 0;
            }

            ShowCurrentImage();
        }
    }
}