using System;
using System.Collections.Generic;
/// Клас ProductEditData використовується для додавання або редагування товару. Він зберігає дані, які користувач
/// вводить у формі редагування
namespace BagsOn.Models
{
    public class ProductEditData
    {
        public int ProductId { get; set; }

        public int VariantId { get; set; }

        public string ModelName { get; set; } = string.Empty;

        public int BrandId { get; set; }

        public int TypeId { get; set; }

        public int MaterialId { get; set; }

        public int ColorId { get; set; }

        public List<int> CategoryIds { get; set; } = new List<int>();

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public DateTime ArrivalDate { get; set; }

        public List<string> ImagePaths { get; set; } = new List<string>();
    }
}