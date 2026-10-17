using SmartAndPro.ERP.Domains.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Domains.Entities.PublicBooking;

/// <summary>
/// Entity for hotel reviews submitted by guests after their stay
/// </summary>
public class HotelReview : Entity
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the hotel being reviewed
    /// </summary>
    public Guid HotelId { get; set; }
    public virtual PublicHotel Hotel { get; set; } = null!;

    /// <summary>
    /// Optional reference to booking (if guest booked through system)
    /// </summary>
    public Guid? BookingId { get; set; }
    public virtual PublicBooking? Booking { get; set; }

    /// <summary>
    /// Guest name (required even if no booking)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string GuestName { get; set; } = string.Empty;

    /// <summary>
    /// Guest email for verification
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string GuestEmail { get; set; } = string.Empty;

    /// <summary>
    /// Rating from 1 to 5 stars
    /// </summary>
    [Range(1, 5)]
    public int Rating { get; set; }

    /// <summary>
    /// Written review comment
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Review title (optional short summary)
    /// </summary>
    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>
    /// When the stay occurred
    /// </summary>
    public DateTime StayDate { get; set; }

    /// <summary>
    /// Whether review is verified (linked to actual booking)
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Whether review is approved by admin
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// Whether review is visible to public
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    /// Admin response to review (optional)
    /// </summary>
    [MaxLength(500)]
    public string? AdminResponse { get; set; }

    /// <summary>
    /// When admin responded
    /// </summary>
    public DateTime? AdminResponseDate { get; set; }

    public HotelReview()
    {
        Id = Guid.NewGuid();
        IsVerified = false;
        IsApproved = false;
        IsVisible = true;
        CreatedAt = DateTime.Now;
    }
}
