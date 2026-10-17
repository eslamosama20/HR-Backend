namespace SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;

public class PublicRoomTypeResponseDto
{
    public Guid Id { get; set; }

    public Guid HotelId { get; set; }

    public string HotelName { get; set; }

    public string Name { get; set; }

    public string? NameAr { get; set; }

    public string? Description { get; set; }

    public string? DescriptionAr { get; set; }

    public double? SizeInSqMeters { get; set; }

    public int MaxGuests { get; set; }

    public int NumberOfBeds { get; set; }

    public string? BedType { get; set; }

    public decimal BasePrice { get; set; }

    public decimal? WeekendPrice { get; set; }

    public string? MainImageUrl { get; set; }
    public string? MainImagePublicId { get; set; }
    public string? AdditionalImageUrls { get; set; }

    public string? Amenities { get; set; }

    public string? ViewType { get; set; }

    public decimal ViewPrice { get; set; }

    public decimal MunicipalityTaxPercentage { get; set; }

    public int TotalRooms { get; set; }

    public int AvailableQuantity { get; set; }

    public bool IsActive { get; set; }

    public Guid? ErpRoomTypeId { get; set; }

    public string? CancellationPolicy { get; set; }

    public string? CancellationPolicyAr { get; set; }

    public bool PayAtHotel { get; set; }

    public bool PayNow { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    // Calculated pricing based on seasonal rules
    public decimal? CurrentPrice { get; set; }
}
