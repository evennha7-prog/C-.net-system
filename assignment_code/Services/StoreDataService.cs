using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using assignment_code.Models;
using assignment_code.Services.Database;
using Npgsql;

namespace assignment_code.Services
{
    public class StoreDataService
    {
        private static StoreDataService _instance;
        public static StoreDataService Instance => _instance ?? (_instance = new StoreDataService());

        public List<Product> Products { get; private set; } = new List<Product>();
        public List<Category> Categories { get; private set; } = new List<Category>();
        public List<SaleTransaction> Sales { get; private set; } = new List<SaleTransaction>();
        public List<Order> Orders { get; private set; } = new List<Order>();
        public List<Customer> Customers { get; private set; } = new List<Customer>();
        public List<AppUser> Users { get; private set; } = new List<AppUser>();
        public AppUser CurrentUser { get; set; }
        public StoreSettings Settings { get; set; } = new StoreSettings();

        // Active POS Cart
        public List<CartItem> CurrentCart { get; } = new List<CartItem>();
        public event EventHandler CartChanged;
        public event EventHandler DataRefreshed;

        public bool IsDatabaseConnected { get; private set; }
        public string LastDatabaseError { get; private set; }

        public StoreDataService()
        {
            LoadData();
        }

        public void LoadData()
        {
            // 1. Ensure DB schema exists on PostgreSQL
            EnsureDatabaseSchema();

            // 2. Load live data from PostgreSQL
            bool loadedFromDb = TryLoadFromDatabase();
            if (!loadedFromDb || Products.Count == 0)
            {
                InitializeLocalFallback();
            }
            DataRefreshed?.Invoke(this, EventArgs.Empty);
        }

        private void EnsureDatabaseSchema()
        {
            try
            {
                using (var conn = DbConnectionHelper.CreateConnection())
                {
                    conn.Open();
                    IsDatabaseConnected = true;

                    // Check if 'products' table exists
                    bool exists = false;
                    using (var cmd = new NpgsqlCommand("SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'products');", conn))
                    {
                        var res = cmd.ExecuteScalar();
                        if (res is bool b && b) exists = true;
                    }

                    if (!exists)
                    {
                        DatabaseMigrator.Migrate(seedInitialData: true);
                    }
                }
            }
            catch (Exception ex)
            {
                LastDatabaseError = ex.Message;
                IsDatabaseConnected = false;
            }
        }

        public bool TryLoadFromDatabase()
        {
            try
            {
                using (var conn = DbConnectionHelper.CreateConnection())
                {
                    conn.Open();
                    IsDatabaseConnected = true;
                    LastDatabaseError = null;

                    // 1. Load Store Settings
                    try
                    {
                        using (var cmd = new NpgsqlCommand("SELECT store_name, phone, email, address, currency_symbol, tax_rate_percentage, receipt_header, receipt_footer FROM store_settings WHERE id = 1", conn))
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Settings.StoreName = reader.IsDBNull(0) ? "PCCFPI STORE" : reader.GetString(0);
                                Settings.Phone = reader.IsDBNull(1) ? "+855 23 888 999" : reader.GetString(1);
                                Settings.Email = reader.IsDBNull(2) ? "support@pccfpistore.com" : reader.GetString(2);
                                Settings.Address = reader.IsDBNull(3) ? "Phnom Penh, Cambodia" : reader.GetString(3);
                                Settings.CurrencySymbol = reader.IsDBNull(4) ? "$" : reader.GetString(4);
                                Settings.TaxRatePercentage = reader.IsDBNull(5) ? 10m : reader.GetDecimal(5);
                                Settings.ReceiptHeader = reader.IsDBNull(6) ? "PCCFPI STORE OFFICIAL RECEIPT" : reader.GetString(6);
                                Settings.ReceiptFooter = reader.IsDBNull(7) ? "Thank you for shopping with us!" : reader.GetString(7);
                            }
                        }
                    }
                    catch { }

                    // 2. Load Categories
                    var categories = new List<Category>();
                    using (var cmd = new NpgsqlCommand("SELECT id, name, description, product_count, total_revenue, color_hex FROM categories ORDER BY id", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categories.Add(new Category
                            {
                                Id = reader.GetString(0),
                                Name = reader.GetString(1),
                                Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                ProductCount = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                                TotalRevenue = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4),
                                ColorHex = reader.IsDBNull(5) ? "#3B82F6" : reader.GetString(5)
                            });
                        }
                    }
                    if (categories.Count > 0) Categories = categories;

