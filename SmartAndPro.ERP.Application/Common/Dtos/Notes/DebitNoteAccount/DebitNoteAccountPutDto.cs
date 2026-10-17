using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Notes.DebitNoteAccount;

public class DebitNoteAccountPutDto
{
    
    public Guid? Id { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "Amount Field is required")]
    public decimal? Amount { get; set; }
    
    
    public decimal? Tax { get; set; } = 0;
    public string? Description { get; set; }

    
    public Guid? DebitNoteId { get; set; } = Guid.Empty;
    
    
    [JsonInclude]
    [Required(ErrorMessage = "MainAccountId Field is required")]
    public Guid MainAccountId { get; set; }
    
    public bool? IsActive { get; set; } = true;

}