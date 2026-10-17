using SmartAndPro.ERP.Domains.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;

public class PublicBookingPostDto
{
    [Required]
    public Guid HotelId { get; set; }

    [Required]
    [MaxLength(100)]
    public string GuestName { get; set; }

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public string GuestEmail { get; set; }

    [Required]
    [MaxLength(50)]
    public string GuestPhone { get; set; }

    [MaxLength(10)]
    public string? CountryCode { get; set; }

    [MaxLength(100)]
    public string? GuestNationality { get; set; }

    [Required]
    [Range(1, 50)]
    public int NumberOfAdults { get; set; }

    [Range(0, 20)]
    public int NumberOfChildren { get; set; }

    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }

    [MaxLength(1000)]
    public string? SpecialRequests { get; set; }

    [Required]
    public List<BookingRoomDto> Rooms { get; set; }

    /// <summary>
    /// Paid services selected for this booking
    /// </summary>
    public List<PublicBookingPaidServiceDto>? PaidServices { get; set; }

    [Required]
    [Range(0.01, 999999.99)]
    public decimal TotalAmount { get; set; }

    [Range(0, 999999.99)]
    public decimal DiscountAmount { get; set; }

    public string? PromoCode { get; set; }

    // Payment info
    [MaxLength(50)]
    public string? PaymentMethod { get; set; }

    public string? TransactionId { get; set; }

    public string? PaymentDetails { get; set; }

    public int? PaymobOrderId { get; set; }

    // Optional ERP link
    public Guid? ErpReservationId { get; set; }
}

public class BookingRoomDto
{
    [Required]
    public Guid RoomTypeId { get; set; }

    [Required]
    [Range(1, 100)]
    public int Quantity { get; set; }

    [Required]
    [Range(0.01, 999999.99)]
    public decimal PricePerNight { get; set; }
}
