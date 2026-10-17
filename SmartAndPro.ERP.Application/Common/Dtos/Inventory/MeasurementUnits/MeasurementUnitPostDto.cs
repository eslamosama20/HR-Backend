using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.MeasurementUnits;

public class MeasurementUnitPostDto
{
    [JsonInclude]
    [Required(ErrorMessage = "ArabicName Field is required")]
    public string ArabicName { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "EnglishName Field is required")]
    public string EnglishName { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "ArabicSymbol Field is required")]
    public string ArabicSymbol { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "EnglishSymbol Field is required")]
    public string EnglishSymbol { get; set; }

    public bool? IsActive { get; set; } = true;


    public Guid? CreatedBy { get; set; }
}