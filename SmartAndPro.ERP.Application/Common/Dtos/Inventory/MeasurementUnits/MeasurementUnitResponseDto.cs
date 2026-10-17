namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.MeasurementUnits;

public class MeasurementUnitResponseDto
{
    public Guid Id { get; set; }
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public string ArabicSymbol { get; set; }
    public string EnglishSymbol { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
}