namespace SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;

public class PublicHotelFiltersDto
{
    public string? Name { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }

    public int? MinStars { get; set; }

    public int? MaxStars { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsFeatured { get; set; }

    // Filter by amenities (comma-separated)
    public string? Amenities { get; set; }

    // Filter by minimum price (from room types)
    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    // Geo search
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? RadiusInKm { get; set; }

    // Date range for availability search
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }

    // Guests count for room filtering
    public int? GuestsCount { get; set; }
}
