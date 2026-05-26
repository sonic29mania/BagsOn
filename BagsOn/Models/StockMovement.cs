using System;
/// Клас StockMovement описує одну операцію руху товару на складі. Він використовується для фіксації надходження,
/// списання, резервування або інших змін кількості товару.
namespace BagsOn.Models
{
    public class StockMovement
    {
        public int MovementId { get; set; }

        public int VariantId { get; set; }

        public int? OrderId { get; set; }

        public string MovementType { get; set; } = string.Empty;

        public int QuantityChange { get; set; }

        public int QuantityBefore { get; set; }

        public int QuantityAfter { get; set; }

        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

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

        public string OrderText
        {
            get
            {
                if (OrderId == null)
                {
                    return "—";
                }

                return "№" + OrderId.Value;
            }
        }
    }
}