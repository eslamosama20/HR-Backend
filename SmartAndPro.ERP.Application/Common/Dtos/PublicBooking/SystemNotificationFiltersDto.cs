namespace SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;

public class SystemNotificationFiltersDto
{
    public string? Type { get; set; }
    public Guid? BookingId { get; set; }
    public Guid? UserId { get; set; }
    public bool? IsRead { get; set; }
    public string? Priority { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
}
