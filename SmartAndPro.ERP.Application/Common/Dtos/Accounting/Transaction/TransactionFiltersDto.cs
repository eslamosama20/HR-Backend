namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.Transaction;

public class TransactionFiltersDto
{
    public string? SearchKeyword { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Description { get; set; }
    public Guid? TransactionTypeId { get; set; }
    public DateOnly? FromTransactionDate { get; set; }
    public DateOnly? ToTransactionDate { get; set; }
    public DateOnly? TransactionDate { get; set; }
    public DateTime? FromCreatedAt { get; set; }
    public DateTime? ToCreatedAt { get; set; }
    public bool? IsActive { get; set; }
}