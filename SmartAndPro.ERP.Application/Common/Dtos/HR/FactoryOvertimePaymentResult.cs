namespace SmartAndPro.ERP.Application.Common.Dtos.HR;

public class FactoryOvertimePaymentResult
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = "";
    public string EmployeeTypeName { get; set; } = "";
    public decimal MonthlyRate { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal OvertimePayment { get; set; }
}