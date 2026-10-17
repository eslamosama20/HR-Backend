using SmartAndPro.ERP.Application.Common.Dtos.Accounting.MainAccount;

namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.Transaction;

public class TransactionEntryChildResponseDto
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public decimal CreditAmount { get; set; }
    
    public decimal DebitAmount { get; set; }
    
    public string? Description { get; set; }

    public Guid TransactionId { get; set; }
    
     public Guid MainAccountId { get; set; }
    
    public MainAccountResponseDto MainAccount { get; set; }

    public DateTime CreatedAt { get; set; }
}