using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;

namespace SmartAndPro.ERP.Domains.Entities.Pharmacy;

// ─── Patient ───────────────────────────────────────────────────────
public class PharmacyPatient : Entity, IEntityTypeConfiguration<PharmacyPatient>
{
    public int SeqNo { get; set; }
    public string PatientCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = "Male";
    public DateTime? Dob { get; set; }
    public string? NationalId { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? InsuranceClass { get; set; }
    public bool HasInsurance { get; set; }
    public int ActivePrescriptions { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicDiseases { get; set; }
    public DateTime? LastVisit { get; set; }
    public string? Notes { get; set; }
    public Guid? StoreDetailsId { get; set; }

    public void Configure(EntityTypeBuilder<PharmacyPatient> b)
    {
        b.ToTable("PharmacyPatient").HasKey(x => x.Id);
        b.Property(x => x.SeqNo).ValueGeneratedOnAdd();
        b.Property(x => x.PatientCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Gender).HasMaxLength(10).IsRequired();
        b.Property(x => x.NationalId).HasMaxLength(20);
        b.Property(x => x.Phone).HasMaxLength(20);
        b.Property(x => x.Mobile).HasMaxLength(20);
        b.Property(x => x.Address).HasMaxLength(500);
        b.Property(x => x.Email).HasMaxLength(200);
        b.Property(x => x.InsuranceClass).HasMaxLength(100);
        b.Property(x => x.Allergies).HasMaxLength(500);
        b.Property(x => x.ChronicDiseases).HasMaxLength(500);
        b.Property(x => x.Notes).HasMaxLength(500);
        b.HasIndex(x => x.SeqNo).IsUnique();
        b.HasIndex(x => x.PatientCode);
        b.HasIndex(x => x.NationalId);
        b.HasIndex(x => x.Mobile);
        b.HasQueryFilter(x => x.IsActive);
    }
}

// ─── Doctor ────────────────────────────────────────────────────────
public class PharmacyDoctor : Entity, IEntityTypeConfiguration<PharmacyDoctor>
{
    public int SeqNo { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Specialization { get; set; }
    public string? LicenseNo { get; set; }
    public string? Mobile { get; set; }
    public Guid? StoreDetailsId { get; set; }

    public void Configure(EntityTypeBuilder<PharmacyDoctor> b)
    {
        b.ToTable("PharmacyDoctor").HasKey(x => x.Id);
        b.Property(x => x.SeqNo).ValueGeneratedOnAdd();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Specialization).HasMaxLength(200);
        b.Property(x => x.LicenseNo).HasMaxLength(50);
        b.Property(x => x.Mobile).HasMaxLength(20);
        b.HasIndex(x => x.SeqNo).IsUnique();
        b.HasIndex(x => x.Name);
        b.HasIndex(x => x.LicenseNo);
        b.HasQueryFilter(x => x.IsActive);
    }
}

// ─── Insurance Company ─────────────────────────────────────────────
public class PharmacyInsuranceCompany : Entity, IEntityTypeConfiguration<PharmacyInsuranceCompany>
{
    public int SeqNo { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DefaultCoveragePercentage { get; set; }
    public Guid? StoreDetailsId { get; set; }

    public void Configure(EntityTypeBuilder<PharmacyInsuranceCompany> b)
    {
        b.ToTable("PharmacyInsuranceCompany").HasKey(x => x.Id);
        b.Property(x => x.SeqNo).ValueGeneratedOnAdd();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.DefaultCoveragePercentage).HasColumnType("decimal(5,2)");
        b.HasIndex(x => x.SeqNo).IsUnique();
        b.HasIndex(x => x.Name);
        b.HasQueryFilter(x => x.IsActive);
    }
}

// ─── Prescription ──────────────────────────────────────────────────
public class PharmacyPrescription : Entity, IEntityTypeConfiguration<PharmacyPrescription>
{
    public int SeqNo { get; set; }
    public string RxNo { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid? PatientId { get; set; }
    public string? DoctorName { get; set; }
    public Guid? DoctorId { get; set; }
    public string? BranchId { get; set; }
    public string? BranchName { get; set; }
    public string? StoreId { get; set; }
    public string? StoreName { get; set; }
    public string PaymentType { get; set; } = "Cash";
    public string Priority { get; set; } = "Normal";
    public string Status { get; set; } = "Pending";
    public bool InsuranceEnabled { get; set; }
    public Guid? InsuranceCompanyId { get; set; }
    public string? InsuranceName { get; set; }
    public string? InsuranceMemberNo { get; set; }
    public string? InsuranceApprovalNo { get; set; }
    public decimal InsuranceCoveragePercentage { get; set; }
    public decimal InsurancePatientSharePercentage { get; set; }
    public decimal InsuranceCoveredAmount { get; set; }
    public decimal InsurancePatientPayableAmount { get; set; }
    public string? InsuranceStatus { get; set; }
    public int ItemsCount { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PatientPayable { get; set; }
    public string? Notes { get; set; }
    public string? CancelReason { get; set; }
    public string? PosTerminalId { get; set; }
    public int? SalesInvoiceId { get; set; }
    public string? PosQueueNo { get; set; }
    public Guid? StoreDetailsId { get; set; }

    // Navigation
    public PharmacyPatient? Patient { get; set; }
    public PharmacyDoctor? Doctor { get; set; }
    public PharmacyInsuranceCompany? InsuranceCompany { get; set; }
    public ICollection<PharmacyPrescriptionItem> Items { get; set; } = new List<PharmacyPrescriptionItem>();
    public ICollection<PharmacyAttachment> Attachments { get; set; } = new List<PharmacyAttachment>();

    public void Configure(EntityTypeBuilder<PharmacyPrescription> b)
    {
        b.ToTable("PharmacyPrescription").HasKey(x => x.Id);
        b.Property(x => x.SeqNo).ValueGeneratedOnAdd();
        b.Property(x => x.RxNo).HasMaxLength(50).IsRequired();
        b.Property(x => x.PatientName).HasMaxLength(200).IsRequired();
        b.Property(x => x.DoctorName).HasMaxLength(200);
        b.Property(x => x.BranchId).HasMaxLength(50);
        b.Property(x => x.BranchName).HasMaxLength(200);
        b.Property(x => x.StoreId).HasMaxLength(50);
        b.Property(x => x.StoreName).HasMaxLength(200);
        b.Property(x => x.PaymentType).HasMaxLength(20).IsRequired();
        b.Property(x => x.Priority).HasMaxLength(20).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.InsuranceName).HasMaxLength(200);
        b.Property(x => x.InsuranceMemberNo).HasMaxLength(100);
        b.Property(x => x.InsuranceApprovalNo).HasMaxLength(100);
        b.Property(x => x.InsuranceCoveragePercentage).HasColumnType("decimal(5,2)");
        b.Property(x => x.InsurancePatientSharePercentage).HasColumnType("decimal(5,2)");
        b.Property(x => x.InsuranceCoveredAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.InsurancePatientPayableAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.InsuranceStatus).HasMaxLength(30);
        b.Property(x => x.SubTotal).HasColumnType("decimal(18,2)");
        b.Property(x => x.DiscountTotal).HasColumnType("decimal(18,2)");
        b.Property(x => x.TaxTotal).HasColumnType("decimal(18,2)");
        b.Property(x => x.GrandTotal).HasColumnType("decimal(18,2)");
        b.Property(x => x.PatientPayable).HasColumnType("decimal(18,2)");
        b.Property(x => x.Notes).HasMaxLength(500);
        b.Property(x => x.CancelReason).HasMaxLength(500);
        b.Property(x => x.PosTerminalId).HasMaxLength(50);
        b.Property(x => x.PosQueueNo).HasMaxLength(50);

        b.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Doctor).WithMany().HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.InsuranceCompany).WithMany().HasForeignKey(x => x.InsuranceCompanyId).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.SeqNo).IsUnique();
        b.HasIndex(x => x.RxNo);
        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.PatientId);
        b.HasIndex(x => x.DoctorId);
        b.HasIndex(x => x.InsuranceCompanyId);
        b.HasQueryFilter(x => x.IsActive);
    }
}

