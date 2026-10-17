using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.StoreDetails;

public class StoreDetailsPostDto
{
    [JsonInclude]
    [Required(ErrorMessage = "ArabicName Field is required")]
    public string? ArabicName { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "EnglishName Field is required")]
    public string? EnglishName { get; set; }


    public Guid? StoreStatusId { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "StoreType Field is required")]
    public string StoreType { get; set; }
    
    public Guid? MainAccountId { get; set; }

    public Guid? CompanyId { get; set; }

    public Guid? CreatedBy { get; set; }

    public bool? IsActive { get; set; } = true;

    public string? StoreCategory { get; set; }
}