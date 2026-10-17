namespace SmartAndPro.ERP.Domains.Enums.Wasafaty;

public enum WasafatyPrescriptionStatus
{
    Pending = 0,
    Verified = 1,
    Dispensing = 2,
    Dispensed = 3,
    PartiallyDispensed = 4,
    Cancelled = 5,
    Rejected = 6
}

public enum WasafatyClaimStatus
{
    NotClaimed = 0,
    Submitted = 1,
    Approved = 2,
    Rejected = 3,
    PartiallyApproved = 4
}

public enum WasafatySyncStatus
{
    Pending = 0,
    Synced = 1,
    Failed = 2,
    Skipped = 3
}

public enum WasafatySyncDirection
{
    WasafatyToErp = 0,
    ErpToWasafaty = 1
}
