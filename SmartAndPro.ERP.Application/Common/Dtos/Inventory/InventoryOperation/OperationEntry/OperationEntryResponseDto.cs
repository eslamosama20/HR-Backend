using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainPurchaseClass;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainSalesClass;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.StoreDetails;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.SupplierDetails;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.InventoryOperation.OperationEntry;

public class OperationEntryResponseDto
{
    public Guid Id { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; }
    
    public Guid MainPurchaseClassId { get; set; }
    public MainPurchaseClassResponseDto MainPurchaseClass { get; set; }
    
    public Guid? StoreDetailsId { get; set; }
    public StoreDetailsResponseDto StoreDetails { get; set; }
    
    
    
    public Guid InventoryOperationId { get; set; }
        
    public bool? IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}