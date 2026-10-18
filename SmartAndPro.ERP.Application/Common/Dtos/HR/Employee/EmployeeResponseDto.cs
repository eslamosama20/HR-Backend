using SmartAndPro.ERP.Application.Common.Dtos.HR.Attendance;
using SmartAndPro.ERP.Application.Common.Dtos.HR.EmploymentType;
using SmartAndPro.ERP.Application.Common.Dtos.HR.Shift;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.Employee;

public class EmployeeResponseDto
{
    public Guid Id { get; set; }

    public string ReferenceNumber { get; set; }
    public string Name { get; set; }
    public string? Phone { get; set; }
    
    public Guid EmploymentTypeId { get; set; }
    public EmploymentTypeResponseDto EmploymentType { get; set; }
    
    public Guid ShiftId { get; set; }
    public ShiftResponseDto Shift { get; set; }
    
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    
    public decimal? MonthlyInsurance { get; set; }
    
    public int LeaveBalance { get; set; } = 0;
    
    public int? RemainingLeaveBalance { get; set; }
    public decimal MonthlyRate { get; set; }
    public decimal SupplementaryRate { get; set; }
    public decimal OvertimeRateMultiplier { get; set; } = 1.5m;
    public decimal AbsentRateMultiplier { get; set; } = 1.5m;
    public decimal LateRateMultiplier { get; set; } = 1.5m;
    public string? FactoryType { get; set; }
    
    // Falcon Factory fields
    public string? Department { get; set; }
    public string? Workshop { get; set; }
    public string? JobTitle { get; set; }
    public string? NationalId { get; set; }
    public string? Address { get; set; }
    public string? Governorate { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? InsuranceNumber { get; set; }
    public string? InsuranceTitle { get; set; }

    // Detailed Allowances & Bonuses
    public decimal LivingAllowance { get; set; } = 0;
    public decimal MealAllowance { get; set; } = 0;
    public decimal TransportAllowance { get; set; } = 0;
    public decimal HousingAllowance { get; set; } = 0;
    public decimal ExtraIncrease { get; set; } = 0;
    public decimal SupervisionBonus { get; set; } = 0;
    public decimal MasteryBonus { get; set; } = 0;
    public decimal RegularityBonus { get; set; } = 0;

    public decimal TotalGrossSalary => MonthlyRate + LivingAllowance + MealAllowance + TransportAllowance + HousingAllowance + ExtraIncrease + SupervisionBonus + MasteryBonus + RegularityBonus;
    public decimal OvertimeBaseSalary => MonthlyRate + LivingAllowance + MealAllowance + TransportAllowance;

    public string? DeviceId { get; set; }

    public bool? IsActive { get; set; } = true;
    
    public List<AttendanceResponseDto>? AttendanceRecords { get; set; }
    
    public DateTime CreatedAt { get; set; }
}