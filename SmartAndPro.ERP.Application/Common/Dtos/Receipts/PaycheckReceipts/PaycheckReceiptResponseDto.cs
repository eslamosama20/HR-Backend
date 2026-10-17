using SmartAndPro.ERP.Application.Common.Dtos.Attachments;
using SmartAndPro.ERP.Application.Common.Dtos.ReceiptConnections;
using SmartAndPro.ERP.Application.Common.Dtos.Receipts.PaycheckReceiptAccountResponse;
using SmartAndPro.ERP.Application.Common.Dtos.Treasuries;
using SmartAndPro.ERP.Domains.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Receipts.PaycheckReceipts;

public class PaycheckReceiptResponseDto
{
    public Guid Id { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "Amount Field is required")]
    public decimal Amount { get; set; }
    
    public decimal? Tax { get; set; } = 0;
    
    public string? ReferenceNumber { get; set; }
    
    public string? Description { get; set; }

    public Guid TreasuryId { get; set; }
    
    public TreasuryResponseDto Treasury { get; set; }

    public Guid? TransactionId { get; set; }

    public List<PaycheckReceiptAccountResponseDto>? PaycheckReceiptAccounts { get; set; }
    public List<AttachmentResponseDto> Attachments { get; set; }
    public DateOnly? ReceiptDate { get; set; }
        
    public bool? IsActive { get; set; } = true;
    public bool IsFrontal { get; set; } = false;

    public SmartAndPro.ERP.Domains.Enums.PaymentMethodType? PaymentMethodType { get; set; }

    public string? CheckNumber { get; set; }
    public string? BankName { get; set; }
    public string? BankReferenceNumber { get; set; }
    public string? VoucherNumber { get; set; }
    public string? TransactionNumber { get; set; }
 
    public DateTime CreatedAt { get; set; }

    public List<ReceiptConnectionResponseDto>? ReceiptConnections { get; set; }

    // Calculated properties for linked operations
    public decimal PaidAmount { get; set; } = 0;
    public decimal RemainingAmount { get; set; } = 0;
    public decimal TotalInvoiceAmount { get; set; } = 0;
}