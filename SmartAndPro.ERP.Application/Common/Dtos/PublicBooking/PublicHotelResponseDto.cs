namespace SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;

public class PublicHotelResponseDto
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string? NameAr { get; set; }

    public string? Description { get; set; }

    public string? DescriptionAr { get; set; }

    public string? Address { get; set; }

    public string? AddressAr { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }

    public int Stars { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Website { get; set; }

    public string? MainImageUrl { get; set; }
    public string? MainImagePublicId { get; set; }
    public string? AdditionalImageUrls { get; set; }

    public string? Amenities { get; set; }

    public string? CheckInTime { get; set; }

    public string? CheckOutTime { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string? Policies { get; set; }

    public bool IsActive { get; set; }

    public bool IsFeatured { get; set; }

    public Guid? ErpHotelId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    // Navigation - room types count
    public int RoomTypesCount { get; set; }

    // Navigation - bookings count
    public int BookingsCount { get; set; }

    // Navigation - room types (for detailed view)
    public List<PublicRoomTypeResponseDto>? RoomTypes { get; set; }

    // Navigation - paid services offered by hotel
    public List<PublicHotelPaidServiceDto>? PaidServices { get; set; }
}
