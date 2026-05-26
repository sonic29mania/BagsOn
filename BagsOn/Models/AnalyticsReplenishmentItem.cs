namespace BagsOn.Models
{
    /// Клас AnalyticsReplenishmentItem зберігає дані для аналітики поповнення складу. 
    public class AnalyticsReplenishmentItem
    {
        public string ModelName { get; set; } = string.Empty;

        public string BrandName { get; set; } = string.Empty;

        public string ColorName { get; set; } = string.Empty;

        public int AvailableQuantity { get; set; }

        public int MinQuantity { get; set; }

        public int QuantityToBuy { get; set; }

        public string StockStatus { get; set; } = string.Empty;
    }
}