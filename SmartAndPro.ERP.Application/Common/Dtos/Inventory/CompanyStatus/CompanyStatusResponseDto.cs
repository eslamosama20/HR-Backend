namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyStatus;

public class CompanyStatusResponseDto
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public string ArabicName { get; set; }

    public string EnglishName { get; set; }
    public DateTime CreatedAt { get; set; }
}