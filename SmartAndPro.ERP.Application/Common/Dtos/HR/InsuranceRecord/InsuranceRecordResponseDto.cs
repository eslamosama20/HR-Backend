using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.InsuranceRecord;

public class InsuranceRecordResponseDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public string InsuranceNumber { get; set; }
    public InsuranceType InsuranceType { get; set; }
    public string? InsuranceProvider { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public decimal? MonthlyPremium { get; set; }
    public decimal? CoverageAmount { get; set; }
    public int PreReminderDays { get; set; }
    public bool IsCritical { get; set; }
    public int DaysUntilExpiration { get; set; }
    public string? Notes { get; set; }
    public DocumentStatus Status { get; set; }
    public Guid? AttachmentId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
