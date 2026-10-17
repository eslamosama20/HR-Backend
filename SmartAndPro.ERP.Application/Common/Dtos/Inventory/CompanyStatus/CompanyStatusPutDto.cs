using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyStatus;

public class CompanyStatusPutDto
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

    
    public bool? IsActive { get; set; } = true;

}