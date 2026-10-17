using SmartAndPro.ERP.Application.Common.Dtos.Accounting.MainAccount;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainSalesClass;

namespace SmartAndPro.ERP.Application.Common.Dtos.Notes.SalesCreditNoteItem;

public class SalesCreditNoteItemResponseDto
{
    public Guid Id { get; set; }

    public string? ItemNo { get; set; }

    public Guid MainSalesClassId { get; set; }
    public MainSalesClassResponseDto MainSalesClass { get; set; }
    public int? NoOfNights { get; set; } = 1;
    public decimal Price { get; set; }
    public decimal Quantity { get; set; } = 1;
    public Guid SalesCreditNoteId { get; set; }
    
    public bool? IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

}