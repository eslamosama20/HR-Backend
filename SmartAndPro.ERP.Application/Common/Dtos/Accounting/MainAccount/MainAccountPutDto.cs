using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.MainAccount;

public class MainAccountPutDto
{
    [JsonInclude]
    [Required(ErrorMessage = "Id Field is required")]
    public Guid Id { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "ArabicName Field is required")]    
    public string ArabicName { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "EnglishName Field is required")]
    public string EnglishName { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "ReferenceCode Field is required")]
    public string ReferenceCode { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "DetailAccountId Field is required")]
    public Guid DetailAccountId { get; set; }
    

    public string? Icon { get; set; }

    public bool? IsActive { get; set; } = true;

}