using SmartAndPro.ERP.Domains.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAndPro.ERP.Domains.Entities.PublicBooking;

/// <summary>
/// Seasonal and special pricing for room types
/// Allows different pricing for different date ranges
/// </summary>
public class PublicRoomPricing : Entity
{
    [Required]
    public Guid RoomTypeId { get; set; }

    [Required]
    [MaxLength(200)]
    public string SeasonName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? SeasonNameAr { get; set; }

    /// <summary>
    /// Start date of pricing period
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date of pricing period
    /// </summary>
    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Weekday price (Sunday-Thursday)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal WeekdayPrice { get; set; }

    /// <summary>
    /// Weekend price (Friday-Saturday)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal WeekendPrice { get; set; }

    /// <summary>
    /// Minimum nights required for booking in this season
    /// </summary>
    public int? MinimumNights { get; set; }

    /// <summary>
    /// Discount percentage for early booking (e.g., 10%)
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal? EarlyBookingDiscount { get; set; }

    /// <summary>
    /// Days before check-in to qualify for early booking discount
    /// </summary>
    public int? EarlyBookingDays { get; set; }

    /// <summary>
    /// Is this pricing active
    /// </summary>
    public new bool IsActive { get; set; } = true;

    /// <summary>
    /// Priority (higher priority pricing applies first)
    /// </summary>
    public int Priority { get; set; } = 0;

    // Navigation properties
    [ForeignKey(nameof(RoomTypeId))]
    public virtual PublicRoomType RoomType { get; set; } = null!;
}
