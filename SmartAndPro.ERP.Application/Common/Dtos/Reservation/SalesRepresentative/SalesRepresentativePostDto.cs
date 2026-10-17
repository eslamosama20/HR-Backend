using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.SalesRepresentative;

public class SalesRepresentativePostDto
{
    [JsonInclude]
    [Required(ErrorMessage = "ArabicName Field is required")]
    public string ArabicName { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "EnglishName Field is required")]
    public string EnglishName { get; set; }

    public Guid? MainAccountId { get; set; }

    public bool? IsActive { get; set; } = true;

    public bool IsFrontal { get; set; } = false;
}