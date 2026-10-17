namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.Transaction;

public class TransactionBulkDeleteFailureDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string ReasonCode { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string ReasonAr { get; set; } = string.Empty;
}