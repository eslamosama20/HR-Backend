using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.EmploymentType;

public class EmploymentTypePutDto
{
    [JsonInclude]
    [Required(ErrorMessage = "Id Field is required")]
    public Guid? Id { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "Code Field is required")]
    public string Code { get; set; }
    
    public string? Description { get; set; }

    public string? WeeklyDayOff { get; set; }

}