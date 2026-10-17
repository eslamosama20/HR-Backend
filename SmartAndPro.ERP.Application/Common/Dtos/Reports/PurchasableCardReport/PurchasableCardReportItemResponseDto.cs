namespace SmartAndPro.ERP.Application.Common.Dtos.Reports.PurchasableCardReport;

public class PurchasableCardReportItemResponseDto
{
    public string? StoreDetailsArabicName { get; set; }
    public string? StoreDetailsEnglishName { get; set; }
    public string UnitMeasurementName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}