namespace SmartAndPro.ERP.WebAPI.Controllers.Integrations.Wasafaty.Dtos;

// ═══════════════════════════ FHIR Response DTOs ═══════════════════════════

/// <summary>FHIR Bundle containing MedicationRequest entries</summary>
public class FhirBundle
{
    public string ResourceType { get; set; } = "Bundle";
    public string? Type { get; set; }
    public int? Total { get; set; }
    public List<FhirBundleEntry> Entry { get; set; } = new();
}

public class FhirBundleEntry
{
    public string? FullUrl { get; set; }
    public FhirMedicationRequest? Resource { get; set; }
}

/// <summary>FHIR R4 MedicationRequest — simplified mapping for Wasafaty</summary>
public class FhirMedicationRequest
{
    public string ResourceType { get; set; } = "MedicationRequest";
    public string Id { get; set; } = string.Empty;
    public string? Status { get; set; }          // active, completed, cancelled
    public string? Intent { get; set; }           // order, proposal
    public string? Priority { get; set; }         // routine, urgent, stat
    public FhirReference? Subject { get; set; }   // Patient reference
    public FhirReference? Requester { get; set; } // Practitioner reference
    public FhirCodeableConcept? MedicationCodeableConcept { get; set; }
    public List<FhirDosageInstruction>? DosageInstruction { get; set; }
    public FhirDispenseRequest? DispenseRequest { get; set; }
    public List<FhirInsurance>? Insurance { get; set; }
    public FhirMeta? Meta { get; set; }
    public List<FhirExtension>? Extension { get; set; }

    // Wasafaty-specific extensions
    public string? PatientNationalId { get; set; }
    public string? PatientName { get; set; }
    public string? DoctorName { get; set; }
    public string? DoctorLicenseNo { get; set; }
    public string? FacilityName { get; set; }
    public string? InsuranceName { get; set; }
    public string? InsurancePolicyNo { get; set; }
    public decimal? InsuranceCoveragePercent { get; set; }
    public List<FhirMedicationItem>? Items { get; set; }
}

public class FhirReference
{
    public string? Reference { get; set; }
    public string? Display { get; set; }
    public FhirIdentifier? Identifier { get; set; }
}

public class FhirIdentifier
{
    public string? System { get; set; }
    public string? Value { get; set; }
}

public class FhirCodeableConcept
{
    public List<FhirCoding>? Coding { get; set; }
    public string? Text { get; set; }
}

public class FhirCoding
{
    public string? System { get; set; }
    public string? Code { get; set; }
    public string? Display { get; set; }
}

public class FhirDosageInstruction
{
    public string? Text { get; set; }
    public FhirTiming? Timing { get; set; }
    public FhirCodeableConcept? Route { get; set; }
    public FhirDoseAndRate? DoseAndRate { get; set; }
}

public class FhirTiming
{
    public FhirTimingRepeat? Repeat { get; set; }
}

public class FhirTimingRepeat
{
    public int? Frequency { get; set; }
    public int? Period { get; set; }
    public string? PeriodUnit { get; set; } // d, wk, mo
    public int? BoundsDuration { get; set; }
}

public class FhirDoseAndRate
{
    public FhirQuantity? DoseQuantity { get; set; }
}

public class FhirQuantity
{
    public decimal? Value { get; set; }
    public string? Unit { get; set; }
}

public class FhirDispenseRequest
{
    public FhirQuantity? Quantity { get; set; }
    public int? NumberOfRepeatsAllowed { get; set; }
}

public class FhirInsurance
{
    public FhirReference? Coverage { get; set; }
}

public class FhirMeta
{
    public DateTime? LastUpdated { get; set; }
    public string? VersionId { get; set; }
}

public class FhirExtension
{
    public string? Url { get; set; }
    public string? ValueString { get; set; }
    public decimal? ValueDecimal { get; set; }
    public bool? ValueBoolean { get; set; }
}

