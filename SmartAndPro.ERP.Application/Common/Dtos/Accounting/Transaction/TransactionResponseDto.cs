using SmartAndPro.ERP.Application.Common.Dtos.Accounting.ClientProfile;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.TransactionType;
using SmartAndPro.ERP.Application.Common.Dtos.Attachments;

namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.Transaction;

public class TransactionResponseDto
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public Guid TransactionTypeId { get; set; }
    public TransactionTypeResponseDto TransactionType { get; set; }

    public string ReferenceNumber { get; set; }
    
    public string? Description { get; set; }
    
    public DateOnly TransactionDate { get; set; }

    public Guid ClientProfileId { get; set; }
    
    public bool IsOpeningTransaction { get; set; }
    
    public int? SaleInvoiceId { get; set; }
    public int? PurchaseInvoiceId { get; set; }

    public int? SalesReturnId { get; set; }
    public int? PurchaseReturnId { get; set; }

    public Guid? PaycheckReceiptId { get; set; }
    public Guid? PaymentReceiptId { get; set; }
    
    public Guid? InventoryPermitId { get; set; }
    
    public Guid? InventoryStocktakingId { get; set; }

    public Guid? ReservationId { get; set; }

    public Guid? DebitNoteId { get; set; }
    public Guid? CreditNoteId { get; set; }
    
    
    public bool IsTransactionPost1 { get; set; } = false;
    public ClientProfileResponseDto ClientProfile { get; set; }
    
    public List<TransactionEntryChildResponseDto> TransactionEntries { get; set; }
    
    public List<AttachmentResponseDto> Attachments { get; set; }
    
    public bool IsFrontal { get; set; } = false;
    

    public DateTime CreatedAt { get; set; }
}