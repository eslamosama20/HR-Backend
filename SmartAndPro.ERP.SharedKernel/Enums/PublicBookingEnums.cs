namespace SmartAndPro.ERP.SharedKernel.Enums;

/// <summary>
/// Status of public booking
/// </summary>
public enum PublicBookingStatus
{
    /// <summary>
    /// Booking submitted but not confirmed
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Booking confirmed by hotel
    /// </summary>
    Confirmed = 1,

    /// <summary>
    /// Guest checked in
    /// </summary>
    CheckedIn = 2,

    /// <summary>
    /// Guest checked out
    /// </summary>
    CheckedOut = 3,

    /// <summary>
    /// Booking cancelled
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Guest didn't show up
    /// </summary>
    NoShow = 5
}

/// <summary>
/// Payment status for booking
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment not yet made
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Partial payment received
    /// </summary>
    PartiallyPaid = 1,

    /// <summary>
    /// Full payment received
    /// </summary>
    Paid = 2,

    /// <summary>
    /// Payment refunded
    /// </summary>
    Refunded = 3,

    /// <summary>
    /// Payment failed
    /// </summary>
    Failed = 4
}
