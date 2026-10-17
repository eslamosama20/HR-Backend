namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.MeasurementUnits;

public class MeasurementUnitFiltersDto
{
    public string? ArabicName { get; set; }
    public string? EnglishName { get; set; }
    public string? ArabicSymbol { get; set; }
    public string? EnglishSymbol { get; set; }
    public Guid? CreatedBy { get; set; }
    public string? SearchKeyword { get; set; }
}