using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;

namespace SmartAndPro.ERP.Domains.Entities.Auth.Configuration
{
    public class OTPStatusesConfigurations : IEntityTypeConfiguration<OTPStatuses>
    {
        public void Configure(EntityTypeBuilder<OTPStatuses> builder)
        {
            builder.HasKey(p => p.StatusID);
            builder.Property(p => p.ArabicDescription).HasMaxLength(50);
            builder.Property(p => p.ArabicDescription).IsRequired(false);
            builder.Property(p => p.EnglishDescription).HasMaxLength(50);
        }
    }
}
