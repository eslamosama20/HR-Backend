using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;

namespace SmartAndPro.ERP.Domains.Entities.Auth.Configuration
{
    public class LoginActionLogsConfigurations : IEntityTypeConfiguration<LoginActionLogs>
    {
        public void Configure(EntityTypeBuilder<LoginActionLogs> builder)
        {
            builder.HasKey(p => new { p.UserId, p.LoginTime });
            builder.Property(u => u.LoginTime)
            .HasColumnType("datetime");
        }
    }
}
