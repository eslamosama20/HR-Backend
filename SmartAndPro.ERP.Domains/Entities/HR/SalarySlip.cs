using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartAndPro.ERP.Domains.Entities.HR;

public class SalarySlip : Entity, IEntityTypeConfiguration<SalarySlip>
{
    public string? ReferenceNumber { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal? Allowances { get; set; }
    public decimal? Overtime { get; set; }
    public decimal? Bonus { get; set; }
    public decimal? Deductions { get; set; }
    public decimal? Insurance { get; set; }
    public decimal? Tax { get; set; }
    public decimal NetSalary { get; set; }
    public DateOnly? PaymentDate { get; set; }
    public bool IsPaid { get; set; } = false;
    public string? Notes { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public int PreReminderDays { get; set; } = 7;
    public bool IsCritical { get; set; } = false;
    public Guid? AttachmentId { get; set; }

    public void Configure(EntityTypeBuilder<SalarySlip> builder)
    {
        builder.ToTable("SalarySlips").HasKey(x => x.Id);

        builder.Property(x => x.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.ReferenceNumber).IsUnique();

        builder.Property(x => x.BasicSalary)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.Allowances)
            .HasPrecision(18, 2);

        builder.Property(x => x.Overtime)
            .HasPrecision(18, 2);

        builder.Property(x => x.Bonus)
            .HasPrecision(18, 2);

        builder.Property(x => x.Deductions)
            .HasPrecision(18, 2);

        builder.Property(x => x.Insurance)
            .HasPrecision(18, 2);

        builder.Property(x => x.Tax)
            .HasPrecision(18, 2);

        builder.Property(x => x.NetSalary)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.EmployeeId, x.Year, x.Month }).IsUnique();
        builder.HasIndex(x => x.ExpirationDate);
        builder.HasQueryFilter(p => p.IsActive);
    }
}
