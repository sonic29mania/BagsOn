namespace BagsOn.Models
{
    public class AnalyticsTopProduct
    {
        public string ModelName { get; set; } = string.Empty;

        public string BrandName { get; set; } = string.Empty;

        public int SoldQuantity { get; set; }

        public decimal Revenue { get; set; }
    }
}