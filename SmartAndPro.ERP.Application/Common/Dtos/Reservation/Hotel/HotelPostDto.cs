using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.Hotel;

public class HotelPostDto
{
    [JsonInclude]
    [Required(ErrorMessage = "ArabicName Field is required")]
    public string ArabicName { get; set; } = string.Empty;

    //[JsonInclude]
    public string? EnglishName { get; set; }

    public Guid? MainAccountId { get; set; }

    public bool? IsActive { get; set; } = true;

    public bool IsFrontal { get; set; } = false;

    public string? Logo { get; set; }
    public string? LogoUrl { get; set; }
    public string? LogoPublicId { get; set; }
    public string? ContactNumber { get; set; }

    public string? Email { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? IBAN { get; set; }
    
    //[JsonInclude]
    //public string TaxNumber { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }

    public string? Street { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? ShortAddress { get; set; }
    public string? BuildingNumber { get; set; }
    public string? PostalCode { get; set; }
    public string? Website { get; set; }
    public string? AdditionalContactNumber { get; set; }
    public string? Fax { get; set; }
    public string? GaztNumber { get; set; }
    public string? SubNumber { get; set; }
    public string? Instructions { get; set; }
    public string? OtherId { get; set; }
}
