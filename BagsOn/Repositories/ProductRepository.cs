using BagsOn.Data;
using BagsOn.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BagsOn.Repositories
{
    public class ProductRepository
    {
        public async Task<List<Product>> GetAllProductsAsync()
        {
            List<Product> products = new List<Product>();

            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
                SELECT
                    product_id,
                    variant_id,
                    model_name,
                    brand_name,
                    brand_segment,
                    type_name,
                    type_group,
                    category_name,
                    material_name,
                    material_group,
                    color_name,
                    price,
                    quantity,
                    arrival_date,
                    image_path,
                    is_active,
                    total_value
                FROM v_products_full
                WHERE is_active = TRUE
                ORDER BY product_id, variant_id;
            ";

            await using var command = new NpgsqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Product product = new Product
                {
                    ProductId = reader.GetInt32(reader.GetOrdinal("product_id")),
                    VariantId = reader.GetInt32(reader.GetOrdinal("variant_id")),

                    ModelName = reader.GetString(reader.GetOrdinal("model_name")),
                    BrandName = reader.GetString(reader.GetOrdinal("brand_name")),

                    BrandSegment = reader["brand_segment"] == DBNull.Value
                        ? ""
                        : reader.GetString(reader.GetOrdinal("brand_segment")),

                    TypeName = reader.GetString(reader.GetOrdinal("type_name")),

                    TypeGroup = reader["type_group"] == DBNull.Value
                        ? ""
                        : reader.GetString(reader.GetOrdinal("type_group")),

                    CategoryName = reader["category_name"] == DBNull.Value
                        ? ""
                        : reader.GetString(reader.GetOrdinal("category_name")),

                    MaterialName = reader.GetString(reader.GetOrdinal("material_name")),

                    MaterialGroup = reader["material_group"] == DBNull.Value
                        ? ""
                        : reader.GetString(reader.GetOrdinal("material_group")),

                    ColorName = reader["color_name"] == DBNull.Value
                        ? ""
                        : reader.GetString(reader.GetOrdinal("color_name")),

                    Price = reader.GetDecimal(reader.GetOrdinal("price")),
                    Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                    ArrivalDate = reader.GetDateTime(reader.GetOrdinal("arrival_date")),

                    ImagePath = reader["image_path"] == DBNull.Value
                        ? ""
                        : reader.GetString(reader.GetOrdinal("image_path")),

                    IsActive = reader.GetBoolean(reader.GetOrdinal("is_active")),
                    TotalValue = reader.GetDecimal(reader.GetOrdinal("total_value"))
                };

                products.Add(product);
            }

            return products;
        }


        public async Task<List<ReferenceItem>> GetBrandSegmentsAsync()
        {
            return await GetReferenceItemsAsync("brand_segments", "segment_id", "segment_name");
        }


        public async Task<List<ReferenceItem>> GetBrandsAsync(int? segmentId = null)
        {
            List<ReferenceItem> items = new List<ReferenceItem>();

            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
        SELECT brand_id, brand_name
        FROM brands
        ORDER BY brand_name;
    ";

            await using var command = new NpgsqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(new ReferenceItem
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            }

            return items;
        }
        public async Task<List<ProductImage>> GetImagesByVariantIdAsync(int variantId)
        {
            List<ProductImage> images = new List<ProductImage>();

            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
        SELECT
            image_id,
            product_id,
            variant_id,
            image_path,
            is_main,
            sort_order,
            alt_text
        FROM product_images
        WHERE variant_id = @variant_id
        ORDER BY is_main DESC, sort_order ASC, image_id ASC;
    ";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@variant_id", variantId);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                ProductImage image = new ProductImage
                {
                    ImageId = reader.GetInt32(reader.GetOrdinal("image_id")),

                    ProductId = reader["product_id"] == DBNull.Value
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("product_id")),

                    VariantId = reader["variant_id"] == DBNull.Value
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("variant_id")),

                    ImagePath = reader.GetString(reader.GetOrdinal("image_path")),
                    IsMain = reader.GetBoolean(reader.GetOrdinal("is_main")),
                    SortOrder = reader.GetInt32(reader.GetOrdinal("sort_order")),

                    AltText = reader["alt_text"] == DBNull.Value
                        ? ""
                        : reader.GetString(reader.GetOrdinal("alt_text"))
                };

                images.Add(image);
            }

            return images;
        }

        public async Task<List<ReferenceItem>> GetTypesAsync()
        {
            return await GetReferenceItemsAsync("product_types", "type_id", "type_name");
        }


        public async Task<List<ReferenceItem>> GetCategoriesAsync()
        {
            return await GetReferenceItemsAsync("categories", "category_id", "category_name");
        }


        public async Task<List<ReferenceItem>> GetMaterialsAsync()
        {
            return await GetReferenceItemsAsync("materials", "material_id", "material_name");
        }


        public async Task<List<ReferenceItem>> GetColorsAsync()
        {
            return await GetReferenceItemsAsync("colors", "color_id", "color_name");
        }


        private async Task<List<ReferenceItem>> GetReferenceItemsAsync(
            string tableName,
            string idColumn,
            string nameColumn)
        {
            List<ReferenceItem> items = new List<ReferenceItem>();

            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = $@"
                SELECT {idColumn}, {nameColumn}
                FROM {tableName}
                ORDER BY {nameColumn};
            ";

            await using var command = new NpgsqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(new ReferenceItem
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            }

            return items;
        }


        public async Task<int> AddBrandAsync(string brandName, int? segmentId)
        {
            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
                INSERT INTO brands (brand_name, segment_id)
                VALUES (@brand_name, @segment_id)
                ON CONFLICT (brand_name) DO UPDATE
                SET segment_id = EXCLUDED.segment_id
                RETURNING brand_id;
            ";

            await using var command = new NpgsqlCommand(query, connection);

            command.Parameters.AddWithValue("@brand_name", brandName);

            if (segmentId == null)
            {
                command.Parameters.AddWithValue("@segment_id", DBNull.Value);
            }
            else
            {
                command.Parameters.AddWithValue("@segment_id", segmentId.Value);
            }

            object? result = await command.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }


        public async Task<int> EnsureNoBrandAsync()
        {
            return await AddBrandAsync("Без бренду", null);
        }


        public async Task AddProductWithVariantAsync(
     string modelName,
     int brandId,
     int typeId,
     int materialId,
     List<int> categoryIds,
     int colorId,
     decimal price,
     int quantity,
     DateTime arrivalDate,
     List<string> imagePaths)
        {
            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                string? mainImagePath = imagePaths.Count > 0 ? imagePaths[0] : null;

                string insertProductQuery = @"
            INSERT INTO products (
                model_name,
                brand_id,
                type_id,
                material_id,
                color_id,
                price,
                quantity,
                arrival_date,
                image_path
            )
            VALUES (
                @model_name,
                @brand_id,
                @type_id,
                @material_id,
                @color_id,
                0,
                0,
                @arrival_date,
                @image_path
            )
            RETURNING product_id;
        ";

                await using var productCommand = new NpgsqlCommand(insertProductQuery, connection, transaction);

                productCommand.Parameters.AddWithValue("@model_name", modelName);
                productCommand.Parameters.AddWithValue("@brand_id", brandId);
                productCommand.Parameters.AddWithValue("@type_id", typeId);
                productCommand.Parameters.AddWithValue("@material_id", materialId);
                productCommand.Parameters.AddWithValue("@color_id", colorId);
                productCommand.Parameters.AddWithValue("@arrival_date", arrivalDate);

                if (string.IsNullOrWhiteSpace(mainImagePath))
                {
                    productCommand.Parameters.AddWithValue("@image_path", DBNull.Value);
                }
                else
                {
                    productCommand.Parameters.AddWithValue("@image_path", mainImagePath);
                }

                object? productResult = await productCommand.ExecuteScalarAsync();
                int productId = Convert.ToInt32(productResult);


                foreach (int categoryId in categoryIds)
                {
                    string categoryQuery = @"
                INSERT INTO product_categories (product_id, category_id)
                VALUES (@product_id, @category_id)
                ON CONFLICT DO NOTHING;
            ";

                    await using var categoryCommand = new NpgsqlCommand(categoryQuery, connection, transaction);

                    categoryCommand.Parameters.AddWithValue("@product_id", productId);
                    categoryCommand.Parameters.AddWithValue("@category_id", categoryId);

                    await categoryCommand.ExecuteNonQueryAsync();
                }


                string sku = $"SKU-{productId}-{colorId}-{DateTime.Now:yyyyMMddHHmmss}";

                string insertVariantQuery = @"
            INSERT INTO product_variants (
                product_id,
                sku,
                color_id,
                price,
                quantity,
                arrival_date
            )
            VALUES (
                @product_id,
                @sku,
                @color_id,
                @price,
                @quantity,
                @arrival_date
            )
            RETURNING variant_id;
        ";

                await using var variantCommand = new NpgsqlCommand(insertVariantQuery, connection, transaction);

                variantCommand.Parameters.AddWithValue("@product_id", productId);
                variantCommand.Parameters.AddWithValue("@sku", sku);
                variantCommand.Parameters.AddWithValue("@color_id", colorId);
                variantCommand.Parameters.AddWithValue("@price", price);
                variantCommand.Parameters.AddWithValue("@quantity", quantity);
                variantCommand.Parameters.AddWithValue("@arrival_date", arrivalDate);

                object? variantResult = await variantCommand.ExecuteScalarAsync();
                int variantId = Convert.ToInt32(variantResult);




        string colorCompatibilityQuery = @"
            INSERT INTO product_variant_colors (variant_id, color_id)
            VALUES (@variant_id, @color_id)
            ON CONFLICT DO NOTHING;
        ";

                await using var colorCommand = new NpgsqlCommand(colorCompatibilityQuery, connection, transaction);

                colorCommand.Parameters.AddWithValue("@variant_id", variantId);
                colorCommand.Parameters.AddWithValue("@color_id", colorId);

                await colorCommand.ExecuteNonQueryAsync();


                for (int i = 0; i < imagePaths.Count; i++)
                {
                    string imageQuery = @"
                INSERT INTO product_images (
                    product_id,
                    variant_id,
                    image_path,
                    is_main,
                    sort_order,
                    alt_text
                )
                VALUES (
                    @product_id,
                    @variant_id,
                    @image_path,
                    @is_main,
                    @sort_order,
                    @alt_text
                );
            ";

                    await using var imageCommand = new NpgsqlCommand(imageQuery, connection, transaction);

                    imageCommand.Parameters.AddWithValue("@product_id", productId);
                    imageCommand.Parameters.AddWithValue("@variant_id", variantId);
                    imageCommand.Parameters.AddWithValue("@image_path", imagePaths[i]);
                    imageCommand.Parameters.AddWithValue("@is_main", i == 0);
                    imageCommand.Parameters.AddWithValue("@sort_order", i + 1);
                    imageCommand.Parameters.AddWithValue("@alt_text", $"{modelName} фото {i + 1}");

                    await imageCommand.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        // Метод отримує список моделей товарів для ComboBox
        public async Task<List<ReferenceItem>> GetProductsForVariantAsync()
        {
            List<ReferenceItem> items = new List<ReferenceItem>();

            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
        SELECT
            p.product_id,
            p.model_name || ' — ' || b.brand_name AS product_name
        FROM products p
        JOIN brands b ON p.brand_id = b.brand_id
        WHERE p.is_active = TRUE
        ORDER BY p.model_name;
    ";

            await using var command = new NpgsqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(new ReferenceItem
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            }

            return items;
        }


        // Метод додає новий варіант до вже існуючого товару
        public async Task AddVariantAsync(
            int productId,
            int colorId,
            decimal price,
            int quantity,
            DateTime arrivalDate,
            List<string> imagePaths)
        {
            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                string sku = $"SKU-{productId}-{colorId}-{DateTime.Now:yyyyMMddHHmmss}";

                string insertVariantQuery = @"
            INSERT INTO product_variants (
                product_id,
                sku,
                color_id,
                price,
                quantity,
                arrival_date
            )
            VALUES (
                @product_id,
                @sku,
                @color_id,
                @price,
                @quantity,
                @arrival_date
            )
            RETURNING variant_id;
        ";

                await using var variantCommand = new NpgsqlCommand(insertVariantQuery, connection, transaction);

                variantCommand.Parameters.AddWithValue("@product_id", productId);
                variantCommand.Parameters.AddWithValue("@sku", sku);
                variantCommand.Parameters.AddWithValue("@color_id", colorId);
                variantCommand.Parameters.AddWithValue("@price", price);
                variantCommand.Parameters.AddWithValue("@quantity", quantity);
                variantCommand.Parameters.AddWithValue("@arrival_date", arrivalDate);

                object? variantResult = await variantCommand.ExecuteScalarAsync();

                int variantId = Convert.ToInt32(variantResult);


                // Додаємо колір у додаткову таблицю для сумісності
                string colorQuery = @"
            INSERT INTO product_variant_colors (variant_id, color_id)
            VALUES (@variant_id, @color_id)
            ON CONFLICT DO NOTHING;
        ";

                await using var colorCommand = new NpgsqlCommand(colorQuery, connection, transaction);

                colorCommand.Parameters.AddWithValue("@variant_id", variantId);
                colorCommand.Parameters.AddWithValue("@color_id", colorId);

                await colorCommand.ExecuteNonQueryAsync();


                // Додаємо фото саме до цього варіанту
                for (int i = 0; i < imagePaths.Count; i++)
                {
                    string imageQuery = @"
                INSERT INTO product_images (
                    product_id,
                    variant_id,
                    image_path,
                    is_main,
                    sort_order,
                    alt_text
                )
                VALUES (
                    @product_id,
                    @variant_id,
                    @image_path,
                    @is_main,
                    @sort_order,
                    @alt_text
                );
            ";

                    await using var imageCommand = new NpgsqlCommand(imageQuery, connection, transaction);

                    imageCommand.Parameters.AddWithValue("@product_id", productId);
                    imageCommand.Parameters.AddWithValue("@variant_id", variantId);
                    imageCommand.Parameters.AddWithValue("@image_path", imagePaths[i]);
                    imageCommand.Parameters.AddWithValue("@is_main", i == 0);
                    imageCommand.Parameters.AddWithValue("@sort_order", i + 1);
                    imageCommand.Parameters.AddWithValue("@alt_text", $"Фото варіанту {i + 1}");

                    await imageCommand.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ProductEditData?> GetProductForEditAsync(int productId, int variantId)
        {
            ProductEditData? product = null;

            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            string query = @"
        SELECT
            p.product_id,
            pv.variant_id,
            p.model_name,
            p.brand_id,
            p.type_id,
            p.material_id,
            pv.color_id,
            pv.price,
            pv.quantity,
            pv.arrival_date
        FROM products p
        JOIN product_variants pv ON p.product_id = pv.product_id
        WHERE p.product_id = @product_id
          AND pv.variant_id = @variant_id;
    ";

            await using var command = new NpgsqlCommand(query, connection);

            command.Parameters.AddWithValue("@product_id", productId);
            command.Parameters.AddWithValue("@variant_id", variantId);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                product = new ProductEditData
                {
                    ProductId = reader.GetInt32(reader.GetOrdinal("product_id")),
                    VariantId = reader.GetInt32(reader.GetOrdinal("variant_id")),
                    ModelName = reader.GetString(reader.GetOrdinal("model_name")),
                    BrandId = reader.GetInt32(reader.GetOrdinal("brand_id")),
                    TypeId = reader.GetInt32(reader.GetOrdinal("type_id")),
                    MaterialId = reader.GetInt32(reader.GetOrdinal("material_id")),

                    ColorId = reader["color_id"] == DBNull.Value
                        ? 0
                        : reader.GetInt32(reader.GetOrdinal("color_id")),

                    Price = reader.GetDecimal(reader.GetOrdinal("price")),
                    Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                    ArrivalDate = reader.GetDateTime(reader.GetOrdinal("arrival_date"))
                };
            }

            await reader.CloseAsync();

            if (product == null)
            {
                return null;
            }

            string categoriesQuery = @"
        SELECT category_id
        FROM product_categories
        WHERE product_id = @product_id;
    ";

            await using var categoriesCommand = new NpgsqlCommand(categoriesQuery, connection);
            categoriesCommand.Parameters.AddWithValue("@product_id", productId);

            await using var categoriesReader = await categoriesCommand.ExecuteReaderAsync();

            while (await categoriesReader.ReadAsync())
            {
                product.CategoryIds.Add(categoriesReader.GetInt32(0));
            }

            await categoriesReader.CloseAsync();

            string imagesQuery = @"
        SELECT image_path
        FROM product_images
        WHERE variant_id = @variant_id
        ORDER BY is_main DESC, sort_order ASC, image_id ASC;
    ";

            await using var imagesCommand = new NpgsqlCommand(imagesQuery, connection);
            imagesCommand.Parameters.AddWithValue("@variant_id", variantId);

            await using var imagesReader = await imagesCommand.ExecuteReaderAsync();

            while (await imagesReader.ReadAsync())
            {
                product.ImagePaths.Add(imagesReader.GetString(0));
            }

            return product;
        }
        public async Task UpdateProductVariantAsync(
    int productId,
    int variantId,
    string modelName,
    int brandId,
    int typeId,
    int materialId,
    List<int> categoryIds,
    int colorId,
    decimal price,
    int quantity,
    DateTime arrivalDate,
    List<string> imagePaths,
    bool replaceImages)
        {
            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                string updateProductQuery = @"
            UPDATE products
            SET
                model_name = @model_name,
                brand_id = @brand_id,
                type_id = @type_id,
                material_id = @material_id,
                color_id = @color_id,
                price = @price,
                quantity = @quantity,
                arrival_date = @arrival_date,
                image_path = @image_path
            WHERE product_id = @product_id;
        ";

                await using var productCommand = new NpgsqlCommand(updateProductQuery, connection, transaction);

                string? mainImagePath = imagePaths.Count > 0 ? imagePaths[0] : null;

                productCommand.Parameters.AddWithValue("@product_id", productId);
                productCommand.Parameters.AddWithValue("@model_name", modelName);
                productCommand.Parameters.AddWithValue("@brand_id", brandId);
                productCommand.Parameters.AddWithValue("@type_id", typeId);
                productCommand.Parameters.AddWithValue("@material_id", materialId);
                productCommand.Parameters.AddWithValue("@color_id", colorId);
                productCommand.Parameters.AddWithValue("@price", price);
                productCommand.Parameters.AddWithValue("@quantity", quantity);
                productCommand.Parameters.AddWithValue("@arrival_date", arrivalDate);

                if (string.IsNullOrWhiteSpace(mainImagePath))
                {
                    productCommand.Parameters.AddWithValue("@image_path", DBNull.Value);
                }
                else
                {
                    productCommand.Parameters.AddWithValue("@image_path", mainImagePath);
                }

                await productCommand.ExecuteNonQueryAsync();


                string updateVariantQuery = @"
            UPDATE product_variants
            SET
                color_id = @color_id,
                price = @price,
                quantity = @quantity,
                arrival_date = @arrival_date,
                updated_at = CURRENT_TIMESTAMP
            WHERE variant_id = @variant_id;
        ";

                await using var variantCommand = new NpgsqlCommand(updateVariantQuery, connection, transaction);

                variantCommand.Parameters.AddWithValue("@variant_id", variantId);
                variantCommand.Parameters.AddWithValue("@color_id", colorId);
                variantCommand.Parameters.AddWithValue("@price", price);
                variantCommand.Parameters.AddWithValue("@quantity", quantity);
                variantCommand.Parameters.AddWithValue("@arrival_date", arrivalDate);

                await variantCommand.ExecuteNonQueryAsync();


                string deleteCategoriesQuery = @"
            DELETE FROM product_categories
            WHERE product_id = @product_id;
        ";

                await using var deleteCategoriesCommand = new NpgsqlCommand(deleteCategoriesQuery, connection, transaction);
                deleteCategoriesCommand.Parameters.AddWithValue("@product_id", productId);
                await deleteCategoriesCommand.ExecuteNonQueryAsync();


                foreach (int categoryId in categoryIds)
                {
                    string insertCategoryQuery = @"
                INSERT INTO product_categories (product_id, category_id)
                VALUES (@product_id, @category_id)
                ON CONFLICT DO NOTHING;
            ";

                    await using var categoryCommand = new NpgsqlCommand(insertCategoryQuery, connection, transaction);

                    categoryCommand.Parameters.AddWithValue("@product_id", productId);
                    categoryCommand.Parameters.AddWithValue("@category_id", categoryId);

                    await categoryCommand.ExecuteNonQueryAsync();
                }


                string deleteVariantColorQuery = @"
            DELETE FROM product_variant_colors
            WHERE variant_id = @variant_id;
        ";

                await using var deleteColorCommand = new NpgsqlCommand(deleteVariantColorQuery, connection, transaction);
                deleteColorCommand.Parameters.AddWithValue("@variant_id", variantId);
                await deleteColorCommand.ExecuteNonQueryAsync();


                string insertColorQuery = @"
            INSERT INTO product_variant_colors (variant_id, color_id)
            VALUES (@variant_id, @color_id)
            ON CONFLICT DO NOTHING;
        ";

                await using var colorCommand = new NpgsqlCommand(insertColorQuery, connection, transaction);

                colorCommand.Parameters.AddWithValue("@variant_id", variantId);
                colorCommand.Parameters.AddWithValue("@color_id", colorId);

                await colorCommand.ExecuteNonQueryAsync();


                if (replaceImages)
                {
                    string deleteImagesQuery = @"
                DELETE FROM product_images
                WHERE variant_id = @variant_id;
            ";

                    await using var deleteImagesCommand = new NpgsqlCommand(deleteImagesQuery, connection, transaction);
                    deleteImagesCommand.Parameters.AddWithValue("@variant_id", variantId);
                    await deleteImagesCommand.ExecuteNonQueryAsync();


                    for (int i = 0; i < imagePaths.Count; i++)
                    {
                        string insertImageQuery = @"
                    INSERT INTO product_images (
                        product_id,
                        variant_id,
                        image_path,
                        is_main,
                        sort_order,
                        alt_text
                    )
                    VALUES (
                        @product_id,
                        @variant_id,
                        @image_path,
                        @is_main,
                        @sort_order,
                        @alt_text
                    );
                ";

                        await using var imageCommand = new NpgsqlCommand(insertImageQuery, connection, transaction);

                        imageCommand.Parameters.AddWithValue("@product_id", productId);
                        imageCommand.Parameters.AddWithValue("@variant_id", variantId);
                        imageCommand.Parameters.AddWithValue("@image_path", imagePaths[i]);
                        imageCommand.Parameters.AddWithValue("@is_main", i == 0);
                        imageCommand.Parameters.AddWithValue("@sort_order", i + 1);
                        imageCommand.Parameters.AddWithValue("@alt_text", $"{modelName} фото {i + 1}");

                        await imageCommand.ExecuteNonQueryAsync();
                    }
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task DeleteVariantAsync(int productId, int variantId)
        {
            await using var connection = DatabaseManager.GetConnection();
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Робимо варіант неактивним, але не видаляємо його фізично з бази
                string deleteVariantQuery = @"
            UPDATE product_variants
            SET 
                is_active = FALSE,
                updated_at = CURRENT_TIMESTAMP
            WHERE variant_id = @variant_id;
        ";

                await using var variantCommand = new NpgsqlCommand(deleteVariantQuery, connection, transaction);

                variantCommand.Parameters.AddWithValue("@variant_id", variantId);

                await variantCommand.ExecuteNonQueryAsync();


                // Якщо у товару більше немає активних варіантів,
                // сам товар також робимо неактивним
                string checkProductQuery = @"
            UPDATE products
            SET is_active = FALSE
            WHERE product_id = @product_id
              AND NOT EXISTS (
                  SELECT 1
                  FROM product_variants
                  WHERE product_id = @product_id
                    AND is_active = TRUE
              );
        ";

                await using var productCommand = new NpgsqlCommand(checkProductQuery, connection, transaction);

                productCommand.Parameters.AddWithValue("@product_id", productId);

                await productCommand.ExecuteNonQueryAsync();

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