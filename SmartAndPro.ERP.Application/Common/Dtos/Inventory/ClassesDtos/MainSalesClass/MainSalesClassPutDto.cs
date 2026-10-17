using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainSalesClass;

public class MainSalesClassPutDto
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
    [Required(ErrorMessage = "DetailSalesClassId Field is required")]
    public Guid DetailSalesClassId { get; set; }

    
    [JsonInclude]
    [Required(ErrorMessage = "MeasurementUnitId Field is required")]
    public Guid MeasurementUnitId { get; set; }
    
    [JsonInclude]
    public bool? IsTaxExempted { get; set; }

    public string? ClassType { get; set; }
    

    public string? Icon { get; set; }

    public bool? IsActive { get; set; } = true;

}