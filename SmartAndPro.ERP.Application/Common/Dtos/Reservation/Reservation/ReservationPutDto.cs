using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationRoomEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationServiceEntry;

namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.Reservation;

public class ReservationPutDto
{
    [JsonInclude]
    [Required(ErrorMessage = "Id Field is required")]
    public Guid Id { get; set; }
    
    public string? ReferenceNumber { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "GuestName Field is required")]    
    public string GuestName { get; set; }
    
    
    public string? GuestPhone { get; set; }
    
    public string? GuestEmail { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "Nationality Field is required")]
    public string Nationality { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "ArrivalDate Field is required")]
    public DateOnly ArrivalDate { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "DepartureDate Field is required")]
    public DateOnly DepartureDate { get; set; }
    
    public int NumberOfNights { get; set; } = 1;
    
    
    public Guid? ReservationCompanyId { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "ReservationClientCompanyId Field is required")]
    public Guid ReservationClientCompanyId { get; set; }
    
    
    public Guid? SalesRepresentativeId { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "HotelId Field is required")]
    public Guid HotelId { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "ReservationSeasonId Field is required")]
    public Guid ReservationSeasonId { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "ReservationType Field is required")]
    public string ReservationType { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "ReservationRoomEntries Field is required")]
    public List<ReservationRoomEntryPutDto> ReservationRoomEntries { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "ReservationServiceEntries Field is required")]
    public List<ReservationServiceEntryPutDto> ReservationServiceEntries { get; set; }

    public int? SaleInvoiceId { get; set; }

    public bool? IsSentToZatca { get; set; } = false;
    public string? InvoiceHash { get; set; }
    public string? GeneratedQR { get; set; }
    public bool? IsFrontal { get; set; } = false;
    public bool? IsConfirmed { get; set; } = false;

    public bool? IsActive { get; set; } = true;
    public bool? IsCancelled { get; set; } = false;


    public Guid? BankAccountId { get; set; }
    public Guid? CompanyPhoneId { get; set; }
    public Guid? CompanyPolicyId { get; set; }
    public string? HotelRemarks { get; set; }
    public string? ClientRemarks { get; set; }
}