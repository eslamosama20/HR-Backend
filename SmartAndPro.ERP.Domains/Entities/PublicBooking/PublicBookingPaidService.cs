using SmartAndPro.ERP.Domains.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAndPro.ERP.Domains.Entities.PublicBooking;

/// <summary>
/// Paid services selected by customer during booking
/// Links a booking to specific paid services with their prices
/// </summary>
public class PublicBookingPaidService : Entity
{
    /// <summary>
    /// Booking that includes this service
    /// </summary>
    [Required]
    public Guid PublicBookingId { get; set; }
    
    [ForeignKey(nameof(PublicBookingId))]
    public virtual PublicBooking PublicBooking { get; set; } = null!;

    /// <summary>
    /// Hotel-Service relationship (includes the price)
    /// </summary>
    [Required]
    public Guid PublicHotelPaidServiceId { get; set; }
    
    [ForeignKey(nameof(PublicHotelPaidServiceId))]
    public virtual PublicHotelPaidService HotelPaidService { get; set; } = null!;

    /// <summary>
    /// Price of service at time of booking (frozen to prevent price changes affecting past bookings)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PriceAtBooking { get; set; }

    /// <summary>
    /// Quantity (e.g., 2 transfers, 3 tour tickets)
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Total price for this service (PriceAtBooking * Quantity)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Optional notes from customer about this service
    /// </summary>
    [MaxLength(500)]
    public string? CustomerNotes { get; set; }
}
