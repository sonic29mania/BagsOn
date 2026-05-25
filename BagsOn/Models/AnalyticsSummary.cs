namespace BagsOn.Models
{
    public class AnalyticsSummary
    {
        public int TotalOrders { get; set; }

        public decimal Revenue { get; set; }

        public decimal AverageCheck { get; set; }

        public int CompletedOrders { get; set; }

        public int CancelledOrders { get; set; }

        public int LowStockCount { get; set; }

        public int OutOfStockCount { get; set; }

        public int ReservedPositionsCount { get; set; }
    }
}