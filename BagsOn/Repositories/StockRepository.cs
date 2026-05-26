using BagsOn.Data;
using BagsOn.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BagsOn.Repositories
{
    // Клас StockRepository відповідає за роботу зі складом у базі даних.
    public class StockRepository
    {
        // Метод GetStockItemsAsync отримує повний список товарів на складі.
        public async Task<List<StockItem>> GetStockItemsAsync()
        {
            List<StockItem> stockItems = new List<StockItem>();

            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
                SELECT
                    stock_item_id,
                    product_id,
                    variant_id,
                    model_name,
                    brand_name,
                    type_name,
                    type_group,
                    category_name,
                    material_name,
                    material_group,
                    color_name,
                    price,
                    total_quantity,
                    reserved_quantity,
                    available_quantity,
                    min_quantity,
                    location,
                    updated_at,
                    image_path,
                    stock_status,
                    stock_warning,
                    total_value,
                    available_value
                FROM v_stock_full
                ORDER BY
                    stock_status,
                    brand_name,
                    model_name,
                    color_name;
            ";

            await using var command = new NpgsqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                stockItems.Add(ReadStockItem(reader));
            }

            return stockItems;
        }
        // Метод ReadStockItem перетворює один рядок результату SQL-запиту
        private StockItem ReadStockItem(NpgsqlDataReader reader)
        {
            return new StockItem
            {
                StockItemId = GetInt(reader, "stock_item_id"),
                ProductId = GetInt(reader, "product_id"),
                VariantId = GetInt(reader, "variant_id"),

                ModelName = GetString(reader, "model_name"),
                BrandName = GetString(reader, "brand_name"),
                TypeName = GetString(reader, "type_name"),
                TypeGroup = GetString(reader, "type_group"),
                CategoryName = GetString(reader, "category_name"),
                MaterialName = GetString(reader, "material_name"),
                MaterialGroup = GetString(reader, "material_group"),
                ColorName = GetString(reader, "color_name"),

                Price = GetDecimal(reader, "price"),

                TotalQuantity = GetInt(reader, "total_quantity"),
                ReservedQuantity = GetInt(reader, "reserved_quantity"),
                AvailableQuantity = GetInt(reader, "available_quantity"),
                MinQuantity = GetInt(reader, "min_quantity"),

                Location = GetString(reader, "location"),

                UpdatedAt = GetDateTime(reader, "updated_at"),

                ImagePath = GetString(reader, "image_path"),

                StockStatus = GetString(reader, "stock_status"),
                StockWarning = GetString(reader, "stock_warning"),

                TotalValue = GetDecimal(reader, "total_value"),
                AvailableValue = GetDecimal(reader, "available_value")
            };
        }

        private string GetString(NpgsqlDataReader reader, string columnName)
        {
            int index = reader.GetOrdinal(columnName);

            if (reader[index] == DBNull.Value)
            {
                return string.Empty;
            }

            return reader[index].ToString() ?? string.Empty;
        }

        private int GetInt(NpgsqlDataReader reader, string columnName)
        {
            int index = reader.GetOrdinal(columnName);

            if (reader[index] == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToInt32(reader[index]);
        }
        // Метод GetDecimal безпечно зчитує десяткове число з вказаної колонки.
        private decimal GetDecimal(NpgsqlDataReader reader, string columnName)
        {
            int index = reader.GetOrdinal(columnName);

            if (reader[index] == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToDecimal(reader[index]);
        }
        // Метод GetDateTime безпечно зчитує дату і час з вказаної колонки.
        private DateTime GetDateTime(NpgsqlDataReader reader, string columnName)
        {
            int index = reader.GetOrdinal(columnName);

            if (reader[index] == DBNull.Value)
            {
                return DateTime.Now;
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
        // Метод GetStockMovementsAsync отримує історію руху конкретного варіанту товару.
        public async Task<List<StockMovement>> GetStockMovementsAsync(int variantId)
        {
            List<StockMovement> movements = new List<StockMovement>();

            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
        SELECT
            movement_id,
            variant_id,
            order_id,
            movement_type,
            quantity_change,
            quantity_before,
            quantity_after,
            comment,
            created_at
        FROM stock_movements
        WHERE variant_id = @variant_id
        ORDER BY created_at DESC, movement_id DESC;
    ";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@variant_id", variantId);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                movements.Add(new StockMovement
                {
                    MovementId = GetInt(reader, "movement_id"),
                    VariantId = GetInt(reader, "variant_id"),
                    OrderId = GetNullableInt(reader, "order_id"),
                    MovementType = GetString(reader, "movement_type"),
                    QuantityChange = GetInt(reader, "quantity_change"),
                    QuantityBefore = GetInt(reader, "quantity_before"),
                    QuantityAfter = GetInt(reader, "quantity_after"),
                    Comment = GetString(reader, "comment"),
                    CreatedAt = GetDateTime(reader, "created_at")
                });
            }

            return movements;
        }

        // Метод AddIncomingAsync додає надходження товару на склад.
        public async Task AddIncomingAsync(int variantId, int quantity, string comment)
        {
            if (quantity <= 0)
            {
                throw new ArgumentException("Кількість надходження має бути більшою за 0.");
            }

            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                string ensureStockItemQuery = @"
            INSERT INTO stock_items (
                variant_id,
                total_quantity,
                reserved_quantity,
                min_quantity,
                location
            )
            SELECT
                pv.variant_id,
                COALESCE(pv.quantity, 0),
                0,
                2,
                ''
            FROM product_variants pv
            WHERE pv.variant_id = @variant_id
            ON CONFLICT (variant_id) DO NOTHING;
        ";

                await using (var ensureCommand = new NpgsqlCommand(ensureStockItemQuery, connection, transaction))
                {
                    ensureCommand.Parameters.AddWithValue("@variant_id", variantId);
                    await ensureCommand.ExecuteNonQueryAsync();
                }

                int quantityBefore = 0;

                string getQuantityQuery = @"
            SELECT total_quantity
            FROM stock_items
            WHERE variant_id = @variant_id
            FOR UPDATE;
        ";

                await using (var getCommand = new NpgsqlCommand(getQuantityQuery, connection, transaction))
                {
                    getCommand.Parameters.AddWithValue("@variant_id", variantId);

                    object? result = await getCommand.ExecuteScalarAsync();

                    if (result == null || result == DBNull.Value)
                    {
                        throw new Exception("Не вдалося знайти товар на складі.");
                    }

                    quantityBefore = Convert.ToInt32(result);
                }

                int quantityAfter = quantityBefore + quantity;

                string updateStockQuery = @"
            UPDATE stock_items
            SET
                total_quantity = @quantity_after,
                updated_at = CURRENT_TIMESTAMP
            WHERE variant_id = @variant_id;
        ";

                await using (var updateStockCommand = new NpgsqlCommand(updateStockQuery, connection, transaction))
                {
                    updateStockCommand.Parameters.AddWithValue("@variant_id", variantId);
                    updateStockCommand.Parameters.AddWithValue("@quantity_after", quantityAfter);
                    await updateStockCommand.ExecuteNonQueryAsync();
                }

                string updateVariantQuery = @"
            UPDATE product_variants
            SET
                quantity = @quantity_after,
                updated_at = CURRENT_TIMESTAMP
            WHERE variant_id = @variant_id;
        ";

                await using (var updateVariantCommand = new NpgsqlCommand(updateVariantQuery, connection, transaction))
                {
                    updateVariantCommand.Parameters.AddWithValue("@variant_id", variantId);
                    updateVariantCommand.Parameters.AddWithValue("@quantity_after", quantityAfter);
                    await updateVariantCommand.ExecuteNonQueryAsync();
                }

                string insertMovementQuery = @"
            INSERT INTO stock_movements (
                variant_id,
                order_id,
                movement_type,
                quantity_change,
                quantity_before,
                quantity_after,
                comment
            )
            VALUES (
                @variant_id,
                NULL,
                'Надходження',
                @quantity_change,
                @quantity_before,
                @quantity_after,
                @comment
            );
        ";

                await using (var movementCommand = new NpgsqlCommand(insertMovementQuery, connection, transaction))
                {
                    movementCommand.Parameters.AddWithValue("@variant_id", variantId);
                    movementCommand.Parameters.AddWithValue("@quantity_change", quantity);
                    movementCommand.Parameters.AddWithValue("@quantity_before", quantityBefore);
                    movementCommand.Parameters.AddWithValue("@quantity_after", quantityAfter);
                    movementCommand.Parameters.AddWithValue("@comment", comment ?? string.Empty);

                    await movementCommand.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Метод GetNullableInt безпечно зчитує ціле число, яке може бути відсутнім.
        private int? GetNullableInt(NpgsqlDataReader reader, string columnName)
        {
            int index = reader.GetOrdinal(columnName);

            if (reader[index] == DBNull.Value)
            {
                return null;
            }

            return Convert.ToInt32(reader[index]);
        }
        // Метод WriteOffStockAsync виконує списання товару зі складу.
        public async Task WriteOffStockAsync(int variantId, int quantity, string reason, string comment)
        {
            if (quantity <= 0)
            {
                throw new ArgumentException("Кількість для списання має бути більшою за 0.");
            }

            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Перевіряємо, чи існує запис товару на складі.
                // Якщо його немає — створюємо на основі product_variants.
                string ensureStockItemQuery = @"
            INSERT INTO stock_items (
                variant_id,
                total_quantity,
                reserved_quantity,
                min_quantity,
                location
            )
            SELECT
                pv.variant_id,
                COALESCE(pv.quantity, 0),
                0,
                2,
                ''
            FROM product_variants pv
            WHERE pv.variant_id = @variant_id
            ON CONFLICT (variant_id) DO NOTHING;
        ";

                await using (var ensureCommand = new NpgsqlCommand(ensureStockItemQuery, connection, transaction))
                {
                    ensureCommand.Parameters.AddWithValue("@variant_id", variantId);
                    await ensureCommand.ExecuteNonQueryAsync();
                }

                int quantityBefore = 0;
                int reservedQuantity = 0;

                // Блокуємо рядок складу, щоб паралельно ніхто не змінив кількість.
                string getQuantityQuery = @"
            SELECT
                total_quantity,
                reserved_quantity
            FROM stock_items
            WHERE variant_id = @variant_id
            FOR UPDATE;
        ";

                await using (var getCommand = new NpgsqlCommand(getQuantityQuery, connection, transaction))
                {
                    getCommand.Parameters.AddWithValue("@variant_id", variantId);

                    await using var reader = await getCommand.ExecuteReaderAsync();

                    if (!await reader.ReadAsync())
                    {
                        throw new Exception("Не вдалося знайти товар на складі.");
                    }

                    quantityBefore = Convert.ToInt32(reader["total_quantity"]);
                    reservedQuantity = Convert.ToInt32(reader["reserved_quantity"]);
                }

                int availableQuantity = quantityBefore - reservedQuantity;

                if (quantity > availableQuantity)
                {
                    throw new Exception(
                        $"Не можна списати {quantity} шт., тому що доступно тільки {availableQuantity} шт.\n\n" +
                        $"Всього на складі: {quantityBefore} шт.\n" +
                        $"Зарезервовано: {reservedQuantity} шт."
                    );
                }

                int quantityAfter = quantityBefore - quantity;

                string updateStockQuery = @"
            UPDATE stock_items
            SET
                total_quantity = @quantity_after,
                updated_at = CURRENT_TIMESTAMP
            WHERE variant_id = @variant_id;
        ";

                await using (var updateStockCommand = new NpgsqlCommand(updateStockQuery, connection, transaction))
                {
                    updateStockCommand.Parameters.AddWithValue("@variant_id", variantId);
                    updateStockCommand.Parameters.AddWithValue("@quantity_after", quantityAfter);

                    await updateStockCommand.ExecuteNonQueryAsync();
                }

                // Синхронізуємо кількість з product_variants,
                // щоб Асортимент теж бачив актуальний залишок.
                string updateVariantQuery = @"
            UPDATE product_variants
            SET
                quantity = @quantity_after,
                updated_at = CURRENT_TIMESTAMP
            WHERE variant_id = @variant_id;
        ";

                await using (var updateVariantCommand = new NpgsqlCommand(updateVariantQuery, connection, transaction))
                {
                    updateVariantCommand.Parameters.AddWithValue("@variant_id", variantId);
                    updateVariantCommand.Parameters.AddWithValue("@quantity_after", quantityAfter);

                    await updateVariantCommand.ExecuteNonQueryAsync();
                }

                string fullComment = reason;

                if (!string.IsNullOrWhiteSpace(comment))
                {
                    fullComment += ": " + comment;
                }

                string insertMovementQuery = @"
            INSERT INTO stock_movements (
                variant_id,
                order_id,
                movement_type,
                quantity_change,
                quantity_before,
                quantity_after,
                comment
            )
            VALUES (
                @variant_id,
                NULL,
                'Списання',
                @quantity_change,
                @quantity_before,
                @quantity_after,
                @comment
            );
        ";

                await using (var movementCommand = new NpgsqlCommand(insertMovementQuery, connection, transaction))
                {
                    movementCommand.Parameters.AddWithValue("@variant_id", variantId);
                    movementCommand.Parameters.AddWithValue("@quantity_change", -quantity);
                    movementCommand.Parameters.AddWithValue("@quantity_before", quantityBefore);
                    movementCommand.Parameters.AddWithValue("@quantity_after", quantityAfter);
                    movementCommand.Parameters.AddWithValue("@comment", fullComment);

                    await movementCommand.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Метод AdjustStockAsync виконує коригування залишку товару на складі.
        public async Task AdjustStockAsync(int variantId, int newTotalQuantity, string comment)
        {
            if (newTotalQuantity < 0)
            {
                throw new ArgumentException("Кількість не може бути меншою за 0.");
            }

            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Якщо запису складу ще немає — створюємо його на основі product_variants.
                string ensureStockItemQuery = @"
            INSERT INTO stock_items (
                variant_id,
                total_quantity,
                reserved_quantity,
                min_quantity,
                location
            )
            SELECT
                pv.variant_id,
                COALESCE(pv.quantity, 0),
                0,
                2,
                ''
            FROM product_variants pv
            WHERE pv.variant_id = @variant_id
            ON CONFLICT (variant_id) DO NOTHING;
        ";

                await using (var ensureCommand = new NpgsqlCommand(ensureStockItemQuery, connection, transaction))
                {
                    ensureCommand.Parameters.AddWithValue("@variant_id", variantId);
                    await ensureCommand.ExecuteNonQueryAsync();
                }

                int quantityBefore = 0;
                int reservedQuantity = 0;

                string getQuantityQuery = @"
            SELECT
                total_quantity,
                reserved_quantity
            FROM stock_items
            WHERE variant_id = @variant_id
            FOR UPDATE;
        ";

                await using (var getCommand = new NpgsqlCommand(getQuantityQuery, connection, transaction))
                {
                    getCommand.Parameters.AddWithValue("@variant_id", variantId);

                    await using var reader = await getCommand.ExecuteReaderAsync();

                    if (!await reader.ReadAsync())
                    {
                        throw new Exception("Не вдалося знайти товар на складі.");
                    }

                    quantityBefore = Convert.ToInt32(reader["total_quantity"]);
                    reservedQuantity = Convert.ToInt32(reader["reserved_quantity"]);
                }

                if (newTotalQuantity < reservedQuantity)
                {
                    throw new Exception(
                        $"Неможливо встановити залишок {newTotalQuantity} шт., тому що вже зарезервовано {reservedQuantity} шт.\n\n" +
                        $"Новий залишок не може бути меншим за кількість у резерві."
                    );
                }

                int quantityChange = newTotalQuantity - quantityBefore;

                if (quantityChange == 0)
                {
                    throw new Exception("Кількість не змінилася. Коригування не потрібне.");
                }

                string updateStockQuery = @"
            UPDATE stock_items
            SET
                total_quantity = @new_total_quantity,
                updated_at = CURRENT_TIMESTAMP
            WHERE variant_id = @variant_id;
        ";

                await using (var updateStockCommand = new NpgsqlCommand(updateStockQuery, connection, transaction))
                {
                    updateStockCommand.Parameters.AddWithValue("@variant_id", variantId);
                    updateStockCommand.Parameters.AddWithValue("@new_total_quantity", newTotalQuantity);

                    await updateStockCommand.ExecuteNonQueryAsync();
                }

                // Синхронізуємо кількість з асортиментом.
                string updateVariantQuery = @"
            UPDATE product_variants
            SET
                quantity = @new_total_quantity,
                updated_at = CURRENT_TIMESTAMP
            WHERE variant_id = @variant_id;
        ";

                await using (var updateVariantCommand = new NpgsqlCommand(updateVariantQuery, connection, transaction))
                {
                    updateVariantCommand.Parameters.AddWithValue("@variant_id", variantId);
                    updateVariantCommand.Parameters.AddWithValue("@new_total_quantity", newTotalQuantity);

                    await updateVariantCommand.ExecuteNonQueryAsync();
                }

                string movementComment = string.IsNullOrWhiteSpace(comment)
                    ? "Коригування після інвентаризації"
                    : comment;

                string insertMovementQuery = @"
            INSERT INTO stock_movements (
                variant_id,
                order_id,
                movement_type,
                quantity_change,
                quantity_before,
                quantity_after,
                comment
            )
            VALUES (
                @variant_id,
                NULL,
                'Коригування',
                @quantity_change,
                @quantity_before,
                @quantity_after,
                @comment
            );
        ";

                await using (var movementCommand = new NpgsqlCommand(insertMovementQuery, connection, transaction))
                {
                    movementCommand.Parameters.AddWithValue("@variant_id", variantId);
                    movementCommand.Parameters.AddWithValue("@quantity_change", quantityChange);
                    movementCommand.Parameters.AddWithValue("@quantity_before", quantityBefore);
                    movementCommand.Parameters.AddWithValue("@quantity_after", newTotalQuantity);
                    movementCommand.Parameters.AddWithValue("@comment", movementComment);

                    await movementCommand.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}