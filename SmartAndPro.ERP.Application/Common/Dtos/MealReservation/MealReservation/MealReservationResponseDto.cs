using SmartAndPro.ERP.Application.Common.Dtos.Attachments;
using SmartAndPro.ERP.Application.Common.Dtos.MealReservation.MealReservationEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationRoomEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationServiceEntry;
using SmartAndPro.ERP.Domains.Entities.Accounting;

namespace SmartAndPro.ERP.Application.Common.Dtos.MealReservation.MealReservation;

public class MealReservationResponseDto
{
    public Guid Id { get; set; }

    public string ReferenceNumber { get; set; }
    
    public Guid CustomerProfileId { get; set; }
    public CustomerProfile CustomerProfile { get; set; }
    
    public Guid? TransactionId { get; set; }
    public List<MealReservationEntryResponseDto> MealReservationEntries { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public string? CustomerReservationNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}