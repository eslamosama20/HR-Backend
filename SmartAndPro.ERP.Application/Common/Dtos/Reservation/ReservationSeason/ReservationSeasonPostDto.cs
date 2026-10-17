using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationSeason;

public class ReservationSeasonPostDto
{
    [JsonInclude]
    [Required(ErrorMessage = "ArabicName Field is required")]
    public string ArabicName { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "EnglishName Field is required")]
    public string EnglishName { get; set; }

    public Guid? RoomRevenueMainAccountId { get; set; }

    public Guid? RoomExpenseMainAccountId { get; set; }

    public Guid? MealRevenueMainAccountId { get; set; }

    public Guid? MealExpenseMainAccountId { get; set; }

    public Guid? OtherRevenueMainAccountId { get; set; }

    public Guid? OtherExpenseMainAccountId { get; set; }

    public bool? IsActive { get; set; } = true;

    public bool IsFrontal { get; set; } = false;
}