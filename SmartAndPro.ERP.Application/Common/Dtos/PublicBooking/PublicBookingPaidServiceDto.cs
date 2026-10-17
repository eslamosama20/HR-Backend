using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;

/// <summary>
/// DTO for paid services selected during booking
/// </summary>
public class PublicBookingPaidServiceDto
{
    public Guid? Id { get; set; }

    [Required]
    public Guid PublicHotelPaidServiceId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal PriceAtBooking { get; set; }

    [Required]
    [Range(1, 100)]
    public int Quantity { get; set; } = 1;

    [MaxLength(500)]
    public string? CustomerNotes { get; set; }

    // For display purposes
    public PublicPaidServiceResponseDto? Service { get; set; }
}
