using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.InventoryOperation.OperationEntry;

public class OperationEntryPostDto
{
    [JsonInclude]
    [Required(ErrorMessage = "UnitPrice Field is required")]
    public decimal UnitPrice { get; set; }  
    
    [JsonInclude]
    [Required(ErrorMessage = "Quantity Field is required")]
    public decimal Quantity { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "MainPurchaseClassId Field is required")]
    public Guid MainPurchaseClassId { get; set; }
    
    public Guid? StoreDetailsId { get; set; }
    
    
    public Guid? InventoryOperationId { get; set; }
    
    public bool? IsActive { get; set; } = true;
}