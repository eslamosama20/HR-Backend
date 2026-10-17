using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SmartAndPro.ERP.Application.Common.Dtos.Attachments;
using SmartAndPro.ERP.Application.Common.Dtos.ReceiptConnections;
using SmartAndPro.ERP.Application.Common.Dtos.Receipts.PaymentReceiptAccountResponse;
using SmartAndPro.ERP.Application.Common.Dtos.Treasuries;

namespace SmartAndPro.ERP.Application.Common.Dtos.Receipts.PaymentReceipts;

public class PaymentReceiptResponseDto
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

    public List<PaymentReceiptAccountResponseDto>? PaymentReceiptAccounts { get; set; }

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

}