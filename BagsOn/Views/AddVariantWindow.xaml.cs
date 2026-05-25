using BagsOn.Repositories;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BagsOn.Views
{
    public partial class AddVariantWindow : Window
    {
        private readonly ProductRepository _productRepository;

        private readonly List<string> _selectedImagePaths;

        private int? _preselectedProductId;

        public AddVariantWindow()
        {
            InitializeComponent();

            _productRepository = new ProductRepository();
            _selectedImagePaths = new List<string>();

            Loaded += AddVariantWindow_Loaded;
        }
        public AddVariantWindow(int productId) : this()
        {
            _preselectedProductId = productId;
        }

        private async void AddVariantWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadComboBoxesAsync();
        }


        private async Task LoadComboBoxesAsync()
        {
            ProductComboBox.ItemsSource = await _productRepository.GetProductsForVariantAsync();
            ColorComboBox.ItemsSource = await _productRepository.GetColorsAsync();

            ArrivalDatePicker.SelectedDate = DateTime.Today;

            if (_preselectedProductId != null)
            {
                ProductComboBox.SelectedValue = _preselectedProductId.Value;
                ProductComboBox.IsEnabled = false;
            }
        }


        private void ChooseImagesButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = "Зображення|*.jpg;*.jpeg;*.png;*.webp";
            dialog.Multiselect = true;

            if (dialog.ShowDialog() == true)
            {
                string projectImageFolder = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Images",
                    "products"
                );

                if (!Directory.Exists(projectImageFolder))
                {
                    Directory.CreateDirectory(projectImageFolder);
                }

                foreach (string filePath in dialog.FileNames)
                {
                    string fileName = Path.GetFileName(filePath);
                    string newFilePath = Path.Combine(projectImageFolder, fileName);

                    File.Copy(filePath, newFilePath, true);

                    string relativePath = $"Images/products/{fileName}";

                    if (!_selectedImagePaths.Contains(relativePath))
                    {
                        _selectedImagePaths.Add(relativePath);
                    }
                }

                RefreshImagesList();
            }
        }
        private void RefreshImagesList()
        {
            ImagesInfoTextBox.Text = $"Обрано фото: {_selectedImagePaths.Count}";

            ImagesListBox.ItemsSource = null;
            ImagesListBox.ItemsSource = _selectedImagePaths;
        }
        private void RemoveSelectedImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (ImagesListBox.SelectedItem is not string selectedImagePath)
            {
                MessageBox.Show("Спочатку виберіть фото, яке потрібно видалити.");
                return;
            }

            _selectedImagePaths.Remove(selectedImagePath);

            RefreshImagesList();
        }
        private void PlusQuantityButton_Click(object sender, RoutedEventArgs e)
        {
            int quantity = GetCurrentQuantity();

            quantity++;

            QuantityTextBox.Text = quantity.ToString();
        }


        private void MinusQuantityButton_Click(object sender, RoutedEventArgs e)
        {
            int quantity = GetCurrentQuantity();

            if (quantity > 1)
            {
                quantity--;
            }

            QuantityTextBox.Text = quantity.ToString();
        }


        private int GetCurrentQuantity()
        {
            if (int.TryParse(QuantityTextBox.Text, out int quantity))
            {
                if (quantity < 1)
                {
                    return 1;
                }

                return quantity;
            }

            return 1;
        }


        private void IntegerTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }


        private void IntegerTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));

                if (!Regex.IsMatch(text, "^[0-9]+$"))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }


        private void PriceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            string currentText = textBox.Text;

            int selectionStart = textBox.SelectionStart;
            int selectionLength = textBox.SelectionLength;

            string newText = currentText.Remove(selectionStart, selectionLength)
                                        .Insert(selectionStart, e.Text);

            e.Handled = !IsValidPriceText(newText);
        }


        private void PriceTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));

                if (!IsValidPriceText(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }


        private bool IsValidPriceText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            return Regex.IsMatch(text, @"^\d+([,.]\d{0,2})?$");
        }


        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ProductComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Виберіть модель сумки.");
                    return;
                }

                if (ColorComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Виберіть колір варіанту.");
                    return;
                }

                if (ArrivalDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Виберіть дату надходження товару.");
                    return;
                }

                string priceText = PriceTextBox.Text.Replace(',', '.');

                if (!decimal.TryParse(
                        priceText,
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out decimal price))
                {
                    MessageBox.Show("Ціна введена неправильно.");
                    return;
                }

                if (!int.TryParse(QuantityTextBox.Text, out int quantity))
                {
                    MessageBox.Show("Кількість введена неправильно.");
                    return;
                }

                if (price < 0)
                {
                    MessageBox.Show("Ціна не може бути меншою за 0.");
                    return;
                }

                if (quantity < 1)
                {
                    MessageBox.Show("Кількість повинна бути мінімум 1.");
                    return;
                }

                await _productRepository.AddVariantAsync(
                    (int)ProductComboBox.SelectedValue,
                    (int)ColorComboBox.SelectedValue,
                    price,
                    quantity,
                    ArrivalDatePicker.SelectedDate.Value,
                    _selectedImagePaths
                );

                MessageBox.Show("Варіант товару успішно додано.");

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при додаванні варіанту:\n" + ex.Message);
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}