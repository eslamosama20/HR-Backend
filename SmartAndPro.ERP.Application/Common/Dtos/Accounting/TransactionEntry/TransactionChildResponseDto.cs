using SmartAndPro.ERP.Application.Common.Dtos.Accounting.ClientProfile;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.TransactionType;
using SmartAndPro.ERP.Application.Common.Dtos.Attachments;

namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.TransactionEntry;

public class TransactionChildResponseDto
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public Guid TransactionTypeId { get; set; }
    public TransactionTypeResponseDto TransactionType { get; set; }

    public string ReferenceNumber { get; set; }
    
    public string? Description { get; set; }
    
    public DateOnly TransactionDate { get; set; }

    public Guid ClientProfileId { get; set; }
    
    public ClientProfileResponseDto ClientProfile { get; set; }
    
    public int? SaleInvoiceId { get; set; }
    public int? PurchaseInvoiceId { get; set; }
    
    public bool IsTransactionPost1 { get; set; } = false;
    
    public List<AttachmentResponseDto> Attachments { get; set; }

    public DateTime CreatedAt { get; set; }
}