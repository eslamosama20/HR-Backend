using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainPurchaseClass;

public class MainPurchaseClassPostDto
{
    [JsonInclude]
    [Required(ErrorMessage = "ArabicName Field is required")]    
    public string ArabicName { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "EnglishName Field is required")]
    public string EnglishName { get; set; }
    

    public string? ReferenceCode { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "DetailPurchaseClassId Field is required")]
    public Guid DetailPurchaseClassId { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "MeasurementUnitId Field is required")]
    public Guid MeasurementUnitId { get; set; }

    public string? Icon { get; set; }

    public string? ImageUrl { get; set; }

    public Guid? ClientProfileId { get; set; }
    
    public bool? IsActive { get; set; } = true;

    // Pharmacy fields
    public string? ItemType { get; set; }
    public bool NeedsBatch { get; set; }
    public bool NeedsExpiry { get; set; }
    public bool IsPharmacyItem { get; set; }
    public string? SfdaCode { get; set; }
    public bool IsControlled { get; set; }
}