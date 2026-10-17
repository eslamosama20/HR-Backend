using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationView;

public class ReservationViewPutDto
{
    [JsonInclude]
    [Required(ErrorMessage = "Id Field is required")]
    public Guid Id { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "ArabicName Field is required")]
    public string ArabicName { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "EnglishName Field is required")]
    public string EnglishName { get; set; }

    public bool? IsActive { get; set; } = true;

    public bool IsFrontal { get; set; } = false;
    public decimal Cost { get; set; }
    public Guid? SalesClassId { get; set; }

}