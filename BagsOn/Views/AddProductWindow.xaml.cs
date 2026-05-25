using BagsOn.Models;
using BagsOn.Repositories;
using Microsoft.VisualBasic;
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
    public partial class AddProductWindow : Window
    {
        private readonly ProductRepository _productRepository;

        private readonly List<string> _selectedImagePaths;

        public AddProductWindow()
        {
            InitializeComponent();

            _productRepository = new ProductRepository();
            _selectedImagePaths = new List<string>();

            Loaded += AddProductWindow_Loaded;
        }


        private async void AddProductWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadComboBoxesAsync();
        }


        private async Task LoadComboBoxesAsync()
        {
            await _productRepository.EnsureNoBrandAsync();

            SegmentComboBox.ItemsSource = await _productRepository.GetBrandSegmentsAsync();

            BrandComboBox.ItemsSource = await _productRepository.GetBrandsAsync();

            TypeComboBox.ItemsSource = await _productRepository.GetTypesAsync();
            CategoryListBox.ItemsSource = await _productRepository.GetCategoriesAsync();
            MaterialComboBox.ItemsSource = await _productRepository.GetMaterialsAsync();
            ColorComboBox.ItemsSource = await _productRepository.GetColorsAsync();

            ArrivalDatePicker.SelectedDate = DateTime.Today;
        }


        private async void SegmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BrandComboBox == null)
            {
                return;
            }

            if (SegmentComboBox.SelectedValue == null)
            {
                BrandComboBox.ItemsSource = await _productRepository.GetBrandsAsync();
                BrandComboBox.SelectedIndex = -1;
                return;
            }

            int segmentId = (int)SegmentComboBox.SelectedValue;

            BrandComboBox.ItemsSource = await _productRepository.GetBrandsAsync(segmentId);
            BrandComboBox.SelectedIndex = -1;
        }


        private async void AddBrandButton_Click(object sender, RoutedEventArgs e)
        {
            if (SegmentComboBox.SelectedValue == null)
            {
                MessageBox.Show("Спочатку виберіть сегмент бренду.");
                return;
            }

            string brandName = Interaction.InputBox(
                "Введіть назву нового бренду:",
                "Додавання бренду",
                ""
            );

            if (string.IsNullOrWhiteSpace(brandName))
            {
                return;
            }

            brandName = brandName.Trim();

            try
            {
                int segmentId = (int)SegmentComboBox.SelectedValue;

                int newBrandId = await _productRepository.AddBrandAsync(brandName, segmentId);

                BrandComboBox.ItemsSource = await _productRepository.GetBrandsAsync(segmentId);
                BrandComboBox.SelectedValue = newBrandId;

                MessageBox.Show("Бренд успішно додано.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при додаванні бренду:\n" + ex.Message);
            }
        }


        private async void NoBrandButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int noBrandId = await _productRepository.EnsureNoBrandAsync();

                SegmentComboBox.SelectedIndex = -1;

                BrandComboBox.ItemsSource = await _productRepository.GetBrandsAsync();
                BrandComboBox.SelectedValue = noBrandId;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при виборі бренду:\n" + ex.Message);
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
                return quantity < 1 ? 1 : quantity;
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


        private List<int> GetSelectedCategoryIds()
        {
            List<int> ids = new List<int>();

            foreach (object item in CategoryListBox.SelectedItems)
            {
                if (item is ReferenceItem referenceItem)
                {
                    ids.Add(referenceItem.Id);
                }
            }

            return ids;
        }


        private int GetSelectedColorId()
        {
            return (int)ColorComboBox.SelectedValue;
        }
        private void CategoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int count = CategoryListBox.SelectedItems.Count;

            if (count == 0)
            {
                CategoriesSummaryTextBlock.Text = "Оберіть одну або декілька категорій";
            }
            else
            {
                CategoriesSummaryTextBlock.Text = $"Обрано категорій: {count}";
            }
        }
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ModelNameTextBox.Text))
                {
                    MessageBox.Show("Введіть назву моделі.");
                    return;
                }

                if (BrandComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Виберіть бренд або натисніть 'Без бренду'.");
                    return;
                }

                if (TypeComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Виберіть тип товару.");
                    return;
                }

                List<int> categoryIds = GetSelectedCategoryIds();

                if (categoryIds.Count == 0)
                {
                    MessageBox.Show("Виберіть хоча б одну категорію.");
                    return;
                }

                if (MaterialComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Виберіть матеріал.");
                    return;
                }

                if (ColorComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Виберіть колір варіанту.");
                    return;
                }

                int colorId = GetSelectedColorId();

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

                await _productRepository.AddProductWithVariantAsync(
    ModelNameTextBox.Text.Trim(),
    (int)BrandComboBox.SelectedValue,
    (int)TypeComboBox.SelectedValue,
    (int)MaterialComboBox.SelectedValue,
    categoryIds,
    colorId,
    price,
    quantity,
    ArrivalDatePicker.SelectedDate.Value,
    _selectedImagePaths
);

                MessageBox.Show("Товар успішно додано.");

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при додаванні товару:\n" + ex.Message);
            }
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
        private void RefreshImagesList()
        {
            ImagesInfoTextBox.Text = $"Обрано фото: {_selectedImagePaths.Count}";

            ImagesListBox.ItemsSource = null;
            ImagesListBox.ItemsSource = _selectedImagePaths;
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}