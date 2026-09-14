using System;

namespace assignment_code.Models
{
    public class Order
    {
        public string OrderId { get; set; }
        public string CustomerName { get; set; }
        public string ItemsSummary { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } // "Pending", "Processing", "Shipped", "Delivered", "Cancelled"
        public string PaymentStatus { get; set; } // "Paid", "Unpaid", "Refunded"
        public DateTime OrderDate { get; set; }
    }
}
