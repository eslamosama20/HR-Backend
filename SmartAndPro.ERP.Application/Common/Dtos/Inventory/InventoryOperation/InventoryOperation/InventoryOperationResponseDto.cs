using SmartAndPro.ERP.Application.Common.Dtos.Inventory.InventoryOperation.OperationEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.SupplierDetails;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Purchase;
using SmartAndPro.ERP.Application.Common.Dtos.Permits.InventoryPermit;
using SmartAndPro.ERP.Application.Common.Dtos.Return.Purchase;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.InventoryOperation.InventoryOperation;

public class InventoryOperationResponseDto
{
    public Guid Id { get; set; }

    public string ReferenceNumber { get; set; }
    public string? Description { get; set; }
    
    public DateTime? OperationDate { get; set; }
    
    public string OperationSource { get; set; }
    
    public List<OperationEntryResponseDto> OperationEntries { get; set; }
    
    public Guid? InventoryPermitId { get; set; }
    public InventoryPermitResponseDto? InventoryPermit { get; set; }
    
    public int? PurchaseInvoiceId { get; set; }
    public PurchaseInvoiceResponseDto? PurchaseInvoice { get; set; }
    
    public int? PurchaseReturnId { get; set; }
    public PurchaseReturnResponseDto? PurchaseReturn { get; set; }
    public Guid? TransactionId { get; set; }
    
    public Guid? SupplierDetailsId { get; set; }
    
    public Guid? InventoryStocktakingId { get; set; }

    public SupplierDetailsResponseDto SupplierDetails { get; set; }
    
    public bool? IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}