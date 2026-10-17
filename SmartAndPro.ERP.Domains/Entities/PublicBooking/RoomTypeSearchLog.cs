using SmartAndPro.ERP.Domains.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAndPro.ERP.Domains.Entities.PublicBooking;

/// <summary>
/// Tracks room type searches to identify high-demand hotels and dates
/// </summary>
public class RoomTypeSearchLog : Entity
{
    /// <summary>
    /// Hotel being searched
    /// </summary>
    [Required]
    public Guid HotelId { get; set; }

    /// <summary>
    /// Room type being searched (nullable for hotel-level searches)
    /// </summary>
    public Guid? RoomTypeId { get; set; }

    /// <summary>
    /// Check-in date searched
    /// </summary>
    [Required]
    public DateTime CheckInDate { get; set; }

    /// <summary>
    /// Check-out date searched
    /// </summary>
    [Required]
    public DateTime CheckOutDate { get; set; }

    /// <summary>
    /// Number of guests
    /// </summary>
    public int NumberOfGuests { get; set; }

    /// <summary>
    /// Number of rooms requested
    /// </summary>
    public int NumberOfRooms { get; set; } = 1;

    /// <summary>
    /// City or location searched
    /// </summary>
    [MaxLength(100)]
    public string? City { get; set; }

    /// <summary>
    /// Search result: was availability found?
    /// </summary>
    public bool AvailabilityFound { get; set; }

    /// <summary>
    /// Number of available rooms found
    /// </summary>
    public int? AvailableRoomsCount { get; set; }

    /// <summary>
    /// IP address of searcher
    /// </summary>
    [MaxLength(50)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent (browser/device)
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Did this search result in a booking?
    /// </summary>
    public bool ConvertedToBooking { get; set; } = false;

    /// <summary>
    /// Related booking ID if converted
    /// </summary>
    public Guid? BookingId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(HotelId))]
    public virtual PublicHotel Hotel { get; set; } = null!;

    [ForeignKey(nameof(RoomTypeId))]
    public virtual PublicRoomType? RoomType { get; set; }

    [ForeignKey(nameof(BookingId))]
    public virtual Domains.Entities.PublicBooking.PublicBooking? Booking { get; set; }
}
