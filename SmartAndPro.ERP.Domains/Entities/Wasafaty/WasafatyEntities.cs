using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Enums.Wasafaty;

namespace SmartAndPro.ERP.Domains.Entities.Wasafaty;

/// <summary>
/// Links an incoming Wasafaty (وصفتي) e-prescription to the internal ERP pharmacy prescription.
/// Stores the FHIR resource reference and tracks dispensing + insurance claim status.
/// </summary>
public class WasafatyPrescription : Entity, IEntityTypeConfiguration<WasafatyPrescription>
{
    /// <summary>FHIR MedicationRequest resource ID from NHIC</summary>
    public string FhirResourceId { get; set; } = string.Empty;

    /// <summary>Patient Saudi National ID (رقم الهوية)</summary>
    public string PatientNationalId { get; set; } = string.Empty;

    /// <summary>Patient name from Wasafaty</summary>
    public string PatientName { get; set; } = string.Empty;

    /// <summary>Doctor name from Wasafaty</summary>
    public string? DoctorName { get; set; }

    /// <summary>Doctor license number from MOH</summary>
    public string? DoctorLicenseNo { get; set; }

    /// <summary>Prescribing facility name</summary>
    public string? PrescriberFacilityName { get; set; }

    /// <summary>Insurance company name (if insured)</summary>
    public string? InsuranceName { get; set; }

    /// <summary>Insurance policy/member number</summary>
    public string? InsurancePolicyNo { get; set; }

    /// <summary>Insurance coverage percentage (0-100)</summary>
    public decimal InsuranceCoveragePercent { get; set; }

    /// <summary>Total items count in prescription</summary>
    public int ItemsCount { get; set; }

    /// <summary>Prescription status lifecycle</summary>
    public WasafatyPrescriptionStatus Status { get; set; } = WasafatyPrescriptionStatus.Pending;

    /// <summary>Insurance claim status</summary>
    public WasafatyClaimStatus ClaimStatus { get; set; } = WasafatyClaimStatus.NotClaimed;

    /// <summary>NPHIES claim reference ID after submission</summary>
    public string? NphiesClaimId { get; set; }

    /// <summary>FK to internal PharmacyPrescription once mapped</summary>
    public Guid? InternalPrescriptionId { get; set; }

    /// <summary>FK to CashierPayment once paid</summary>
    public Guid? CashierPaymentId { get; set; }

    /// <summary>The SaleInvoice ID if linked</summary>
    public int? SaleInvoiceId { get; set; }

    /// <summary>Store where this prescription is being dispensed</summary>
    public Guid? StoreDetailsId { get; set; }

    /// <summary>Wasafaty OTP verified flag</summary>
    public bool OtpVerified { get; set; }

    /// <summary>When the prescription was received from Wasafaty</summary>
    public DateTime ReceivedAt { get; set; }

    /// <summary>When the prescription was dispensed</summary>
    public DateTime? DispensedAt { get; set; }

    /// <summary>Original FHIR JSON for audit trail</summary>
    public string? RawFhirJson { get; set; }

    /// <summary>Error message if sync/claim failed</summary>
    public string? ErrorMessage { get; set; }

    // Navigation
    public virtual ICollection<WasafatyPrescriptionItem> Items { get; set; } = new List<WasafatyPrescriptionItem>();

    public void Configure(EntityTypeBuilder<WasafatyPrescription> builder)
    {
        builder.ToTable("WasafatyPrescriptions").HasKey(x => x.Id);

        builder.Property(x => x.FhirResourceId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PatientNationalId).HasMaxLength(20).IsRequired();
        builder.Property(x => x.PatientName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DoctorName).HasMaxLength(200);
        builder.Property(x => x.DoctorLicenseNo).HasMaxLength(50);
        builder.Property(x => x.PrescriberFacilityName).HasMaxLength(300);
        builder.Property(x => x.InsuranceName).HasMaxLength(200);
        builder.Property(x => x.InsurancePolicyNo).HasMaxLength(100);
        builder.Property(x => x.InsuranceCoveragePercent).HasColumnType("decimal(5,2)");
        builder.Property(x => x.NphiesClaimId).HasMaxLength(100);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);

        builder.HasIndex(x => x.FhirResourceId).IsUnique();
        builder.HasIndex(x => x.PatientNationalId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.InternalPrescriptionId);
        builder.HasIndex(x => x.CashierPaymentId);
        builder.HasIndex(x => x.ReceivedAt);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.WasafatyPrescription)
            .HasForeignKey(x => x.WasafatyPrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => x.IsActive);
    }
}

/// <summary>
/// Individual medication item from a Wasafaty prescription (maps to FHIR MedicationRequest line).
/// </summary>
public class WasafatyPrescriptionItem : Entity, IEntityTypeConfiguration<WasafatyPrescriptionItem>
{
    public Guid WasafatyPrescriptionId { get; set; }

