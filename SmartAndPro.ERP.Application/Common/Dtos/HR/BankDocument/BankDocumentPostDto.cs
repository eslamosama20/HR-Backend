using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.BankDocument;

public class BankDocumentPostDto
{
    public Guid EmployeeId { get; set; }
    public string BankName { get; set; }
    public string DocumentType { get; set; }
    public string? AccountNumber { get; set; }
    public string? DocumentNumber { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public int PreReminderDays { get; set; } = 30;
    public bool IsCritical { get; set; } = false;
    public string? Notes { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Approved;
    public Guid? AttachmentId { get; set; }
}
