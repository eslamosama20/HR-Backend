namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationMeal;

public class ReservationMealResponseDto
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public string ArabicName { get; set; }

    public string EnglishName { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsFrontal { get; set; } = false;
    public decimal Cost { get; set; }
    public Guid? SalesClassId { get; set; }
}