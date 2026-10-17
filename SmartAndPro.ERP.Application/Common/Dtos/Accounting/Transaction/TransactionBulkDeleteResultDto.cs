namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.Transaction;

public class TransactionBulkDeleteResultDto
{
    public int Requested { get; set; }
    public int Deleted { get; set; }
    public List<TransactionBulkDeleteFailureDto> Failed { get; set; } = new();
}