using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.TrailBalance;

public class TrailBalanceRequestDto
{
    [JsonInclude]
    [Required(ErrorMessage = "StartDate Field is required")]    
    public DateOnly StartDate { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "EndDate Field is required")]
    public DateOnly EndDate { get; set; }
    
    [JsonInclude]
    public string? Level { get; set; }
}