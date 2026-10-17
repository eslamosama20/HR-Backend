namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.Transaction;

public class TransactionBulkDeleteRequestDto
{
    public List<Guid> Ids { get; set; } = new();
}