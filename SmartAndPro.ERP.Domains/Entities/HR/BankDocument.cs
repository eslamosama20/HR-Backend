using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartAndPro.ERP.Domains.Entities.HR;

public class BankDocument : Entity, IEntityTypeConfiguration<BankDocument>
{
    public string? ReferenceNumber { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; }
    public string BankName { get; set; }
    public string DocumentType { get; set; }
    public string? AccountNumber { get; set; }
    public string? DocumentNumber { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public int PreReminderDays { get; set; } = 30;
    public bool IsCritical { get; set; } = false;
    public string? Notes { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Approved;
    public Guid? AttachmentId { get; set; }

    public void Configure(EntityTypeBuilder<BankDocument> builder)
    {
        builder.ToTable("BankDocuments").HasKey(x => x.Id);

        builder.Property(x => x.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.ReferenceNumber).IsUnique();

        builder.Property(x => x.BankName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.DocumentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.AccountNumber)
            .HasMaxLength(50);

        builder.Property(x => x.DocumentNumber)
            .HasMaxLength(100);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.ExpirationDate);
        builder.HasQueryFilter(p => p.IsActive);
    }
}
