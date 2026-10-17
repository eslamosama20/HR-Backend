using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;

public class SystemNotificationPostDto
{
    [JsonInclude]
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [JsonInclude]
    [Required(ErrorMessage = "Message is required")]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    [JsonInclude]
    [Required(ErrorMessage = "Type is required")]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    public Guid? BookingId { get; set; }

    public Guid? UserId { get; set; }

    [MaxLength(20)]
    public string Priority { get; set; } = "Medium";

    [MaxLength(500)]
    public string? ActionUrl { get; set; }

    public string? Metadata { get; set; }
}
