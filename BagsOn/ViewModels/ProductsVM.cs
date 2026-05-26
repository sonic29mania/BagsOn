using BagsOn.Models;
using BagsOn.Repositories;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BagsOn.ViewModels
{// Клас ProductsVM є ViewModel для сторінки асортименту товарів.
    public class ProductsVM : BaseViewModel
    {
        private readonly ProductRepository _productRepository;

        public ObservableCollection<Product> Products { get; set; }

        public ICollectionView ProductsView { get; set; }

        public ObservableCollection<FilterOption> Brands { get; set; }
        public ObservableCollection<FilterOption> Types { get; set; }
        public ObservableCollection<FilterOption> Categories { get; set; }
        public ObservableCollection<FilterOption> Materials { get; set; }
        public ObservableCollection<FilterOption> Colors { get; set; }

        public ObservableCollection<FilterChip> ActiveFilterChips { get; set; }


        private string _searchText = string.Empty;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                RefreshFilters();
            }
        }


        private string _minPriceText = string.Empty;

        public string MinPriceText
        {
            get => _minPriceText;
            set
            {
                _minPriceText = value;
                OnPropertyChanged();
                RefreshFilters();
            }
        }


        private string _maxPriceText = string.Empty;

        public string MaxPriceText
        {
            get => _maxPriceText;
            set
            {
                _maxPriceText = value;
                OnPropertyChanged();
                RefreshFilters();
            }
        }


        private bool _onlyInStock;

        public bool OnlyInStock
        {
            get => _onlyInStock;
            set
            {
                _onlyInStock = value;
                OnPropertyChanged();
                RefreshFilters();
            }
        }


        public string BrandSummary => GetFilterSummary("Бренд", Brands);
        public string TypeSummary => GetFilterSummary("Тип", Types);
        public string CategorySummary => GetFilterSummary("Категорія", Categories);
        public string MaterialSummary => GetFilterSummary("Матеріал", Materials);
        public string ColorSummary => GetFilterSummary("Колір", Colors);

        // Конструктор ProductsVM створює репозиторій товарів, колекції для товарів і фільтрів,
        // налаштовує фільтрацію ProductsView та запускає початкове завантаження даних з бази.
        public ProductsVM()
        {
            _productRepository = new ProductRepository();

            Products = new ObservableCollection<Product>();

            Brands = new ObservableCollection<FilterOption>();
            Types = new ObservableCollection<FilterOption>();
            Categories = new ObservableCollection<FilterOption>();
            Materials = new ObservableCollection<FilterOption>();
            Colors = new ObservableCollection<FilterOption>();

            ActiveFilterChips = new ObservableCollection<FilterChip>();

            ProductsView = CollectionViewSource.GetDefaultView(Products);
            ProductsView.Filter = FilterProducts;

            _ = LoadProductsAsync();
            _ = LoadFiltersAsync();
        }

        // Метод LoadProductsAsync асинхронно завантажує список активних товарів з бази даних.
        public async Task LoadProductsAsync()
        {
            var productsFromDb = await _productRepository.GetAllProductsAsync();

            Products.Clear();

            foreach (var product in productsFromDb)
            {
                Products.Add(product);
            }

            ProductsView.Refresh();
        }

        // Метод асинхронно завантажує значення для фільтрів
        private async Task LoadFiltersAsync()
        {
            ClearFilterCollection(Brands);
            ClearFilterCollection(Types);
            ClearFilterCollection(Categories);
            ClearFilterCollection(Materials);
            ClearFilterCollection(Colors);

            AddFilterOptions(Brands, await _productRepository.GetBrandsAsync());
            AddFilterOptions(Types, await _productRepository.GetTypesAsync());
            AddFilterOptions(Categories, await _productRepository.GetCategoriesAsync());
            AddFilterOptions(Materials, await _productRepository.GetMaterialsAsync());
            AddFilterOptions(Colors, await _productRepository.GetColorsAsync());

            RefreshFilters();
        }


        private void AddFilterOptions(
            ObservableCollection<FilterOption> target,
            System.Collections.Generic.List<ReferenceItem> source)
        {
            foreach (var item in source)
            {
                FilterOption option = new FilterOption
                {
                    Id = item.Id,
                    Name = item.Name
                };

                option.PropertyChanged += FilterOption_PropertyChanged;

                target.Add(option);
            }
        }

        // Метод очищає колекцію фільтра.
        private void ClearFilterCollection(ObservableCollection<FilterOption> collection)
        {
            foreach (var item in collection)
            {
                item.PropertyChanged -= FilterOption_PropertyChanged;
            }

            collection.Clear();
        }

        // Метод реагує на зміну вибору окремого фільтра.
        private void FilterOption_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FilterOption.IsSelected))
            {
                RefreshFilters();
            }
        }

        // Метод оновлює фільтроване відображення товарів.
        private void RefreshFilters()
        {
            ProductsView.Refresh();

            RebuildActiveFilterChips();

            OnPropertyChanged(nameof(BrandSummary));
            OnPropertyChanged(nameof(TypeSummary));
            OnPropertyChanged(nameof(CategorySummary));
            OnPropertyChanged(nameof(MaterialSummary));
            OnPropertyChanged(nameof(ColorSummary));
        }

        // Метод формує короткий текст для кнопки або заголовка фільтра.
        private string GetFilterSummary(string title, ObservableCollection<FilterOption> options)
        {
            int count = options.Count(x => x.IsSelected);

            if (count == 0)
            {
                return title;
            }

            if (count == 1)
            {
                return options.First(x => x.IsSelected).Name;
            }

            return $"{title}: {count}";
        }

        // Метод перевіряє, чи повинен товар відображатися у списку.
        private bool FilterProducts(object obj)
        {
            if (obj is not Product product)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string search = SearchText.Trim().ToLower();

                bool containsSearch =
                    product.ModelName.ToLower().Contains(search) ||
                    product.BrandName.ToLower().Contains(search) ||
                    product.TypeName.ToLower().Contains(search) ||
                    product.CategoryName.ToLower().Contains(search) ||
                    product.MaterialName.ToLower().Contains(search) ||
                    product.ColorName.ToLower().Contains(search);

                if (!containsSearch)
                {
                    return false;
                }
            }

            if (!MatchesExactFilter(product.BrandName, Brands))
            {
                return false;
            }

            if (!MatchesExactFilter(product.TypeName, Types))
            {
                return false;
            }

            if (!MatchesContainsFilter(product.CategoryName, Categories))
            {
                return false;
            }

            if (!MatchesExactFilter(product.MaterialName, Materials))
            {
                return false;
            }

            if (!MatchesContainsFilter(product.ColorName, Colors))
            {
                return false;
            }

            if (TryParsePrice(MinPriceText, out decimal minPrice))
            {
                if (product.Price < minPrice)
                {
                    return false;
                }
            }

            if (TryParsePrice(MaxPriceText, out decimal maxPrice))
            {
                if (product.Price > maxPrice)
                {
                    return false;
                }
            }

            if (OnlyInStock && product.Quantity <= 0)
            {
                return false;
            }

            return true;
        }

        // Метод перевіряє точну відповідність значення вибраним елементам фільтра.
        private bool MatchesExactFilter(string value, ObservableCollection<FilterOption> options)
        {
            var selected = options.Where(x => x.IsSelected).ToList();

            if (selected.Count == 0)
            {
                return true;
            }

            return selected.Any(x => value == x.Name);
        }

        // Метод перевіряє, чи містить значення назву одного з вибраних фільтрів.
        private bool MatchesContainsFilter(string value, ObservableCollection<FilterOption> options)
        {
            var selected = options.Where(x => x.IsSelected).ToList();

            if (selected.Count == 0)
            {
                return true;
            }

            string lowerValue = value.ToLower();

            return selected.Any(x => lowerValue.Contains(x.Name.ToLower()));
        }

        // Метод TryParsePrice намагається перетворити введений текст у десяткове число.
        private bool TryParsePrice(string text, out decimal price)
        {
            price = 0;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            text = text.Replace(',', '.');

            return decimal.TryParse(
                text,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out price
            );
        }

        // Метод перебудовує список активних фільтрів, які показуються в інтерфейсі як окремі мітки.
        private void RebuildActiveFilterChips()
        {
            ActiveFilterChips.Clear();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                ActiveFilterChips.Add(new FilterChip
                {
                    Label = $"Пошук: {SearchText}",
                    Key = "search"
                });
            }

            AddOptionChips("Бренд", Brands);
            AddOptionChips("Тип", Types);
            AddOptionChips("Категорія", Categories);
            AddOptionChips("Матеріал", Materials);
            AddOptionChips("Колір", Colors);

            if (!string.IsNullOrWhiteSpace(MinPriceText))
            {
                ActiveFilterChips.Add(new FilterChip
                {
                    Label = $"Ціна від: {MinPriceText}",
                    Key = "minPrice"
                });
            }

            if (!string.IsNullOrWhiteSpace(MaxPriceText))
            {
                ActiveFilterChips.Add(new FilterChip
                {
                    Label = $"Ціна до: {MaxPriceText}",
                    Key = "maxPrice"
                });
            }

            if (OnlyInStock)
            {
                ActiveFilterChips.Add(new FilterChip
                {
                    Label = "В наявності",
                    Key = "stock"
                });
            }
        }

        // Метод AddOptionChips додає в інтерфейс мітки для вибраних елементів конкретного фільтра.
        private void AddOptionChips(string title, ObservableCollection<FilterOption> options)
        {
            foreach (var option in options.Where(x => x.IsSelected))
            {
                ActiveFilterChips.Add(new FilterChip
                {
                    Label = $"{title}: {option.Name}",
                    Key = title,
                    Option = option
                });
            }
        }

        // Метод видаляє один активний фільтр за натисканням на його мітку.
        public void RemoveFilterChip(FilterChip chip)
        {
            if (chip.Option != null)
            {
                chip.Option.IsSelected = false;
                return;
            }

            if (chip.Key == "search")
            {
                SearchText = string.Empty;
            }
            else if (chip.Key == "minPrice")
            {
                MinPriceText = string.Empty;
            }
            else if (chip.Key == "maxPrice")
            {
                MaxPriceText = string.Empty;
            }
            else if (chip.Key == "stock")
            {
                OnlyInStock = false;
            }

            RefreshFilters();
        }

        // Метод  повністю очищає всі фільтри сторінки асортименту.
        public void ClearFilters()
        {
            SearchText = string.Empty;
            MinPriceText = string.Empty;
            MaxPriceText = string.Empty;
            OnlyInStock = false;

            foreach (var item in Brands)
            {
                item.IsSelected = false;
            }

            foreach (var item in Types)
            {
                item.IsSelected = false;
            }

            foreach (var item in Categories)
            {
                item.IsSelected = false;
            }

            foreach (var item in Materials)
            {
                item.IsSelected = false;
            }

            foreach (var item in Colors)
            {
                item.IsSelected = false;
            }

            RefreshFilters();
        }
    }
}