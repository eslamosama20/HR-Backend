using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;

public class PublicHotelPutDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    [MaxLength(200)]
    public string? NameAr { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(2000)]
    public string? DescriptionAr { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(500)]
    public string? AddressAr { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [Range(1, 5)]
    public int Stars { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(200)]
    public string? Website { get; set; }

    [MaxLength(500)]
    public string? MainImageUrl { get; set; }

    [MaxLength(200)]
    public string? MainImagePublicId { get; set; }

    public string? AdditionalImageUrls { get; set; } // JSON array

    public string? Amenities { get; set; } // JSON array

    [MaxLength(10)]
    public string? CheckInTime { get; set; }

    [MaxLength(10)]
    public string? CheckOutTime { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string? Policies { get; set; }

    public bool IsActive { get; set; }

    public bool IsFeatured { get; set; }

    public Guid? ErpHotelId { get; set; }

    /// <summary>
    /// Paid services offered by this hotel with prices
    /// </summary>
    public List<PublicHotelPaidServiceDto>? PaidServices { get; set; }
}
