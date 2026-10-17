using SmartAndPro.ERP.Application.Common.Dtos.Attachments;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.SupplierDetails;
using SmartAndPro.ERP.Application.Common.Dtos.PurchaseQuotations.PurchaseQuotationItems;

namespace SmartAndPro.ERP.Application.Common.Dtos.PurchaseQuotations
{
    public class PurchaseQuotationDto
    {
        public int Id { get; set; }
        public string InvoiceNo { get; set; }
        public string? Currency { get; set; }
        public Guid? SupplierDetailsId { get; set; }
        public SupplierDetailsResponseDto SupplierDetails { get; set; }
        public string? Description { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal NetPrice { get; set; }
        //public Guid? TransactionId { get; set; }
        public bool IsFrontal { get; set; } = false;
        public string Status { get; set; }
        public int? PurchaseInvoiceId { get; set; }

        public List<PurchaseQuotationItemDto> PurchaseQuotationItems { get; set; }
        
        public List<AttachmentResponseDto> Attachments { get; set; }
    }
}
