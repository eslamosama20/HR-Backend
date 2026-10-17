namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.Reservation;

public class ReservationActionLogResponseDto
{
    public Guid Id { get; set; }

    public Guid ReservationId { get; set; }

    public string ActionType { get; set; }

    public string ExecutedBy { get; set; }

    public DateTime OccurredAt { get; set; }
}
