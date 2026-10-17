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
    
    public string? DeviceId { get; set; }

}