namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.FinancePeriods;

public class FinancePeriodsResponseDto
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public string ArabicName { get; set; }

    public string EnglishName { get; set; }
    public DateTime CreatedAt { get; set; }
}