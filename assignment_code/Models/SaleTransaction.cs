using System;
using System.Collections.Generic;

namespace assignment_code.Models
{
    public class CartItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice => Product?.Price ?? 0m;
        public decimal Subtotal => UnitPrice * Quantity;
    }

    public class SaleTransaction
    {
        public string InvoiceNo { get; set; }
        public DateTime Timestamp { get; set; }
        public string CustomerName { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } // "Cash", "Credit Card", "QR Pay"
        public string Status { get; set; } // "Completed", "Refunded"
    }
}
