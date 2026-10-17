using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainPurchaseClass;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainSalesClass;

namespace SmartAndPro.ERP.Application.Common.Dtos.Permits.InventoryPermitEntry;

public class InventoryPermitEntryResponseDto
{
    public Guid Id { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; }
    
    public decimal RetailPrice { get; set; }
    public decimal WholesalePrice { get; set; }
    public decimal BulkPrice { get; set; }
    
    public Guid? MainPurchaseClassId { get; set; }
    public Guid? MainSalesClassId { get; set; }
    
    public string? ShelfNumber { get; set; }
    public string? RowNumber { get; set; }
    public string? BatchNo { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    
    public MainPurchaseClassResponseDto? MainPurchaseClass { get; set; }
    public MainSalesClassResponseDto? MainSalesClass { get; set; }
    
    public Guid InventoryPermitId { get; set; }
        
    public bool? IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}