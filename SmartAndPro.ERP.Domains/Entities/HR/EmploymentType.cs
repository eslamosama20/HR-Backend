using SmartAndPro.ERP.Domains.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartAndPro.ERP.Domains.Entities.HR;

public class EmploymentType : Entity , IEntityTypeConfiguration<EmploymentType>
{
    public sealed override Guid Id { get; set; }
    
    public string Code { get; set; }
    public string? Description { get; set; }
    
    public List<DayOfWeek> WeeklyDayOff { get; set; }
    
    
    public void Configure(EntityTypeBuilder<EmploymentType> builder)
    {
        builder
            .ToTable("EmploymentType")
            .HasKey(x => x.Id);
        
        
        builder.HasIndex(x => x.Code)
            .IsUnique();
        
        builder.Property(x => x.WeeklyDayOff)
            .HasConversion(
                v => v != null ? string.Join(",", v.Select(d => d.ToString())) : null,
                v => v != null
                    ? v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => Enum.Parse<DayOfWeek>(s))
                        .ToList()
                    : null
            );
        
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.HasQueryFilter(p => p.IsActive);
    }
}