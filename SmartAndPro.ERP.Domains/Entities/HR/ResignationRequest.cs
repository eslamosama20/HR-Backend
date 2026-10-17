using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartAndPro.ERP.Domains.Entities.HR;

public class ResignationRequest : Entity, IEntityTypeConfiguration<ResignationRequest>
{
    public string? ReferenceNumber { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; }
    public DateOnly RequestDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public DateOnly LastWorkingDay { get; set; }
    public string Reason { get; set; }
    public string? Description { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
    public Guid? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public int PreReminderDays { get; set; } = 7;
    public bool IsCritical { get; set; } = false;
    public Guid? AttachmentId { get; set; }

    public void Configure(EntityTypeBuilder<ResignationRequest> builder)
    {
        builder.ToTable("ResignationRequests").HasKey(x => x.Id);

        builder.Property(x => x.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.ReferenceNumber).IsUnique();

        builder.Property(x => x.Reason)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ExpirationDate);
        builder.HasQueryFilter(p => p.IsActive);
    }
}
