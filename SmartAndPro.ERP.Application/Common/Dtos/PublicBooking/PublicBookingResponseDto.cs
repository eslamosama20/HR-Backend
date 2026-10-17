using SmartAndPro.ERP.SharedKernel.Enums;

namespace SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;

public class PublicBookingResponseDto
{
    public Guid Id { get; set; }

    public Guid HotelId { get; set; }

    public string HotelName { get; set; }

    public string BookingReference { get; set; }

    public string GuestName { get; set; }

    public string GuestEmail { get; set; }

    public string GuestPhone { get; set; }

    public string? CountryCode { get; set; }

    public string? GuestNationality { get; set; }

    public int NumberOfAdults { get; set; }

    public int NumberOfChildren { get; set; }

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }

    public int NumberOfNights { get; set; }

    public string? SpecialRequests { get; set; }

    public List<BookingRoomResponseDto> Rooms { get; set; }

    /// <summary>
    /// Paid services included in this booking
    /// </summary>
    public List<PublicBookingPaidServiceDto>? PaidServices { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal FinalAmount { get; set; }

    public string? PromoCode { get; set; }

    public PublicBookingStatus Status { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public string? PaymentMethod { get; set; }

    public string? TransactionId { get; set; }

    public string? PaymentDetails { get; set; }

    public int? PaymobOrderId { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? CancellationReason { get; set; }

    public DateTime? CancellationDate { get; set; }

    public Guid? ErpReservationId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }
}

public class BookingRoomResponseDto
{
    public Guid RoomTypeId { get; set; }

    public string RoomTypeName { get; set; }

    public int Quantity { get; set; }

    public decimal PricePerNight { get; set; }

    public decimal TotalPrice { get; set; }
}
