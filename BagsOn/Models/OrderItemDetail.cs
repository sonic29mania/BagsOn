/// Клас OrderItemDetail описує окрему позицію всередині замовлення.
namespace BagsOn.Models
{
    public class OrderItemDetail
    {
        public int OrderItemId { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public string ModelName { get; set; } = string.Empty;

        public string BrandName { get; set; } = string.Empty;

        public string ColorName { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal DiscountPercent { get; set; }

        public decimal LineTotal { get; set; }
    }
}