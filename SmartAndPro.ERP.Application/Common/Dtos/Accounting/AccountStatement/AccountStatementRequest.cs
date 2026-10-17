using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.AccountStatement;

public class AccountStatementRequest
{
    [JsonInclude]
    [Required(ErrorMessage = "StartDate Field is required")]    
    public DateOnly StartDate { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "EndDate Field is required")]
    public DateOnly EndDate { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "AccountId Field is required")]
    public Guid AccountId { get; set; }
    
    public Guid? ProjectId { get; set; }
    
    public bool IsFrontal { get; set; } = false;
}