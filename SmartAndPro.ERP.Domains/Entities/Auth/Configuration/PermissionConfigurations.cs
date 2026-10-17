using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Entities.Auth.Authorization;

namespace SmartAndPro.ERP.Domains.Entities.Auth.Configuration;

public class PermissionConfigurations : IEntityTypeConfiguration<Permission>
{
    
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.IsActive)
            .HasDefaultValue(true);
        builder.Property(p => p.Name).IsRequired(true);
        builder.HasIndex(p => p.Name)
            .IsUnique();
        
        builder.Property(p => p.Description).HasMaxLength(200);
        
        builder.HasOne(r => r.DomainDetails)
            .WithMany()
            .HasForeignKey(rd => rd.DomainDetailsId);
        
        builder.Navigation(r => r.DomainDetails)
            .AutoInclude();

        // Apply a global filter to exclude soft-deleted roles
        builder.HasQueryFilter(r => r.IsActive);
        
    }
    
}