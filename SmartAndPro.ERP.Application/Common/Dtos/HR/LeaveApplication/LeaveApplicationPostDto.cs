using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.LeaveApplication;

public class LeaveApplicationPostDto
{
    public Guid EmployeeId { get; set; }
    public LeaveType LeaveType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int TotalDays { get; set; }
    public string Reason { get; set; }
    public string? Notes { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public int PreReminderDays { get; set; } = 3;
    public bool IsCritical { get; set; } = false;
    public Guid? AttachmentId { get; set; }
}
