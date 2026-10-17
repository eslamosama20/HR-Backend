namespace SmartAndPro.ERP.Application.Common.Dtos.Permits.InventoryPermit;

public class PermitProductBarcodeResultDto
{
    public Guid Id { get; set; }
    public string ArabicName { get; set; } = string.Empty;
    public string EnglishName { get; set; } = string.Empty;
    public string ReferenceCode { get; set; } = string.Empty;
    public string ProductType { get; set; } = string.Empty;
}
