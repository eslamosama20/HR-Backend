using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.Shift;

public class ShiftPostDto
{   
    public string? ReferenceNumber { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "Name Field is required")]
    public string? Name { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "StartTime Field is required")]
    public TimeOnly? StartTime { get; set; }
    [JsonInclude]
    [Required(ErrorMessage = "EndTime Field is required")]
    public TimeOnly? EndTime { get; set; }

    
    public int? GracePeriodInMinutes { get; set; } = 0;
}