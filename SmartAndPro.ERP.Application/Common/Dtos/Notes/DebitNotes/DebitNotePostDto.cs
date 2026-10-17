using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SmartAndPro.ERP.Application.Common.Dtos.Notes.DebitNoteAccount;

namespace SmartAndPro.ERP.Application.Common.Dtos.Notes.DebitNotes;

public class DebitNotePostDto
{
    [JsonInclude]
    [Required(ErrorMessage = "Amount Field is required")]
    public decimal Amount { get; set; }
    
    public decimal? Tax { get; set; } = 0;
    
    public string? ReferenceNumber { get; set; }
    
    public string? Description { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "SupplierDetailsId Field is required")]
    public Guid SupplierDetailsId { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "DebitNoteAccounts Field is required")]
    public List<DebitNoteAccountPostDto>? DebitNoteAccounts { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "ReceiptDate Field is required")]
    public DateOnly? ReceiptDate { get; set; }
    

    
    public bool? IsActive { get; set; } = true;
}