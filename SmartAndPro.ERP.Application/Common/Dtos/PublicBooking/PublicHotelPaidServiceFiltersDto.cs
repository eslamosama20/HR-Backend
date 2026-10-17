namespace SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;

/// <summary>
/// Filters for querying hotel paid services
/// </summary>
public class PublicHotelPaidServiceFiltersDto
{
    /// <summary>
    /// Filter by specific hotel
    /// </summary>
    public Guid? PublicHotelId { get; set; }

    /// <summary>
    /// Filter by specific service
    /// </summary>
    public Guid? PublicPaidServiceId { get; set; }

    /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Search in service names or notes
    /// </summary>
    public string? SearchTerm { get; set; }
}
