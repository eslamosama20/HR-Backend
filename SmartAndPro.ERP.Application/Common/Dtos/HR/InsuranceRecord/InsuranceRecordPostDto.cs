using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.InsuranceRecord;

public class InsuranceRecordPostDto
{
    public Guid EmployeeId { get; set; }
    public string InsuranceNumber { get; set; }
    public InsuranceType InsuranceType { get; set; }
    public string? InsuranceProvider { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public decimal? MonthlyPremium { get; set; }
    public decimal? CoverageAmount { get; set; }
    public int PreReminderDays { get; set; } = 30;
    public bool IsCritical { get; set; } = true;
    public string? Notes { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Approved;
    public Guid? AttachmentId { get; set; }
}
