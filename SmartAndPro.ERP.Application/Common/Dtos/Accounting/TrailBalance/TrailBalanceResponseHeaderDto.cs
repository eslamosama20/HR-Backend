namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.TrailBalance;

public class TrailBalanceResponseHeaderDto
{
    public string AccountNumber { get; set; }
    public string AccountEnglishName { get; set; }
    public string AccountArabicName { get; set; }
    public decimal PeriodDebit { get; set; } = 0;
    public decimal PeriodCredit { get; set; } = 0;
    public decimal PreviousDebit { get; set; } = 0;
    public decimal PreviousCredit { get; set; } = 0;
    public decimal BalanceDebit { get; set; } = 0;
    public decimal BalanceCredit { get; set; } = 0;
    public decimal Balance { get; set; } = 0;

    public List<TrailBalanceResponseDetailDto> Details { get; set; }
}