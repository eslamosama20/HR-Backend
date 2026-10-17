using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.SharedKernel.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAndPro.ERP.Domains.Entities.PublicBooking;

/// <summary>
/// Payment transaction records for tracking all payment operations
/// </summary>
public class PaymentTransaction : Entity
{
    /// <summary>
    /// Related booking ID
    /// </summary>
    [Required]
    public Guid BookingId { get; set; }

    /// <summary>
    /// Transaction amount
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Payment method used (Stripe, PayTabs, Cash, BankTransfer)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Transaction type (Payment, Refund)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string TransactionType { get; set; } = "Payment";

    /// <summary>
    /// Payment status
    /// </summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>
    /// Gateway transaction ID from Stripe/PayTabs
    /// </summary>
    [MaxLength(200)]
    public string? GatewayTransactionId { get; set; }

    /// <summary>
    /// Full gateway response stored as JSON
    /// </summary>
    public string? GatewayResponse { get; set; }

    /// <summary>
    /// Currency code (SAR, USD, EUR, etc.)
    /// </summary>
    [MaxLength(10)]
    public string Currency { get; set; } = "SAR";

    /// <summary>
    /// Customer card last 4 digits (if applicable)
    /// </summary>
    [MaxLength(4)]
    public string? CardLast4 { get; set; }

    /// <summary>
    /// Card brand (Visa, Mastercard, Mada, etc.)
    /// </summary>
    [MaxLength(50)]
    public string? CardBrand { get; set; }

    /// <summary>
    /// Error message if transaction failed
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Transaction processing date
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// IP address of the customer
    /// </summary>
    [MaxLength(50)]
    public string? CustomerIpAddress { get; set; }

    /// <summary>
    /// User agent (browser/device info)
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    // Navigation properties
    [ForeignKey(nameof(BookingId))]
    public virtual Domains.Entities.PublicBooking.PublicBooking Booking { get; set; } = null!;
}
