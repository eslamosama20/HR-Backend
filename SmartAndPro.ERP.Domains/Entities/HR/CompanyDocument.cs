using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartAndPro.ERP.Domains.Entities.HR;

public class CompanyDocument : Entity, IEntityTypeConfiguration<CompanyDocument>
{
    public string? ReferenceNumber { get; set; }
    public string DocumentName { get; set; }
    public string DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public string? IssuingAuthority { get; set; }
    public DateOnly? IssueDate { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public int PreReminderDays { get; set; } = 30;
    public bool IsCritical { get; set; } = true;
    public string? Description { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Approved;
    public Guid? AttachmentId { get; set; }

    public void Configure(EntityTypeBuilder<CompanyDocument> builder)
    {
        builder.ToTable("CompanyDocuments").HasKey(x => x.Id);

        builder.Property(x => x.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.ReferenceNumber).IsUnique();

        builder.Property(x => x.DocumentName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.DocumentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.DocumentNumber)
            .HasMaxLength(100);

        builder.Property(x => x.IssuingAuthority)
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.ExpirationDate);
        builder.HasIndex(x => x.IsCritical);
        builder.HasQueryFilter(p => p.IsActive);
    }
}
