using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;

namespace SmartAndPro.ERP.Domains.Entities.Auth.Configuration
{
    internal class PasswordLogConfigurations : IEntityTypeConfiguration<PasswordsLog>
    {
        public void Configure(EntityTypeBuilder<PasswordsLog> builder)
        {
            builder.HasKey(p => new { p.UserId, p.CreatedAt });
            builder.Property(p => p.Password).HasMaxLength(200); 
            builder.Property(u => u.CreatedAt)
            .HasColumnType("datetime");
            builder.Property(u => u.ExpireDate)
            .HasColumnType("datetime");

        }
    }
}
