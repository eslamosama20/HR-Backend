using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartAndPro.ERP.Domains.Entities.HR;

public class HRTrustReceipt : Entity, IEntityTypeConfiguration<HRTrustReceipt>
{
    public string? ReferenceNumber { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; }
    public string ItemDescription { get; set; }
    public string? ItemSerialNumber { get; set; }
    public decimal? ItemValue { get; set; }
    public DateOnly ReceiveDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public DateOnly? ReturnDate { get; set; }
    public bool IsReturned { get; set; } = false;
    public string? Notes { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public int PreReminderDays { get; set; } = 7;
    public bool IsCritical { get; set; } = false;
    public DocumentStatus Status { get; set; } = DocumentStatus.Approved;
    public Guid? AttachmentId { get; set; }

    public void Configure(EntityTypeBuilder<HRTrustReceipt> builder)
    {
        builder.ToTable("HRTrustReceipts").HasKey(x => x.Id);

        builder.Property(x => x.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.ReferenceNumber).IsUnique();

        builder.Property(x => x.ItemDescription)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ItemSerialNumber)
            .HasMaxLength(100);

        builder.Property(x => x.ItemValue)
            .HasPrecision(18, 2);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.IsReturned);
        builder.HasIndex(x => x.ExpirationDate);
        builder.HasQueryFilter(p => p.IsActive);
    }
}
