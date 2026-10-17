using SmartAndPro.ERP.Application.Common.Dtos.Attachments;
using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale
{
    internal class SaleInvoiceResponseDto
    {
        public int Id { get; set; }
        public string? InvoiceNo { get; set; }
        public string? Currency { get; set; }
        public Guid? CustomerProfileId { get; set; }
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
        public List<SaleItemDto> SaleItems { get; set; } = new List<SaleItemDto>();
        public bool? IsQuotation { get; set; } = false;
        public bool? IsFrontal { get; set; } = false;
        public List<Guid>? ReservationIds { get; set; }

        public bool IsCleared { get; set; }
        public string? InvoiceHash { get; set; }
        public string? GeneratedQR { get; set; }
        
        public List<AttachmentResponseDto> Attachments { get; set; }
        public SaleInvoiceType SaleInvoiceType { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }


    }
}
