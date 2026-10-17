namespace SmartAndPro.ERP.Application.Common.Dtos.HR.SalarySlip;

public class SalarySlipResponseDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal? Allowances { get; set; }
    public decimal? Overtime { get; set; }
    public decimal? Bonus { get; set; }
    public decimal? Deductions { get; set; }
    public decimal? Insurance { get; set; }
    public decimal? Tax { get; set; }
    public decimal NetSalary { get; set; }
    public DateOnly? PaymentDate { get; set; }
    public bool IsPaid { get; set; }
    public string? Notes { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public int PreReminderDays { get; set; }
    public bool IsCritical { get; set; }
    public Guid? AttachmentId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
