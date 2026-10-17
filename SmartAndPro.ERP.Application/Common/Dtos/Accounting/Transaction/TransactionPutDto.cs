using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.TransactionEntry;

namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.Transaction;

public class TransactionPutDto
{
    [JsonInclude]
    [Required(ErrorMessage = "Id Field is required")]
    public Guid? Id { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "TransactionType Field is required")]
    public string? TransactionTypeId { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "TransactionDate Field is required")]
    public DateOnly? TransactionDate { get; set; }
    

    public string? ReferenceNumber { get; set; }
    
    public bool? IsOpeningTransaction { get; set; } = false;
    public List<TransactionEntryPutDto> TransactionEntries { get; set; }
    
    public string? Description { get; set; }
    
    public bool IsFrontal { get; set; } = false;

    
    public bool? IsActive { get; set; } = true;

}