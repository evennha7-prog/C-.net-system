using System;
using System.Collections.Generic;
using System.Data.Common;

namespace assignment_code.Services.Database
{
    public class MigrationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string> ExecutedSteps { get; } = new List<string>();
        public int TablesCreated { get; set; }
        public int RowsSeeded { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public Exception Error { get; set; }
    }

    public static class DatabaseMigrator
    {
        public static MigrationResult Migrate(bool seedInitialData = true)
        {
            var result = new MigrationResult();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                using (var conn = DbConnectionHelper.CreateConnection())
                {
                    conn.Open();
                    result.ExecutedSteps.Add($"[Connected] Connected to {DbConnectionHelper.ProviderDisplayName} [{conn.Database}] on [{conn.DataSource}]");

                    using (var tx = conn.BeginTransaction())
                    {
                        try
                        {
                            if (DbConnectionHelper.IsSqlServer)
                            {
                                MigrateSqlServer(conn, tx, result, seedInitialData);
                            }
                            else
                            {
                                MigratePostgreSql(conn, tx, result, seedInitialData);
                            }

                            tx.Commit();
                            sw.Stop();
                            result.Success = true;
                            result.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                            result.Message = $"Database migration completed successfully in {result.ElapsedMilliseconds}ms! ({result.TablesCreated} tables verified/created, {result.RowsSeeded} initial records seeded).";
                        }
                        catch
                        {
                            tx.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                result.Success = false;
                result.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                result.Error = ex;
                result.Message = $"Database migration failed: {ex.Message}";
                result.ExecutedSteps.Add($"[Error] {ex.Message}");
            }

            return result;
        }

        #region Microsoft SQL Server Migration

        private static void MigrateSqlServer(DbConnection conn, DbTransaction tx, MigrationResult result, bool seedInitialData)
        {
            // 1. History Table
            CreateMigrationsHistoryTableSqlServer(conn, tx, result);

            // 2. Domain Tables
            CreateUsersTableSqlServer(conn, tx, result);
            CreateCategoriesTableSqlServer(conn, tx, result);
            CreateProductsTableSqlServer(conn, tx, result);
            CreateCustomersTableSqlServer(conn, tx, result);
            CreateOrdersTableSqlServer(conn, tx, result);
            CreateSalesTableSqlServer(conn, tx, result);
            CreateSaleItemsTableSqlServer(conn, tx, result);
            CreateStoreSettingsTableSqlServer(conn, tx, result);

            // 3. Indexes
            CreateIndexesSqlServer(conn, tx, result);

            // 4. Seed Data
            if (seedInitialData)
            {
                SeedInitialDataSqlServer(conn, tx, result);
            }
        }

        private static void CreateMigrationsHistoryTableSqlServer(DbConnection conn, DbTransaction tx, MigrationResult result)
        {
            const string sql = @"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__schema_migrations')
                BEGIN
                    CREATE TABLE __schema_migrations (
                        id INT IDENTITY(1,1) PRIMARY KEY,
                        migration_name NVARCHAR(150) NOT NULL UNIQUE,
                        applied_at DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET()
                    );
                END";

            ExecuteNonQuery(conn, tx, sql);
            result.ExecutedSteps.Add("[Schema] Verified table '__schema_migrations'");
        }

        private static void CreateUsersTableSqlServer(DbConnection conn, DbTransaction tx, MigrationResult result)
        {
            const string sql = @"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'users')
                BEGIN
                    CREATE TABLE users (
                        id NVARCHAR(50) PRIMARY KEY,
                        full_name NVARCHAR(150) NOT NULL,
                        email NVARCHAR(150) NOT NULL UNIQUE,
                        password_hash NVARCHAR(255),
                        role NVARCHAR(50) NOT NULL DEFAULT 'Cashier',
                        status NVARCHAR(50) NOT NULL DEFAULT 'Active',
                        last_login DATETIME2 NULL,
                        created_at DATETIME2 DEFAULT GETDATE()
                    );
                END";

            ExecuteNonQuery(conn, tx, sql);
            RecordMigration(conn, tx, "001_create_users_table");
            result.TablesCreated++;
            result.ExecutedSteps.Add("[Table] Verified/Created 'users'");
        }

        private static void CreateCategoriesTableSqlServer(DbConnection conn, DbTransaction tx, MigrationResult result)
        {
            const string sql = @"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'categories')
                BEGIN
                    CREATE TABLE categories (
                        id NVARCHAR(50) PRIMARY KEY,
                        name NVARCHAR(150) NOT NULL,
                        description NVARCHAR(MAX),
                        product_count INT DEFAULT 0,
                        total_revenue DECIMAL(14,2) DEFAULT 0.00,
                        color_hex NVARCHAR(20) DEFAULT '#3B82F6',
                        created_at DATETIME2 DEFAULT GETDATE()
                    );
                END";

            ExecuteNonQuery(conn, tx, sql);
            RecordMigration(conn, tx, "002_create_categories_table");
            result.TablesCreated++;
            result.ExecutedSteps.Add("[Table] Verified/Created 'categories'");
        }

        private static void CreateProductsTableSqlServer(DbConnection conn, DbTransaction tx, MigrationResult result)
        {
            const string sql = @"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'products')
                BEGIN
                    CREATE TABLE products (
                        id NVARCHAR(50) PRIMARY KEY,
                        sku NVARCHAR(50) NOT NULL UNIQUE,
                        name NVARCHAR(200) NOT NULL,
                        category NVARCHAR(150),
                        price DECIMAL(14,2) NOT NULL DEFAULT 0.00,
                        cost DECIMAL(14,2) NOT NULL DEFAULT 0.00,
                        stock INT NOT NULL DEFAULT 0,
                        status NVARCHAR(50) NOT NULL DEFAULT 'In Stock',
                        date_added NVARCHAR(50),
                        created_at DATETIME2 DEFAULT GETDATE()
                    );
                END";

            ExecuteNonQuery(conn, tx, sql);
            RecordMigration(conn, tx, "003_create_products_table");
            result.TablesCreated++;
            result.ExecutedSteps.Add("[Table] Verified/Created 'products'");
        }

        private static void CreateCustomersTableSqlServer(DbConnection conn, DbTransaction tx, MigrationResult result)
        {
            const string sql = @"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'customers')
                BEGIN
                    CREATE TABLE customers (
                        id NVARCHAR(50) PRIMARY KEY,
                        full_name NVARCHAR(150) NOT NULL,
                        email NVARCHAR(150),
                        phone NVARCHAR(50),
                        total_orders INT DEFAULT 0,
                        total_spent DECIMAL(14,2) DEFAULT 0.00,
                        tier NVARCHAR(50) DEFAULT 'Regular',
                        status NVARCHAR(50) DEFAULT 'Active',
                        created_at DATETIME2 DEFAULT GETDATE()
                    );
                END";

            ExecuteNonQuery(conn, tx, sql);
            RecordMigration(conn, tx, "004_create_customers_table");
            result.TablesCreated++;
            result.ExecutedSteps.Add("[Table] Verified/Created 'customers'");
        }

        private static void CreateOrdersTableSqlServer(DbConnection conn, DbTransaction tx, MigrationResult result)
        {
            const string sql = @"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'orders')
                BEGIN
                    CREATE TABLE orders (
                        order_id NVARCHAR(50) PRIMARY KEY,
                        customer_name NVARCHAR(150) NOT NULL,
                        items_summary NVARCHAR(MAX),
                        total_amount DECIMAL(14,2) NOT NULL DEFAULT 0.00,
                        status NVARCHAR(50) NOT NULL DEFAULT 'Processing',
                        payment_status NVARCHAR(50) NOT NULL DEFAULT 'Paid',
                        order_date DATETIME2 DEFAULT GETDATE(),
                        created_at DATETIME2 DEFAULT GETDATE()
                    );
                END";

            ExecuteNonQuery(conn, tx, sql);
            RecordMigration(conn, tx, "005_create_orders_table");
            result.TablesCreated++;
            result.ExecutedSteps.Add("[Table] Verified/Created 'orders'");
        }

        private static void CreateSalesTableSqlServer(DbConnection conn, DbTransaction tx, MigrationResult result)
        {
            const string sql = @"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'sales')
                BEGIN
                    CREATE TABLE sales (
                        invoice_no NVARCHAR(50) PRIMARY KEY,
                        timestamp DATETIME2 DEFAULT GETDATE(),
                        customer_name NVARCHAR(150) NOT NULL,
                        subtotal DECIMAL(14,2) NOT NULL DEFAULT 0.00,
                        tax_amount DECIMAL(14,2) NOT NULL DEFAULT 0.00,
                        discount_amount DECIMAL(14,2) NOT NULL DEFAULT 0.00,
                        total_amount DECIMAL(14,2) NOT NULL DEFAULT 0.00,
                        payment_method NVARCHAR(50) NOT NULL DEFAULT 'Cash',
                        status NVARCHAR(50) NOT NULL DEFAULT 'Completed',
                        created_at DATETIME2 DEFAULT GETDATE()
                    );
                END";

            ExecuteNonQuery(conn, tx, sql);
            RecordMigration(conn, tx, "006_create_sales_table");
            result.TablesCreated++;
            result.ExecutedSteps.Add("[Table] Verified/Created 'sales'");
        }

        private static void CreateSaleItemsTableSqlServer(DbConnection conn, DbTransaction tx, MigrationResult result)
        {
            const string sql = @"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'sale_items')
                BEGIN
                    CREATE TABLE sale_items (
                        id INT IDENTITY(1,1) PRIMARY KEY,
                        invoice_no NVARCHAR(50) NOT NULL FOREIGN KEY REFERENCES sales(invoice_no) ON DELETE CASCADE,
                        product_id NVARCHAR(50),
                        product_name NVARCHAR(200) NOT NULL,
                        unit_price DECIMAL(14,2) NOT NULL DEFAULT 0.00,
                        quantity INT NOT NULL DEFAULT 1,
                        subtotal DECIMAL(14,2) NOT NULL DEFAULT 0.00
                    );
                END";

            ExecuteNonQuery(conn, tx, sql);
            RecordMigration(conn, tx, "007_create_sale_items_table");
            result.TablesCreated++;
            result.ExecutedSteps.Add("[Table] Verified/Created 'sale_items'");
        }

        private static void CreateStoreSettingsTableSqlServer(DbConnection conn, DbTransaction tx, MigrationResult result)
        {
            const string sql = @"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'store_settings')
                BEGIN
                    CREATE TABLE store_settings (
                        id INT PRIMARY KEY DEFAULT 1,
                        store_name NVARCHAR(150) NOT NULL DEFAULT 'PCCFPI STORE',
                        phone NVARCHAR(50),
                        email NVARCHAR(150),
                        address NVARCHAR(MAX),
                        currency_symbol NVARCHAR(10) DEFAULT '$',
                        tax_rate_percentage DECIMAL(5,2) DEFAULT 8.00,
                        receipt_header NVARCHAR(MAX),
                        receipt_footer NVARCHAR(MAX),
                        auto_print_receipt BIT DEFAULT 1,
                        updated_at DATETIME2 DEFAULT GETDATE()
                    );
                END";

            ExecuteNonQuery(conn, tx, sql);
            RecordMigration(conn, tx, "008_create_store_settings_table");
            result.TablesCreated++;
            result.ExecutedSteps.Add("[Table] Verified/Created 'store_settings'");
        }

        private static void CreateIndexesSqlServer(DbConnection conn, DbTransaction tx, MigrationResult result)
        {
            const string sql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_products_sku')
                    CREATE INDEX idx_products_sku ON products(sku);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_products_category')
                    CREATE INDEX idx_products_category ON products(category);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_users_email')
                    CREATE INDEX idx_users_email ON users(email);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_sales_timestamp')
                    CREATE INDEX idx_sales_timestamp ON sales(timestamp);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_sale_items_invoice')
                    CREATE INDEX idx_sale_items_invoice ON sale_items(invoice_no);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_orders_date')
                    CREATE INDEX idx_orders_date ON orders(order_date);
            ";

            ExecuteNonQuery(conn, tx, sql);
            result.ExecutedSteps.Add("[Indexes] Created performance indexes for SKU, Categories, Users, and Invoices");
        }

        private static void SeedInitialDataSqlServer(DbConnection conn, DbTransaction tx, MigrationResult result)
        {
            // 1. Seed Users
            int userCount = Convert.ToInt32(ExecuteScalar(conn, tx, "SELECT COUNT(*) FROM users;"));
            if (userCount == 0)
            {
                const string seedUsersSql = @"
                    INSERT INTO users (id, full_name, email, password_hash, role, status, last_login) VALUES
                    ('USR-00', 'System Administrator', 'admin@pccfpistore.com', 'admin123', 'Administrator', 'Active', GETDATE()),
                    ('USR-01', 'Stephanie Sharkey', 'stephanie@pccfpistore.com', 'admin123', 'Administrator', 'Active', DATEADD(hour, -1, GETDATE())),
                    ('USR-02', 'Alexander Vance', 'alex@pccfpistore.com', 'manager123', 'Store Manager', 'Active', DATEADD(hour, -3, GETDATE())),
                    ('USR-03', 'Mia Thornton', 'mia.t@pccfpistore.com', 'cashier123', 'Cashier', 'Active', DATEADD(hour, -5, GETDATE())),
                    ('USR-04', 'Jordan Lee', 'jordan@pccfpistore.com', 'cashier123', 'Cashier', 'Active', DATEADD(day, -1, GETDATE())),
                    ('USR-05', 'Carlos Mendez', 'carlos@pccfpistore.com', 'manager123', 'Store Manager', 'Suspended', DATEADD(day, -14, GETDATE()));
                ";
                int inserted = ExecuteNonQuery(conn, tx, seedUsersSql);
                result.RowsSeeded += inserted;
                result.ExecutedSteps.Add($"[Seed] Seeded {inserted} default user accounts.");
            }

            // 2. Seed Categories
            int catCount = Convert.ToInt32(ExecuteScalar(conn, tx, "SELECT COUNT(*) FROM categories;"));
            if (catCount == 0)
            {
                const string seedCatSql = @"
                    INSERT INTO categories (id, name, description, product_count, total_revenue, color_hex) VALUES
                    ('CAT-01', 'Electronics', 'Smart devices, audio & gadgets', 142, 45200.00, '#3B82F6'),
                    ('CAT-02', 'Beverages', 'Artisanal coffee, tea & juices', 86, 12400.00, '#10B981'),
                    ('CAT-03', 'Snacks', 'Healthy organic snacks & confectionery', 115, 18900.00, '#F59E0B'),
                    ('CAT-04', 'Apparel', 'Modern fashion & premium accessories', 98, 28600.00, '#8B5CF6'),
                    ('CAT-05', 'Stationery', 'Eco-friendly office & creative supplies', 64, 8400.00, '#EC4899'),
                    ('CAT-06', 'Home Goods', 'Minimalist decor & lifestyle items', 55, 15300.00, '#06B6D4');
                ";
                int inserted = ExecuteNonQuery(conn, tx, seedCatSql);
                result.RowsSeeded += inserted;
                result.ExecutedSteps.Add($"[Seed] Seeded {inserted} categories.");
            }

            // 3. Seed Products
            int prodCount = Convert.ToInt32(ExecuteScalar(conn, tx, "SELECT COUNT(*) FROM products;"));
            if (prodCount == 0)
            {
                const string seedProdSql = @"
                    INSERT INTO products (id, sku, name, category, price, cost, stock, status, date_added) VALUES
                    ('PRD-01', 'ELE-001', 'Wireless ANC Headphones', 'Electronics', 149.99, 80.00, 45, 'In Stock', '12 Mar 2026'),
                    ('PRD-02', 'ELE-002', 'Smart Fitness Watch V3', 'Electronics', 199.50, 110.00, 28, 'In Stock', '14 Mar 2026'),
                    ('PRD-03', 'ELE-003', 'Ultra-Fast GaN Charger 65W', 'Electronics', 39.99, 16.00, 80, 'In Stock', '15 Mar 2026'),
                    ('PRD-04', 'BEV-001', 'Artisan Cold Brew Coffee 330ml', 'Beverages', 4.50, 1.60, 120, 'In Stock', '16 Mar 2026'),
                    ('PRD-05', 'BEV-002', 'Organic Ceremonial Matcha', 'Beverages', 24.00, 10.50, 18, 'Low Stock', '16 Mar 2026'),
                    ('PRD-06', 'SNK-001', 'Roasted Sea Salt Almonds', 'Snacks', 6.99, 2.80, 65, 'In Stock', '17 Mar 2026'),
                    ('PRD-07', 'SNK-002', 'Dark Chocolate Quinoa Bites', 'Snacks', 5.49, 2.10, 8, 'Low Stock', '17 Mar 2026'),
                    ('PRD-08', 'APP-001', 'Organic Cotton Oversized Tee', 'Apparel', 32.00, 12.00, 50, 'In Stock', '18 Mar 2026'),
                    ('PRD-09', 'APP-002', 'Minimalist Leather Cardholder', 'Apparel', 45.00, 18.00, 0, 'Out of Stock', '18 Mar 2026'),
                    ('PRD-10', 'STA-001', 'Hardcover Dotted Journal A5', 'Stationery', 18.50, 6.00, 40, 'In Stock', '18 Mar 2026');
                ";
                int inserted = ExecuteNonQuery(conn, tx, seedProdSql);
                result.RowsSeeded += inserted;
                result.ExecutedSteps.Add($"[Seed] Seeded {inserted} products.");
            }

            // 4. Seed Customers
            int custCount = Convert.ToInt32(ExecuteScalar(conn, tx, "SELECT COUNT(*) FROM customers;"));
            if (custCount == 0)
            {
                const string seedCustSql = @"
                    INSERT INTO customers (id, full_name, email, phone, total_orders, total_spent, tier, status) VALUES
                    ('CUST-001', 'Sarah Jenkins', 'sarah.j@gmail.com', '+1 555-0192', 18, 1420.50, 'VIP', 'Active'),
                    ('CUST-002', 'David Miller', 'dmiller@outlook.com', '+1 555-0144', 9, 680.00, 'Regular', 'Active'),
                    ('CUST-003', 'Elena Rostova', 'elena.r@techcorp.io', '+1 555-0188', 24, 2890.00, 'VIP', 'Active'),
                    ('CUST-004', 'Marcus Chen', 'mchen@gmail.com', '+1 555-0112', 4, 210.00, 'Regular', 'Active'),
                    ('CUST-005', 'Aisha Khan', 'aisha.k@domain.com', '+1 555-0177', 2, 78.50, 'New', 'Active'),
                    ('CUST-006', 'Lucas Vance', 'lvance@yahoo.com', '+1 555-0131', 1, 45.00, 'New', 'Inactive');
                ";
                int inserted = ExecuteNonQuery(conn, tx, seedCustSql);
                result.RowsSeeded += inserted;
                result.ExecutedSteps.Add($"[Seed] Seeded {inserted} customers.");
            }

            // 5. Seed Orders
            int ordCount = Convert.ToInt32(ExecuteScalar(conn, tx, "SELECT COUNT(*) FROM orders;"));
            if (ordCount == 0)
            {
                const string seedOrdSql = @"
                    INSERT INTO orders (order_id, customer_name, items_summary, total_amount, status, payment_status, order_date) VALUES
                    ('ORD-9821', 'Sarah Jenkins', '2x Wireless ANC Headphones', 299.98, 'Delivered', 'Paid', DATEADD(day, -1, GETDATE())),
                    ('ORD-9822', 'David Miller', '1x Smart Watch, 2x Cold Brew', 208.50, 'Shipped', 'Paid', DATEADD(day, -1, GETDATE())),
                    ('ORD-9823', 'Elena Rostova', '4x Hardcover Dotted Journal', 74.00, 'Processing', 'Paid', DATEADD(hour, -4, GETDATE())),
                    ('ORD-9824', 'Marcus Chen', '1x GaN Charger 65W', 39.99, 'Pending', 'Unpaid', DATEADD(hour, -2, GETDATE())),
                    ('ORD-9825', 'Aisha Khan', '2x Sea Salt Almonds, 1x Tee', 45.98, 'Delivered', 'Paid', DATEADD(day, -3, GETDATE())),
                    ('ORD-9826', 'Lucas Vance', '1x Leather Cardholder', 45.00, 'Cancelled', 'Refunded', DATEADD(day, -5, GETDATE()));
                ";
                int inserted = ExecuteNonQuery(conn, tx, seedOrdSql);
                result.RowsSeeded += inserted;
                result.ExecutedSteps.Add($"[Seed] Seeded {inserted} orders.");
            }

            // 6. Seed Sales & Sale Items
            int salesCount = Convert.ToInt32(ExecuteScalar(conn, tx, "SELECT COUNT(*) FROM sales;"));
            if (salesCount == 0)
            {
                const string seedSalesSql = @"
                    INSERT INTO sales (invoice_no, timestamp, customer_name, subtotal, tax_amount, discount_amount, total_amount, payment_method, status) VALUES
                    ('INV-20260318-001', DATEADD(hour, -6, GETDATE()), 'Sarah Jenkins', 149.99, 12.00, 0.00, 161.99, 'Credit Card', 'Completed'),
                    ('INV-20260318-002', DATEADD(hour, -4, GETDATE()), 'David Miller', 204.00, 16.32, 0.00, 220.32, 'Cash', 'Completed'),
                    ('INV-20260318-003', DATEADD(hour, -2, GETDATE()), 'Walk-in Customer', 32.00, 2.56, 0.00, 34.56, 'ABA QR / KHQR', 'Completed');

                    INSERT INTO sale_items (invoice_no, product_id, product_name, unit_price, quantity, subtotal) VALUES
                    ('INV-20260318-001', 'PRD-01', 'Wireless ANC Headphones', 149.99, 1, 149.99),
                    ('INV-20260318-002', 'PRD-02', 'Smart Fitness Watch V3', 199.50, 1, 199.50),
                    ('INV-20260318-002', 'BEV-001', 'Artisan Cold Brew Coffee 330ml', 4.50, 1, 4.50),
                    ('INV-20260318-003', 'APP-001', 'Organic Cotton Oversized Tee', 32.00, 1, 32.00);
                ";
                int inserted = ExecuteNonQuery(conn, tx, seedSalesSql);
                result.RowsSeeded += inserted;
                result.ExecutedSteps.Add("[Seed] Seeded sales and transaction line items.");
            }

            // 7. Seed Store Settings
            int settingsCount = Convert.ToInt32(ExecuteScalar(conn, tx, "SELECT COUNT(*) FROM store_settings;"));
            if (settingsCount == 0)
            {
                string storeName = EnvLoader.Get("STORE_NAME", "PCCFPI STORE");
                string storePhone = EnvLoader.Get("STORE_PHONE", "+1 (555) 019-2834");
                string storeEmail = EnvLoader.Get("STORE_EMAIL", "support@pccfpistore.com");
                string storeAddr = EnvLoader.Get("STORE_ADDRESS", "100 Retail Boulevard, Suite 400");
                decimal taxRate = EnvLoader.GetDecimal("TAX_RATE_PERCENTAGE", 8.0m);
                string symbol = EnvLoader.Get("CURRENCY_SYMBOL", "$");
                string recHeader = EnvLoader.Get("RECEIPT_HEADER", "THANK YOU FOR SHOPPING AT PCCFPI STORE!");
                string recFooter = EnvLoader.Get("RECEIPT_FOOTER", "Returns accepted within 30 days with receipt.");
                bool autoPrint = EnvLoader.GetBool("AUTO_PRINT_RECEIPT", true);

                const string seedSettingsSql = @"
                    INSERT INTO store_settings (id, store_name, phone, email, address, currency_symbol, tax_rate_percentage, receipt_header, receipt_footer, auto_print_receipt)
                    VALUES (1, @store_name, @phone, @email, @address, @symbol, @tax_rate, @rec_header, @rec_footer, @auto_print);
                ";

                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = seedSettingsSql;
                    cmd.AddParam("@store_name", storeName);
                    cmd.AddParam("@phone", storePhone);
                    cmd.AddParam("@email", storeEmail);
                    cmd.AddParam("@address", storeAddr);
                    cmd.AddParam("@symbol", symbol);
                    cmd.AddParam("@tax_rate", taxRate);
                    cmd.AddParam("@rec_header", recHeader);
                    cmd.AddParam("@rec_footer", recFooter);
                    cmd.AddParam("@auto_print", autoPrint ? 1 : 0);
                    int inserted = cmd.ExecuteNonQuery();
                    result.RowsSeeded += inserted;
                    result.ExecutedSteps.Add("[Seed] Seeded store configuration parameters.");
                }
            }
        }

        #endregion

        #region PostgreSQL Migration

        private static void MigratePostgreSql(DbConnection conn, DbTransaction tx, MigrationResult result, bool seedInitialData)
        {
            // 1. History Table
            CreateMigrationsHistoryTablePostgreSql(conn, tx, result);

            // 2. Domain Tables
            CreateUsersTablePostgreSql(conn, tx, result);
            CreateCategoriesTablePostgreSql(conn, tx, result);
            CreateProductsTablePostgreSql(conn, tx, result);
            CreateCustomersTablePostgreSql(conn, tx, result);
            CreateOrdersTablePostgreSql(conn, tx, result);
            CreateSalesTablePostgreSql(conn, tx, result);
            CreateSaleItemsTablePostgreSql(conn, tx, result);
            CreateStoreSettingsTablePostgreSql(conn, tx, result);

            // 3. Indexes
            CreateIndexesPostgreSql(conn, tx, result);

            // 4. Seed Data
            if (seedInitialData)
            {
                SeedInitialDataPostgreSql(conn, tx, result);
            }
        }

        private static void CreateMigrationsHistoryTablePostgreSql(DbConnection conn, DbTransaction tx, MigrationResult result)
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS __schema_migrations (
                    id SERIAL PRIMARY KEY,
                    migration_name VARCHAR(150) UNIQUE NOT NULL,
                    applied_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
                );";

            ExecuteNonQuery(conn, tx, sql);
            result.ExecutedSteps.Add("[Schema] Verified table '__schema_migrations'");
        }

