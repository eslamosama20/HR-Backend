namespace SmartAndPro.ERP.Application.Common.Dtos.Reports.InventoryStocks;

public class InventoryStocksResponseDto
{
    public Guid StoreDetailsId { get; set; }
    public string StoreDetailsEnglishName { get; set; }
    public string StoreDetailsArabicName { get; set; }
    
    public List<StoredItemsResponseDto> StoredItems  { get; set; }
}