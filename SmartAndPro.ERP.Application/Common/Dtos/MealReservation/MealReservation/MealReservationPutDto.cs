using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SmartAndPro.ERP.Application.Common.Dtos.MealReservation.MealReservationEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationRoomEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationServiceEntry;

namespace SmartAndPro.ERP.Application.Common.Dtos.MealReservation.MealReservation;

public class MealReservationPutDto
{
    [JsonInclude]
    [Required(ErrorMessage = "Id Field is required")]
    public Guid Id { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "CustomerProfileId Field is required")]
    public Guid CustomerProfileId { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "MealReservationEntries Field is required")]
    public List<MealReservationEntryPutDto> MealReservationEntries { get; set; }

    public string? Description { get; set; }
    public string? CustomerReservationNumber { get; set; }
    public string? Notes { get; set; }

}