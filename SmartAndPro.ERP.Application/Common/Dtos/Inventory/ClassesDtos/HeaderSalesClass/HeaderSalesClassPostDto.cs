using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.HeaderSalesClass;

public class HeaderSalesClassPostDto
{
    [JsonInclude]
    [Required(ErrorMessage = "ArabicName Field is required")]    
    public string ArabicName { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "EnglishName Field is required")]
    public string EnglishName { get; set; }
    

    public string? ReferenceCode { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "RootSalesClassId Field is required")]
    public Guid RootSalesClassId { get; set; }
    
    public string? Icon { get; set; }
    

    public Guid? ClientProfileId { get; set; }
    
    public bool? IsActive { get; set; } = true;
}