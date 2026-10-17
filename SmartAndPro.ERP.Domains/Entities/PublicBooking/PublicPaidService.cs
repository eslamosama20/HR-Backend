using SmartAndPro.ERP.Domains.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Domains.Entities.PublicBooking;

/// <summary>
/// Paid services that can be offered by hotels (transportation, tours, etc.)
/// </summary>
public class PublicPaidService : Entity
{
    [Required]
    [MaxLength(200)]
    public string NameEn { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string NameAr { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? DescriptionEn { get; set; }

    [MaxLength(1000)]
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// Icon or image for the service
    /// </summary>
    [MaxLength(500)]
    public string? Icon { get; set; }

    /// <summary>
    /// Service type code for categorization
    /// e.g., "TRANSPORT_MAKKAH_MADINAH", "TOURS_MADINAH", etc.
    /// </summary>
    [MaxLength(100)]
    public string? ServiceTypeCode { get; set; }

    /// <summary>
    /// Is service active and available
    /// </summary>
    public override bool IsActive { get; set; } = true;

    /// <summary>
    /// Display order in lists
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Hotels offering this service (many-to-many)
    /// </summary>
    public List<PublicHotelPaidService> HotelPaidServices { get; set; } = new();
}
