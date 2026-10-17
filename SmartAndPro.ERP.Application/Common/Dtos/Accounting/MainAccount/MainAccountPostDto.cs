using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.MainAccount;

public class MainAccountPostDto
{
    [JsonInclude]
    [Required(ErrorMessage = "ArabicName Field is required")]    
    public string ArabicName { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "EnglishName Field is required")]
    public string EnglishName { get; set; }
    

    public string? ReferenceCode { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "DetailAccountId Field is required")]
    public Guid DetailAccountId { get; set; }
    
    public string? Icon { get; set; }

    public Guid? ClientProfileId { get; set; }
    
    public bool? IsActive { get; set; } = true;
}