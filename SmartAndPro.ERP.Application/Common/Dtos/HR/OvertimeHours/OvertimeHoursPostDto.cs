using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.OvertimeHours;

public class OvertimeHoursPostDto
{   
    
    [JsonInclude]
    [Required(ErrorMessage = "EmployeeId Field is required")]
    public Guid EmployeeId { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "Date Field is required")]
    public DateOnly Date { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "HoursCount Field is required")]
    public decimal HoursCount { get; set; }
    
    [JsonInclude]
    public string? Reason { get; set; }
}