    /// <summary>SFDA drug code (Saudi Food and Drug Authority)</summary>
    public string DrugCode { get; set; } = string.Empty;

    /// <summary>Drug trade name</summary>
    public string DrugName { get; set; } = string.Empty;

    /// <summary>Drug generic/scientific name</summary>
    public string? GenericName { get; set; }

    /// <summary>Dosage form (tablet, capsule, syrup, etc.)</summary>
    public string? DosageForm { get; set; }

    /// <summary>Strength (e.g. 500mg)</summary>
    public string? Strength { get; set; }

    /// <summary>Dosage instructions text</summary>
    public string? DosageInstruction { get; set; }

    /// <summary>Frequency (e.g. "3 times daily")</summary>
    public string? Frequency { get; set; }

    /// <summary>Duration in days</summary>
    public int? DurationDays { get; set; }

    /// <summary>Requested quantity from doctor</summary>
    public decimal RequestedQty { get; set; }

    /// <summary>Actually dispensed quantity</summary>
    public decimal DispensedQty { get; set; }

    /// <summary>Unit price at time of dispensing</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Line total after discount</summary>
    public decimal LineTotal { get; set; }

    /// <summary>Insurance covered amount for this line</summary>
    public decimal InsuranceCoveredAmount { get; set; }

    /// <summary>Patient pay amount for this line</summary>
    public decimal PatientPayAmount { get; set; }

    /// <summary>Whether substitution allowed by doctor</summary>
    public bool AllowSubstitute { get; set; }

    /// <summary>Whether this is a controlled substance</summary>
    public bool IsControlled { get; set; }

    /// <summary>Mapped ERP product ID (ProductUnit)</summary>
    public int? ErpProductId { get; set; }

    /// <summary>Batch number used for dispensing</summary>
    public string? BatchNo { get; set; }

    /// <summary>Expiry date of dispensed batch</summary>
    public DateTime? ExpiryDate { get; set; }

    // Navigation
    public WasafatyPrescription? WasafatyPrescription { get; set; }

    public void Configure(EntityTypeBuilder<WasafatyPrescriptionItem> builder)
    {
        builder.ToTable("WasafatyPrescriptionItems").HasKey(x => x.Id);

        builder.Property(x => x.DrugCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DrugName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.GenericName).HasMaxLength(300);
        builder.Property(x => x.DosageForm).HasMaxLength(50);
        builder.Property(x => x.Strength).HasMaxLength(50);
        builder.Property(x => x.DosageInstruction).HasMaxLength(500);
        builder.Property(x => x.Frequency).HasMaxLength(100);
        builder.Property(x => x.BatchNo).HasMaxLength(50);

        builder.Property(x => x.RequestedQty).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DispensedQty).HasColumnType("decimal(18,2)");
        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.LineTotal).HasColumnType("decimal(18,2)");
        builder.Property(x => x.InsuranceCoveredAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.PatientPayAmount).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.WasafatyPrescriptionId);
        builder.HasIndex(x => x.DrugCode);
        builder.HasQueryFilter(x => x.IsActive);
    }
}

/// <summary>
/// Sync log for Wasafaty API interactions — tracks all inbound/outbound calls for audit.
/// </summary>
public class WasafatySyncLog : Entity, IEntityTypeConfiguration<WasafatySyncLog>
{
    public string EventType { get; set; } = string.Empty;
    public WasafatySyncDirection Direction { get; set; }
    public WasafatySyncStatus Status { get; set; }
    public string? RequestPayload { get; set; }
    public string? ResponsePayload { get; set; }
    public string? ErrorMessage { get; set; }
    public string? FhirResourceId { get; set; }
    public int DurationMs { get; set; }

    public void Configure(EntityTypeBuilder<WasafatySyncLog> builder)
    {
        builder.ToTable("WasafatySyncLogs").HasKey(x => x.Id);

        builder.Property(x => x.EventType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RequestPayload).HasMaxLength(4000);
        builder.Property(x => x.ResponsePayload).HasMaxLength(4000);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
        builder.Property(x => x.FhirResourceId).HasMaxLength(200);

        builder.HasIndex(x => x.EventType);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAt);
    }
}

/// <summary>
/// Persists the Wasafaty/NHIC OAuth access token so it survives server restarts.
/// Only one active record expected at a time.
/// </summary>
public class WasafatyAuthToken : Entity, IEntityTypeConfiguration<WasafatyAuthToken>
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }

    public void Configure(EntityTypeBuilder<WasafatyAuthToken> builder)
    {
        builder.ToTable("WasafatyAuthTokens");
        builder.HasKey(x => x.Id);
    }
}
