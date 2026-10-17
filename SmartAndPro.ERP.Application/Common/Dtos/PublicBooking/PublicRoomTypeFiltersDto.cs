namespace SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;

public class PublicRoomTypeFiltersDto
{
    public Guid? HotelId { get; set; }

    public string? Name { get; set; }

    public int? MinGuests { get; set; }

    public int? MaxGuests { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public bool? IsActive { get; set; }

    public string? ViewType { get; set; }

    public string? BedType { get; set; }

    // Filter by amenities (comma-separated)
    public string? Amenities { get; set; }

    // Availability check
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public int? RequiredRooms { get; set; }
}
