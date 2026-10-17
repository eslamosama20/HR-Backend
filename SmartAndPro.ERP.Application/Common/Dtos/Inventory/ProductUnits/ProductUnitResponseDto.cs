using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainPurchaseClass;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainSalesClass;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.StoreDetails;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.ProductUnits;

public class ProductUnitResponseDto
{
    public Guid Id { get; set; }
    
    public decimal? Quantity { get; set; } = 0;
    
    public Guid? MainSalesClassId { get; set; }
    public MainSalesClassResponseDto MainSalesClass { get; set; }
    
    public Guid? MainPurchaseClassId { get; set; }
    public MainPurchaseClassResponseDto MainPurchaseClass { get; set; }
    
    public Guid? StoreDetailsId { get; set; }
    public StoreDetailsResponseDto StoreDetails { get; set; }

}