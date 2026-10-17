using SmartAndPro.ERP.Application.Common.Dtos.Attachments;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationMeal;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationRoomEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationServiceEntry;

namespace SmartAndPro.ERP.Application.Common.Dtos.MealReservation.MealReservationEntry;

public class MealReservationEntryResponseDto
{
    public Guid Id { get; set; }

    public DateOnly ArrivalDate { get; set; }

    public DateOnly DepartureDate { get; set; }

    public int NumberOfNights { get; set; }

    public Guid ReservationMealId { get; set; }
    public ReservationMealResponseDto ReservationMeal { get; set; }

    public int NumberOfPersons { get; set; }

    public int Frees { get; set; }

    public decimal MealRate { get; set; }

    public decimal MealCost { get; set; }

    public string GuestName { get; set; }

    public string Nationality { get; set; }

    public string? EntringMeal { get; set; }

    public string? ExitingMeal { get; set; }


    public Guid MealReservationId { get; set; }
    
    public DateTime CreatedAt { get; set; }
}