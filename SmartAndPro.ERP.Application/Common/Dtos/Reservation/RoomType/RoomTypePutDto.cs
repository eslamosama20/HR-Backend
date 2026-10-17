using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.RoomType;

public class RoomTypePutDto
{
    [JsonInclude]
    [Required(ErrorMessage = "Id Field is required")]
    public Guid Id { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "ArabicName Field is required")]
    public string ArabicName { get; set; } = string.Empty;

    [JsonInclude]
    [Required(ErrorMessage = "EnglishName Field is required")]
    public string EnglishName { get; set; } = string.Empty;

    public int? PersonNo { get; set; }

    public string? Images { get; set; } // JSON array of base64 images
    public string? ImageUrls { get; set; } // JSON array of cloudinary image URLs

    public bool? IsActive { get; set; } = true;

    public bool IsFrontal { get; set; } = false;

    public string? Logo { get; set; }
    public string? LogoUrl { get; set; }
    public string? LogoPublicId { get; set; }
}
