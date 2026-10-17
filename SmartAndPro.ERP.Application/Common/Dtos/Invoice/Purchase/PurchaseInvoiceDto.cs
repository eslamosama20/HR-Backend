using SmartAndPro.ERP.Application.Common.Dtos.Attachments;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.SupplierDetails;

namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Purchase
{
    public class PurchaseInvoiceDto
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
        public Guid? TransactionId { get; set; }
        public bool IsFrontal { get; set; } = false;

        public List<PurchaseItemDto> PurchaseItems { get; set; }
        
        public List<AttachmentResponseDto> Attachments { get; set; }

        public int? PurchaseQuotationId { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }

    }
}
