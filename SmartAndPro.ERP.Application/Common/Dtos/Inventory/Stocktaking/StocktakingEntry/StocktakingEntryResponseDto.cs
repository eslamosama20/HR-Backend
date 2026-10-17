using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainPurchaseClass;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.StoreDetails;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.Stocktaking.StocktakingEntry;

public class StocktakingEntryResponseDto
{
    public Guid Id { get; set; }
    public decimal ReportedQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    
    public Guid MainPurchaseClassId { get; set; }
    public MainPurchaseClassResponseDto MainPurchaseClass { get; set; }
    
    public Guid InventoryStocktakingId { get; set; }
    
    public bool? IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}