        private static void CreateUsersTablePostgreSql(DbConnection conn, DbTransaction tx, MigrationResult result)
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS users (
                    id VARCHAR(50) PRIMARY KEY,
                    full_name VARCHAR(150) NOT NULL,
                    email VARCHAR(150) UNIQUE NOT NULL,
                    password_hash VARCHAR(255),
                    role VARCHAR(50) NOT NULL DEFAULT 'Cashier',
                    status VARCHAR(50) NOT NULL DEFAULT 'Active',
                    last_login TIMESTAMP WITH TIME ZONE,
                    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
                );";

            ExecuteNonQuery(conn, tx, sql);
            RecordMigration(conn, tx, "001_create_users_table");
            result.TablesCreated++;
            result.ExecutedSteps.Add("[Table] Verified/Created 'users'");
        }

        private static void CreateCategoriesTablePostgreSql(DbConnection conn, DbTransaction tx, MigrationResult result)
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS categories (
                    id VARCHAR(50) PRIMARY KEY,
                    name VARCHAR(150) NOT NULL,
                    description TEXT,
                    product_count INT DEFAULT 0,
                    total_revenue NUMERIC(14,2) DEFAULT 0.00,
                    color_hex VARCHAR(20) DEFAULT '#3B82F6',
                    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
                );";

