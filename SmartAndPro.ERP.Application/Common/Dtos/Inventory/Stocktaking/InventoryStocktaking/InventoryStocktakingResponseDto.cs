using SmartAndPro.ERP.Application.Common.Dtos.Attachments;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.Stocktaking.StocktakingEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.StoreDetails;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.SupplierDetails;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Purchase;
using SmartAndPro.ERP.Application.Common.Dtos.Permits.InventoryPermit;
using SmartAndPro.ERP.Application.Common.Dtos.Return.Purchase;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.Stocktaking.InventoryStocktaking;

public class InventoryStocktakingResponseDto
{
    public Guid Id { get; set; }

    public string ReferenceNumber { get; set; }
    public string? Description { get; set; }
    
    public DateTime? StocktakingDate { get; set; }
    
    public List<StocktakingEntryResponseDto>? StocktakingEntries { get; set; }
    
    public Guid? StoreDetailsId { get; set; }
    public StoreDetailsResponseDto StoreDetails { get; set; }
    
    public Guid? TransactionId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public List<AttachmentResponseDto> Attachments { get; set; }
    
    public bool IsActive { get; set; }
}