namespace SmartAndPro.ERP.Application.Common.Dtos.Reports.InventoryStocks;

public class StoredItemsResponseDto
{
    public Guid MainPurchaseClassId { get; set; }
    public string MainPurchaseClassReferenceCode  { get; set; }
    public string MainPurchaseClassEnglishName { get; set; }
    public string MainPurchaseClassArabicName  { get; set; }
    
    public string UnitArabicName { get; set; }
    public string UnitEnglishName { get; set; }
    
    public decimal Quantity { get; set; }
    public decimal LastPrice { get; set; }
    public decimal LastMonthAverage { get; set; }
    public string? BatchNo { get; set; }
    public DateOnly? ExpiryDate { get; set; }

}