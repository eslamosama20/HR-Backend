using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.Stocktaking.StocktakingEntry;

public class StocktakingEntryPutDto
{
    public Guid? Id { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "ReportedQuantity Field is required")]
    public decimal ReportedQuantity { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "ActualQuantity Field is required")]
    public decimal ActualQuantity { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "MainPurchaseClassId Field is required")]
    public Guid MainPurchaseClassId { get; set; }
    
    
    [JsonInclude]
    [Required(ErrorMessage = "InventoryStocktakingId Field is required")]
    public Guid? InventoryStocktakingId { get; set; }
    
    public bool? IsActive { get; set; } = true;

}