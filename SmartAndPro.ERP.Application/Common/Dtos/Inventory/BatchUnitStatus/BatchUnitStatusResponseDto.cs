namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.BatchUnitStatus;

public class BatchUnitStatusResponseDto
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public string ArabicName { get; set; }

    public string EnglishName { get; set; }
    public DateTime CreatedAt { get; set; }
}