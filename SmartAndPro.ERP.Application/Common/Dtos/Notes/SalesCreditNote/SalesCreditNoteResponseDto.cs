using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.CustomerProfile;
using SmartAndPro.ERP.Application.Common.Dtos.Attachments;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale;
using SmartAndPro.ERP.Application.Common.Dtos.Notes.SalesCreditNoteItem;

namespace SmartAndPro.ERP.Application.Common.Dtos.Notes.SalesCreditNote;

public class SalesCreditNoteResponseDto
{
    public Guid Id { get; set; }
 

    public string AdjustmentReason { get; set; }

    public int SaleInvoiceId { get; set; }
    
    public SaleInvoiceDto SaleInvoice { get; set; }
    
    
    public string? Description { get; set; }

    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? SalesCreditNoteNo { get; set; }

    public decimal? DiscountAmount { get; set; } = 0;

    public Guid CustomerProfileId { get; set; }
    public CustomerProfileResponseDto CustomerProfile { get; set; }

    public Guid? TransactionId { get; set; }
    public List<SalesCreditNoteItemResponseDto> SalesCreditNoteItems { get; set; } 
    
    public List<AttachmentResponseDto> Attachments { get; set; }
        
    public bool? IsCleared { get; set; } = false;
        
    public string? InvoiceHash { get; set; }
        
    public string? GeneratedQR { get; set; }
    public bool? IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}