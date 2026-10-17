using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.Stocktaking.StocktakingEntry;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.Stocktaking.InventoryStocktaking;

public class InventoryStocktakingPostDto
{

    public string? ReferenceNumber { get; set; }
    
    public string? Description { get; set; }
    
    public DateTime? StocktakingDate { get; set; } 

    [JsonInclude]
    [Required(ErrorMessage = "StocktakingEntries Field is required")]
    public List<StocktakingEntryPostDto>? StocktakingEntries { get; set; }
    
    public Guid? StoreDetailsId { get; set; }
    
    public bool? IsActive { get; set; } = true;
}