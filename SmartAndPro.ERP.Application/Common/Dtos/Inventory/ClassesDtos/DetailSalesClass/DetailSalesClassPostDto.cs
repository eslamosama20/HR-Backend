using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.DetailSalesClass;

public class DetailSalesClassPostDto
{
    [JsonInclude]
    [Required(ErrorMessage = "ArabicName Field is required")]    
    public string ArabicName { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "EnglishName Field is required")]
    public string EnglishName { get; set; }
    

    public string? ReferenceCode { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "HeaderSalesClassId Field is required")]
    public Guid HeaderSalesClassId { get; set; }
    
    public string? Icon { get; set; }

    public Guid? ClientProfileId { get; set; }
    
    public bool? IsActive { get; set; } = true;
    public bool IsForProjects { get; set; } = false;
}