namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.Reservation;

public class ReservationFiltersDto 
{
    public string? ReferenceNumber { get; set; }
    public string? GuestName { get; set; }
    public string? GuestPhone { get; set; }
    public string? Nationality { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? ReservationCompanyId { get; set; }
    public Guid? ReservationClientCompanyId { get; set; }
    public Guid? SalesRepresentativeId { get; set; }
    public Guid? HotelId { get; set; }
    public Guid? ReservationSeasonId { get; set; }
    public string? ReservationType { get; set; }
    public bool? IsFrontal { get; set; }
    public bool? IsConfirmed { get; set; }
    public bool? IsSentToZatca { get; set; }
    public bool? IsCancelled { get; set; } 


}