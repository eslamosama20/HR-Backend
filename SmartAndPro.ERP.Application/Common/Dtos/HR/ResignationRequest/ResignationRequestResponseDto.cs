using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.ResignationRequest;

public class ResignationRequestResponseDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public DateOnly RequestDate { get; set; }
    public DateOnly LastWorkingDay { get; set; }
    public string Reason { get; set; }
    public string? Description { get; set; }
    public DocumentStatus Status { get; set; }
    public Guid? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public int PreReminderDays { get; set; }
    public bool IsCritical { get; set; }
    public Guid? AttachmentId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
