using SmartAndPro.ERP.Domains.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartAndPro.ERP.Domains.Entities.HR;

public class Shift : Entity , IEntityTypeConfiguration<Shift>
{
    public sealed override Guid Id { get; set; }
    
    public string? ReferenceNumber { get; set; }
    public string Name { get; set; }
    
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    
    public int? GracePeriodInMinutes { get; set; } = 0;
    
    
    
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder
            .ToTable("Shift")
            .HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        
        builder.Property(x => x.GracePeriodInMinutes)
            .IsRequired()
            .HasDefaultValue(0)
            .HasMaxLength(300);
        
        
        builder.HasIndex(x => x.Name)
            .IsUnique();
        
        builder.Property(x => x.StartTime)
            .IsRequired();
        
        builder.Property(x => x.EndTime)
            .IsRequired();
        
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.HasQueryFilter(p => p.IsActive);
    }
}