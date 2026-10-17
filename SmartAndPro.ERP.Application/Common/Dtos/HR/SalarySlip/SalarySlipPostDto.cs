namespace SmartAndPro.ERP.Application.Common.Dtos.HR.SalarySlip;

public class SalarySlipPostDto
{
    public Guid EmployeeId { get; set; }
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
    public string? Notes { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public int PreReminderDays { get; set; } = 7;
    public bool IsCritical { get; set; } = false;
    public Guid? AttachmentId { get; set; }
}
