using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.CheckInLog;

public class CheckInLogPutDto
{
    [JsonInclude]
    [Required(ErrorMessage = "Id Field is required")]
    public Guid? Id { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "EmployeeId Field is required")]
    public Guid EmployeeId { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "Time Field is required")]
    public DateTime Time { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "LogType Field is required")]
    public string LogType { get; set; }

}