using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationRoomEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationServiceEntry;

namespace SmartAndPro.ERP.Application.Common.Dtos.MealReservation.MealReservationEntry;

public class MealReservationEntryPutDto
{
    [JsonInclude]
    [Required(ErrorMessage = "Id Field is required")]
    public Guid Id { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "ArrivalDate Field is required")]
    public DateOnly ArrivalDate { get; set; }

    
    [JsonInclude]
    [Required(ErrorMessage = "DepartureDate Field is required")]
    public DateOnly DepartureDate { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "NumberOfNights Field is required")]
    public int NumberOfNights { get; set; } = 1;

    [JsonInclude]
    [Required(ErrorMessage = "ReservationMealId Field is required")]
    public Guid ReservationMealId { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "NumberOfPersons Field is required")]
    public int NumberOfPersons { get; set; } = 1;

    public int? Frees { get; set; } = 0;

    [JsonInclude]
    [Required(ErrorMessage = "MealRate Field is required")]
    public decimal MealRate { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "MealCost Field is required")]
    public decimal MealCost { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "GuestName Field is required")]
    public string GuestName { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "Nationality Field is required")]
    public string Nationality { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "EntringMeal Field is required")]
    public string? EntringMeal { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "ExitingMeal Field is required")]
    public string? ExitingMeal { get; set; }
}