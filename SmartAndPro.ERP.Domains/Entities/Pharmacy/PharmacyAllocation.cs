using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;

namespace SmartAndPro.ERP.Domains.Entities.Pharmacy;

// ─── Pharmacy Allocation (PDF spec §12) ─────────────────────────────
public class PharmacyAllocation : Entity, IEntityTypeConfiguration<PharmacyAllocation>
{
    public Guid PrescriptionId { get; set; }
    public string AllocationReferenceId { get; set; } = string.Empty;
    public string CommitMode { get; set; } = "payload_logged"; // deduction | reservation | payload_logged
    public string Status { get; set; } = "committed";          // committed | reversed | failed
    public string? ConfirmedBy { get; set; }
    public string? Notes { get; set; }
    public DateTime? PreparedAt { get; set; }
    public DateTime? CommittedAt { get; set; }
    public string? PayloadJson { get; set; }
    public Guid? StoreDetailsId { get; set; }

    // Navigation
    public PharmacyPrescription? Prescription { get; set; }
    public ICollection<PharmacyAllocationLine> Lines { get; set; } = new List<PharmacyAllocationLine>();

    public void Configure(EntityTypeBuilder<PharmacyAllocation> b)
    {
        b.ToTable("PharmacyAllocation").HasKey(x => x.Id);
        b.Property(x => x.AllocationReferenceId).HasMaxLength(100).IsRequired();
        b.Property(x => x.CommitMode).HasMaxLength(30).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.ConfirmedBy).HasMaxLength(200);
        b.Property(x => x.Notes).HasMaxLength(500);

        b.HasOne(x => x.Prescription).WithMany().HasForeignKey(x => x.PrescriptionId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.PrescriptionId);
        b.HasIndex(x => x.AllocationReferenceId).IsUnique();
        b.HasQueryFilter(x => x.IsActive);
    }
}

// ─── Pharmacy Allocation Line (PDF spec §12.9) ─────────────────────
public class PharmacyAllocationLine : Entity, IEntityTypeConfiguration<PharmacyAllocationLine>
{
    public Guid AllocationId { get; set; }
    public Guid? PrescriptionItemId { get; set; }
    public Guid MainPurchaseClassId { get; set; }
    public Guid StoreId { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal RequestedQuantity { get; set; }
    public decimal CommittedQuantity { get; set; }
    public decimal AvailableQuantityAtCommit { get; set; }

    public PharmacyAllocation? Allocation { get; set; }
    public MainPurchaseClass? MainPurchaseClass { get; set; }

    public void Configure(EntityTypeBuilder<PharmacyAllocationLine> b)
    {
        b.ToTable("PharmacyAllocationLine").HasKey(x => x.Id);
        b.Property(x => x.BatchNo).HasMaxLength(100);
        b.Property(x => x.RequestedQuantity).HasColumnType("decimal(18,2)");
        b.Property(x => x.CommittedQuantity).HasColumnType("decimal(18,2)");
        b.Property(x => x.AvailableQuantityAtCommit).HasColumnType("decimal(18,2)");

        b.HasOne(x => x.Allocation).WithMany(a => a.Lines).HasForeignKey(x => x.AllocationId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.MainPurchaseClass).WithMany().HasForeignKey(x => x.MainPurchaseClassId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.AllocationId);
        b.HasIndex(x => x.MainPurchaseClassId);
        b.HasQueryFilter(x => x.IsActive);
    }
}

// ─── Pharmacy Pickup – Send-to-POS bridge (PDF spec §14/15) ────────
public class PharmacyPickup : Entity, IEntityTypeConfiguration<PharmacyPickup>
{
    public Guid PrescriptionId { get; set; }
    public Guid? AllocationId { get; set; }
    public string? AllocationReferenceId { get; set; }
    public string? AllocationCommitMode { get; set; }
    public string RxNo { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string PaymentType { get; set; } = "Cash";
    public string? InsuranceStatus { get; set; }
    public string PrescriptionStatus { get; set; } = "ready";
    public Guid StoreId { get; set; }
    public string? StoreName { get; set; }
    public string? PosTerminalId { get; set; }
    public string? CashierUserId { get; set; }
    public DateTime SentAt { get; set; }
    public string PickupStatus { get; set; } = "pending"; // pending | ready_for_pickup | picked_up | checked_out | cancelled

    // Financials
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal InsuranceCovered { get; set; }
    public decimal PatientPayable { get; set; }

    // Checkout link
    public int? PosInvoiceId { get; set; }
    public string? PosInvoiceNo { get; set; }

    public Guid? StoreDetailsId { get; set; }

    // Navigation
    public PharmacyPrescription? Prescription { get; set; }
    public PharmacyAllocation? Allocation { get; set; }

    public void Configure(EntityTypeBuilder<PharmacyPickup> b)
    {
        b.ToTable("PharmacyPickup").HasKey(x => x.Id);
        b.Property(x => x.RxNo).HasMaxLength(50).IsRequired();
        b.Property(x => x.PatientName).HasMaxLength(200).IsRequired();
        b.Property(x => x.PaymentType).HasMaxLength(20);
        b.Property(x => x.InsuranceStatus).HasMaxLength(30);
        b.Property(x => x.PrescriptionStatus).HasMaxLength(30);
        b.Property(x => x.StoreName).HasMaxLength(200);
        b.Property(x => x.PosTerminalId).HasMaxLength(50);
        b.Property(x => x.CashierUserId).HasMaxLength(100);
        b.Property(x => x.PickupStatus).HasMaxLength(30).IsRequired();
        b.Property(x => x.AllocationReferenceId).HasMaxLength(100);
        b.Property(x => x.AllocationCommitMode).HasMaxLength(30);
        b.Property(x => x.PosInvoiceNo).HasMaxLength(50);

        b.Property(x => x.Subtotal).HasColumnType("decimal(18,2)");
        b.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.TaxAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.InsuranceCovered).HasColumnType("decimal(18,2)");
        b.Property(x => x.PatientPayable).HasColumnType("decimal(18,2)");

        b.HasOne(x => x.Prescription).WithMany().HasForeignKey(x => x.PrescriptionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Allocation).WithMany().HasForeignKey(x => x.AllocationId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.PrescriptionId);
        b.HasIndex(x => x.AllocationId);
        b.HasIndex(x => x.PickupStatus);
        b.HasQueryFilter(x => x.IsActive);
    }
}
