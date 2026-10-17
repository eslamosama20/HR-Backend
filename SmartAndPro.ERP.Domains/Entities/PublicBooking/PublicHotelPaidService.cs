using SmartAndPro.ERP.Domains.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAndPro.ERP.Domains.Entities.PublicBooking;

/// <summary>
/// Junction table for many-to-many relationship between hotels and paid services
/// Allows each hotel to set their own price for each service
/// </summary>
public class PublicHotelPaidService : Entity
{
    /// <summary>
    /// Hotel offering the service
    /// </summary>
    [Required]
    public Guid PublicHotelId { get; set; }
    public PublicHotel PublicHotel { get; set; }

    /// <summary>
    /// Service being offered
    /// </summary>
    [Required]
    public Guid PublicPaidServiceId { get; set; }
    public PublicPaidService PublicPaidService { get; set; }

    /// <summary>
    /// Price set by the hotel for this service (in SAR)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    /// <summary>
    /// Is this service currently available for this hotel
    /// </summary>
    public override bool IsActive { get; set; } = true;

    /// <summary>
    /// Additional notes or details specific to this hotel's offering
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
}
