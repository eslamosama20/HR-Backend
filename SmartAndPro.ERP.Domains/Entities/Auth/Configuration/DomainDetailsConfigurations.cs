using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Entities.Auth.Authorization;

namespace SmartAndPro.ERP.Domains.Entities.Auth.Configuration;

public class DomainDetailsConfigurations : IEntityTypeConfiguration<DomainDetails>
{
    
    public void Configure(EntityTypeBuilder<DomainDetails> builder)
    {
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.IsActive)
            .HasDefaultValue(true);
        builder.Property(p => p.Name).IsRequired(true);
        builder.HasIndex(p => p.Name)
            .IsUnique();
        
        builder.HasQueryFilter(r => r.IsActive);
        
    }
    
}