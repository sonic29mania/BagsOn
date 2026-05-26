/// Клас ProductImage описує зображення товару.
namespace BagsOn.Models
{
    public class ProductImage
    {
        public int ImageId { get; set; }

        public int? ProductId { get; set; }

        public int? VariantId { get; set; }

        public string ImagePath { get; set; } = string.Empty;

        public bool IsMain { get; set; }

        public int SortOrder { get; set; }

        public string AltText { get; set; } = string.Empty;
    }
}