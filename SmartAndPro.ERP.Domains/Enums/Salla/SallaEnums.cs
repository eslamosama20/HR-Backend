namespace SmartAndPro.ERP.Domains.Enums.Salla;

public enum SallaOrderStatus
{
    New = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3,
    Refunded = 4,
    Restoring = 5
}

public enum SallaPaymentStatus
{
    Pending = 0,
    Paid = 1,
    Failed = 2,
    Refunded = 3
}

public enum SallaFulfillmentStatus
{
    Pending = 0,
    Fulfilled = 1,
    PartiallyFulfilled = 2,
    Unfulfilled = 3
}

public enum SallaSyncStatus
{
    Pending = 0,
    Synced = 1,
    Failed = 2,
    Skipped = 3
}

public enum SallaSyncDirection
{
    SallaToErp = 0,
    ErpToSalla = 1
}
