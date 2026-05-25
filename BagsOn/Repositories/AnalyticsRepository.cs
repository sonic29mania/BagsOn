using BagsOn.Data;
using BagsOn.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BagsOn.Repositories
{
    public class AnalyticsRepository
    {
        public async Task<AnalyticsSummary> GetSummaryAsync(DateTime dateFrom, DateTime dateTo)
        {
            AnalyticsSummary summary = new AnalyticsSummary();

            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
                SELECT
                    COUNT(*) AS total_orders,

                    COALESCE(SUM(
                        CASE
                            WHEN status_name = 'Виконано' THEN total_amount
                            ELSE 0
                        END
                    ), 0) AS revenue,

                    COALESCE(AVG(
                        CASE
                            WHEN status_name = 'Виконано' THEN total_amount
                            ELSE NULL
                        END
                    ), 0) AS average_check,

                    COUNT(*) FILTER (WHERE status_name = 'Виконано') AS completed_orders,

                    COUNT(*) FILTER (WHERE status_name = 'Скасовано') AS cancelled_orders
                FROM v_orders_full
                WHERE order_date >= @date_from
                  AND order_date < @date_to;
            ";

            await using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@date_from", dateFrom.Date);
                command.Parameters.AddWithValue("@date_to", dateTo.Date.AddDays(1));

                await using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    summary.TotalOrders = Convert.ToInt32(reader["total_orders"]);
                    summary.Revenue = Convert.ToDecimal(reader["revenue"]);
                    summary.AverageCheck = Convert.ToDecimal(reader["average_check"]);
                    summary.CompletedOrders = Convert.ToInt32(reader["completed_orders"]);
                    summary.CancelledOrders = Convert.ToInt32(reader["cancelled_orders"]);
                }
            }

            string stockQuery = @"
                SELECT
                    COUNT(*) FILTER (WHERE stock_status = 'Мало') AS low_stock_count,
                    COUNT(*) FILTER (WHERE stock_status = 'Немає') AS out_of_stock_count,
                    COUNT(*) FILTER (WHERE reserved_quantity > 0) AS reserved_positions_count
                FROM v_stock_full;
            ";

            await using (var stockCommand = new NpgsqlCommand(stockQuery, connection))
            {
                await using var reader = await stockCommand.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    summary.LowStockCount = Convert.ToInt32(reader["low_stock_count"]);
                    summary.OutOfStockCount = Convert.ToInt32(reader["out_of_stock_count"]);
                    summary.ReservedPositionsCount = Convert.ToInt32(reader["reserved_positions_count"]);
                }
            }

            return summary;
        }

        public async Task<List<AnalyticsTopProduct>> GetTopProductsAsync(DateTime dateFrom, DateTime dateTo)
        {
            List<AnalyticsTopProduct> products = new List<AnalyticsTopProduct>();

            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
                SELECT
                    p.model_name,
                    b.brand_name,
                    COALESCE(SUM(oi.quantity), 0) AS sold_quantity,
                    COALESCE(SUM(oi.line_total), 0) AS revenue
                FROM order_items oi
                JOIN v_orders_full o ON oi.order_id = o.order_id
                JOIN products p ON oi.product_id = p.product_id
                LEFT JOIN brands b ON p.brand_id = b.brand_id
                WHERE o.order_date >= @date_from
                  AND o.order_date < @date_to
                  AND o.status_name = 'Виконано'
                GROUP BY
                    p.product_id,
                    p.model_name,
                    b.brand_name
                ORDER BY revenue DESC, sold_quantity DESC
                LIMIT 10;
            ";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@date_from", dateFrom.Date);
            command.Parameters.AddWithValue("@date_to", dateTo.Date.AddDays(1));

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                products.Add(new AnalyticsTopProduct
                {
                    ModelName = reader["model_name"].ToString() ?? string.Empty,
                    BrandName = reader["brand_name"].ToString() ?? string.Empty,
                    SoldQuantity = Convert.ToInt32(reader["sold_quantity"]),
                    Revenue = Convert.ToDecimal(reader["revenue"])
                });
            }

            return products;
        }

        public async Task<List<AnalyticsReplenishmentItem>> GetReplenishmentItemsAsync()
        {
            List<AnalyticsReplenishmentItem> items = new List<AnalyticsReplenishmentItem>();

            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
                SELECT
                    model_name,
                    brand_name,
                    color_name,
                    available_quantity,
                    min_quantity,
                    GREATEST(min_quantity - available_quantity, 0) AS quantity_to_buy,
                    stock_status
                FROM v_stock_full
                WHERE available_quantity <= min_quantity
                   OR stock_status = 'Немає'
                ORDER BY
                    available_quantity ASC,
                    model_name ASC;
            ";

            await using var command = new NpgsqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(new AnalyticsReplenishmentItem
                {
                    ModelName = reader["model_name"].ToString() ?? string.Empty,
                    BrandName = reader["brand_name"].ToString() ?? string.Empty,
                    ColorName = reader["color_name"].ToString() ?? string.Empty,
                    AvailableQuantity = Convert.ToInt32(reader["available_quantity"]),
                    MinQuantity = Convert.ToInt32(reader["min_quantity"]),
                    QuantityToBuy = Convert.ToInt32(reader["quantity_to_buy"]),
                    StockStatus = reader["stock_status"].ToString() ?? string.Empty
                });
            }

            return items;
        }
        public async Task<List<AnalyticsStockMovement>> GetStockMovementsForAnalyticsAsync(DateTime dateFrom, DateTime dateTo)
        {
            List<AnalyticsStockMovement> movements = new List<AnalyticsStockMovement>();

            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
        SELECT
            sm.movement_id,
            sm.variant_id,
            sm.created_at,
            sm.movement_type,
            sm.quantity_change,
            sm.quantity_before,
            sm.quantity_after,
            COALESCE(sm.comment, '') AS comment,

            p.model_name,
            b.brand_name,
            COALESCE(c.color_name, '') AS color_name,

            CASE
                WHEN sm.movement_id = (
                    SELECT sm2.movement_id
                    FROM stock_movements sm2
                    WHERE sm2.variant_id = sm.variant_id
                    ORDER BY sm2.created_at DESC, sm2.movement_id DESC
                    LIMIT 1
                )
                THEN TRUE
                ELSE FALSE
            END AS is_last_movement

        FROM stock_movements sm
        JOIN product_variants pv ON sm.variant_id = pv.variant_id
        JOIN products p ON pv.product_id = p.product_id
        LEFT JOIN brands b ON p.brand_id = b.brand_id
        LEFT JOIN colors c ON pv.color_id = c.color_id

        WHERE sm.created_at >= @date_from
          AND sm.created_at < @date_to

        ORDER BY sm.created_at DESC, sm.movement_id DESC;
    ";

            await using var command = new NpgsqlCommand(query, connection);

            command.Parameters.AddWithValue("@date_from", dateFrom.Date);
            command.Parameters.AddWithValue("@date_to", dateTo.Date.AddDays(1));

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                movements.Add(new AnalyticsStockMovement
                {
                    MovementId = Convert.ToInt32(reader["movement_id"]),
                    VariantId = Convert.ToInt32(reader["variant_id"]),
                    CreatedAt = Convert.ToDateTime(reader["created_at"]),
                    MovementType = reader["movement_type"].ToString() ?? string.Empty,
                    QuantityChange = Convert.ToInt32(reader["quantity_change"]),
                    QuantityBefore = Convert.ToInt32(reader["quantity_before"]),
                    QuantityAfter = Convert.ToInt32(reader["quantity_after"]),
                    Comment = reader["comment"].ToString() ?? string.Empty,
                    ModelName = reader["model_name"].ToString() ?? string.Empty,
                    BrandName = reader["brand_name"].ToString() ?? string.Empty,
                    ColorName = reader["color_name"].ToString() ?? string.Empty,
                    IsLastMovement = Convert.ToBoolean(reader["is_last_movement"])
                });
            }

            return movements;
        }
    }
}