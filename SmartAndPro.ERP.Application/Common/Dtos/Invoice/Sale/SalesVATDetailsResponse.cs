namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale
{
    public class SalesVATDetailsResponse
    {
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; }
        public string SupplierId { get; set; }
        public string SuppilerArabicName { get; set; }
        public string SuppilerEnglishName { get; set; }
        public string MainAccountId { get; set; }
        public DateTime IssueDate { get; set; }

        public List<VATItemDetails> ItemDetails { get; set; }
    }

    public class VATItemDetails
    {
        public string ReferenceCode { get; set; }
        public string ItemArabicName { get; set; }
        public string ItemEnglishName { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal VATAmount { get; set; }  // القيمة المضافة
        public bool IsTaxExempted { get; set; }
    }
}
