-- ==============================================================================
-- PCCFPI STORE - Supabase PostgreSQL Database Schema (Schema Only)
-- ==============================================================================

-- 1. Schema Migrations Tracking
CREATE TABLE IF NOT EXISTS __schema_migrations (
    id SERIAL PRIMARY KEY,
    migration_name VARCHAR(150) UNIQUE NOT NULL,
    applied_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 2. Users Table
CREATE TABLE IF NOT EXISTS users (
    id VARCHAR(50) PRIMARY KEY,
    full_name VARCHAR(150) NOT NULL,
    email VARCHAR(150) UNIQUE NOT NULL,
    password_hash VARCHAR(255),
    role VARCHAR(50) NOT NULL DEFAULT 'Cashier',
    status VARCHAR(50) NOT NULL DEFAULT 'Active',
    last_login TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 3. Categories Table
CREATE TABLE IF NOT EXISTS categories (
    id VARCHAR(50) PRIMARY KEY,
    name VARCHAR(150) NOT NULL,
    description TEXT,
    product_count INT DEFAULT 0,
    total_revenue NUMERIC(14,2) DEFAULT 0.00,
    color_hex VARCHAR(20) DEFAULT '#3B82F6',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 4. Products Table
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
);

-- 5. Customers Table
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
);

-- 6. Orders Table
CREATE TABLE IF NOT EXISTS orders (
    order_id VARCHAR(50) PRIMARY KEY,
    customer_name VARCHAR(150) NOT NULL,
    items_summary TEXT,
    total_amount NUMERIC(14,2) NOT NULL DEFAULT 0.00,
    status VARCHAR(50) NOT NULL DEFAULT 'Processing',
    payment_status VARCHAR(50) NOT NULL DEFAULT 'Paid',
    order_date TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 7. Sales Table
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
);

-- 8. Sale Items Table
CREATE TABLE IF NOT EXISTS sale_items (
    id SERIAL PRIMARY KEY,
    invoice_no VARCHAR(50) NOT NULL REFERENCES sales(invoice_no) ON DELETE CASCADE,
    product_id VARCHAR(50),
    product_name VARCHAR(200) NOT NULL,
    unit_price NUMERIC(14,2) NOT NULL DEFAULT 0.00,
    quantity INT NOT NULL DEFAULT 1,
    subtotal NUMERIC(14,2) NOT NULL DEFAULT 0.00
);

-- 9. Store Settings Table
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
);

-- 10. Performance Indexes
CREATE INDEX IF NOT EXISTS idx_products_sku ON products(sku);
CREATE INDEX IF NOT EXISTS idx_products_category ON products(category);
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
CREATE INDEX IF NOT EXISTS idx_sales_timestamp ON sales(timestamp);
CREATE INDEX IF NOT EXISTS idx_sale_items_invoice ON sale_items(invoice_no);
CREATE INDEX IF NOT EXISTS idx_orders_date ON orders(order_date);
