using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.CompanyDocument;

public class CompanyDocumentPostDto
{
    public string DocumentName { get; set; }
    public string DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public string? IssuingAuthority { get; set; }
    public DateOnly? IssueDate { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public int PreReminderDays { get; set; } = 30;
    public bool IsCritical { get; set; } = true;
    public string? Description { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Approved;
    public Guid? AttachmentId { get; set; }
}
