using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartAndPro.ERP.Domains.Entities.HR;

public class Attendance : Entity , IEntityTypeConfiguration<Attendance>
{
    public sealed override Guid Id { get; set; }
    
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; }
    
    public DateOnly AttendanceDate { get; set; }
    public AttendanceFlag AttendanceFlag { get; set; } = AttendanceFlag.Present;
    public string? Reason { get; set; }
    
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder
            .ToTable("Attendance")
            .HasKey(x => x.Id);
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.HasIndex(x => new { x.EmployeeId, x.AttendanceDate })
            .IsUnique();
        
        builder.Property(x => x.AttendanceFlag)
            .IsRequired()
            .HasConversion(
                attendanceFlag => attendanceFlag.ToString(),
                attendanceFlagString => Enum.Parse<AttendanceFlag>(attendanceFlagString)
            );
        
        builder.HasQueryFilter(p => p.IsActive);
    }
}