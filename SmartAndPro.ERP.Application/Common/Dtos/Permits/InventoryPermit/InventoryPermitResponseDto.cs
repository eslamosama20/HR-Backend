using SmartAndPro.ERP.Application.Common.Dtos.Accounting.MainAccount;
using SmartAndPro.ERP.Application.Common.Dtos.Attachments;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.StoreDetails;
using SmartAndPro.ERP.Application.Common.Dtos.Permits.InventoryPermitEntry;

namespace SmartAndPro.ERP.Application.Common.Dtos.Permits.InventoryPermit;

public class InventoryPermitResponseDto
{
    public Guid Id { get; set; }

    public string ReferenceNumber { get; set; }
    public string? Description { get; set; }
    
    public DateOnly PermitDate { get; set; }
    public Guid StoreDetailsId { get; set; }
    public Guid MainAccountId { get; set; }
    
    public StoreDetailsResponseDto StoreDetails { get; set; }
    public MainAccountResponseDto MainAccount { get; set; }

    public string PermitType { get; set; }
    
    public decimal ShippingCost { get; set; }
    
    public List<InventoryPermitEntryResponseDto> PermitEntries { get; set; }
    
    public List<AttachmentResponseDto> Attachments { get; set; }
    
    public Guid? TransactionId { get; set; }

    
    public bool? IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}