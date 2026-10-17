using SmartAndPro.ERP.Domains.Entities.Inventory.StagnantInventoryManagement;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.StagnantInventory;

public class StagnantInventoryFiltersDto
{
    public Guid? StoreDetailsId { get; set; }
    
    public StagnantReason? Reason { get; set; }
    
    public bool? IsDisposed { get; set; }
    
    public bool? IsAutoAdded { get; set; }
    
    public DateTime? FromDate { get; set; }
    
    public DateTime? ToDate { get; set; }
    
    public string? SearchTerm { get; set; }
}
