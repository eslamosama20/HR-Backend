using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationService;

public class ReservationServicePostDto
{
    [JsonInclude]
    [Required(ErrorMessage = "ArabicName Field is required")]
    public string ArabicName { get; set; } = string.Empty;

    [JsonInclude]
    [Required(ErrorMessage = "EnglishName Field is required")]
    public string EnglishName { get; set; } = string.Empty;

    public Guid? RevenueMainAccountId { get; set; }

    public Guid? RevenueDetailAccountId { get; set; }

    public Guid? ExpenseMainAccountId { get; set; }

    public Guid? ExpenseDetailAccountId { get; set; }

    public bool? IsActive { get; set; } = true;

    public bool IsFrontal { get; set; } = false;
    public Guid? SalesClassId { get; set; }

    public string? Logo { get; set; }
    public string? LogoUrl { get; set; }
    public string? LogoPublicId { get; set; }
    public string? ContactNumber { get; set; }
    public string? Email { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? IBAN { get; set; }
    public string? TaxNumber { get; set; }
}
