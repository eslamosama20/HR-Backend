using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Purchase;


namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale
{
    public class SalesInvoiceDetailsResponse
    {
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; }
        public string SupplierId { get; set; }
        public string SuppilerArabicName { get; set; }
        public string SuppilerEnglishName { get; set; }
        public string MainAccountId { get; set; }
        public decimal TotalPrice { get; set; }
        public string Description { get; set; }
        public DateTime IssueDate { get; set; }

        public List<ItemDetails> ItemDetails { get; set; }

        public decimal TotalItemPrice { get; set; }     // Sum of item prices
        public decimal TotalItemQuantity { get; set; }  // Sum of item quantities

        public decimal TotalInvoicePriceWithTax { get; set; }
        public decimal TotalInvoicePrice { get; set; }
        public decimal DiscountAmount { get; set; }
        
        public bool IsCleared { get; set; }
        public string? InvoiceHash { get; set; }
        public string? GeneratedQR { get; set; }
        public bool? IsFrontal { get; set; } = false;

    }
}
