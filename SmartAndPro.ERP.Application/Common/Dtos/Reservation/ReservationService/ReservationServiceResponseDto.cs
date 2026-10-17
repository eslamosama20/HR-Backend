using SmartAndPro.ERP.Application.Common.Dtos.Accounting.MainAccount;

namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationService;

public class ReservationServiceResponseDto
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public string ArabicName { get; set; } = string.Empty;

    public string EnglishName { get; set; } = string.Empty;

    public Guid? RevenueMainAccountId { get; set; }

    public Guid? ExpenseMainAccountId { get; set; }

    public MainAccountResponseDto? RevenueMainAccount { get; set; }

    public MainAccountResponseDto? ExpenseMainAccount { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsFrontal { get; set; } = false;
    public Guid? SalesClassId { get; set; }

    public string? LogoUrl { get; set; }
    public string? LogoPublicId { get; set; }
    public string? ContactNumber { get; set; }
    public string? Email { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? IBAN { get; set; }
    public string? TaxNumber { get; set; }
}
