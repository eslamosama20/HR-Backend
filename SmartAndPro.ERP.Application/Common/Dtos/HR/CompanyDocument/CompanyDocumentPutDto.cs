using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.CompanyDocument;

public class CompanyDocumentPutDto
{
    public Guid Id { get; set; }
    public string? ReferenceNumber { get; set; }
    public string DocumentName { get; set; }
    public string DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public string? IssuingAuthority { get; set; }
    public DateOnly? IssueDate { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public int PreReminderDays { get; set; }
    public bool IsCritical { get; set; }
    public string? Description { get; set; }
    public DocumentStatus Status { get; set; }
    public Guid? AttachmentId { get; set; }
}
