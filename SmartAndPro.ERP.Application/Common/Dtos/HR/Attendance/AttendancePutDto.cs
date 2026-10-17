using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.Attendance;

public class AttendancePutDto
{
    [JsonInclude]
    [Required(ErrorMessage = "Id Field is required")]
    public Guid? Id { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "EmployeeId Field is required")]
    public Guid EmployeeId { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "AttendanceDate Field is required")]
    public DateOnly AttendanceDate { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "AttendanceFlag Field is required")]
    public string AttendanceFlag { get; set; }

    [JsonInclude]
    public string? Reason { get; set; }
}