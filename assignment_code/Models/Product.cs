

using System;

namespace assignment_code.Models
{
    public class Product
    {
        public string Id { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public int Stock { get; set; }
        public string Status { get; set; } // "In Stock", "Low Stock", "Out of Stock"
        public string DateAdded { get; set; }
    }
}
