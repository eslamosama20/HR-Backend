using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartAndPro.ERP.Domains.Entities.HR;

public class InsuranceRecord : Entity, IEntityTypeConfiguration<InsuranceRecord>
{
    public string? ReferenceNumber { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; }
    public string InsuranceNumber { get; set; }
    public InsuranceType InsuranceType { get; set; }
    public string? InsuranceProvider { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public decimal? MonthlyPremium { get; set; }
    public decimal? CoverageAmount { get; set; }
    public int PreReminderDays { get; set; } = 30;
    public bool IsCritical { get; set; } = true;
    public string? Notes { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Approved;
    public Guid? AttachmentId { get; set; }

    public void Configure(EntityTypeBuilder<InsuranceRecord> builder)
    {
        builder.ToTable("InsuranceRecords").HasKey(x => x.Id);

        builder.Property(x => x.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.ReferenceNumber).IsUnique();

        builder.Property(x => x.InsuranceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.InsuranceProvider)
            .HasMaxLength(200);

        builder.Property(x => x.MonthlyPremium)
            .HasPrecision(18, 2);

        builder.Property(x => x.CoverageAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.InsuranceType);
        builder.HasIndex(x => x.ExpirationDate);
        builder.HasIndex(x => x.IsCritical);
        builder.HasQueryFilter(p => p.IsActive);
    }
}
