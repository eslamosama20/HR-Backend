using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationServiceEntry;

public class ReservationServiceEntryPostDto
{
    [JsonInclude]
    [Required(ErrorMessage = "ReservationServiceId Field is required")]
    public Guid ReservationServiceId { get; set; }
    
    public string? Description { get; set; }
    
    /*[JsonInclude]
    [Required(ErrorMessage = "StartDate Field is required")]
    public DateOnly StartDate { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "EndDate Field is required")]
    public DateOnly EndDate { get; set; }*/
    
    [JsonInclude]
    [Required(ErrorMessage = "Quantity Field is required")]
    public decimal Quantity { get; set; }
    
    
    public decimal? SalesPrice { get; set; } = 0;
    
    
    public decimal? CostPrice { get; set; } = 0;
    
    [JsonInclude]
    public Guid? ReservationSupplierId { get; set; }
    
    public Guid? ReservationId { get; set; }
}