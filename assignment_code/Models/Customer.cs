using System;

namespace assignment_code.Models
{
    public class Customer
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public string Tier { get; set; } // "VIP", "Regular", "New"
        public string Status { get; set; } // "Active", "Inactive"
    }
}
