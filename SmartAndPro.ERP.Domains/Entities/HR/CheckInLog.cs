using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartAndPro.ERP.Domains.Entities.HR;

public class CheckInLog : Entity , IEntityTypeConfiguration<CheckInLog>
{
    public sealed override Guid Id { get; set; }
    
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; }
    
    public DateTime Time { get; set; }
    public LogType LogType { get; set; } = LogType.IN;
    
    
    public void Configure(EntityTypeBuilder<CheckInLog> builder)
    {
        builder
            .ToTable("CheckInLog")
            .HasKey(x => x.Id);
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        // EmployeeId + Time + LogType
        builder.HasIndex(x => new { x.EmployeeId, x.Time })
            .IsUnique();
        
        builder.Property(x => x.LogType)
            .IsRequired()
            .HasConversion(
                logType => logType.ToString(),
                logTypeString => Enum.Parse<LogType>(logTypeString)
            );
        
        builder.HasQueryFilter(p => p.IsActive);
    }
}