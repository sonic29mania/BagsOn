using BagsOn.Data;
using BagsOn.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BagsOn.Repositories
{
    //Клас OrderRepository відповідає за роботу із замовленнями в базі даних.
    public class OrderRepository
    {
        /// Метод GetAllOrdersAsync отримує список активних замовлень.
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            List<Order> orders = new List<Order>();

            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
    SELECT
        order_id,
        order_date,
        order_time,
        delivery_date,
        delivery_time,
        delivery_address,
        comment,
        customer_comment,
        manager_comment,
        total_amount,
        is_active,
        is_archived,
        archived_at,
        archive_reason,
        customer_id,
        customer_name,
        phone,
        email,
        city,
        customer_address,
        status_id,
        status_name,
        delivery_type_id,
        delivery_type_name,
        items_count
    FROM v_orders_full
   WHERE COALESCE(is_archived, FALSE) = FALSE
  AND status_name NOT IN ('Виконано', 'Скасовано')
    ORDER BY order_date DESC, order_time DESC;
";
            await using var command = new NpgsqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                orders.Add(ReadOrder(reader));
            }

            return orders;
        }

        /// Метод GetArchivedOrdersAsync отримує список архівних замовлень.
        public async Task<List<Order>> GetArchivedOrdersAsync()
        {
            List<Order> orders = new List<Order>();

            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
        SELECT
            order_id,
            order_date,
            order_time,
            delivery_date,
            delivery_time,
            delivery_address,
            comment,
            customer_comment,
            manager_comment,
            total_amount,
            is_active,
            is_archived,

            COALESCE(archived_at, order_date::timestamp) AS archived_at,

            COALESCE(
                NULLIF(archive_reason, ''),
                CASE
                    WHEN status_name = 'Виконано' THEN 'Замовлення виконано'
                    WHEN status_name = 'Скасовано' THEN 'Замовлення скасовано'
                    ELSE 'Архівне замовлення'
                END
            ) AS archive_reason,

            customer_id,
            customer_name,
            phone,
            email,
            city,
            customer_address,
            status_id,
            status_name,
            delivery_type_id,
            delivery_type_name,
            items_count
        FROM v_orders_full
        WHERE COALESCE(is_archived, FALSE) = TRUE
           OR status_name IN ('Виконано', 'Скасовано')
        ORDER BY archived_at DESC, order_date DESC;
    ";

            await using var command = new NpgsqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                orders.Add(ReadOrder(reader));
            }

            return orders;
        }
        /// Метод ReadOrder створює об'єкт Order на основі даних,
        /// отриманих з результату SQL-запиту.
        /// Він зчитує значення з NpgsqlDataReader і заповнює властивості замовлення.
        private Order ReadOrder(NpgsqlDataReader reader)
        {
            Order order = new Order
            {
                OrderId = GetInt(reader, "order_id"),
                OrderDate = GetDateTime(reader, "order_date", DateTime.Today),
                OrderTime = GetTimeSpan(reader, "order_time", TimeSpan.Zero),

                DeliveryDate = GetNullableDateTime(reader, "delivery_date"),
                DeliveryTime = GetNullableTimeSpan(reader, "delivery_time"),

                DeliveryAddress = GetString(reader, "delivery_address"),
                Comment = GetString(reader, "comment"),
                CustomerComment = GetString(reader, "customer_comment"),
                ManagerComment = GetString(reader, "manager_comment"),

                TotalAmount = GetDecimal(reader, "total_amount"),

                IsActive = GetBool(reader, "is_active", true),
                IsArchived = GetBool(reader, "is_archived", false),

                ArchivedAt = GetNullableDateTime(reader, "archived_at"),
                ArchiveReason = GetString(reader, "archive_reason"),

                CustomerId = GetNullableInt(reader, "customer_id"),
                CustomerName = GetString(reader, "customer_name"),
                Phone = GetString(reader, "phone"),
                Email = GetString(reader, "email"),
                City = GetString(reader, "city"),
                CustomerAddress = GetString(reader, "customer_address"),

                StatusId = GetInt(reader, "status_id"),
                StatusName = GetString(reader, "status_name"),

                DeliveryTypeId = GetInt(reader, "delivery_type_id"),
                DeliveryTypeName = GetString(reader, "delivery_type_name"),

                ItemsCount = GetInt(reader, "items_count")
            };

            return order;
        }

        /// Метод GetOrderStatusesAsync отримує список статусів замовлень.
        public async Task<List<ReferenceItem>> GetOrderStatusesAsync()
        {
            List<ReferenceItem> items = new List<ReferenceItem>();

            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
                SELECT status_id, status_name
                FROM order_statuses
                ORDER BY status_id;
            ";

            await using var command = new NpgsqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(new ReferenceItem
                {
                    Id = reader["status_id"] == DBNull.Value ? 0 : reader.GetInt32(0),
                    Name = reader["status_name"] == DBNull.Value ? "" : reader.GetString(1)
                });
            }

            return items;
        }

        /// Метод GetDeliveryTypesAsync отримує список типів доставки.
        public async Task<List<ReferenceItem>> GetDeliveryTypesAsync()
        {
            List<ReferenceItem> items = new List<ReferenceItem>();

            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
                SELECT delivery_type_id, delivery_type_name
                FROM delivery_types
                ORDER BY delivery_type_id;
            ";

            await using var command = new NpgsqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(new ReferenceItem
                {
                    Id = reader["delivery_type_id"] == DBNull.Value ? 0 : reader.GetInt32(0),
                    Name = reader["delivery_type_name"] == DBNull.Value ? "" : reader.GetString(1)
                });
            }

            return items;
        }

        /// Метод GetArchiveReasonsAsync отримує список причин архівації замовлення.
        public async Task<List<ReferenceItem>> GetArchiveReasonsAsync()
        {
            List<ReferenceItem> reasons = new List<ReferenceItem>();

            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
                SELECT reason_id, reason_name
                FROM order_archive_reasons
                ORDER BY reason_id;
            ";

            await using var command = new NpgsqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                reasons.Add(new ReferenceItem
                {
                    Id = reader["reason_id"] == DBNull.Value ? 0 : reader.GetInt32(0),
                    Name = reader["reason_name"] == DBNull.Value ? "" : reader.GetString(1)
                });
            }

            return reasons;
        }

        /// Метод UpdateOrderAsync оновлює інформацію про замовлення та пов'язаного клієнта.
        public async Task UpdateOrderAsync(
            int orderId,
            int? customerId,
            string customerName,
            string phone,
            string email,
            string city,
            string customerAddress,
            int statusId,
            string customerComment,
            string managerComment)
        {
            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                if (customerId != null)
                {
                    string updateCustomerQuery = @"
                        UPDATE customers
                        SET
                            customer_name = @customer_name,
                            full_name = @customer_name,
                            phone = @phone,
                            email = @email,
                            city = @city,
                            address = @address
                        WHERE customer_id = @customer_id;
                    ";

                    await using var customerCommand =
                        new NpgsqlCommand(updateCustomerQuery, connection, transaction);

                    customerCommand.Parameters.AddWithValue("@customer_id", customerId.Value);
                    customerCommand.Parameters.AddWithValue("@customer_name", customerName);
                    customerCommand.Parameters.AddWithValue("@phone", phone);

                    customerCommand.Parameters.AddWithValue(
                        "@email",
                        string.IsNullOrWhiteSpace(email) ? DBNull.Value : email
                    );

                    customerCommand.Parameters.AddWithValue(
                        "@city",
                        string.IsNullOrWhiteSpace(city) ? DBNull.Value : city
                    );

                    customerCommand.Parameters.AddWithValue(
                        "@address",
                        string.IsNullOrWhiteSpace(customerAddress) ? DBNull.Value : customerAddress
                    );

                    await customerCommand.ExecuteNonQueryAsync();
                }

                string updateOrderQuery = @"
    UPDATE orders
    SET
        status_id = @status_id,

        status = CASE
            WHEN (
                SELECT status_name
                FROM order_statuses
                WHERE status_id = @status_id
            ) IN ('Нове', 'В обробці', 'Очікує оплати') THEN 'Новый'

            WHEN (
                SELECT status_name
                FROM order_statuses
                WHERE status_id = @status_id
            ) IN ('Оплачено', 'Відправлено') THEN 'Оплачен'

            WHEN (
                SELECT status_name
                FROM order_statuses
                WHERE status_id = @status_id
            ) = 'Виконано' THEN 'Завершен'

            WHEN (
                SELECT status_name
                FROM order_statuses
                WHERE status_id = @status_id
            ) = 'Скасовано' THEN 'Отменен'

            ELSE status
        END,

        is_active = CASE
            WHEN (
                SELECT status_name
                FROM order_statuses
                WHERE status_id = @status_id
            ) IN ('Виконано', 'Скасовано') THEN FALSE
            ELSE TRUE
        END,

        is_archived = CASE
            WHEN (
                SELECT status_name
                FROM order_statuses
                WHERE status_id = @status_id
            ) IN ('Виконано', 'Скасовано') THEN TRUE
            ELSE FALSE
        END,

        archived_at = CASE
            WHEN (
                SELECT status_name
                FROM order_statuses
                WHERE status_id = @status_id
            ) IN ('Виконано', 'Скасовано') THEN CURRENT_TIMESTAMP
            ELSE NULL
        END,

        archive_reason = CASE
            WHEN (
                SELECT status_name
                FROM order_statuses
                WHERE status_id = @status_id
            ) = 'Виконано' THEN 'Замовлення успішно виконано'

            WHEN (
                SELECT status_name
                FROM order_statuses
                WHERE status_id = @status_id
            ) = 'Скасовано' THEN 'Замовлення скасовано'

            ELSE NULL
        END,

        archive_reason_id = CASE
            WHEN (
                SELECT status_name
                FROM order_statuses
                WHERE status_id = @status_id
            ) IN ('Виконано', 'Скасовано') THEN archive_reason_id
            ELSE NULL
        END,

        archive_comment = CASE
            WHEN (
                SELECT status_name
                FROM order_statuses
                WHERE status_id = @status_id
            ) IN ('Виконано', 'Скасовано') THEN archive_comment
            ELSE NULL
        END,

        customer_comment = @customer_comment,
        manager_comment = @manager_comment,
        updated_at = CURRENT_TIMESTAMP
    WHERE order_id = @order_id;
";

                await using var orderCommand =
                    new NpgsqlCommand(updateOrderQuery, connection, transaction);

                orderCommand.Parameters.AddWithValue("@order_id", orderId);
                orderCommand.Parameters.AddWithValue("@status_id", statusId);

                orderCommand.Parameters.AddWithValue(
                    "@customer_comment",
                    string.IsNullOrWhiteSpace(customerComment) ? DBNull.Value : customerComment
                );

                orderCommand.Parameters.AddWithValue(
                    "@manager_comment",
                    string.IsNullOrWhiteSpace(managerComment) ? DBNull.Value : managerComment
                );

                await orderCommand.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// Метод ArchiveOrderAsync переносить замовлення до архіву.
        public async Task ArchiveOrderAsync(
            int orderId,
            int archiveReasonId,
            string archiveReason,
            string archiveComment)
        {
            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string fullReason = archiveReason;

            if (!string.IsNullOrWhiteSpace(archiveComment))
            {
                fullReason += ": " + archiveComment;
            }

            string query = @"
                UPDATE orders
                SET
                    is_active = FALSE,
                    is_archived = TRUE,
                    archived_at = CURRENT_TIMESTAMP,
                    archive_reason_id = @archive_reason_id,
                    archive_reason = @archive_reason,
                    archive_comment = @archive_comment,
                    updated_at = CURRENT_TIMESTAMP
                WHERE order_id = @order_id;
            ";

            await using var command = new NpgsqlCommand(query, connection);

            command.Parameters.AddWithValue("@order_id", orderId);
            command.Parameters.AddWithValue("@archive_reason_id", archiveReasonId);
            command.Parameters.AddWithValue("@archive_reason", fullReason);

            command.Parameters.AddWithValue(
                "@archive_comment",
                string.IsNullOrWhiteSpace(archiveComment) ? DBNull.Value : archiveComment
            );

            await command.ExecuteNonQueryAsync();
        }

        /// Метод GetOrderItemsAsync отримує список товарів, які входять до конкретного замовлення.
        public async Task<List<OrderItemDetail>> GetOrderItemsAsync(int orderId)
        {
            List<OrderItemDetail> items = new List<OrderItemDetail>();

            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
                SELECT
                    oi.order_item_id,
                    oi.order_id,
                    oi.product_id,

                    p.model_name,
                    b.brand_name,

                    COALESCE(product_info.color_name, '') AS color_name,
                    COALESCE(product_info.image_path, p.image_path, '') AS image_path,

                    oi.quantity,
                    oi.unit_price,
                    oi.discount_percent,
                    oi.line_total

                FROM order_items oi

                JOIN products p ON oi.product_id = p.product_id
                JOIN brands b ON p.brand_id = b.brand_id

                LEFT JOIN LATERAL (
                    SELECT
                        vf.color_name,
                        vf.image_path
                    FROM v_products_full vf
                    WHERE vf.product_id = oi.product_id
                    ORDER BY vf.variant_id
                    LIMIT 1
                ) product_info ON TRUE

                WHERE oi.order_id = @order_id
                ORDER BY oi.order_item_id;
            ";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@order_id", orderId);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                OrderItemDetail item = new OrderItemDetail
                {
                    OrderItemId = GetInt(reader, "order_item_id"),
                    OrderId = GetInt(reader, "order_id"),
                    ProductId = GetInt(reader, "product_id"),

                    ModelName = GetString(reader, "model_name"),
                    BrandName = GetString(reader, "brand_name"),
                    ColorName = GetString(reader, "color_name"),
                    ImagePath = GetString(reader, "image_path"),

                    Quantity = GetInt(reader, "quantity"),
                    UnitPrice = GetDecimal(reader, "unit_price"),
                    DiscountPercent = GetDecimal(reader, "discount_percent"),
                    LineTotal = GetDecimal(reader, "line_total")
                };

                items.Add(item);
            }

            return items;
        }
        /// Метод UpdateOrderStatusAsync змінює статус замовлення.
        public async Task UpdateOrderStatusAsync(int orderId, int statusId)
        {
            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
        UPDATE orders
        SET
            status_id = @status_id,

            status = CASE
                WHEN (
                    SELECT status_name
                    FROM order_statuses
                    WHERE status_id = @status_id
                ) IN ('Нове', 'В обробці', 'Очікує оплати') THEN 'Новый'

                WHEN (
                    SELECT status_name
                    FROM order_statuses
                    WHERE status_id = @status_id
                ) IN ('Оплачено', 'Відправлено') THEN 'Оплачен'

                WHEN (
                    SELECT status_name
                    FROM order_statuses
                    WHERE status_id = @status_id
                ) = 'Виконано' THEN 'Завершен'

                WHEN (
                    SELECT status_name
                    FROM order_statuses
                    WHERE status_id = @status_id
                ) = 'Скасовано' THEN 'Отменен'

                ELSE status
            END,

            is_active = CASE
                WHEN (
                    SELECT status_name
                    FROM order_statuses
                    WHERE status_id = @status_id
                ) IN ('Виконано', 'Скасовано') THEN FALSE
                ELSE TRUE
            END,

            is_archived = CASE
                WHEN (
                    SELECT status_name
                    FROM order_statuses
                    WHERE status_id = @status_id
                ) IN ('Виконано', 'Скасовано') THEN TRUE
                ELSE FALSE
            END,

            archived_at = CASE
                WHEN (
                    SELECT status_name
                    FROM order_statuses
                    WHERE status_id = @status_id
                ) IN ('Виконано', 'Скасовано') THEN CURRENT_TIMESTAMP
                ELSE NULL
            END,

            archive_reason = CASE
                WHEN (
                    SELECT status_name
                    FROM order_statuses
                    WHERE status_id = @status_id
                ) = 'Виконано' THEN 'Замовлення успішно виконано'

                WHEN (
                    SELECT status_name
                    FROM order_statuses
                    WHERE status_id = @status_id
                ) = 'Скасовано' THEN 'Замовлення скасовано'

                ELSE NULL
            END,

            archive_reason_id = CASE
                WHEN (
                    SELECT status_name
                    FROM order_statuses
                    WHERE status_id = @status_id
                ) IN ('Виконано', 'Скасовано') THEN archive_reason_id
                ELSE NULL
            END,

            archive_comment = CASE
                WHEN (
                    SELECT status_name
                    FROM order_statuses
                    WHERE status_id = @status_id
                ) IN ('Виконано', 'Скасовано') THEN archive_comment
                ELSE NULL
            END,

            updated_at = CURRENT_TIMESTAMP
        WHERE order_id = @order_id;
    ";

            await using var command = new NpgsqlCommand(query, connection);

            command.Parameters.AddWithValue("@order_id", orderId);
            command.Parameters.AddWithValue("@status_id", statusId);

            await command.ExecuteNonQueryAsync();
        }
        /// Метод CancelOrderAsync деактивує замовлення.
        public async Task CancelOrderAsync(int orderId)
        {
            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
                UPDATE orders
                SET
                    is_active = FALSE,
                    updated_at = CURRENT_TIMESTAMP
                WHERE order_id = @order_id;
            ";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@order_id", orderId);

            await command.ExecuteNonQueryAsync();
        }

        /// Метод GetString безпечно зчитує текстове значення з вказаної колонки.
        private string GetString(NpgsqlDataReader reader, string columnName)
        {
            int index = reader.GetOrdinal(columnName);

            if (reader[index] == DBNull.Value)
            {
                return "";
            }

            return reader.GetString(index);
        }

        /// Метод GetInt безпечно зчитує ціле число з вказаної колонки.
        private int GetInt(NpgsqlDataReader reader, string columnName)
        {
            int index = reader.GetOrdinal(columnName);

            if (reader[index] == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToInt32(reader[index]);
        }

        /// Метод GetNullableInt безпечно зчитує ціле число, яке може бути відсутнім.
        private int? GetNullableInt(NpgsqlDataReader reader, string columnName)
        {
            int index = reader.GetOrdinal(columnName);

            if (reader[index] == DBNull.Value)
            {
                return null;
            }

            return Convert.ToInt32(reader[index]);
        }

        /// Метод GetDecimal безпечно зчитує десяткове число з вказаної колонки.
        private decimal GetDecimal(NpgsqlDataReader reader, string columnName)
        {
            int index = reader.GetOrdinal(columnName);

            if (reader[index] == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToDecimal(reader[index]);
        }

        /// Метод GetBool безпечно зчитує логічне значення з вказаної колонки.
        private bool GetBool(NpgsqlDataReader reader, string columnName, bool defaultValue)
        {
            int index = reader.GetOrdinal(columnName);

            if (reader[index] == DBNull.Value)
            {
                return defaultValue;
            }

            return Convert.ToBoolean(reader[index]);
        }
        /// Метод GetDateTime безпечно зчитує дату з вказаної колонки.
        private DateTime GetDateTime(NpgsqlDataReader reader, string columnName, DateTime defaultValue)
        {
            int index = reader.GetOrdinal(columnName);

            if (reader[index] == DBNull.Value)
            {
                return defaultValue;
            }

            object value = reader[index];

            if (value is DateTime dateTimeValue)
            {
                return dateTimeValue;
            }

            if (value is DateOnly dateOnlyValue)
            {
                return dateOnlyValue.ToDateTime(TimeOnly.MinValue);
            }

            return DateTime.Parse(value.ToString()!);
        }


        private DateTime? GetNullableDateTime(NpgsqlDataReader reader, string columnName)
        {
            int index = reader.GetOrdinal(columnName);

            if (reader[index] == DBNull.Value)
            {
                return null;
            }

            object value = reader[index];

            if (value is DateTime dateTimeValue)
            {
                return dateTimeValue;
            }

            if (value is DateOnly dateOnlyValue)
            {
                return dateOnlyValue.ToDateTime(TimeOnly.MinValue);
            }

            return DateTime.Parse(value.ToString()!);
        }


        private TimeSpan GetTimeSpan(NpgsqlDataReader reader, string columnName, TimeSpan defaultValue)
        {
            int index = reader.GetOrdinal(columnName);

            if (reader[index] == DBNull.Value)
            {
                return defaultValue;
            }

            object value = reader[index];

            if (value is TimeSpan timeSpanValue)
            {
                return timeSpanValue;
            }

            if (value is TimeOnly timeOnlyValue)
            {
                return timeOnlyValue.ToTimeSpan();
            }

            return TimeSpan.Parse(value.ToString()!);
        }

        /// Метод GetNullableTimeSpan безпечно зчитує час, який може бути відсутнім.
        private TimeSpan? GetNullableTimeSpan(NpgsqlDataReader reader, string columnName)
        {
            int index = reader.GetOrdinal(columnName);

            if (reader[index] == DBNull.Value)
            {
                return null;
            }

            object value = reader[index];

            if (value is TimeSpan timeSpanValue)
            {
                return timeSpanValue;
            }

            if (value is TimeOnly timeOnlyValue)
            {
                return timeOnlyValue.ToTimeSpan();
            }

            return TimeSpan.Parse(value.ToString()!);
        }

        /// Метод RestoreOrderFromArchiveAsync відновлює замовлення з архіву.
        public async Task<bool> RestoreOrderFromArchiveAsync(int orderId)
        {
            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
        UPDATE orders o
        SET
            is_active = TRUE,
            is_archived = FALSE,
            archived_at = NULL,
            archive_reason_id = NULL,
            archive_reason = NULL,
            archive_comment = NULL,
            updated_at = CURRENT_TIMESTAMP
        WHERE o.order_id = @order_id
          AND COALESCE(o.is_archived, FALSE) = TRUE
          AND NOT EXISTS (
              SELECT 1
              FROM order_statuses os
              WHERE os.status_id = o.status_id
                AND os.status_name IN ('Виконано', 'Скасовано')
          );
    ";

            await using var command = new NpgsqlCommand(query, connection);

            command.Parameters.AddWithValue("@order_id", orderId);

            int affectedRows = await command.ExecuteNonQueryAsync();

            return affectedRows > 0;
        }
    }
}