/// <summary>Simplified medication item within a Wasafaty prescription bundle</summary>
public class FhirMedicationItem
{
    public string DrugCode { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? DosageForm { get; set; }
    public string? Strength { get; set; }
    public string? DosageInstruction { get; set; }
    public string? Frequency { get; set; }
    public int? DurationDays { get; set; }
    public decimal Quantity { get; set; }
    public bool AllowSubstitute { get; set; }
    public bool IsControlled { get; set; }
}

// ═══════════════════════════ Wasafaty API Request/Response DTOs ═══════════════════════════

/// <summary>Wasafaty webhook notification payload</summary>
public class WasafatyNotificationPayload
{
    public string Event { get; set; } = string.Empty;
    public WasafatyNotificationData? Data { get; set; }
}

public class WasafatyNotificationData
{
    public string FhirId { get; set; } = string.Empty;
    public string? PatientNationalId { get; set; }
    public string? Status { get; set; }
    public DateTime? Timestamp { get; set; }
}

/// <summary>OTP verification request — patient enters OTP from Wasafaty app</summary>
public class VerifyOtpRequest
{
    public string NationalId { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public string? FhirResourceId { get; set; }
}

/// <summary>OTP verification response</summary>
public class VerifyOtpResponse
{
    public bool Verified { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
}

/// <summary>Fetch prescription by NID + OTP or by FHIR ID</summary>
public class FetchPrescriptionRequest
{
    public string? NationalId { get; set; }
    public string? Otp { get; set; }
    public string? FhirResourceId { get; set; }
}

/// <summary>Response for fetched prescription — mapped for cashier display</summary>
public class WasafatyPrescriptionResponse
{
    public Guid Id { get; set; }
    public string FhirResourceId { get; set; } = string.Empty;
    public string PatientNationalId { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string? DoctorName { get; set; }
    public string? DoctorLicenseNo { get; set; }
    public string? PrescriberFacilityName { get; set; }
    public string? InsuranceName { get; set; }
    public string? InsurancePolicyNo { get; set; }
    public decimal InsuranceCoveragePercent { get; set; }
    public int ItemsCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool OtpVerified { get; set; }
    public DateTime ReceivedAt { get; set; }
    public List<WasafatyPrescriptionItemResponse> Items { get; set; } = new();
}

public class WasafatyPrescriptionItemResponse
{
    public Guid Id { get; set; }
    public string DrugCode { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? DosageForm { get; set; }
    public string? Strength { get; set; }
    public string? DosageInstruction { get; set; }
    public string? Frequency { get; set; }
    public int? DurationDays { get; set; }
    public decimal RequestedQty { get; set; }
    public decimal DispensedQty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public decimal InsuranceCoveredAmount { get; set; }
    public decimal PatientPayAmount { get; set; }
    public bool AllowSubstitute { get; set; }
    public bool IsControlled { get; set; }
    public int? ErpProductId { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

/// <summary>Mark prescription as dispensed — sent after cashier payment</summary>
public class DispenseRequest
{
    public Guid WasafatyPrescriptionId { get; set; }
    public int? SaleInvoiceId { get; set; }
    public Guid? CashierPaymentId { get; set; }
    public List<DispenseItemRequest> Items { get; set; } = new();
}

public class DispenseItemRequest
{
    public Guid ItemId { get; set; }
    public decimal DispensedQty { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? ErpProductId { get; set; }
    public decimal UnitPrice { get; set; }
}

/// <summary>NPHIES insurance claim submission request</summary>
public class SubmitClaimRequest
{
    public Guid WasafatyPrescriptionId { get; set; }
}

/// <summary>NPHIES claim response</summary>
public class SubmitClaimResponse
{
    public bool Success { get; set; }
    public string? NphiesClaimId { get; set; }
    public string? Status { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
}

/// <summary>Wasafaty integration settings response (dashboard)</summary>
public class WasafatySettingsResponse
{
    public bool IsConnected { get; set; }
    public string? FacilityCode { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
    public int PendingPrescriptions { get; set; }
    public int DispensedToday { get; set; }
    public int PendingClaims { get; set; }
}

/// <summary>Sync log response for audit screen</summary>
public class WasafatySyncLogResponse
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? FhirResourceId { get; set; }
    public int DurationMs { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Manual token set (for development/testing)</summary>
public class SetTokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public long? ExpiresAtUnix { get; set; }
}
