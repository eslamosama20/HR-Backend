namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.RoomType;

public class RoomTypeLookupResponseDto
{
    public Guid Id { get; set; }
    public string ArabicName { get; set; } = string.Empty;
    public string EnglishName { get; set; } = string.Empty;
    public int PersonNo { get; set; }
    public bool IsFrontal { get; set; }
}
