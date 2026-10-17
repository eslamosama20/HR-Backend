namespace SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;

public class PublicPaidServiceResponseDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; }
    public string NameAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? Icon { get; set; }
    public string? ServiceTypeCode { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
