namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.Transaction;

/// <summary>
/// Returned by POST /api/accounting/transaction/import
/// </summary>
public class TransactionImportResultDto
{
    public int Imported { get; set; }
    public int Updated { get; set; }
    public List<string> Headers { get; set; } = new();
    public List<TransactionImportFailureDto> Failed { get; set; } = new();
}

public class TransactionImportFailureDto
{
    public int Row { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public List<string> Values { get; set; } = new();
}
