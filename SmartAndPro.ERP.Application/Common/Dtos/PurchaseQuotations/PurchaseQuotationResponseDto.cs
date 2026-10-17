using SmartAndPro.ERP.Application.Common.Dtos.Attachments;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Purchase;
using SmartAndPro.ERP.Application.Common.Dtos.PurchaseQuotations.PurchaseQuotationItems;

namespace SmartAndPro.ERP.Application.Common.Dtos.PurchaseQuotations
{
    public class PurchaseQuotationResponseDto
    {
        public int Id { get; set; }
        public string? InvoiceNo { get; set; }
        public string? Currency { get; set; }
        public Guid? SupplierDetailsId { get; set; }
        public string? Description { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal NetPrice { get; set; }
        public Guid? TransactionId { get; set; }
        public List<PurchaseItemDto> PurchaseItems { get; set; } = new List<PurchaseItemDto>();
        public List<AttachmentResponseDto> Attachments { get; set; } = new List<AttachmentResponseDto>();
        public bool IsFrontal { get; set; } = false;
        
    }
}

