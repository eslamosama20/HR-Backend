using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.TransactionEntry;

public class TransactionEntryPutDto
{
    
    public Guid? Id { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "CreditAmount Field is required")]
    public decimal? CreditAmount { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "DebitAmount Field is required")]
    public decimal? DebitAmount { get; set; }
    
    public string? Description { get; set; }
    
    
    public Guid? TransactionId { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "MainAccountId Field is required")]
    public Guid MainAccountId { get; set; }
    
    public bool? IsActive { get; set; } = true;

}