// ─── Prescription Item ─────────────────────────────────────────────
public class PharmacyPrescriptionItem : Entity, IEntityTypeConfiguration<PharmacyPrescriptionItem>
{
    public int SeqNo { get; set; }
    public Guid PrescriptionId { get; set; }
    public string? PrescriptionRxNo { get; set; }
    public int LineNo { get; set; }
    public Guid? ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal RequestedQty { get; set; }
    public decimal DispensedQty { get; set; }
    public string? StoreId { get; set; }
    public string? BatchId { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public bool AllowSubstitute { get; set; }
    public bool IsControlled { get; set; }
    public string AvailabilityStatus { get; set; } = "Available";
    public string? Notes { get; set; }
    public Guid? StoreDetailsId { get; set; }

    public PharmacyPrescription? Prescription { get; set; }
    public MainPurchaseClass? Item { get; set; }

    public void Configure(EntityTypeBuilder<PharmacyPrescriptionItem> b)
    {
        b.ToTable("PharmacyPrescriptionItem").HasKey(x => x.Id);
        b.Property(x => x.SeqNo).ValueGeneratedOnAdd();
        b.Property(x => x.PrescriptionRxNo).HasMaxLength(50);
        b.Property(x => x.ItemCode).HasMaxLength(100).IsRequired();
        b.Property(x => x.ItemName).HasMaxLength(200).IsRequired();
        b.Property(x => x.StoreId).HasMaxLength(50);
        b.Property(x => x.BatchId).HasMaxLength(50);
        b.Property(x => x.BatchNo).HasMaxLength(50);
        b.Property(x => x.RequestedQty).HasColumnType("decimal(18,2)");
        b.Property(x => x.DispensedQty).HasColumnType("decimal(18,2)");
        b.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        b.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.TaxAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.LineTotal).HasColumnType("decimal(18,2)");
        b.Property(x => x.AvailabilityStatus).HasMaxLength(30).IsRequired();
        b.Property(x => x.Notes).HasMaxLength(500);

        b.HasOne(x => x.Prescription).WithMany(p => p.Items).HasForeignKey(x => x.PrescriptionId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Item).WithMany().HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.SetNull);
        b.HasIndex(x => x.SeqNo).IsUnique();
        b.HasIndex(x => x.PrescriptionId);
        b.HasIndex(x => x.ItemId);
        b.HasQueryFilter(x => x.IsActive);
    }
}

