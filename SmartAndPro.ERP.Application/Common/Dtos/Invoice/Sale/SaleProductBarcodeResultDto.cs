namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale;

public class SaleProductBarcodeResultDto
{
    public Guid Id { get; set; }
    public string ArabicName { get; set; } = string.Empty;
    public string EnglishName { get; set; } = string.Empty;
    public string ReferenceCode { get; set; } = string.Empty;
    public string MeasurementUnitName { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public decimal? AvailableQuantity { get; set; }
}
