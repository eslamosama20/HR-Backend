using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationRoomEntry;

public class ReservationRoomEntryPutDto
{
    
    public Guid? Id { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "RoomTypeId Field is required")]
    public Guid RoomTypeId { get; set; }
    
    public int NumberOfRooms { get; set; } = 1;
    
    [JsonInclude]
    [Required(ErrorMessage = "RoomRate Field is required")]
    public decimal RoomRate { get; set; }


    public decimal? RoomCost { get; set; }
    
    
    public Guid? ReservationMealId { get; set; }
    
    
    public decimal? MealRate { get; set; }
    
    
    public decimal? MealCost { get; set; }
    
    
    public Guid? ReservationViewId { get; set; }
    
    
    public decimal? ViewRate { get; set; }
    
    
    public decimal? ViewCost { get; set; }
    
    public Guid? ReservationId { get; set; }

}

