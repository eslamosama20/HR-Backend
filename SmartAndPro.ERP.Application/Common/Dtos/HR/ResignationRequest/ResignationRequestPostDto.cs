using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.ResignationRequest;

public class ResignationRequestPostDto
{
    public Guid EmployeeId { get; set; }
    public DateOnly RequestDate { get; set; }
    public DateOnly LastWorkingDay { get; set; }
    public string Reason { get; set; }
    public string? Description { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public int PreReminderDays { get; set; } = 7;
    public bool IsCritical { get; set; } = false;
    public Guid? AttachmentId { get; set; }
}
