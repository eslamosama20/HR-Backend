using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.TransactionEntry;

namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.Transaction;

public class TransactionPostDto
{
    [JsonInclude]
    [Required(ErrorMessage = "TransactionTypeId Field is required")]
    public Guid? TransactionTypeId { get; set; }
    
    public string? ReferenceNumber { get; set; }
    
    public string? Description { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "TransactionEntry Field is required")]
    public List<TransactionEntryPostDto>? TransactionEntries { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "TransactionDate Field is required")]
    public DateOnly? TransactionDate { get; set; }
    
    
    public bool? IsOpeningTransaction { get; set; } = false;

    public Guid? ClientProfileId { get; set; }

    public bool IsFrontal { get; set; } = false;
    
    public bool? IsActive { get; set; } = true;
}