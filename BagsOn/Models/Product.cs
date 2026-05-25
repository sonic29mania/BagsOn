using System;

namespace BagsOn.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        public int VariantId { get; set; }

        public string ModelName { get; set; } = string.Empty;

        public string BrandName { get; set; } = string.Empty;

        public string BrandSegment { get; set; } = string.Empty;

        public string TypeName { get; set; } = string.Empty;

        public string TypeGroup { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public string MaterialName { get; set; } = string.Empty;

        public string MaterialGroup { get; set; } = string.Empty;

        public string ColorName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public DateTime ArrivalDate { get; set; }

        public string ImagePath { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public decimal TotalValue { get; set; }
    }
}