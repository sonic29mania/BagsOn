using System;

namespace BagsOn.Models
{
    public class AnalyticsStockMovement
    {
        public int MovementId { get; set; }

        public int VariantId { get; set; }

        public DateTime CreatedAt { get; set; }

        public string MovementType { get; set; } = string.Empty;

        public int QuantityChange { get; set; }

        public int QuantityBefore { get; set; }

        public int QuantityAfter { get; set; }

        public string Comment { get; set; } = string.Empty;

        public string ModelName { get; set; } = string.Empty;

        public string BrandName { get; set; } = string.Empty;

        public string ColorName { get; set; } = string.Empty;

        public bool IsLastMovement { get; set; }

        public string QuantityChangeText
        {
            get
            {
                if (QuantityChange > 0)
                {
                    return "+" + QuantityChange;
                }

                return QuantityChange.ToString();
            }
        }

        public string LastMovementText
        {
            get
            {
                return IsLastMovement ? "Останній рух" : "";
            }
        }
    }
}