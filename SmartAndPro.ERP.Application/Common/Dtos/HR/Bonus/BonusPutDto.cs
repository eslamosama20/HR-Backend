using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.Bonus;

public class BonusPutDto
{
    [JsonInclude]
    [Required(ErrorMessage = "Id Field is required")]
    public Guid? Id { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "EmployeeId Field is required")]
    public Guid EmployeeId { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "Date Field is required")]
    public DateOnly Date { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "Amount Field is required")]
    public decimal Amount { get; set; }

    [JsonInclude]
    public string? Reason { get; set; }
}