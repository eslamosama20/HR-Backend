using SmartAndPro.ERP.Application.Common.Dtos.Accounting.CustomerProfile;
using SmartAndPro.ERP.Application.Common.Dtos.Attachments;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.Reservation;
using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale
{
    public class SaleInvoiceDto
    {
        public int Id { get; set; }
        public string InvoiceNo { get; set; }
        public string? Currency { get; set; }
        public Guid? CustomerProfileId { get; set; }
        public CustomerProfileResponseDto CustomerProfile { get; set; }
        public string? Description { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public Guid? StoreDetailsId { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal NetPrice { get; set; }
        public Guid? TransactionId { get; set; }

        public List<SaleItemDto> SaleItems { get; set; }
        
        public List<AttachmentResponseDto> Attachments { get; set; }
        
        public bool? IsQuotation { get; set; } = false;
        public bool? IsFrontal { get; set; } = false;
        public int? ConvertedToInvoiceId { get; set; }
        public string? OriginalQuotationNo { get; set; } // For invoices: the quotation number it was created from

        public List<Guid> ReservationIds { get; set; } = new();
        public List<ReservationResponseDto> Reservations { get; set; } = new();


        public bool IsCleared { get; set; }
        public string? InvoiceHash { get; set; }
        public string? GeneratedQR { get; set; }
        public SaleInvoiceType SaleInvoiceType { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public decimal RefundedAmount { get; set; }
        public string? RefundStatus { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? CustomerDepartmentId { get; set; }

        // Payment tracking fields (populated server-side, not from AutoMapper)
        public decimal PaidAmount { get; set; } = 0;
        public decimal RemainingAmount { get; set; } = 0;
    
        public InvoicePaymentStatus PaymentStatus { get; set; } = InvoicePaymentStatus.Unpaid;

        public string? SellerName { get; set; }
        public string? CashierName { get; set; }
        public string? CreatedBy { get; set; }
    }
}
