using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SmartAndPro.ERP.Application.Common.Dtos.ReceiptConnections;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Application.Common.Dtos.Receipts.PaycheckReceiptAccountResponse;
namespace SmartAndPro.ERP.Application.Common.Dtos.Receipts.PaycheckReceipts;

public class PaycheckReceiptPostDto
{
    [JsonInclude]
    [Required(ErrorMessage = "Amount Field is required")]
    public decimal Amount { get; set; }
    
    public decimal? Tax { get; set; } = 0;
    
    public string? ReferenceNumber { get; set; }
    
    public string? Description { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "TreasuryId Field is required")]
    public Guid TreasuryId { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "PaycheckReceiptAccounts Field is required")]
    public List<PaycheckReceiptAccountPostDto>? PaycheckReceiptAccounts { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "ReceiptDate Field is required")]
    public DateOnly? ReceiptDate { get; set; }
    
    public bool IsFrontal { get; set; } = false;

    
    public bool? IsActive { get; set; } = true;

    public PaymentMethodType? PaymentMethodType { get; set; }
    public List<ReceiptConnectionPostDto> ReceiptConnections { get; set; } = new();


    public string? CheckNumber { get; set; }
    public string? BankName { get; set; }
    public string? BankReferenceNumber { get; set; }
    public string? VoucherNumber { get; set; }
    public string? TransactionNumber { get; set; }


}