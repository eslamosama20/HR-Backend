using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Notes.SalesCreditNoteItem;

public class SalesCreditNoteItemPutDto
{
    
    public Guid? Id { get; set; }
    
    public string? ItemNo { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "MainSalesClassId Field is required")]
    public Guid MainSalesClassId { get; set; }
    
    public int? NoOfNights { get; set; } = 1;
    [JsonInclude]
    [Required(ErrorMessage = "Price Field is required")]
    public decimal Price { get; set; }
    [JsonInclude]
    [Required(ErrorMessage = "Quantity Field is required")]
    public decimal Quantity { get; set; } = 1;
    
    public Guid? SalesCreditNoteId { get; set; }
    
    
    public bool? IsActive { get; set; } = true;

}