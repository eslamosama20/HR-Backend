using SmartAndPro.ERP.Application.Common.Dtos.Accounting.MainAccount;

namespace SmartAndPro.ERP.Application.Common.Dtos.Receipts.PaycheckReceiptAccountResponse;

public class PaycheckReceiptAccountResponseDto
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public decimal Amount { get; set; }
    
    public decimal Tax { get; set; }
    
    public string? Description { get; set; }

    public Guid PaycheckReceiptId { get; set; }
    

    public Guid MainAccountId { get; set; }
    
    public MainAccountResponseDto MainAccount { get; set; }

    public DateTime CreatedAt { get; set; }
}