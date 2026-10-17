using SmartAndPro.ERP.Domains.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Domains.Entities.PublicBooking;

/// <summary>
/// System notifications for ERP ↔ Website communication
/// Stores real-time notifications for admins and users
/// </summary>
public class SystemNotification : Entity
{
    /// <summary>
    /// Notification title
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Notification message/content
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Notification type (Booking, Cancellation, Payment, etc.)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Related booking ID (if applicable)
    /// </summary>
    public Guid? BookingId { get; set; }

    /// <summary>
    /// Target user ID (null means all admins)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Is notification read
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// Read date/time
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Notification priority (Low, Medium, High, Urgent)
    /// </summary>
    [MaxLength(20)]
    public string Priority { get; set; } = "Medium";

    /// <summary>
    /// Action URL (for navigation)
    /// </summary>
    [MaxLength(500)]
    public string? ActionUrl { get; set; }

    /// <summary>
    /// Additional data stored as JSON
    /// </summary>
    public string? Metadata { get; set; }
}
