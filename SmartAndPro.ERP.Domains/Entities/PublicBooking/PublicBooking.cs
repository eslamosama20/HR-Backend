using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.SharedKernel.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAndPro.ERP.Domains.Entities.PublicBooking;

/// <summary>
/// Public booking made through the booking website
/// </summary>
public class PublicBooking : Entity
{
    [Required]
    public Guid HotelId { get; set; }

    /// <summary>
    /// Booking reference number (unique)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string BookingReference { get; set; } = string.Empty;

    /// <summary>
    /// Guest full name
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string GuestName { get; set; } = string.Empty;

    /// <summary>
    /// Guest email
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string GuestEmail { get; set; } = string.Empty;

    /// <summary>
    /// Guest phone number
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string GuestPhone { get; set; } = string.Empty;

    /// <summary>
    /// Country code for phone number (e.g., "+966", "+20")
    /// </summary>
    [MaxLength(10)]
    public string? CountryCode { get; set; }

    /// <summary>
    /// Guest nationality
    /// </summary>
    [MaxLength(100)]
    public string? GuestNationality { get; set; }

    /// <summary>
    /// Number of adults
    /// </summary>
    public int NumberOfAdults { get; set; } = 1;

    /// <summary>
    /// Number of children
    /// </summary>
    public int NumberOfChildren { get; set; } = 0;

    /// <summary>
    /// Check-in date
    /// </summary>
    [Required]
    public DateTime CheckInDate { get; set; }

    /// <summary>
    /// Check-out date
    /// </summary>
    [Required]
    public DateTime CheckOutDate { get; set; }

    /// <summary>
    /// Number of nights
    /// </summary>
    public int NumberOfNights { get; set; }

    /// <summary>
    /// Total booking amount
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Discount applied (if any)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? DiscountAmount { get; set; }

    /// <summary>
    /// Final amount after discount
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal FinalAmount { get; set; }

    /// <summary>
    /// Special requests from guest
    /// </summary>
    [MaxLength(2000)]
    public string? SpecialRequests { get; set; }

    /// <summary>
    /// Booking status
    /// </summary>
    public PublicBookingStatus Status { get; set; } = PublicBookingStatus.Pending;

    /// <summary>
    /// Payment status
    /// </summary>
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    /// <summary>
    /// Payment method (e.g., "CreditCard", "Cash", "BankTransfer")
    /// </summary>
    [MaxLength(50)]
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Payment transaction ID
    /// </summary>
    [MaxLength(200)]
    public string? PaymentTransactionId { get; set; }

    /// <summary>
    /// Paymob order ID for tracking payment
    /// </summary>
    public int? PaymobOrderId { get; set; }

    /// <summary>
    /// Date when booking was confirmed
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// Date when booking was cancelled
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// Cancellation reason
    /// </summary>
    [MaxLength(1000)]
    public string? CancellationReason { get; set; }

    /// <summary>
    /// Refund status for cancelled bookings
    /// </summary>
    public RefundStatus? RefundStatus { get; set; }

    /// <summary>
    /// Amount to be refunded
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? RefundAmount { get; set; }

    /// <summary>
    /// Date when refund was processed
    /// </summary>
    public DateTime? RefundedAt { get; set; }

    /// <summary>
    /// Refund transaction ID
    /// </summary>
    [MaxLength(200)]
    public string? RefundTransactionId { get; set; }

    /// <summary>
    /// Internal notes (not visible to guest)
    /// </summary>
    [MaxLength(2000)]
    public string? InternalNotes { get; set; }

    /// <summary>
    /// Optional: Link to ERP Reservation if synced
    /// </summary>
    public Guid? ErpReservationId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(HotelId))]
    public virtual PublicHotel Hotel { get; set; } = null!;
    
    public virtual ICollection<PublicBookingRoom> BookingRooms { get; set; } = new List<PublicBookingRoom>();
    
    /// <summary>
    /// Paid services selected for this booking
    /// </summary>
    public virtual ICollection<PublicBookingPaidService> BookingPaidServices { get; set; } = new List<PublicBookingPaidService>();
}