            ExecuteNonQuery(conn, tx, sql);
            RecordMigration(conn, tx, "002_create_categories_table");
            result.TablesCreated++;
            result.ExecutedSteps.Add("[Table] Verified/Created 'categories'");
        }

        private static void CreateProductsTablePostgreSql(DbConnection conn, DbTransaction tx, MigrationResult result)
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS products (
                    id VARCHAR(50) PRIMARY KEY,
                    sku VARCHAR(50) UNIQUE NOT NULL,
                    name VARCHAR(200) NOT NULL,
                    category VARCHAR(150),
                    price NUMERIC(14,2) NOT NULL DEFAULT 0.00,
                    cost NUMERIC(14,2) NOT NULL DEFAULT 0.00,
                    stock INT NOT NULL DEFAULT 0,
                    status VARCHAR(50) NOT NULL DEFAULT 'In Stock',
                    date_added VARCHAR(50),
                    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
                );";

            ExecuteNonQuery(conn, tx, sql);
            RecordMigration(conn, tx, "003_create_products_table");
            result.TablesCreated++;
            result.ExecutedSteps.Add("[Table] Verified/Created 'products'");
        }

        private static void CreateCustomersTablePostgreSql(DbConnection conn, DbTransaction tx, MigrationResult result)
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS customers (
                    id VARCHAR(50) PRIMARY KEY,
                    full_name VARCHAR(150) NOT NULL,
                    email VARCHAR(150),
                    phone VARCHAR(50),
                    total_orders INT DEFAULT 0,
                    total_spent NUMERIC(14,2) DEFAULT 0.00,
                    tier VARCHAR(50) DEFAULT 'Regular',
                    status VARCHAR(50) DEFAULT 'Active',
                    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
                );";

            ExecuteNonQuery(conn, tx, sql);
            RecordMigration(conn, tx, "004_create_customers_table");
            result.TablesCreated++;
            result.ExecutedSteps.Add("[Table] Verified/Created 'customers'");
        }

        private static void CreateOrdersTablePostgreSql(DbConnection conn, DbTransaction tx, MigrationResult result)
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS orders (
                    order_id VARCHAR(50) PRIMARY KEY,
                    customer_name VARCHAR(150) NOT NULL,
                    items_summary TEXT,
                    total_amount NUMERIC(14,2) NOT NULL DEFAULT 0.00,
                    status VARCHAR(50) NOT NULL DEFAULT 'Processing',
                    payment_status VARCHAR(50) NOT NULL DEFAULT 'Paid',
                    order_date TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
                    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
                );";

            ExecuteNonQuery(conn, tx, sql);
            RecordMigration(conn, tx, "005_create_orders_table");
            result.TablesCreated++;
            result.ExecutedSteps.Add("[Table] Verified/Created 'orders'");
        }

        private static void CreateSalesTablePostgreSql(DbConnection conn, DbTransaction tx, MigrationResult result)
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS sales (
                    invoice_no VARCHAR(50) PRIMARY KEY,
                    timestamp TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
                    customer_name VARCHAR(150) NOT NULL,
                    subtotal NUMERIC(14,2) NOT NULL DEFAULT 0.00,
                    tax_amount NUMERIC(14,2) NOT NULL DEFAULT 0.00,
                    discount_amount NUMERIC(14,2) NOT NULL DEFAULT 0.00,
                    total_amount NUMERIC(14,2) NOT NULL DEFAULT 0.00,
                    payment_method VARCHAR(50) NOT NULL DEFAULT 'Cash',
                    status VARCHAR(50) NOT NULL DEFAULT 'Completed',
                    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
                );";

            ExecuteNonQuery(conn, tx, sql);
            RecordMigration(conn, tx, "006_create_sales_table");
            result.TablesCreated++;
            result.ExecutedSteps.Add("[Table] Verified/Created 'sales'");
        }

        private static void CreateSaleItemsTablePostgreSql(DbConnection conn, DbTransaction tx, MigrationResult result)
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS sale_items (
                    id SERIAL PRIMARY KEY,
                    invoice_no VARCHAR(50) NOT NULL REFERENCES sales(invoice_no) ON DELETE CASCADE,
                    product_id VARCHAR(50),
                    product_name VARCHAR(200) NOT NULL,
                    unit_price NUMERIC(14,2) NOT NULL DEFAULT 0.00,
                    quantity INT NOT NULL DEFAULT 1,
                    subtotal NUMERIC(14,2) NOT NULL DEFAULT 0.00
                );";

            ExecuteNonQuery(conn, tx, sql);
            RecordMigration(conn, tx, "007_create_sale_items_table");
            result.TablesCreated++;
            result.ExecutedSteps.Add("[Table] Verified/Created 'sale_items'");
        }

        private static void CreateStoreSettingsTablePostgreSql(DbConnection conn, DbTransaction tx, MigrationResult result)
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS store_settings (
                    id INT PRIMARY KEY DEFAULT 1,
                    store_name VARCHAR(150) NOT NULL DEFAULT 'PCCFPI STORE',
                    phone VARCHAR(50),
                    email VARCHAR(150),
                    address TEXT,
                    currency_symbol VARCHAR(10) DEFAULT '$',
                    tax_rate_percentage NUMERIC(5,2) DEFAULT 8.00,
                    receipt_header TEXT,
                    receipt_footer TEXT,
                    auto_print_receipt BOOLEAN DEFAULT true,
                    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
                );";

            ExecuteNonQuery(conn, tx, sql);
            RecordMigration(conn, tx, "008_create_store_settings_table");
            result.TablesCreated++;
            result.ExecutedSteps.Add("[Table] Verified/Created 'store_settings'");
        }

        private static void CreateIndexesPostgreSql(DbConnection conn, DbTransaction tx, MigrationResult result)
        {
            const string sql = @"
                CREATE INDEX IF NOT EXISTS idx_products_sku ON products(sku);
                CREATE INDEX IF NOT EXISTS idx_products_category ON products(category);
                CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
                CREATE INDEX IF NOT EXISTS idx_sales_timestamp ON sales(timestamp);
                CREATE INDEX IF NOT EXISTS idx_sale_items_invoice ON sale_items(invoice_no);
                CREATE INDEX IF NOT EXISTS idx_orders_date ON orders(order_date);
            ";

            ExecuteNonQuery(conn, tx, sql);
            result.ExecutedSteps.Add("[Indexes] Created performance indexes for SKU, Categories, Users, and Invoices");
        }

        private static void SeedInitialDataPostgreSql(DbConnection conn, DbTransaction tx, MigrationResult result)
        {
            // 1. Seed Users
            int userCount = Convert.ToInt32(ExecuteScalar(conn, tx, "SELECT COUNT(*) FROM users;"));
            if (userCount == 0)
            {
                const string seedUsersSql = @"
                    INSERT INTO users (id, full_name, email, password_hash, role, status, last_login) VALUES
                    ('USR-00', 'System Administrator', 'admin@pccfpistore.com', 'admin123', 'Administrator', 'Active', NOW()),
                    ('USR-01', 'Stephanie Sharkey', 'stephanie@pccfpistore.com', 'admin123', 'Administrator', 'Active', NOW() - INTERVAL '1 hour'),
                    ('USR-02', 'Alexander Vance', 'alex@pccfpistore.com', 'manager123', 'Store Manager', 'Active', NOW() - INTERVAL '3 hour'),
                    ('USR-03', 'Mia Thornton', 'mia.t@pccfpistore.com', 'cashier123', 'Cashier', 'Active', NOW() - INTERVAL '5 hour'),
                    ('USR-04', 'Jordan Lee', 'jordan@pccfpistore.com', 'cashier123', 'Cashier', 'Active', NOW() - INTERVAL '1 day'),
                    ('USR-05', 'Carlos Mendez', 'carlos@pccfpistore.com', 'manager123', 'Store Manager', 'Suspended', NOW() - INTERVAL '14 day')
                    ON CONFLICT (id) DO NOTHING;
                ";
                int inserted = ExecuteNonQuery(conn, tx, seedUsersSql);
                result.RowsSeeded += inserted;
                result.ExecutedSteps.Add($"[Seed] Seeded {inserted} default user accounts.");
            }

            // 2. Seed Categories
            int catCount = Convert.ToInt32(ExecuteScalar(conn, tx, "SELECT COUNT(*) FROM categories;"));
            if (catCount == 0)
            {
                const string seedCatSql = @"
                    INSERT INTO categories (id, name, description, product_count, total_revenue, color_hex) VALUES
                    ('CAT-01', 'Electronics', 'Smart devices, audio & gadgets', 142, 45200.00, '#3B82F6'),
                    ('CAT-02', 'Beverages', 'Artisanal coffee, tea & juices', 86, 12400.00, '#10B981'),
                    ('CAT-03', 'Snacks', 'Healthy organic snacks & confectionery', 115, 18900.00, '#F59E0B'),
                    ('CAT-04', 'Apparel', 'Modern fashion & premium accessories', 98, 28600.00, '#8B5CF6'),
                    ('CAT-05', 'Stationery', 'Eco-friendly office & creative supplies', 64, 8400.00, '#EC4899'),
                    ('CAT-06', 'Home Goods', 'Minimalist decor & lifestyle items', 55, 15300.00, '#06B6D4')
                    ON CONFLICT (id) DO NOTHING;
                ";
                int inserted = ExecuteNonQuery(conn, tx, seedCatSql);
                result.RowsSeeded += inserted;
                result.ExecutedSteps.Add($"[Seed] Seeded {inserted} categories.");
            }

            // 3. Seed Products
            int prodCount = Convert.ToInt32(ExecuteScalar(conn, tx, "SELECT COUNT(*) FROM products;"));
            if (prodCount == 0)
            {
                const string seedProdSql = @"
                    INSERT INTO products (id, sku, name, category, price, cost, stock, status, date_added) VALUES
                    ('PRD-01', 'ELE-001', 'Wireless ANC Headphones', 'Electronics', 149.99, 80.00, 45, 'In Stock', '12 Mar 2026'),
                    ('PRD-02', 'ELE-002', 'Smart Fitness Watch V3', 'Electronics', 199.50, 110.00, 28, 'In Stock', '14 Mar 2026'),
                    ('PRD-03', 'ELE-003', 'Ultra-Fast GaN Charger 65W', 'Electronics', 39.99, 16.00, 80, 'In Stock', '15 Mar 2026'),
                    ('PRD-04', 'BEV-001', 'Artisan Cold Brew Coffee 330ml', 'Beverages', 4.50, 1.60, 120, 'In Stock', '16 Mar 2026'),
                    ('PRD-05', 'BEV-002', 'Organic Ceremonial Matcha', 'Beverages', 24.00, 10.50, 18, 'Low Stock', '16 Mar 2026'),
                    ('PRD-06', 'SNK-001', 'Roasted Sea Salt Almonds', 'Snacks', 6.99, 2.80, 65, 'In Stock', '17 Mar 2026'),
                    ('PRD-07', 'SNK-002', 'Dark Chocolate Quinoa Bites', 'Snacks', 5.49, 2.10, 8, 'Low Stock', '17 Mar 2026'),
                    ('PRD-08', 'APP-001', 'Organic Cotton Oversized Tee', 'Apparel', 32.00, 12.00, 50, 'In Stock', '18 Mar 2026'),
                    ('PRD-09', 'APP-002', 'Minimalist Leather Cardholder', 'Apparel', 45.00, 18.00, 0, 'Out of Stock', '18 Mar 2026'),
                    ('PRD-10', 'STA-001', 'Hardcover Dotted Journal A5', 'Stationery', 18.50, 6.00, 40, 'In Stock', '18 Mar 2026')
                    ON CONFLICT (id) DO NOTHING;
                ";
                int inserted = ExecuteNonQuery(conn, tx, seedProdSql);
                result.RowsSeeded += inserted;
                result.ExecutedSteps.Add($"[Seed] Seeded {inserted} products.");
            }

            // 4. Seed Customers
            int custCount = Convert.ToInt32(ExecuteScalar(conn, tx, "SELECT COUNT(*) FROM customers;"));
            if (custCount == 0)
            {
                const string seedCustSql = @"
                    INSERT INTO customers (id, full_name, email, phone, total_orders, total_spent, tier, status) VALUES
                    ('CUST-001', 'Sarah Jenkins', 'sarah.j@gmail.com', '+1 555-0192', 18, 1420.50, 'VIP', 'Active'),
                    ('CUST-002', 'David Miller', 'dmiller@outlook.com', '+1 555-0144', 9, 680.00, 'Regular', 'Active'),
                    ('CUST-003', 'Elena Rostova', 'elena.r@techcorp.io', '+1 555-0188', 24, 2890.00, 'VIP', 'Active'),
                    ('CUST-004', 'Marcus Chen', 'mchen@gmail.com', '+1 555-0112', 4, 210.00, 'Regular', 'Active'),
                    ('CUST-005', 'Aisha Khan', 'aisha.k@domain.com', '+1 555-0177', 2, 78.50, 'New', 'Active'),
                    ('CUST-006', 'Lucas Vance', 'lvance@yahoo.com', '+1 555-0131', 1, 45.00, 'New', 'Inactive')
                    ON CONFLICT (id) DO NOTHING;
                ";
                int inserted = ExecuteNonQuery(conn, tx, seedCustSql);
                result.RowsSeeded += inserted;
                result.ExecutedSteps.Add($"[Seed] Seeded {inserted} customers.");
            }

            // 5. Seed Orders
            int ordCount = Convert.ToInt32(ExecuteScalar(conn, tx, "SELECT COUNT(*) FROM orders;"));
            if (ordCount == 0)
            {
                const string seedOrdSql = @"
                    INSERT INTO orders (order_id, customer_name, items_summary, total_amount, status, payment_status, order_date) VALUES
                    ('ORD-9821', 'Sarah Jenkins', '2x Wireless ANC Headphones', 299.98, 'Delivered', 'Paid', NOW() - INTERVAL '1 day'),
                    ('ORD-9822', 'David Miller', '1x Smart Watch, 2x Cold Brew', 208.50, 'Shipped', 'Paid', NOW() - INTERVAL '1 day'),
                    ('ORD-9823', 'Elena Rostova', '4x Hardcover Dotted Journal', 74.00, 'Processing', 'Paid', NOW() - INTERVAL '4 hour'),
                    ('ORD-9824', 'Marcus Chen', '1x GaN Charger 65W', 39.99, 'Pending', 'Unpaid', NOW() - INTERVAL '2 hour'),
                    ('ORD-9825', 'Aisha Khan', '2x Sea Salt Almonds, 1x Tee', 45.98, 'Delivered', 'Paid', NOW() - INTERVAL '3 day'),
                    ('ORD-9826', 'Lucas Vance', '1x Leather Cardholder', 45.00, 'Cancelled', 'Refunded', NOW() - INTERVAL '5 day')
                    ON CONFLICT (order_id) DO NOTHING;
                ";
                int inserted = ExecuteNonQuery(conn, tx, seedOrdSql);
                result.RowsSeeded += inserted;
                result.ExecutedSteps.Add($"[Seed] Seeded {inserted} orders.");
            }

            // 6. Seed Sales & Sale Items
            int salesCount = Convert.ToInt32(ExecuteScalar(conn, tx, "SELECT COUNT(*) FROM sales;"));
            if (salesCount == 0)
            {
                const string seedSalesSql = @"
                    INSERT INTO sales (invoice_no, timestamp, customer_name, subtotal, tax_amount, discount_amount, total_amount, payment_method, status) VALUES
                    ('INV-20260318-001', NOW() - INTERVAL '6 hour', 'Sarah Jenkins', 149.99, 12.00, 0.00, 161.99, 'Credit Card', 'Completed'),
                    ('INV-20260318-002', NOW() - INTERVAL '4 hour', 'David Miller', 204.00, 16.32, 0.00, 220.32, 'Cash', 'Completed'),
                    ('INV-20260318-003', NOW() - INTERVAL '2 hour', 'Walk-in Customer', 32.00, 2.56, 0.00, 34.56, 'ABA QR / KHQR', 'Completed')
                    ON CONFLICT (invoice_no) DO NOTHING;

                    INSERT INTO sale_items (invoice_no, product_id, product_name, unit_price, quantity, subtotal) VALUES
                    ('INV-20260318-001', 'PRD-01', 'Wireless ANC Headphones', 149.99, 1, 149.99),
                    ('INV-20260318-002', 'PRD-02', 'Smart Fitness Watch V3', 199.50, 1, 199.50),
                    ('INV-20260318-002', 'BEV-001', 'Artisan Cold Brew Coffee 330ml', 4.50, 1, 4.50),
                    ('INV-20260318-003', 'APP-001', 'Organic Cotton Oversized Tee', 32.00, 1, 32.00)
                    ON CONFLICT DO NOTHING;
                ";
                int inserted = ExecuteNonQuery(conn, tx, seedSalesSql);
                result.RowsSeeded += inserted;
                result.ExecutedSteps.Add("[Seed] Seeded sales and transaction line items.");
            }

            // 7. Seed Store Settings
            int settingsCount = Convert.ToInt32(ExecuteScalar(conn, tx, "SELECT COUNT(*) FROM store_settings;"));
            if (settingsCount == 0)
            {
                string storeName = EnvLoader.Get("STORE_NAME", "PCCFPI STORE");
                string storePhone = EnvLoader.Get("STORE_PHONE", "+1 (555) 019-2834");
                string storeEmail = EnvLoader.Get("STORE_EMAIL", "support@pccfpistore.com");
                string storeAddr = EnvLoader.Get("STORE_ADDRESS", "100 Retail Boulevard, Suite 400");
                decimal taxRate = EnvLoader.GetDecimal("TAX_RATE_PERCENTAGE", 8.0m);
                string symbol = EnvLoader.Get("CURRENCY_SYMBOL", "$");
                string recHeader = EnvLoader.Get("RECEIPT_HEADER", "THANK YOU FOR SHOPPING AT PCCFPI STORE!");
                string recFooter = EnvLoader.Get("RECEIPT_FOOTER", "Returns accepted within 30 days with receipt.");
                bool autoPrint = EnvLoader.GetBool("AUTO_PRINT_RECEIPT", true);

                const string seedSettingsSql = @"
                    INSERT INTO store_settings (id, store_name, phone, email, address, currency_symbol, tax_rate_percentage, receipt_header, receipt_footer, auto_print_receipt)
                    VALUES (1, @store_name, @phone, @email, @address, @symbol, @tax_rate, @rec_header, @rec_footer, @auto_print)
                    ON CONFLICT (id) DO NOTHING;
                ";

                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = seedSettingsSql;
                    cmd.AddParam("@store_name", storeName);
                    cmd.AddParam("@phone", storePhone);
                    cmd.AddParam("@email", storeEmail);
                    cmd.AddParam("@address", storeAddr);
                    cmd.AddParam("@symbol", symbol);
                    cmd.AddParam("@tax_rate", taxRate);
                    cmd.AddParam("@rec_header", recHeader);
                    cmd.AddParam("@rec_footer", recFooter);
                    cmd.AddParam("@auto_print", autoPrint);
                    int inserted = cmd.ExecuteNonQuery();
                    result.RowsSeeded += inserted;
                    result.ExecutedSteps.Add("[Seed] Seeded store configuration parameters.");
                }
            }
        }

        #endregion

        private static void RecordMigration(DbConnection conn, DbTransaction tx, string migrationName)
        {
            string sql;
            if (DbConnectionHelper.IsSqlServer)
            {
                sql = @"
                    IF NOT EXISTS (SELECT 1 FROM __schema_migrations WHERE migration_name = @name)
                        INSERT INTO __schema_migrations (migration_name) VALUES (@name);";
            }
            else
            {
                sql = "INSERT INTO __schema_migrations (migration_name) VALUES (@name) ON CONFLICT (migration_name) DO NOTHING;";
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = sql;
                cmd.AddParam("@name", migrationName);
                cmd.ExecuteNonQuery();
            }
        }

        private static int ExecuteNonQuery(DbConnection conn, DbTransaction tx, string sql)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = sql;
                return cmd.ExecuteNonQuery();
            }
        }

        private static object ExecuteScalar(DbConnection conn, DbTransaction tx, string sql)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = sql;
                return cmd.ExecuteScalar();
            }
        }
    }
}
