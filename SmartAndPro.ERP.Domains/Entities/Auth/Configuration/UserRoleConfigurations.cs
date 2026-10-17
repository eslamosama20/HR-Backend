using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Entities.Auth.Authorization;

namespace SmartAndPro.ERP.Domains.Entities.Auth.Configuration;

public class UserRoleConfigurations : IEntityTypeConfiguration<UserRole>
{
    
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Navigation(r => r.Role)
            .AutoInclude();
        
        builder.HasQueryFilter(r => r.IsActive);
    }
    
}