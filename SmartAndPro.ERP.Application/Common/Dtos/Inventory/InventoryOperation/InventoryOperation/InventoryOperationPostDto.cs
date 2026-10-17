using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.InventoryOperation.OperationEntry;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.InventoryOperation.InventoryOperation;

public class InventoryOperationPostDto
{

    
    public string? ReferenceNumber { get; set; }
    
    public string? Description { get; set; }
    [JsonInclude]
    [Required(ErrorMessage = "OperationDate Field is required")]
    public DateTime OperationDate { get; set; }

    
    [JsonInclude]
    [Required(ErrorMessage = "OperationSource Field is required")]
    public string OperationSource { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "OperationEntries Field is required")]
    public List<OperationEntryPostDto>? OperationEntries { get; set; }
    
    public Guid? InventoryPermitId { get; set; }
    public int? PurchaseInvoiceId { get; set; }
    public Guid? TransactionId { get; set; }
    
    public Guid? SupplierDetailsId { get; set; }
    
    public Guid? InventoryStocktakingId { get; set; }
    
    public int? PurchaseReturnId { get; set; }
    
    public bool? IsActive { get; set; } = true;
}