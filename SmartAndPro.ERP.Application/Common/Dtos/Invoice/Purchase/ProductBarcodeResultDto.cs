namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Purchase;

public class ProductBarcodeResultDto
{
    public Guid Id { get; set; }
    public string ArabicName { get; set; } = string.Empty;
    public string EnglishName { get; set; } = string.Empty;
    public string ReferenceCode { get; set; } = string.Empty;
    public string MeasurementUnitName { get; set; } = string.Empty;
}
