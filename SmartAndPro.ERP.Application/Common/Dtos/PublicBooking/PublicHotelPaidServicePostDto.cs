using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;

/// <summary>
/// DTO for linking a paid service to a hotel with custom price
/// </summary>
public class PublicHotelPaidServicePostDto
{
    [Required]
    public Guid PublicHotelId { get; set; }

    [Required]
    public Guid PublicPaidServiceId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Notes { get; set; }
}