// ─── Attachment ────────────────────────────────────────────────────
public class PharmacyAttachment : Entity, IEntityTypeConfiguration<PharmacyAttachment>
{
    public int SeqNo { get; set; }
    public Guid PrescriptionId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime UploadedAt { get; set; }
    public string? Notes { get; set; }
    public string? FilePath { get; set; }
    public Guid? StoreDetailsId { get; set; }

    public PharmacyPrescription? Prescription { get; set; }

    public void Configure(EntityTypeBuilder<PharmacyAttachment> b)
    {
        b.ToTable("PharmacyAttachment").HasKey(x => x.Id);
        b.Property(x => x.SeqNo).ValueGeneratedOnAdd();
        b.Property(x => x.FileName).HasMaxLength(500).IsRequired();
        b.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
        b.Property(x => x.Notes).HasMaxLength(500);
        b.Property(x => x.FilePath).HasMaxLength(1000);

        b.HasOne(x => x.Prescription).WithMany(p => p.Attachments).HasForeignKey(x => x.PrescriptionId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.SeqNo).IsUnique();
        b.HasIndex(x => x.PrescriptionId);
        b.HasQueryFilter(x => x.IsActive);
    }
}

// ─── Insurance Claim ───────────────────────────────────────────────
public class PharmacyInsuranceClaim : Entity, IEntityTypeConfiguration<PharmacyInsuranceClaim>
{
    public string ClaimNo { get; set; } = string.Empty;
    public string? RxNo { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string Insurer { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal ApprovedAmount { get; set; }
    public string Status { get; set; } = "Submitted";
    public string? ReferenceNo { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? Notes { get; set; }
    public Guid? StoreDetailsId { get; set; }

    // ── NPHIES / CCHI Integration ────────────────────────────────
    /// <summary>FK to local PharmacyPrescription</summary>
    public Guid? PrescriptionId { get; set; }
    /// <summary>FK to PharmacyInsuranceCompany</summary>
    public Guid? InsuranceCompanyId { get; set; }
    public string? InsurancePolicyNo { get; set; }
    public string? InsuranceMemberNo { get; set; }
    public decimal CoveragePercentage { get; set; }
    public decimal InsuranceCoveredAmount { get; set; }
    public decimal PatientShareAmount { get; set; }
    /// <summary>Claim ID returned by NPHIES after submission</summary>
    public string? NphiesClaimId { get; set; }
    /// <summary>NPHIES adjudication status: pending|queued|approved|rejected|paid</summary>
    public string? NphiesStatus { get; set; }
    /// <summary>FK to SaleInvoice (int) linked to this claim</summary>
    public int? SaleInvoiceId { get; set; }
    /// <summary>FK to CashierPayment (Guid) for the patient-share payment</summary>
    public Guid? CashierPaymentId { get; set; }
    /// <summary>Raw FHIR ClaimResponse JSON from NPHIES</summary>
    public string? ClaimResponseJson { get; set; }
    public DateTime? NphiesSubmittedAt { get; set; }
    public DateTime? NphiesAdjudicatedAt { get; set; }

    // Navigation
    public PharmacyPrescription? Prescription { get; set; }
    public PharmacyInsuranceCompany? InsuranceCompany { get; set; }

    public void Configure(EntityTypeBuilder<PharmacyInsuranceClaim> b)
    {
        b.ToTable("PharmacyInsuranceClaim").HasKey(x => x.Id);
        b.Property(x => x.ClaimNo).HasMaxLength(50).IsRequired();
        b.Property(x => x.RxNo).HasMaxLength(50);
        b.Property(x => x.PatientName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Insurer).HasMaxLength(200).IsRequired();
        b.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.ApprovedAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.ReferenceNo).HasMaxLength(100);
        b.Property(x => x.Notes).HasMaxLength(500);
        // NPHIES fields
        b.Property(x => x.InsurancePolicyNo).HasMaxLength(100);
        b.Property(x => x.InsuranceMemberNo).HasMaxLength(100);
        b.Property(x => x.CoveragePercentage).HasColumnType("decimal(5,2)");
        b.Property(x => x.InsuranceCoveredAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.PatientShareAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.NphiesClaimId).HasMaxLength(200);
        b.Property(x => x.NphiesStatus).HasMaxLength(50);
        b.Property(x => x.ClaimResponseJson).HasColumnType("nvarchar(max)");
        b.HasOne(x => x.Prescription).WithMany().HasForeignKey(x => x.PrescriptionId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.InsuranceCompany).WithMany().HasForeignKey(x => x.InsuranceCompanyId).OnDelete(DeleteBehavior.SetNull);
        b.HasIndex(x => x.PrescriptionId);
        b.HasIndex(x => x.NphiesClaimId);
        b.HasQueryFilter(x => x.IsActive);
    }
}

// ─── Controlled Drug Record ────────────────────────────────────────
public class PharmacyControlledDrugRecord : Entity, IEntityTypeConfiguration<PharmacyControlledDrugRecord>
{
    public string LogNo { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Prescriber { get; set; } = string.Empty;
    public string Pharmacist { get; set; } = string.Empty;
    public string? PermitNo { get; set; }
    public string Status { get; set; } = "Active";
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public DateTime DispenseDate { get; set; }
    public Guid? StoreDetailsId { get; set; }

    public void Configure(EntityTypeBuilder<PharmacyControlledDrugRecord> b)
    {
        b.ToTable("PharmacyControlledDrugRecord").HasKey(x => x.Id);
        b.Property(x => x.LogNo).HasMaxLength(50).IsRequired();
        b.Property(x => x.PatientName).HasMaxLength(200).IsRequired();
        b.Property(x => x.DrugName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Schedule).HasMaxLength(10).IsRequired();
        b.Property(x => x.Quantity).HasColumnType("decimal(18,2)");
        b.Property(x => x.Prescriber).HasMaxLength(200).IsRequired();
        b.Property(x => x.Pharmacist).HasMaxLength(120).IsRequired();
        b.Property(x => x.PermitNo).HasMaxLength(50);
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.Reason).HasMaxLength(300);
        b.Property(x => x.Notes).HasMaxLength(500);
        b.HasQueryFilter(x => x.IsActive);
    }
}

// ─── Alert ─────────────────────────────────────────────────────────
public class PharmacyAlert : Entity, IEntityTypeConfiguration<PharmacyAlert>
{
    public string Code { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Level { get; set; } = "Info";
    public string State { get; set; } = "New";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Branch { get; set; }
    public string? Owner { get; set; }
    public Guid? StoreDetailsId { get; set; }

    public void Configure(EntityTypeBuilder<PharmacyAlert> b)
    {
        b.ToTable("PharmacyAlert").HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.Type).HasMaxLength(20).IsRequired();
        b.Property(x => x.Level).HasMaxLength(20).IsRequired();
        b.Property(x => x.State).HasMaxLength(20).IsRequired();
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Message).HasMaxLength(500).IsRequired();
        b.Property(x => x.Branch).HasMaxLength(100);
        b.Property(x => x.Owner).HasMaxLength(120);
        b.HasQueryFilter(x => x.IsActive);
    }
}

// ─── Return ────────────────────────────────────────────────────────
public class PharmacyReturn : Entity, IEntityTypeConfiguration<PharmacyReturn>
{
    public string ReturnNo { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? BatchNo { get; set; }
    public decimal Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public DateTime ReturnDate { get; set; }
    public Guid? StoreDetailsId { get; set; }
    /// <summary>Reference invoice number (e.g. INV-001)</summary>
    public string? ReferenceNo { get; set; }
    /// <summary>FK to the original sales invoice ID</summary>
    public int? InvoiceId { get; set; }

    public void Configure(EntityTypeBuilder<PharmacyReturn> b)
    {
        b.ToTable("PharmacyReturn").HasKey(x => x.Id);
        b.Property(x => x.ReturnNo).HasMaxLength(50).IsRequired();
        b.Property(x => x.Source).HasMaxLength(20).IsRequired();
        b.Property(x => x.ItemCode).HasMaxLength(100).IsRequired();
        b.Property(x => x.ItemName).HasMaxLength(200).IsRequired();
        b.Property(x => x.BatchNo).HasMaxLength(50);
        b.Property(x => x.Quantity).HasColumnType("decimal(18,2)");
        b.Property(x => x.Reason).HasMaxLength(300).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.ReferenceNo).HasMaxLength(50);
        b.HasQueryFilter(x => x.IsActive);
    }
}

// ─── Drug Interaction ──────────────────────────────────────────────
public class PharmacyDrugInteraction : Entity, IEntityTypeConfiguration<PharmacyDrugInteraction>
{
    public string? RxNo { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PrimaryDrug { get; set; } = string.Empty;
    public string InteractingDrug { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string State { get; set; } = "Detected";
    public string? RiskDescription { get; set; }
    public string? Recommendation { get; set; }
    public string Pharmacist { get; set; } = string.Empty;
    public DateTime CheckedAt { get; set; }
    public Guid? StoreDetailsId { get; set; }

    public void Configure(EntityTypeBuilder<PharmacyDrugInteraction> b)
    {
        b.ToTable("PharmacyDrugInteraction").HasKey(x => x.Id);
        b.Property(x => x.RxNo).HasMaxLength(50);
        b.Property(x => x.PatientName).HasMaxLength(200).IsRequired();
        b.Property(x => x.PrimaryDrug).HasMaxLength(200).IsRequired();
        b.Property(x => x.InteractingDrug).HasMaxLength(200).IsRequired();
        b.Property(x => x.Severity).HasMaxLength(20).IsRequired();
        b.Property(x => x.State).HasMaxLength(30).IsRequired();
        b.Property(x => x.RiskDescription).HasMaxLength(500);
        b.Property(x => x.Recommendation).HasMaxLength(500);
        b.Property(x => x.Pharmacist).HasMaxLength(120).IsRequired();
        b.HasQueryFilter(x => x.IsActive);
    }
}

// ─── Batch ─────────────────────────────────────────────────────────
public class PharmacyBatch : Entity, IEntityTypeConfiguration<PharmacyBatch>
{
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string BatchNo { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal MinQuantity { get; set; }
    public string? StoreName { get; set; }
    public string Status { get; set; } = "Active";
    public Guid? StoreDetailsId { get; set; }

    public void Configure(EntityTypeBuilder<PharmacyBatch> b)
    {
        b.ToTable("PharmacyBatch").HasKey(x => x.Id);
        b.Property(x => x.ItemCode).HasMaxLength(100).IsRequired();
        b.Property(x => x.ItemName).HasMaxLength(200).IsRequired();
        b.Property(x => x.BatchNo).HasMaxLength(50).IsRequired();
        b.Property(x => x.Quantity).HasColumnType("decimal(18,2)");
        b.Property(x => x.MinQuantity).HasColumnType("decimal(18,2)");
        b.Property(x => x.StoreName).HasMaxLength(100);
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.HasQueryFilter(x => x.IsActive);
    }
}

// ─── Restricted Drug Dispense ──────────────────────────────────────
/// <summary>
/// Records each dispense event for restricted/controlled medications.
/// Supports the full status lifecycle: pending → approved → dispensed / rejected / cancelled.
/// </summary>
public class RestrictedDrugDispense : Entity, IEntityTypeConfiguration<RestrictedDrugDispense>
{
    // Item identity
    public Guid? ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;

    // Batch / stock traceability
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }

    // Store
    public Guid? StoreDetailsId { get; set; }

    // Parties
    public string? PatientName { get; set; }
    public string? DoctorName { get; set; }
    public string? PharmacistName { get; set; }

    // Regulatory
    public string? AuthorizationNo { get; set; }
    public string? DispenseReason { get; set; }
    public string? RestrictionType { get; set; }

    // Workflow
    public string Status { get; set; } = "pending"; // pending | approved | rejected | dispensed | cancelled
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }

    // Audit
    public Guid? DispensedBy { get; set; }
    public DateTime? DispensedAt { get; set; }

    public void Configure(EntityTypeBuilder<RestrictedDrugDispense> b)
    {
        b.ToTable("RestrictedDrugDispense").HasKey(x => x.Id);
        b.Property(x => x.ItemCode).HasMaxLength(100).IsRequired();
        b.Property(x => x.ItemName).HasMaxLength(200).IsRequired();
        b.Property(x => x.BatchNo).HasMaxLength(50);
        b.Property(x => x.Quantity).HasColumnType("decimal(18,2)");
        b.Property(x => x.PatientName).HasMaxLength(200);
        b.Property(x => x.DoctorName).HasMaxLength(200);
        b.Property(x => x.PharmacistName).HasMaxLength(200);
        b.Property(x => x.AuthorizationNo).HasMaxLength(100);
        b.Property(x => x.DispenseReason).HasMaxLength(500);
        b.Property(x => x.RestrictionType).HasMaxLength(100);
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.RejectionReason).HasMaxLength(500);
        b.HasIndex(x => x.StoreDetailsId);
        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.ItemCode);
        b.HasQueryFilter(x => x.IsActive);
    }
}
