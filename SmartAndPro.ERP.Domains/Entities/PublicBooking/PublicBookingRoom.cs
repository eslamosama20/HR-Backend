using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.PublicBooking;

/// <summary>
/// Junction table for many-to-many relationship between bookings and room types
/// </summary>
public class PublicBookingRoom : Entity
{
    [Required]
    public Guid BookingId { get; set; }

    [Required]
    public Guid RoomTypeId { get; set; }

    /// <summary>
    /// Number of rooms of this type booked
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Price per night for this room type (at booking time)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal PricePerNight { get; set; }

    /// <summary>
    /// Total price for this room type (Quantity * Nights * PricePerNight)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    // Navigation properties
    [ForeignKey(nameof(BookingId))]
    public virtual PublicBooking Booking { get; set; } = null!;

    [ForeignKey(nameof(RoomTypeId))]
    public virtual PublicRoomType RoomType { get; set; } = null!;
}
