using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Entities.Auth.Authorization;

namespace SmartAndPro.ERP.Domains.Entities.Auth.Configuration;

public class RolePermissionConfigurations : IEntityTypeConfiguration<RolePermission>
{

    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.IsActive)
            .HasDefaultValue(true);
        builder.Property(p => p.RoleId).IsRequired(true);
        builder.Property(p => p.PermissionId).IsRequired(true);
        

        builder.Navigation(r => r.Role)
            .AutoInclude();
        builder.Navigation(r => r.Permission)
            .AutoInclude();
    }
}