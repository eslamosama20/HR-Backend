using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartAndPro.ERP.Domains.Entities.HR;

public class Employee : Entity , IEntityTypeConfiguration<Employee>
{
    public sealed override Guid Id { get; set; }
    
    public string? ReferenceNumber { get; set; }
    public string Name { get; set; }
    public string? Phone { get; set; }
    
    public Guid EmploymentTypeId { get; set; }
    public EmploymentType EmploymentType { get; set; }
    
    public Guid ShiftId { get; set; }
    public Shift Shift { get; set; }
    
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    
    public List<Attendance> AttendanceRecords { get; set; }
    
    public string? DeviceId { get; set; }
    
    public List<MonthlyLoan> MonthlyLoans { get; set; }
    
    public decimal? MonthlyInsurance { get; set; }
    
    public int LeaveBalance { get; set; } = 0;
    
    public decimal MonthlyRate { get; set; }
    public decimal SupplementaryRate { get; set; }
    
    public decimal OvertimeRateMultiplier { get; set; } = 1.5m;
    public decimal AbsentRateMultiplier { get; set; } = 1.5m;
    public decimal LateRateMultiplier { get; set; } = 1.5m;
    
    public FactoryType? FactoryType { get; set; }
    
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder
            .ToTable("Employee")
            .HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.HasIndex(x => x.ReferenceNumber)
            .IsUnique();
        
        builder.HasIndex(x => x.Name)
            .IsUnique();
        
        builder.Property(x => x.FactoryType)
            .HasConversion(
                v => v.HasValue ? v.Value.ToString() : null,
                v => v != null ? Enum.Parse<FactoryType>(v) : (FactoryType?)null
            );
        
        
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);


        builder.Navigation(x => x.EmploymentType)
            .AutoInclude();
        builder.Navigation(x => x.Shift)
            .AutoInclude();
        
        
        builder.Navigation(x => x.MonthlyLoans)
            .AutoInclude();
        
        builder.HasQueryFilter(p => p.IsActive);
    }
}