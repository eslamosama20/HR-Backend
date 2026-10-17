using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SmartAndPro.ERP.Application.Common.Dtos.Notes.SalesCreditNoteItem;

namespace SmartAndPro.ERP.Application.Common.Dtos.Notes.SalesCreditNote;

public class SalesCreditNotePutDto
{
    [JsonInclude]
    [Required(ErrorMessage = "AdjustmentReason Field is required")]
    public string AdjustmentReason { get; set; }
    [JsonInclude]
    [Required(ErrorMessage = "SaleInvoiceId Field is required")]
    public int SaleInvoiceId { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "Id Field is required")]
    public Guid? Id { get; set; }
    
    public string? Description { get; set; }
    [JsonInclude]
    [Required(ErrorMessage = "IssueDate Field is required")]
    public DateTime IssueDate { get; set; }
    [JsonInclude]
    [Required(ErrorMessage = "DueDate Field is required")]
    public DateTime DueDate { get; set; }
    public string? SalesCreditNoteNo { get; set; }

    public decimal? DiscountAmount { get; set; } = 0;
    [JsonInclude]
    [Required(ErrorMessage = "CustomerProfileId Field is required")]
    public Guid CustomerProfileId { get; set; }

        
    public Guid? TransactionId { get; set; }
        
    public List<SalesCreditNoteItemPutDto> SalesCreditNoteItems { get; set; } 
        
    public bool? IsCleared { get; set; } = false;
        
    public string? InvoiceHash { get; set; }
        
    public string? GeneratedQR { get; set; }
    public bool? IsActive { get; set; } = true;

}