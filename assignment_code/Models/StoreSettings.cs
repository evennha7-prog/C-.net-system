using System;
using assignment_code.Services;

namespace assignment_code.Models
{
    public class StoreSettings
    {
        public string StoreName { get; set; } = EnvLoader.Get("STORE_NAME", "PCCFPI STORE");
        public string CurrencySymbol { get; set; } = EnvLoader.Get("CURRENCY_SYMBOL", "$");
        public decimal TaxRatePercentage { get; set; } = EnvLoader.GetDecimal("TAX_RATE_PERCENTAGE", 8.0m);
        public string Phone { get; set; } = EnvLoader.Get("STORE_PHONE", "+1 (555) 019-2834");
        public string Email { get; set; } = EnvLoader.Get("STORE_EMAIL", "support@pccfpistore.com");
        public string Address { get; set; } = EnvLoader.Get("STORE_ADDRESS", "100 Retail Boulevard, Suite 400");
        public string ReceiptHeader { get; set; } = EnvLoader.Get("RECEIPT_HEADER", "THANK YOU FOR SHOPPING AT PCCFPI STORE!");
        public string ReceiptFooter { get; set; } = EnvLoader.Get("RECEIPT_FOOTER", "Returns accepted within 30 days with receipt.");
        public bool AutoPrintReceipt { get; set; } = EnvLoader.GetBool("AUTO_PRINT_RECEIPT", true);
    }
}
