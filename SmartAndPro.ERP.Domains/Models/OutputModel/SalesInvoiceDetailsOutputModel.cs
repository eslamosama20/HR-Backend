
namespace SmartAndPro.ERP.Domains.Models.OutputModel
{
    public class SalesInvoiceDetailsOutputModel
    {
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; }
        public string SupplierId { get; set; }
        public string SuppilerArabicName { get; set; }
        public string SuppilerEnglishName { get; set; }
        public decimal TotalPrice { get; set; }
        public string Description { get; set; }
        public DateTime IssueDate { get; set; }
        public bool IsTaxExempted { get; set; }

        public List<ItemDetails> ItemDetails { get; set; }


        public decimal TotalItemPrice { get; set; }     // Sum of item prices
        public decimal TotalItemQuantity { get; set; }  // Sum of item quantities
        public decimal TotalItemPriceWithTax { get; set; }
        public decimal TotalInvoicePriceWithTax { get; set; }
        public decimal TotalInvoicePrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? MainAccountId { get; set; }
    }
}
