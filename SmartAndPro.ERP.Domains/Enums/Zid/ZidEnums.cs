namespace SmartAndPro.ERP.Domains.Enums.Zid;

public enum ZidOrderStatus
{
    New = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3,
    Refunded = 4,
    ReadyForShipping = 5,
    Shipped = 6,
    Delivered = 7
}

public enum ZidPaymentStatus
{
    Pending = 0,
    Paid = 1,
    Failed = 2,
    Refunded = 3,
    PartiallyRefunded = 4
}

public enum ZidFulfillmentStatus
{
    Pending = 0,
    Fulfilled = 1,
    PartiallyFulfilled = 2,
    Unfulfilled = 3
}

public enum ZidSyncStatus
{
    Pending = 0,
    Synced = 1,
    Failed = 2,
    Skipped = 3
}

public enum ZidSyncDirection
{
    ZidToErp = 0,
    ErpToZid = 1
}
