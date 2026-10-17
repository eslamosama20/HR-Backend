using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.Stocktaking.StocktakingEntry;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.Stocktaking.InventoryStocktaking;

public class InventoryStocktakingPutDto
{
    [JsonInclude]
    [Required(ErrorMessage = "Id Field is required")]
    public Guid? Id { get; set; }
    
    public string? ReferenceNumber { get; set; }
    
    public string? Description { get; set; }
    
    public DateTime? StocktakingDate { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "StocktakingEntries Field is required")]
    public List<StocktakingEntryPutDto>? StocktakingEntries { get; set; }
    
    public Guid? StoreDetailsId { get; set; }
    
    public bool? IsActive { get; set; } = true;
}