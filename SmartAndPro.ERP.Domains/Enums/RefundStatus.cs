namespace SmartAndPro.ERP.Domains.Enums;

/// <summary>
/// Refund processing status for cancelled bookings
/// </summary>
public enum RefundStatus
{
    /// <summary>
    /// Refund request initiated but not yet processed
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Refund is being processed by payment gateway
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Refund successfully completed
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Refund failed due to technical error
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Refund rejected (e.g., outside refund window, policy violation)
    /// </summary>
    Rejected = 4,

    /// <summary>
    /// Partial refund completed (e.g., after deducting cancellation fee)
    /// </summary>
    PartiallyRefunded = 5
}
