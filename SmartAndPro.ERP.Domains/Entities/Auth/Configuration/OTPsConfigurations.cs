using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;

namespace SmartAndPro.ERP.Domains.Entities.Auth.Configuration
{
    internal class OTPsConfigurations : IEntityTypeConfiguration<OTPs>
    {
        public void Configure(EntityTypeBuilder<OTPs> builder)
        {
            builder.HasKey(p => p.OTPReference);
            builder.Property(e => e.NumberOfFailedTrails).HasDefaultValue(0);
            builder.Property(p => p.OTPReferenceKey).HasMaxLength(200);
            builder.Property(p => p.OTPPayload).HasMaxLength(200);
            builder.Property(u => u.ExpiryDate)
            .HasColumnType("datetime");

        }
    }
}
