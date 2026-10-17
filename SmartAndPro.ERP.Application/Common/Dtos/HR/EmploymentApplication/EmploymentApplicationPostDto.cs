using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.EmploymentApplication;

public class EmploymentApplicationPostDto
{
    public string ApplicantName { get; set; }
    public string? ApplicantPhone { get; set; }
    public string? ApplicantEmail { get; set; }
    public string? ApplicantIdNumber { get; set; }
    public string Position { get; set; }
    public string? Department { get; set; }
    public DateOnly ApplicationDate { get; set; }
    public string? Experience { get; set; }
    public string? Education { get; set; }
    public decimal? ExpectedSalary { get; set; }
    public string? Notes { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public int PreReminderDays { get; set; } = 14;
    public bool IsCritical { get; set; } = false;
    public Guid? AttachmentId { get; set; }
}
