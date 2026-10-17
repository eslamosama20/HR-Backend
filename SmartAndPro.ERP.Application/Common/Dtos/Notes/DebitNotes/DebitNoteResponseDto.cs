using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SmartAndPro.ERP.Application.Common.Dtos.Attachments;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.SupplierDetails;
using SmartAndPro.ERP.Application.Common.Dtos.Notes.DebitNoteAccount;
using SmartAndPro.ERP.Application.Common.Dtos.Treasuries;

namespace SmartAndPro.ERP.Application.Common.Dtos.Notes.DebitNotes;

public class DebitNoteResponseDto
{
    public Guid Id { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "Amount Field is required")]
    public decimal Amount { get; set; }
    
    public decimal? Tax { get; set; } = 0;
    
    public string? ReferenceNumber { get; set; }
    
    public string? Description { get; set; }

    public Guid SupplierDetailsId { get; set; }
    
    public SupplierDetailsResponseDto SupplierDetails { get; set; }

    public Guid? TransactionId { get; set; }

    public List<DebitNoteAccountResponseDto>? DebitNoteAccounts { get; set; }

    public List<AttachmentResponseDto> Attachments { get; set; }

    public DateOnly? ReceiptDate { get; set; }
        
    public bool? IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}