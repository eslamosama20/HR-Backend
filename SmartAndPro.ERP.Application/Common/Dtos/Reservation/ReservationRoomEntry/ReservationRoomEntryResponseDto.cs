using SmartAndPro.ERP.Application.Common.Dtos.Reservation.RoomType;

namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationRoomEntry;

public class ReservationRoomEntryResponseDto
{
    public Guid Id { get; set; }
    
    public Guid RoomTypeId { get; set; }
    public RoomTypeResponseDto RoomType { get; set; }
    
    public int NumberOfRooms { get; set; }
    
    public decimal RoomRate { get; set; }
    
    public decimal? RoomCost { get; set; }
    
    public Guid? ReservationMealId { get; set; }
    
    public decimal? MealRate { get; set; }
    
    public decimal? MealCost { get; set; }
    
    public Guid? ReservationViewId { get; set; }
    
    public decimal? ViewRate { get; set; }
    
    public decimal? ViewCost { get; set; }
    
    public Guid ReservationId { get; set; }
    
    public DateTime CreatedAt { get; set; }

    public string? RoomTypeArabicName { get; set; }
    public string? RoomTypeEnglishName { get; set; }
    public string? ReservationMealArabicName { get; set; }
    public string? ReservationMealEnglishName { get; set; }
    public string? ReservationViewArabicName { get; set; }
    public string? ReservationViewEnglishName { get; set; }
}