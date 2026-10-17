namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.AccountStatement;

public class AccountStatementRecordDto
{
    // 1. رقم القيد
    public string TransactionNumber { get; set; }
    public Guid TransactionId { get; set; }
    
    // 2. تاريخ القيد
    public DateOnly TransactionDate { get; set; }
    
    // 3. مدين
    public decimal DebitAmount { get; set; }
    
    // 4. دائن
    public decimal CreditAmount { get; set; }
    
    // 5. الرصيد
    public decimal Balance { get; set; }
    
    // 6. نوع القيد
    public string TransactionTypeArabicName { get; set; }
    public string TransactionTypeEnglishName { get; set; }
    
    // 7. الوصف
    public string? Description { get; set; }
    
    // خصائص إضافية
    public bool IsTransactionPost1 { get; set; } = false;
    public Guid AccountId { get; set; }
}