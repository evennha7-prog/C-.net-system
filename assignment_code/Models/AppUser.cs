using System;

namespace assignment_code.Models
{
    public class AppUser
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } // "Administrator", "Store Manager", "Cashier"
        public string Status { get; set; } // "Active", "Suspended"
        public DateTime LastLogin { get; set; }
    }
}
