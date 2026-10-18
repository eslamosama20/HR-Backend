namespace SmartAndPro.ERP.Application.Common.Dtos.HR;

public class Factory2PaymentResult
{
    public Guid EmployeeId { get; set; }
    public string? DeviceId { get; set; }
    public string EmployeeName { get; set; } = "";
    public string EmployeeTypeName { get; set; } = "";
    public string? Workshop { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }

    public decimal MonthlyRate { get; set; }
    public decimal SupplementaryRate { get; set; }
    public decimal LivingAllowance { get; set; }
    public decimal MealAllowance { get; set; }
    public decimal TransportAllowance { get; set; }
    public decimal HousingAllowance { get; set; }
    public decimal ExtraIncrease { get; set; }
    public decimal SupervisionBonus { get; set; }
    public decimal MasteryBonus { get; set; }
    public decimal RegularityBonus { get; set; }
    public decimal OtherAllowances { get; set; }

    public decimal TotalBonuses { get; set; }
    public decimal TotalSalary { get; set; }
    public decimal OvertimeBaseSalary { get; set; }

    public decimal AbsentDaysCount { get; set; }
    public decimal DailyRate { get; set; }
    public decimal TotalAbsentPenalty { get; set; }
    public decimal RegularityDeduction { get; set; }

    public double TotalLateHours { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal TotalLateEntryPenalty { get; set; }
    public decimal TotalLoans { get; set; }
    public decimal Insurance { get; set; }
    public decimal TotalPenalties { get; set; }
    public decimal TotalSubtractedAmount { get; set; }
    public decimal NetPay { get; set; }
}
