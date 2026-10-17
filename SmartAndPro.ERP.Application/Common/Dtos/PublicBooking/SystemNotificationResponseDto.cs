namespace SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;

public class SystemNotificationResponseDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public Guid? BookingId { get; set; }

    public Guid? UserId { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public string Priority { get; set; } = "Medium";

    public string? ActionUrl { get; set; }

    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }
}
