namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationServiceEntry;

public class ReservationServiceEntryResponseDto
{
    public Guid Id { get; set; }
    
    public Guid ReservationServiceId { get; set; }
    
    public string? Description { get; set; }
    
    /*public DateOnly StartDate { get; set; }
    
    public DateOnly EndDate { get; set; }*/
    
    public decimal Quantity { get; set; }
    
    public decimal? SalesPrice { get; set; } = 0;
    
    public decimal? CostPrice { get; set; } = 0;
    
    public Guid? ReservationSupplierId { get; set; }
    
    public Guid ReservationId { get; set; }
    
    public DateTime CreatedAt { get; set; }

    public string? ReservationServiceArabicName { get; set; }
    public string? ReservationServiceEnglishName { get; set; }
    public string? ReservationSupplierArabicName { get; set; }
    public string? ReservationSupplierEnglishName { get; set; }
}