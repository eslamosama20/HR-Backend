using SmartAndPro.ERP.Application.Common.Dtos.Attachments;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationRoomEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationServiceEntry;
using SmartAndPro.ERP.Application.Services.HotelAllotmentInvoices;

namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.Reservation;

public class ReservationResponseDto
{
    public Guid Id { get; set; }

    public string ReferenceNumber { get; set; }

    public bool IsActive { get; set; }

    public string? GuestName { get; set; }

    public string GuestPhone { get; set; }

    public string? GuestEmail { get; set; }

    public string Nationality { get; set; }

    public DateOnly ArrivalDate { get; set; }

    public DateOnly DepartureDate { get; set; }

    public int NumberOfNights { get; set; }

    public Guid? ReservationCompanyId { get; set; }

    public Guid ReservationClientCompanyId { get; set; }

    public Guid? SalesRepresentativeId { get; set; }

    public Guid HotelId { get; set; }

    public Guid ReservationSeasonId { get; set; }

    public string ReservationType { get; set; }

    public List<ReservationRoomEntryResponseDto> ReservationRoomEntries { get; set; }

    public List<ReservationServiceEntryResponseDto> ReservationServiceEntries { get; set; }
    
    public List<AttachmentResponseDto> Attachments { get; set; }

    public int? SaleInvoiceId { get; set; }

    public Guid? TransactionId { get; set; }
    
    public bool IsSentToZatca { get; set; } = false;
    public string? InvoiceHash { get; set; }
    public string? GeneratedQR { get; set; }
    public bool IsFrontal { get; set; } = false;
    public bool IsConfirmed { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public Guid? BankAccountId { get; set; }
    public Guid? CompanyPhoneId { get; set; }
    public Guid? CompanyPolicyId { get; set; }
    public bool IsCancelled { get; set; } = false;

    public List<AllotmentUsageService.DailyAvailability>? OverbookedDays { get; set; }
    public string? HotelRemarks { get; set; }
    public string? ClientRemarks { get; set; }
    public string? CreatedByName { get; set; }

    public string? HotelArabicName { get; set; }
    public string? HotelEnglishName { get; set; }
    public string? ReservationCompanyArabicName { get; set; }
    public string? ReservationCompanyEnglishName { get; set; }
    public string? ReservationClientCompanyArabicName { get; set; }
    public string? ReservationClientCompanyEnglishName { get; set; }
    public string? SalesRepresentativeArabicName { get; set; }
    public string? SalesRepresentativeEnglishName { get; set; }
    public string? ReservationSeasonArabicName { get; set; }
    public string? ReservationSeasonEnglishName { get; set; }
    public string? BankAccountName { get; set; }
    public string? CompanyPhoneNumber { get; set; }
    public string? CompanyPolicyParagraph { get; set; }
}