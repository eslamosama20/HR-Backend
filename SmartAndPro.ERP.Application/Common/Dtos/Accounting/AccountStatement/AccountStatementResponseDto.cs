namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.AccountStatement;

public class AccountStatementResponseDto
{
    public decimal PreviousDebit { get; set; } = 0;
    public decimal PreviousCredit { get; set; } = 0;
    public List<AccountStatementRecordDto> AccountStatementRecords { get; set; } = new List<AccountStatementRecordDto>();
}