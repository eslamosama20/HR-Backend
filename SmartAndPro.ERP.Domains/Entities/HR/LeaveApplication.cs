using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartAndPro.ERP.Domains.Entities.HR;

public class LeaveApplication : Entity, IEntityTypeConfiguration<LeaveApplication>
{
    public string? ReferenceNumber { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; }
    public LeaveType LeaveType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int TotalDays { get; set; }
    public string Reason { get; set; }
    public string? Notes { get; set; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public Guid? ApproverId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public int PreReminderDays { get; set; } = 3;
    public bool IsCritical { get; set; } = false;
    public Guid? AttachmentId { get; set; }

    public void Configure(EntityTypeBuilder<LeaveApplication> builder)
    {
        builder.ToTable("LeaveApplications").HasKey(x => x.Id);

        builder.Property(x => x.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.ReferenceNumber).IsUnique();

        builder.Property(x => x.Reason)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.Property(x => x.RejectionReason)
            .HasMaxLength(1000);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.LeaveType);
        builder.HasIndex(x => new { x.EmployeeId, x.StartDate });
        builder.HasIndex(x => x.ExpirationDate);
        builder.HasQueryFilter(p => p.IsActive);
    }
}
