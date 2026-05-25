using BagsOn.Data;
using Npgsql;
using System;
using System.Data;
using System.Threading.Tasks;

namespace BagsOn.Repositories
{
    public class ReportsRepository
    {
        public async Task<DataTable> GetSalesReportAsync(DateTime dateFrom, DateTime dateTo)
        {
            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
                SELECT
                    order_date AS ""Дата"",
                    order_id AS ""№ замовлення"",
                    customer_name AS ""Клієнт"",
                    phone AS ""Телефон"",
                    status_name AS ""Статус"",
                    delivery_type_name AS ""Доставка"",
                    items_count AS ""Товарів"",
                    total_amount AS ""Сума""
                FROM v_orders_full
                WHERE order_date >= @date_from
                  AND order_date < @date_to
                  AND status_name = 'Виконано'
                ORDER BY order_date DESC, order_time DESC;
            ";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@date_from", dateFrom.Date);
            command.Parameters.AddWithValue("@date_to", dateTo.Date.AddDays(1));

            await using var reader = await command.ExecuteReaderAsync();

            DataTable table = new DataTable();
            table.Load(reader);

            return table;
        }

        public async Task<DataTable> GetStockReportAsync()
        {
            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
                SELECT
                    model_name AS ""Модель"",
                    brand_name AS ""Бренд"",
                    color_name AS ""Колір"",
                    material_name AS ""Матеріал"",
                    price AS ""Ціна"",
                    total_quantity AS ""Всього"",
                    reserved_quantity AS ""Резерв"",
                    available_quantity AS ""Доступно"",
                    min_quantity AS ""Мін. залишок"",
                    stock_status AS ""Статус"",
                    stock_warning AS ""Попередження"",
                    location AS ""Місце""
                FROM v_stock_full
                ORDER BY brand_name, model_name, color_name;
            ";

            await using var command = new NpgsqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            DataTable table = new DataTable();
            table.Load(reader);

            return table;
        }

        public async Task<DataTable> GetStockMovementsReportAsync(DateTime dateFrom, DateTime dateTo)
        {
            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
                SELECT
                    sm.created_at AS ""Дата руху"",
                    sm.movement_type AS ""Тип руху"",
                    p.model_name AS ""Модель"",
                    b.brand_name AS ""Бренд"",
                    COALESCE(c.color_name, '') AS ""Колір"",
                    sm.quantity_change AS ""Кількість"",
                    sm.quantity_before AS ""Було"",
                    sm.quantity_after AS ""Стало"",
                    COALESCE(sm.comment, '') AS ""Коментар""
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

            DataTable table = new DataTable();
            table.Load(reader);

            return table;
        }

        public async Task<DataTable> GetLowStockReportAsync()
        {
            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
                SELECT
                    model_name AS ""Модель"",
                    brand_name AS ""Бренд"",
                    color_name AS ""Колір"",
                    available_quantity AS ""Доступно"",
                    min_quantity AS ""Мін. залишок"",
                    GREATEST(min_quantity - available_quantity, 0) AS ""Докупити"",
                    stock_status AS ""Статус"",
                    stock_warning AS ""Попередження""
                FROM v_stock_full
                WHERE available_quantity <= min_quantity
                   OR stock_status IN ('Мало', 'Немає')
                ORDER BY available_quantity ASC, model_name ASC;
            ";

            await using var command = new NpgsqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            DataTable table = new DataTable();
            table.Load(reader);

            return table;
        }
    }
}