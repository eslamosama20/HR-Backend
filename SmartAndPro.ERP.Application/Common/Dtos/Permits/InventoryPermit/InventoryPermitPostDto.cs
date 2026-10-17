using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SmartAndPro.ERP.Application.Common.Dtos.Permits.InventoryPermitEntry;

namespace SmartAndPro.ERP.Application.Common.Dtos.Permits.InventoryPermit;

public class InventoryPermitPostDto
{

    
    public string? ReferenceNumber { get; set; }
    
    public string? Description { get; set; }
    [JsonInclude]
    [Required(ErrorMessage = "PermitDate Field is required")]
    public DateOnly? PermitDate { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "StoreDetailsId Field is required")]
    public Guid StoreDetailsId { get; set; }
    [JsonInclude]
    [Required(ErrorMessage = "MainAccountId Field is required")]
    public Guid MainAccountId { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "PermitEntries Field is required")]
    public List<InventoryPermitEntryPostDto>? PermitEntries { get; set; }
    
    public string PermitType { get; set; }
    
    [JsonInclude]
    public decimal? ShippingCost { get; set; }

    
    public bool? IsActive { get; set; } = true;
}