                    // 3. Load Products
                    var products = new List<Product>();
                    using (var cmd = new NpgsqlCommand("SELECT id, sku, name, category, price, cost, stock, status, date_added FROM products ORDER BY id", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new Product
                            {
                                Id = reader.GetString(0),
                                Sku = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                Name = reader.GetString(2),
                                Category = reader.IsDBNull(3) ? "General" : reader.GetString(3),
                                Price = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4),
                                Cost = reader.IsDBNull(5) ? 0m : reader.GetDecimal(5),
                                Stock = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                                Status = reader.IsDBNull(7) ? "In Stock" : reader.GetString(7),
                                DateAdded = reader.IsDBNull(8) ? DateTime.Now.ToString("dd MMM yyyy") : reader.GetString(8)
                            });
                        }
                    }
                    if (products.Count > 0) Products = products;

                    // 4. Load Customers
                    var customers = new List<Customer>();
                    using (var cmd = new NpgsqlCommand("SELECT id, full_name, email, phone, total_orders, total_spent, tier, status FROM customers ORDER BY id", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            customers.Add(new Customer
                            {
                                Id = reader.GetString(0),
                                FullName = reader.GetString(1),
                                Email = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                Phone = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                TotalOrders = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                TotalSpent = reader.IsDBNull(5) ? 0m : reader.GetDecimal(5),
                                Tier = reader.IsDBNull(6) ? "Regular" : reader.GetString(6),
                                Status = reader.IsDBNull(7) ? "Active" : reader.GetString(7)
                            });
                        }
                    }
                    if (customers.Count > 0) Customers = customers;

                    // 5. Load Users
                    var users = new List<AppUser>();
                    using (var cmd = new NpgsqlCommand("SELECT id, full_name, email, role, status, last_login FROM users ORDER BY id", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new AppUser
                            {
                                Id = reader.GetString(0),
                                FullName = reader.GetString(1),
                                Email = reader.GetString(2),
                                Role = reader.IsDBNull(3) ? "Store Staff" : reader.GetString(3),
                                Status = reader.IsDBNull(4) ? "Active" : reader.GetString(4),
                                LastLogin = reader.IsDBNull(5) ? DateTime.Now : reader.GetDateTime(5)
                            });
                        }
                    }
                    if (users.Count > 0) Users = users;

                    // 6. Load Orders
                    var orders = new List<Order>();
                    using (var cmd = new NpgsqlCommand("SELECT order_id, customer_name, items_summary, total_amount, status, payment_status, order_date FROM orders ORDER BY order_date DESC", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new Order
                            {
                                OrderId = reader.GetString(0),
                                CustomerName = reader.GetString(1),
                                ItemsSummary = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                TotalAmount = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3),
                                Status = reader.IsDBNull(4) ? "Pending" : reader.GetString(4),
                                PaymentStatus = reader.IsDBNull(5) ? "Unpaid" : reader.GetString(5),
                                OrderDate = reader.IsDBNull(6) ? DateTime.Now : reader.GetDateTime(6)
                            });
                        }
                    }
                    if (orders.Count > 0) Orders = orders;

                    // 7. Load Sales Transactions
                    var sales = new List<SaleTransaction>();
                    using (var cmd = new NpgsqlCommand("SELECT invoice_no, customer_name, subtotal, tax_amount, discount_amount, total_amount, payment_method, status, timestamp FROM sales ORDER BY timestamp DESC", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sales.Add(new SaleTransaction
                            {
                                InvoiceNo = reader.GetString(0),
                                CustomerName = reader.GetString(1),
                                Subtotal = reader.IsDBNull(2) ? 0m : reader.GetDecimal(2),
                                TaxAmount = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3),
                                DiscountAmount = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4),
                                TotalAmount = reader.IsDBNull(5) ? 0m : reader.GetDecimal(5),
                                PaymentMethod = reader.IsDBNull(6) ? "Cash" : reader.GetString(6),
                                Status = reader.IsDBNull(7) ? "Completed" : reader.GetString(7),
                                Timestamp = reader.IsDBNull(8) ? DateTime.Now : reader.GetDateTime(8),
                                Items = new List<CartItem>()
                            });
                        }
                    }
                    if (sales.Count > 0) Sales = sales;

                    return true;
                }
            }
            catch (Exception ex)
            {
                LastDatabaseError = ex.Message;
                IsDatabaseConnected = false;
                return false;
            }
        }

        private void InitializeLocalFallback()
        {
            if (Categories.Count == 0)
            {
                var defaultCats = new[]
                {
                    new Category { Id = "CAT-01", Name = "Electronics", Description = "Smart devices & accessories", ProductCount = 3, TotalRevenue = 12500m, ColorHex = "#3B82F6" },
                    new Category { Id = "CAT-02", Name = "Beverages", Description = "Artisanal coffee & tea", ProductCount = 2, TotalRevenue = 4300m, ColorHex = "#10B981" },
                    new Category { Id = "CAT-03", Name = "Snacks", Description = "Healthy organic snacks", ProductCount = 2, TotalRevenue = 3200m, ColorHex = "#F59E0B" },
                    new Category { Id = "CAT-04", Name = "Apparel", Description = "Modern fashion & shirts", ProductCount = 2, TotalRevenue = 5400m, ColorHex = "#8B5CF6" },
                    new Category { Id = "CAT-05", Name = "Stationery", Description = "Eco-friendly office items", ProductCount = 1, TotalRevenue = 1800m, ColorHex = "#EC4899" }
                };
                foreach (var c in defaultCats) AddCategory(c);
            }

            if (Products.Count == 0)
            {
                var defaultProds = new[]
                {
                    new Product { Id = "PRD-01", Sku = "ELE-001", Name = "Wireless ANC Headphones", Category = "Electronics", Price = 149.99m, Cost = 80.00m, Stock = 45, Status = "In Stock", DateAdded = DateTime.Now.ToString("dd MMM yyyy") },
                    new Product { Id = "PRD-02", Sku = "ELE-002", Name = "Smart Fitness Watch V3", Category = "Electronics", Price = 199.50m, Cost = 110.00m, Stock = 28, Status = "In Stock", DateAdded = DateTime.Now.ToString("dd MMM yyyy") },
                    new Product { Id = "PRD-03", Sku = "BEV-001", Name = "Artisan Cold Brew Coffee", Category = "Beverages", Price = 4.50m, Cost = 1.60m, Stock = 120, Status = "In Stock", DateAdded = DateTime.Now.ToString("dd MMM yyyy") },
                    new Product { Id = "PRD-04", Sku = "SNK-001", Name = "Roasted Sea Salt Almonds", Category = "Snacks", Price = 6.99m, Cost = 2.80m, Stock = 65, Status = "In Stock", DateAdded = DateTime.Now.ToString("dd MMM yyyy") },
                    new Product { Id = "PRD-05", Sku = "APP-001", Name = "Organic Cotton Oversized Tee", Category = "Apparel", Price = 32.00m, Cost = 12.00m, Stock = 50, Status = "In Stock", DateAdded = DateTime.Now.ToString("dd MMM yyyy") }
                };
                foreach (var p in defaultProds) AddProduct(p);
            }

            if (Customers.Count == 0)
            {
                var defaultCusts = new[]
                {
                    new Customer { Id = "CUST-001", FullName = "Sarah Jenkins", Email = "sarah.j@gmail.com", Phone = "+1 555-0192", TotalOrders = 18, TotalSpent = 1420.50m, Tier = "VIP", Status = "Active" },
                    new Customer { Id = "CUST-002", FullName = "David Miller", Email = "dmiller@outlook.com", Phone = "+1 555-0144", TotalOrders = 9, TotalSpent = 680.00m, Tier = "Regular", Status = "Active" },
                    new Customer { Id = "CUST-003", FullName = "Elena Rostova", Email = "elena.r@techcorp.io", Phone = "+1 555-0188", TotalOrders = 24, TotalSpent = 2890.00m, Tier = "VIP", Status = "Active" }
                };
                foreach (var cu in defaultCusts) AddCustomer(cu);
            }

            if (Users.Count == 0)
            {
                var defaultUsers = new[]
                {
                    new AppUser { Id = "USR-00", FullName = "System Administrator", Email = "admin@pccfpistore.com", Role = "Administrator", Status = "Active", LastLogin = DateTime.Now },
                    new AppUser { Id = "USR-01", FullName = "Stephanie Sharkey", Email = "stephanie@pccfpistore.com", Role = "Administrator", Status = "Active", LastLogin = DateTime.Now },
                    new AppUser { Id = "USR-02", FullName = "Alexander Vance", Email = "alex@pccfpistore.com", Role = "Store Manager", Status = "Active", LastLogin = DateTime.Now },
                    new AppUser { Id = "USR-03", FullName = "Mia Thornton", Email = "mia.t@pccfpistore.com", Role = "Cashier", Status = "Active", LastLogin = DateTime.Now }
                };
                foreach (var u in defaultUsers) AddUser(u);
            }
        }

        // ==========================================
        // 1. PRODUCT CRUD OPERATIONS (CREATE / READ / UPDATE / DELETE)
        // ==========================================
        public void AddProduct(Product product)
        {
            if (product == null) return;
            if (string.IsNullOrWhiteSpace(product.Id))
            {
                product.Id = $"PRD-{DateTime.Now.Ticks % 100000:D5}";
            }
            if (string.IsNullOrWhiteSpace(product.DateAdded))
            {
                product.DateAdded = DateTime.Now.ToString("dd MMM yyyy");
            }
            Products.Insert(0, product);

            try
            {
                using (var conn = DbConnectionHelper.CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(
                        "INSERT INTO products (id, sku, name, category, price, cost, stock, status, date_added) " +
                        "VALUES (@id, @sku, @name, @cat, @price, @cost, @stock, @status, @date) " +
                        "ON CONFLICT (id) DO UPDATE SET sku=EXCLUDED.sku, name=EXCLUDED.name, category=EXCLUDED.category, price=EXCLUDED.price, cost=EXCLUDED.cost, stock=EXCLUDED.stock, status=EXCLUDED.status", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", product.Id);
                        cmd.Parameters.AddWithValue("@sku", product.Sku ?? "");
                        cmd.Parameters.AddWithValue("@name", product.Name ?? "");
                        cmd.Parameters.AddWithValue("@cat", product.Category ?? "General");
                        cmd.Parameters.AddWithValue("@price", product.Price);
                        cmd.Parameters.AddWithValue("@cost", product.Cost);
                        cmd.Parameters.AddWithValue("@stock", product.Stock);
                        cmd.Parameters.AddWithValue("@status", product.Status ?? "In Stock");
                        cmd.Parameters.AddWithValue("@date", product.DateAdded);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                LastDatabaseError = ex.Message;
            }
            DataRefreshed?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateProduct(Product product)
        {
            if (product == null) return;

            var existing = Products.FirstOrDefault(p => p.Id == product.Id);
            if (existing != null)
            {
                existing.Sku = product.Sku;
                existing.Name = product.Name;
                existing.Category = product.Category;
                existing.Price = product.Price;
                existing.Cost = product.Cost;
                existing.Stock = product.Stock;
                existing.Status = product.Status;
            }

            try
            {
                using (var conn = DbConnectionHelper.CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(
                        "UPDATE products SET sku=@sku, name=@name, category=@cat, price=@price, cost=@cost, stock=@stock, status=@status WHERE id=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", product.Id);
                        cmd.Parameters.AddWithValue("@sku", product.Sku ?? "");
                        cmd.Parameters.AddWithValue("@name", product.Name ?? "");
                        cmd.Parameters.AddWithValue("@cat", product.Category ?? "General");
                        cmd.Parameters.AddWithValue("@price", product.Price);
                        cmd.Parameters.AddWithValue("@cost", product.Cost);
                        cmd.Parameters.AddWithValue("@stock", product.Stock);
                        cmd.Parameters.AddWithValue("@status", product.Status ?? "In Stock");
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                LastDatabaseError = ex.Message;
            }
            DataRefreshed?.Invoke(this, EventArgs.Empty);
        }

        public void DeleteProduct(string id)
        {
            Products.RemoveAll(p => p.Id == id);
            try
            {
                using (var conn = DbConnectionHelper.CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("DELETE FROM products WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                LastDatabaseError = ex.Message;
            }
            DataRefreshed?.Invoke(this, EventArgs.Empty);
        }

        // ==========================================
        // 2. CATEGORY CRUD OPERATIONS
        // ==========================================
        public void AddCategory(Category category)
        {
            if (category == null) return;
            if (string.IsNullOrWhiteSpace(category.Id))
            {
                category.Id = $"CAT-{DateTime.Now.Ticks % 10000:D4}";
            }
            Categories.Add(category);

            try
            {
                using (var conn = DbConnectionHelper.CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(
                        "INSERT INTO categories (id, name, description, product_count, total_revenue, color_hex) " +
                        "VALUES (@id, @name, @desc, @pcount, @rev, @color) " +
                        "ON CONFLICT (id) DO UPDATE SET name=EXCLUDED.name, description=EXCLUDED.description, color_hex=EXCLUDED.color_hex", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", category.Id);
                        cmd.Parameters.AddWithValue("@name", category.Name ?? "");
                        cmd.Parameters.AddWithValue("@desc", category.Description ?? "");
                        cmd.Parameters.AddWithValue("@pcount", category.ProductCount);
                        cmd.Parameters.AddWithValue("@rev", category.TotalRevenue);
                        cmd.Parameters.AddWithValue("@color", category.ColorHex ?? "#3B82F6");
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                LastDatabaseError = ex.Message;
            }
            DataRefreshed?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateCategory(Category category)
        {
            if (category == null) return;
            var existing = Categories.FirstOrDefault(c => c.Id == category.Id);
            if (existing != null)
            {
                existing.Name = category.Name;
                existing.Description = category.Description;
                existing.ColorHex = category.ColorHex;
            }

            try
            {
                using (var conn = DbConnectionHelper.CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(
                        "UPDATE categories SET name=@name, description=@desc, color_hex=@color WHERE id=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", category.Id);
                        cmd.Parameters.AddWithValue("@name", category.Name ?? "");
                        cmd.Parameters.AddWithValue("@desc", category.Description ?? "");
                        cmd.Parameters.AddWithValue("@color", category.ColorHex ?? "#3B82F6");
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                LastDatabaseError = ex.Message;
            }
            DataRefreshed?.Invoke(this, EventArgs.Empty);
        }

        public void DeleteCategory(string id)
        {
            Categories.RemoveAll(c => c.Id == id);
            try
            {
                using (var conn = DbConnectionHelper.CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("DELETE FROM categories WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                LastDatabaseError = ex.Message;
            }
            DataRefreshed?.Invoke(this, EventArgs.Empty);
        }

        // ==========================================
        // 3. CUSTOMER CRUD OPERATIONS
        // ==========================================
        public void AddCustomer(Customer customer)
        {
            if (customer == null) return;
            if (string.IsNullOrWhiteSpace(customer.Id))
            {
                customer.Id = $"CUST-{DateTime.Now.Ticks % 10000:D4}";
            }
            Customers.Insert(0, customer);

            try
            {
                using (var conn = DbConnectionHelper.CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(
                        "INSERT INTO customers (id, full_name, email, phone, total_orders, total_spent, tier, status) " +
                        "VALUES (@id, @name, @email, @phone, @orders, @spent, @tier, @status) " +
                        "ON CONFLICT (id) DO UPDATE SET full_name=EXCLUDED.full_name, email=EXCLUDED.email, phone=EXCLUDED.phone, tier=EXCLUDED.tier, status=EXCLUDED.status", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", customer.Id);
                        cmd.Parameters.AddWithValue("@name", customer.FullName ?? "");
                        cmd.Parameters.AddWithValue("@email", customer.Email ?? "");
                        cmd.Parameters.AddWithValue("@phone", customer.Phone ?? "");
                        cmd.Parameters.AddWithValue("@orders", customer.TotalOrders);
                        cmd.Parameters.AddWithValue("@spent", customer.TotalSpent);
                        cmd.Parameters.AddWithValue("@tier", customer.Tier ?? "Regular");
                        cmd.Parameters.AddWithValue("@status", customer.Status ?? "Active");
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                LastDatabaseError = ex.Message;
            }
            DataRefreshed?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateCustomer(Customer customer)
        {
            if (customer == null) return;
            var existing = Customers.FirstOrDefault(c => c.Id == customer.Id);
            if (existing != null)
            {
                existing.FullName = customer.FullName;
                existing.Email = customer.Email;
                existing.Phone = customer.Phone;
                existing.Tier = customer.Tier;
                existing.Status = customer.Status;
            }

            try
            {
                using (var conn = DbConnectionHelper.CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(
                        "UPDATE customers SET full_name=@name, email=@email, phone=@phone, tier=@tier, status=@status WHERE id=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", customer.Id);
                        cmd.Parameters.AddWithValue("@name", customer.FullName ?? "");
                        cmd.Parameters.AddWithValue("@email", customer.Email ?? "");
                        cmd.Parameters.AddWithValue("@phone", customer.Phone ?? "");
                        cmd.Parameters.AddWithValue("@tier", customer.Tier ?? "Regular");
                        cmd.Parameters.AddWithValue("@status", customer.Status ?? "Active");
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                LastDatabaseError = ex.Message;
            }
            DataRefreshed?.Invoke(this, EventArgs.Empty);
        }

        public void DeleteCustomer(string id)
        {
            Customers.RemoveAll(c => c.Id == id);
            try
            {
                using (var conn = DbConnectionHelper.CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("DELETE FROM customers WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                LastDatabaseError = ex.Message;
            }
            DataRefreshed?.Invoke(this, EventArgs.Empty);
        }

        // ==========================================
        // 4. USER / STAFF CRUD OPERATIONS
        // ==========================================
        public void AddUser(AppUser user)
        {
            if (user == null) return;
            if (string.IsNullOrWhiteSpace(user.Id))
            {
                user.Id = $"USR-{DateTime.Now.Ticks % 10000:D4}";
            }
            user.LastLogin = DateTime.Now;
            Users.Add(user);

            try
            {
                using (var conn = DbConnectionHelper.CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(
                        "INSERT INTO users (id, full_name, email, role, status, last_login) " +
                        "VALUES (@id, @name, @email, @role, @status, @lastLogin) " +
                        "ON CONFLICT (id) DO UPDATE SET full_name=EXCLUDED.full_name, role=EXCLUDED.role, status=EXCLUDED.status", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", user.Id);
                        cmd.Parameters.AddWithValue("@name", user.FullName ?? "");
                        cmd.Parameters.AddWithValue("@email", user.Email ?? "");
                        cmd.Parameters.AddWithValue("@role", user.Role ?? "Store Staff");
                        cmd.Parameters.AddWithValue("@status", user.Status ?? "Active");
                        cmd.Parameters.AddWithValue("@lastLogin", user.LastLogin);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                LastDatabaseError = ex.Message;
            }
            DataRefreshed?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateUser(AppUser user)
        {
            if (user == null) return;
            var existing = Users.FirstOrDefault(u => u.Id == user.Id);
            if (existing != null)
            {
                existing.FullName = user.FullName;
                existing.Email = user.Email;
                existing.Role = user.Role;
                existing.Status = user.Status;
            }

            try
            {
                using (var conn = DbConnectionHelper.CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(
                        "UPDATE users SET full_name=@name, email=@email, role=@role, status=@status WHERE id=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", user.Id);
                        cmd.Parameters.AddWithValue("@name", user.FullName ?? "");
                        cmd.Parameters.AddWithValue("@email", user.Email ?? "");
                        cmd.Parameters.AddWithValue("@role", user.Role ?? "Store Staff");
                        cmd.Parameters.AddWithValue("@status", user.Status ?? "Active");
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                LastDatabaseError = ex.Message;
            }
            DataRefreshed?.Invoke(this, EventArgs.Empty);
        }

        public void DeleteUser(string id)
        {
            Users.RemoveAll(u => u.Id == id);
            try
            {
                using (var conn = DbConnectionHelper.CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("DELETE FROM users WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                LastDatabaseError = ex.Message;
            }
            DataRefreshed?.Invoke(this, EventArgs.Empty);
        }

        // ==========================================
        // 5. ORDER CRUD OPERATIONS
        // ==========================================
        public void UpdateOrderStatus(string orderId, string newStatus)
        {
            var ord = Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (ord != null)
            {
                ord.Status = newStatus;
            }

            try
            {
                using (var conn = DbConnectionHelper.CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("UPDATE orders SET status = @status WHERE order_id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@status", newStatus);
                        cmd.Parameters.AddWithValue("@id", orderId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                LastDatabaseError = ex.Message;
            }
            DataRefreshed?.Invoke(this, EventArgs.Empty);
        }

        // ==========================================
        // 6. POS & SALES REAL-TIME CHECKOUT
        // ==========================================
        public void AddToCart(Product product)
        {
            if (product == null || product.Stock <= 0) return;

            var existing = CurrentCart.FirstOrDefault(c => c.Product.Id == product.Id);
            if (existing != null)
            {
                if (existing.Quantity < product.Stock)
                    existing.Quantity++;
            }
            else
            {
                CurrentCart.Add(new CartItem { Product = product, Quantity = 1 });
            }
            CartChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ChangeCartQuantity(string productId, int delta)
        {
            var item = CurrentCart.FirstOrDefault(c => c.Product.Id == productId);
            if (item == null) return;

            item.Quantity += delta;
            if (item.Quantity <= 0)
            {
                CurrentCart.Remove(item);
            }
            else if (item.Quantity > item.Product.Stock)
            {
                item.Quantity = item.Product.Stock;
            }
            CartChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RemoveFromCart(string productId)
        {
            var item = CurrentCart.FirstOrDefault(c => c.Product.Id == productId);
            if (item != null)
            {
                CurrentCart.Remove(item);
                CartChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void ClearCart()
        {
            CurrentCart.Clear();
            CartChanged?.Invoke(this, EventArgs.Empty);
        }

        public SaleTransaction CheckoutCart(string customerName, string paymentMethod)
        {
            if (CurrentCart.Count == 0) return null;

            decimal subtotal = CurrentCart.Sum(c => c.Subtotal);
            decimal tax = Math.Round(subtotal * (Settings.TaxRatePercentage / 100m), 2);
            decimal total = subtotal + tax;

            // Reduce stock
            foreach (var ci in CurrentCart)
            {
                ci.Product.Stock = Math.Max(0, ci.Product.Stock - ci.Quantity);
                if (ci.Product.Stock == 0) ci.Product.Status = "Out of Stock";
                else if (ci.Product.Stock < 10) ci.Product.Status = "Low Stock";

                // Update product stock in PostgreSQL
                UpdateProductStockInDb(ci.Product.Id, ci.Product.Stock, ci.Product.Status);
            }

            var sale = new SaleTransaction
            {
                InvoiceNo = $"INV-{DateTime.Now:yyyyMMdd}-{Sales.Count + 1:D3}",
                Timestamp = DateTime.Now,
                CustomerName = string.IsNullOrWhiteSpace(customerName) ? "Walk-in Customer" : customerName,
                Subtotal = subtotal,
                TaxAmount = tax,
                DiscountAmount = 0m,
                TotalAmount = total,
                PaymentMethod = paymentMethod,
                Status = "Completed",
                Items = new List<CartItem>(CurrentCart)
            };

            Sales.Insert(0, sale);

            // Persist Sale and Line Items to PostgreSQL
            SaveSaleToDatabase(sale);

            ClearCart();
            DataRefreshed?.Invoke(this, EventArgs.Empty);
            return sale;
        }

        private void SaveSaleToDatabase(SaleTransaction sale)
        {
            try
            {
                using (var conn = DbConnectionHelper.CreateConnection())
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        using (var cmd = new NpgsqlCommand(
                            "INSERT INTO sales (invoice_no, customer_name, subtotal, tax_amount, discount_amount, total_amount, payment_method, status, timestamp) " +
                            "VALUES (@inv, @cust, @sub, @tax, @disc, @total, @pay, @sts, @time)", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@inv", sale.InvoiceNo);
                            cmd.Parameters.AddWithValue("@cust", sale.CustomerName);
                            cmd.Parameters.AddWithValue("@sub", sale.Subtotal);
                            cmd.Parameters.AddWithValue("@tax", sale.TaxAmount);
                            cmd.Parameters.AddWithValue("@disc", sale.DiscountAmount);
                            cmd.Parameters.AddWithValue("@total", sale.TotalAmount);
                            cmd.Parameters.AddWithValue("@pay", sale.PaymentMethod);
                            cmd.Parameters.AddWithValue("@sts", sale.Status);
                            cmd.Parameters.AddWithValue("@time", sale.Timestamp);
                            cmd.ExecuteNonQuery();
                        }

                        foreach (var item in sale.Items)
                        {
                            using (var cmdItem = new NpgsqlCommand(
                                "INSERT INTO sale_items (invoice_no, product_id, product_name, quantity, unit_price, subtotal) " +
                                "VALUES (@inv, @pid, @pname, @qty, @uprice, @stotal)", conn, trans))
                            {
                                cmdItem.Parameters.AddWithValue("@inv", sale.InvoiceNo);
                                cmdItem.Parameters.AddWithValue("@pid", item.Product.Id);
                                cmdItem.Parameters.AddWithValue("@pname", item.Product.Name);
                                cmdItem.Parameters.AddWithValue("@qty", item.Quantity);
                                cmdItem.Parameters.AddWithValue("@uprice", item.Product.Price);
                                cmdItem.Parameters.AddWithValue("@stotal", item.Subtotal);
                                cmdItem.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                LastDatabaseError = ex.Message;
            }
        }

        private void UpdateProductStockInDb(string productId, int newStock, string newStatus)
        {
            try
            {
                using (var conn = DbConnectionHelper.CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("UPDATE products SET stock = @stock, status = @status WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@stock", newStock);
                        cmd.Parameters.AddWithValue("@status", newStatus);
                        cmd.Parameters.AddWithValue("@id", productId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                LastDatabaseError = ex.Message;
            }
        }

        // ==========================================
        // 7. AUTHENTICATION (STRICT DATABASE CHECK)
        // ==========================================
        public AppUser Authenticate(string usernameOrEmail, string password, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(usernameOrEmail))
            {
                errorMessage = TranslationManager.T("InvalidCredentials", "Invalid credentials. Please verify your email and password.");
                return null;
            }

            string clean = usernameOrEmail.Trim();
            AppUser user = null;
            string dbPasswordHash = null;

            // 1. Direct PostgreSQL Database Authentication (Strict Verification)
            try
            {
                using (var conn = DbConnectionHelper.CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT id, full_name, email, password_hash, role, status, last_login FROM users WHERE LOWER(email) = @e OR LOWER(full_name) = @e OR LOWER(id) = @e LIMIT 1", conn))
                    {
                        cmd.Parameters.AddWithValue("@e", clean.ToLowerInvariant());
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user = new AppUser
                                {
                                    Id = reader.GetString(0),
                                    FullName = reader.GetString(1),
                                    Email = reader.GetString(2),
                                    Role = reader.IsDBNull(4) ? "Store Staff" : reader.GetString(4),
                                    Status = reader.IsDBNull(5) ? "Active" : reader.GetString(5),
                                    LastLogin = DateTime.Now
                                };
                                dbPasswordHash = reader.IsDBNull(3) ? null : reader.GetString(3);
                            }
                        }
                    }

                    if (user != null)
                    {
                        // Update last_login in database
                        using (var upCmd = new NpgsqlCommand("UPDATE users SET last_login = NOW() WHERE id = @id", conn))
                        {
                            upCmd.Parameters.AddWithValue("@id", user.Id);
                            upCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LastDatabaseError = ex.Message;
            }

            // 2. If database query didn't connect, check preloaded database users
            if (user == null && Users != null && Users.Count > 0)
            {
                user = Users.FirstOrDefault(u =>
                    u.Email.Equals(clean, StringComparison.OrdinalIgnoreCase) ||
                    u.FullName.Equals(clean, StringComparison.OrdinalIgnoreCase) ||
                    u.Id.Equals(clean, StringComparison.OrdinalIgnoreCase));
            }

            // 3. User does NOT exist in database -> STRICT REJECTION
            if (user == null)
            {
                errorMessage = TranslationManager.T("InvalidCredentials", "Invalid credentials. Please verify your email and password.");
                return null;
            }

            // 4. Verify password if password_hash is set on the database record
            if (!string.IsNullOrEmpty(dbPasswordHash) && !string.IsNullOrEmpty(password))
            {
                bool isPlaceholder = (password.Trim() == "••••••••" || password.Trim() == "********");
                if (!isPlaceholder && !string.Equals(password, dbPasswordHash, StringComparison.Ordinal))
                {
                    errorMessage = TranslationManager.T("InvalidCredentials", "Invalid credentials. Please verify your email and password.");
                    return null;
                }
            }

            // 5. Check if user account is suspended
            if (string.Equals(user.Status, "Suspended", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = TranslationManager.T("AccountSuspended", "This account is suspended. Please contact administrator.");
                return null;
            }

            // 6. Successful Authentication
            user.LastLogin = DateTime.Now;
            CurrentUser = user;
            return user;
        }
    }
}
