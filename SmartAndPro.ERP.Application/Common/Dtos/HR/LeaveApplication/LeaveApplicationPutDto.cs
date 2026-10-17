using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.LeaveApplication;

public class LeaveApplicationPutDto
{
    public Guid Id { get; set; }
    public string? ReferenceNumber { get; set; }
    public Guid EmployeeId { get; set; }
    public LeaveType LeaveType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int TotalDays { get; set; }
    public string Reason { get; set; }
    public string? Notes { get; set; }
    public LeaveStatus Status { get; set; }
    public string? RejectionReason { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public int PreReminderDays { get; set; }
    public bool IsCritical { get; set; }
    public Guid? AttachmentId { get; set; }
}
