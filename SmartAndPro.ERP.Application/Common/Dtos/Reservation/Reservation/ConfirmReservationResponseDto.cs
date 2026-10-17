namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.Reservation;

public class ConfirmReservationResponseDto
{
    public Guid Id { get; set; }
    public bool IsConfirmed { get; set; }
    public DateTime ConfirmedAt { get; set; }
    public ReservationActionLogResponseDto? LastLog { get; set; }
}
