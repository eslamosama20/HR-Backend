using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.BankDocument;

public class BankDocumentResponseDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public string BankName { get; set; }
    public string DocumentType { get; set; }
    public string? AccountNumber { get; set; }
    public string? DocumentNumber { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public int PreReminderDays { get; set; }
    public bool IsCritical { get; set; }
    public int DaysUntilExpiration { get; set; }
    public string? Notes { get; set; }
    public DocumentStatus Status { get; set; }
    public Guid? AttachmentId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
