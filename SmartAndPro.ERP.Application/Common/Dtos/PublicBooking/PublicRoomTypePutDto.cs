using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;

public class PublicRoomTypePutDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public Guid HotelId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    [MaxLength(200)]
    public string? NameAr { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(2000)]
    public string? DescriptionAr { get; set; }

    public double? SizeInSqMeters { get; set; }

    [Required]
    [Range(1, 20)]
    public int MaxGuests { get; set; }

    [Required]
    [Range(1, 10)]
    public int NumberOfBeds { get; set; }

    [MaxLength(50)]
    public string? BedType { get; set; }

    [Required]
    [Range(0.01, 999999.99)]
    public decimal BasePrice { get; set; }

    [Range(0.01, 999999.99)]
    public decimal? WeekendPrice { get; set; }

    [MaxLength(500)]
    public string? MainImageUrl { get; set; }

    [MaxLength(200)]
    public string? MainImagePublicId { get; set; }

    public string? AdditionalImageUrls { get; set; } // JSON array

    public string? Amenities { get; set; }

    [MaxLength(100)]
    public string? ViewType { get; set; }

    [Range(0, 999999.99)]
    public decimal ViewPrice { get; set; } = 0;

    [Range(0, 100)]
    public decimal MunicipalityTaxPercentage { get; set; } = 0;

    [Required]
    [Range(1, 1000)]
    public int TotalRooms { get; set; }

    [Required]
    [Range(0, 1000)]
    public int AvailableQuantity { get; set; }

    public bool IsActive { get; set; }

    public string? CancellationPolicy { get; set; }

    public string? CancellationPolicyAr { get; set; }

    public bool PayAtHotel { get; set; } = true;

    public bool PayNow { get; set; } = true;

    public Guid? ErpRoomTypeId { get; set; }
}
