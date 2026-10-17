using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.AccountStatement;

public class AccountStatementShareRequest
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
    
    [JsonInclude]
    [Required(ErrorMessage = "Method Field is required")]
    public string Method { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "Receiver Field is required")]
    public string Receiver { get; set; }
}

