namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.TransactionType;

public class TransactionTypeResponseDto
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public string ArabicName { get; set; }

    public string EnglishName { get; set; }

    public DateTime CreatedAt { get; set; }
}