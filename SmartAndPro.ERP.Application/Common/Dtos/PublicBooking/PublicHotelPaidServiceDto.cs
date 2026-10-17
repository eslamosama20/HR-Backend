using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;

/// <summary>
/// DTO for hotel-specific paid service with price
/// Used when admin selects services for a hotel
/// </summary>
public class PublicHotelPaidServiceDto
{
    public Guid? Id { get; set; }

    [Required]
    public Guid PublicPaidServiceId { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Include service details for display
    public PublicPaidServiceResponseDto? Service { get; set; }
}
