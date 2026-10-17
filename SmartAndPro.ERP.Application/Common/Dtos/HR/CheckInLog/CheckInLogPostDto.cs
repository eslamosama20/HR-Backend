using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.CheckInLog;

public class CheckInLogPostDto
{   
    
    [JsonInclude]
    [Required(ErrorMessage = "EmployeeId Field is required")]
    public Guid EmployeeId { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "Time Field is required")]
    public DateTime Time { get; set; }

    public string LogType { get; set; }
}