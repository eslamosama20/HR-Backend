using SmartAndPro.ERP.Domains.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAndPro.ERP.Domains.Entities.PublicBooking;

/// <summary>
/// Public-facing hotel entity for booking website (separate from ERP Hotel)
/// Used for public booking system with retail pricing
/// </summary>
public class PublicHotel : Entity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? NameAr { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(2000)]
    public string? DescriptionAr { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(500)]
    public string? AddressAr { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [Range(1, 5)]
    public int Stars { get; set; } = 3;

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(200)]
    public string? Website { get; set; }

    /// <summary>
    /// Main hotel image (legacy base64, kept for data migration - Phase 3)
    /// </summary>
    public string? MainImage { get; set; }

    /// <summary>
    /// Additional images stored as JSON array of base64/URLs (legacy, kept for data migration - Phase 3)
    /// </summary>
    public string? AdditionalImages { get; set; }

    /// <summary>
    /// Main hotel image (Cloudinary URL)
    /// </summary>
    [MaxLength(500)]
    public string? MainImageUrl { get; set; }

    /// <summary>
    /// Cloudinary public ID for main image
    /// </summary>
    [MaxLength(200)]
    public string? MainImagePublicId { get; set; }

    /// <summary>
    /// Additional images stored as JSON array of Cloudinary URLs
    /// </summary>
    public string? AdditionalImageUrls { get; set; }

    /// <summary>
    /// Hotel amenities stored as JSON array
    /// e.g., ["WiFi", "Pool", "Gym", "Parking", "Restaurant"]
    /// </summary>
    public string? Amenities { get; set; }

    /// <summary>
    /// Check-in time (e.g., "14:00")
    /// </summary>
    [MaxLength(10)]
    public string? CheckInTime { get; set; }

    /// <summary>
    /// Check-out time (e.g., "12:00")
    /// </summary>
    [MaxLength(10)]
    public string? CheckOutTime { get; set; }

    /// <summary>
    /// Latitude for map display
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Longitude for map display
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Hotel policies (cancellation, payment, etc.)
    /// </summary>
    public string? Policies { get; set; }

    /// <summary>
    /// Is hotel active and visible on public website
    /// </summary>
    public override bool IsActive { get; set; } = true;

    /// <summary>
    /// Featured hotels shown first in search
    /// </summary>
    public bool IsFeatured { get; set; } = false;

    /// <summary>
    /// Optional: Link to existing ERP Hotel for sync
    /// </summary>
    public Guid? ErpHotelId { get; set; }

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<PublicRoomType> RoomTypes { get; set; } = new List<PublicRoomType>();
    public virtual ICollection<PublicBooking> Bookings { get; set; } = new List<PublicBooking>();
    public virtual ICollection<PublicHotelPaidService> HotelPaidServices { get; set; } = new List<PublicHotelPaidService>();
}
