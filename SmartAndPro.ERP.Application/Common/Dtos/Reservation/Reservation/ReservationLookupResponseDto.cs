namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.Reservation;

public class ReservationLookupResponseDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string? GuestName { get; set; }
    public DateOnly ArrivalDate { get; set; }
    public DateOnly DepartureDate { get; set; }
    public string ReservationType { get; set; } = string.Empty;
    public int? SaleInvoiceId { get; set; }
    public bool IsConfirmed { get; set; }
    public bool IsCancelled { get; set; }
    public bool IsFrontal { get; set; }
    public DateTime CreatedAt { get; set; }
}
