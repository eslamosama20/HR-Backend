using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Receipts.PaymentReceiptAccountResponse;

public class PaymentReceiptAccountPostDto
{
    [JsonInclude]
    [Required(ErrorMessage = "Amount Field is required")]
    public decimal Amount { get; set; }
    
    
    public decimal? Tax { get; set; } = 0;
    public string? Description { get; set; }

    
    public Guid? PaymentReceiptId { get; set; } = Guid.Empty;
    
    [JsonInclude]
    [Required(ErrorMessage = "MainAccountId Field is required")]
    public Guid MainAccountId { get; set; }
    
    
    public bool? IsActive { get; set; } = true;
}