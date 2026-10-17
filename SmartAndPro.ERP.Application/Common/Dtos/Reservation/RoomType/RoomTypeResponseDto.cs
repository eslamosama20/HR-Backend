namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.RoomType;

public class RoomTypeResponseDto
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public string ArabicName { get; set; } = string.Empty;

    public string EnglishName { get; set; } = string.Empty;

    public int? PersonNo { get; set; }

    public string? ImageUrls { get; set; } // JSON array of cloudinary image URLs

    public DateTime CreatedAt { get; set; }

    public bool IsFrontal { get; set; } = false;

    public string? LogoUrl { get; set; }
    public string? LogoPublicId { get; set; }
    public Guid? SalesClassId { get; set; }
}
