using SmartAndPro.ERP.Domains.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAndPro.ERP.Domains.Entities.PublicBooking;

/// <summary>
/// Room types for public booking system with retail pricing
/// Each hotel can have multiple room types
/// </summary>
public class PublicRoomType : Entity
{
    [Required]
    public Guid HotelId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? NameAr { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(2000)]
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// Room size in square meters
    /// </summary>
    public decimal? SizeInSqMeters { get; set; }

    /// <summary>
    /// Maximum number of guests allowed
    /// </summary>
    [Range(1, 20)]
    public int MaxGuests { get; set; } = 2;

    /// <summary>
    /// Number of beds
    /// </summary>
    public int NumberOfBeds { get; set; } = 1;

    /// <summary>
    /// Bed type (e.g., "King", "Queen", "Twin")
    /// </summary>
    [MaxLength(100)]
    public string? BedType { get; set; }

    /// <summary>
    /// Base price per night (retail price, not wholesale)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Weekend price (Friday/Saturday)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? WeekendPrice { get; set; }

    /// <summary>
    /// Main room image (legacy base64, kept for data migration - Phase 3)
    /// </summary>
    public string? MainImage { get; set; }

    /// <summary>
    /// Additional room images stored as JSON array (legacy base64, kept for data migration - Phase 3)
    /// </summary>
    public string? AdditionalImages { get; set; }

    /// <summary>
    /// Main room image (Cloudinary URL)
    /// </summary>
    [MaxLength(500)]
    public string? MainImageUrl { get; set; }

    /// <summary>
    /// Cloudinary public ID for main image
    /// </summary>
    [MaxLength(200)]
    public string? MainImagePublicId { get; set; }

    /// <summary>
    /// Additional room images stored as JSON array of Cloudinary URLs
    /// </summary>
    public string? AdditionalImageUrls { get; set; }

    /// <summary>
    /// Room amenities (WiFi, TV, AC, etc.) stored as JSON array
    /// </summary>
    public string? Amenities { get; set; }

    /// <summary>
    /// View type (e.g., "Sea View", "City View", "Garden View")
    /// </summary>
    [MaxLength(100)]
    public string? ViewType { get; set; }

    /// <summary>
    /// Additional price for the view type
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ViewPrice { get; set; } = 0;

    /// <summary>
    /// Municipality tax percentage (e.g., 5 for 5%)
    /// Applied to base price + view price before VAT
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal MunicipalityTaxPercentage { get; set; } = 0;

    /// <summary>
    /// Total number of rooms of this type available
    /// </summary>
    public int TotalRooms { get; set; } = 1;

    /// <summary>
    /// Available quantity for booking (decreased on booking, increased on cancellation)
    /// </summary>
    public int AvailableQuantity { get; set; } = 1;

    /// <summary>
    /// Is room type active and bookable
    /// </summary>
    public override bool IsActive { get; set; } = true;

    /// <summary>
    /// Featured rooms shown first
    /// </summary>
    public bool IsFeatured { get; set; } = false;

    /// <summary>
    /// Cancellation policy in English
    /// </summary>
    public string? CancellationPolicy { get; set; }

    /// <summary>
    /// Cancellation policy in Arabic
    /// </summary>
    public string? CancellationPolicyAr { get; set; }

    /// <summary>
    /// Allow payment at hotel (pay on arrival)
    /// </summary>
    public bool PayAtHotel { get; set; } = true;

    /// <summary>
    /// Allow online payment (pay now via payment gateway)
    /// </summary>
    public bool PayNow { get; set; } = true;

    /// <summary>
    /// Optional: Link to existing ERP RoomType for sync
    /// </summary>
    public Guid? ErpRoomTypeId { get; set; }

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(HotelId))]
    public virtual PublicHotel Hotel { get; set; } = null!;
    
    public virtual ICollection<PublicRoomPricing> SeasonalPricing { get; set; } = new List<PublicRoomPricing>();
    public virtual ICollection<PublicBookingRoom> BookingRooms { get; set; } = new List<PublicBookingRoom>();
}
