using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;

public class PublicPaidServicePostDto
{
    [Required]
    [MaxLength(200)]
    public string NameEn { get; set; }

    [Required]
    [MaxLength(200)]
    public string NameAr { get; set; }

    [MaxLength(1000)]
    public string? DescriptionEn { get; set; }

    [MaxLength(1000)]
    public string? DescriptionAr { get; set; }

    [MaxLength(500)]
    public string? Icon { get; set; }

    [MaxLength(100)]
    public string? ServiceTypeCode { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; } = 0;
}
