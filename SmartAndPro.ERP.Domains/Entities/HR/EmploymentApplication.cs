using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartAndPro.ERP.Domains.Entities.HR;

public class EmploymentApplication : Entity, IEntityTypeConfiguration<EmploymentApplication>
{
    public string? ReferenceNumber { get; set; }
    public string ApplicantName { get; set; }
    public string? ApplicantPhone { get; set; }
    public string? ApplicantEmail { get; set; }
    public string? ApplicantIdNumber { get; set; }
    public string Position { get; set; }
    public string? Department { get; set; }
    public DateOnly ApplicationDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public string? Experience { get; set; }
    public string? Education { get; set; }
    public decimal? ExpectedSalary { get; set; }
    public string? Notes { get; set; }
    public EmploymentApplicationStatus Status { get; set; } = EmploymentApplicationStatus.Received;
    public DateOnly? ExpirationDate { get; set; }
    public int PreReminderDays { get; set; } = 14;
    public bool IsCritical { get; set; } = false;
    public Guid? AttachmentId { get; set; }

    public void Configure(EntityTypeBuilder<EmploymentApplication> builder)
    {
        builder.ToTable("EmploymentApplications").HasKey(x => x.Id);

        builder.Property(x => x.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.ReferenceNumber).IsUnique();

        builder.Property(x => x.ApplicantName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ApplicantPhone)
            .HasMaxLength(20);

        builder.Property(x => x.ApplicantEmail)
            .HasMaxLength(100);

        builder.Property(x => x.ApplicantIdNumber)
            .HasMaxLength(50);

        builder.Property(x => x.Position)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Department)
            .HasMaxLength(100);

        builder.Property(x => x.Experience)
            .HasMaxLength(2000);

        builder.Property(x => x.Education)
            .HasMaxLength(1000);

        builder.Property(x => x.ExpectedSalary)
            .HasPrecision(18, 2);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ExpirationDate);
        builder.HasQueryFilter(p => p.IsActive);
    }
}
