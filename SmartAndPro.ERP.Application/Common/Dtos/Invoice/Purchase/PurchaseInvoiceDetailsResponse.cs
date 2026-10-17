namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Purchase
{
    public class PurchaseInvoiceDetailsResponse
    {
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; }
        public string SupplierID { get; set; }
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
        public bool IsFrontal { get; set; } = false;

    }

    public class ItemDetails
    {
        public string ReferenceCode { get; set; }
        public string ItemArabicName { get; set; }
        public string ItemEnglishName { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalPriceWithTax { get; set; }

        public bool IsTaxExempted { get; set; } // Indicates if the item is tax exempted

    }
}
