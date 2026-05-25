using System;

namespace BagsOn.Models
{
    public class StockItem
    {
        public int StockItemId { get; set; }

        public int ProductId { get; set; }

        public int VariantId { get; set; }

        public string ModelName { get; set; } = string.Empty;

        public string BrandName { get; set; } = string.Empty;

        public string TypeName { get; set; } = string.Empty;

        public string TypeGroup { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public string MaterialName { get; set; } = string.Empty;

        public string MaterialGroup { get; set; } = string.Empty;

        public string ColorName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int TotalQuantity { get; set; }

        public int ReservedQuantity { get; set; }

        public int AvailableQuantity { get; set; }

        public int MinQuantity { get; set; }

        public string Location { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; }

        public string ImagePath { get; set; } = string.Empty;

        public string StockStatus { get; set; } = string.Empty;

        public string StockWarning { get; set; } = string.Empty;

        public decimal TotalValue { get; set; }

        public decimal AvailableValue { get; set; }

        public bool HasWarning
        {
            get
            {
                return !string.IsNullOrWhiteSpace(StockWarning);
            }
        }
    }
}