using System;

namespace assignment_code.Models
{
    public class Category
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ProductCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public string ColorHex { get; set; }
    }
}
