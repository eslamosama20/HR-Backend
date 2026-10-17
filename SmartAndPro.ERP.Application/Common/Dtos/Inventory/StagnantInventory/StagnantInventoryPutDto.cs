using SmartAndPro.ERP.Domains.Entities.Inventory.StagnantInventoryManagement;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.StagnantInventory;

public class StagnantInventoryPutDto
{
    public Guid Id { get; set; }
    
    public decimal? Quantity { get; set; }
    
    public string? Notes { get; set; }
    
    public bool? IsDisposed { get; set; }
    
    public DateTime? DisposedDate { get; set; }
    
    public string? DisposalMethod { get; set; }
}
