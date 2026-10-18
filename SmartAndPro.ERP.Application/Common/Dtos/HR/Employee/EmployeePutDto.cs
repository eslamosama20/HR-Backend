using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.Employee;

public class EmployeePutDto
{
    [JsonInclude]
    [Required(ErrorMessage = "Id Field is required")]
    public Guid? Id { get; set; }
    
    public string? ReferenceNumber { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "Name Field is required")]
    public string Name { get; set; }
    
    public string? Phone { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "EmploymentTypeId Field is required")]
    public Guid EmploymentTypeId { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "ShiftId Field is required")]
    public Guid ShiftId { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "StartDate Field is required")]
    public DateOnly StartDate { get; set; }
    
    public DateOnly? EndDate { get; set; }
    
    public decimal? MonthlyInsurance { get; set; }
    
    public int LeaveBalance { get; set; } = 0;
    
    [JsonInclude]
    [Required(ErrorMessage = "MonthlyRate Field is required")]
    public decimal MonthlyRate { get; set; }
    [JsonInclude]
    [Required(ErrorMessage = "SupplementaryRate Field is required")]
    public decimal SupplementaryRate { get; set; }
    
    public decimal? OvertimeRateMultiplier { get; set; } = 1.5m;
    public decimal? AbsentRateMultiplier { get; set; } = 1.5m;
    public decimal? LateRateMultiplier { get; set; } = 1.5m;
    
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
    public decimal? LivingAllowance { get; set; } = 0;
    public decimal? MealAllowance { get; set; } = 0;
    public decimal? TransportAllowance { get; set; } = 0;
    public decimal? HousingAllowance { get; set; } = 0;
    public decimal? ExtraIncrease { get; set; } = 0;
    public decimal? SupervisionBonus { get; set; } = 0;
    public decimal? MasteryBonus { get; set; } = 0;
    public decimal? RegularityBonus { get; set; } = 0;

    public string? DeviceId { get; set; }

}