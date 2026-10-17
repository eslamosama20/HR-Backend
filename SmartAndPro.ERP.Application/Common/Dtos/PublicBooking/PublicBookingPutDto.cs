using SmartAndPro.ERP.SharedKernel.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;

public class PublicBookingPutDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public Guid HotelId { get; set; }

    [Required]
    [MaxLength(100)]
    public string GuestName { get; set; }

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public string GuestEmail { get; set; }

    [Required]
    [MaxLength(50)]
    public string GuestPhone { get; set; }

    [MaxLength(100)]
    public string? GuestNationality { get; set; }

    [Required]
    [Range(1, 50)]
    public int NumberOfAdults { get; set; }

    [Range(0, 20)]
    public int NumberOfChildren { get; set; }

    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }

    [MaxLength(1000)]
    public string? SpecialRequests { get; set; }

    [Required]
    public PublicBookingStatus Status { get; set; }

    [Required]
    public PaymentStatus PaymentStatus { get; set; }

    [MaxLength(50)]
    public string? PaymentMethod { get; set; }

    public string? TransactionId { get; set; }

    public string? PaymentDetails { get; set; }

    public int? PaymobOrderId { get; set; }

    [MaxLength(500)]
    public string? CancellationReason { get; set; }

    public Guid? ErpReservationId { get; set; }
}
