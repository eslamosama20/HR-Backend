using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.Reservation;

public class ReservationPrintTemplateCommand 
{
    [JsonInclude]
    [Required(ErrorMessage = "TemplateId Field is required")]   
    public Guid TemplateId { get; set; }
    [JsonInclude]
    [Required(ErrorMessage = "ReservationIds Field is required")]   
    public List<Guid> ReservationIds { get; set; }
}