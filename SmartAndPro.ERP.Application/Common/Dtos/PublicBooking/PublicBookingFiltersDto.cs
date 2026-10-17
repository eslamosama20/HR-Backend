using SmartAndPro.ERP.SharedKernel.Enums;

namespace SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;

public class PublicBookingFiltersDto
{
    public Guid? HotelId { get; set; }

    public string? BookingReference { get; set; }

    public string? GuestName { get; set; }

    public string? GuestEmail { get; set; }

    public string? GuestPhone { get; set; }

    public PublicBookingStatus? Status { get; set; }

    public PaymentStatus? PaymentStatus { get; set; }

    public DateTime? CheckInFrom { get; set; }

    public DateTime? CheckInTo { get; set; }

    public DateTime? CheckOutFrom { get; set; }

    public DateTime? CheckOutTo { get; set; }

    public DateTime? BookingDateFrom { get; set; }

    public DateTime? BookingDateTo { get; set; }

    public decimal? MinAmount { get; set; }

    public decimal? MaxAmount { get; set; }

    public string? PaymentMethod { get; set; }

    public Guid? ErpReservationId { get; set; }

    /// <summary>
    /// Generic search keyword. When provided, searches across all entity properties marked with [Searchable].
    /// </summary>
    public string? SearchKeyword { get; set; }
}
