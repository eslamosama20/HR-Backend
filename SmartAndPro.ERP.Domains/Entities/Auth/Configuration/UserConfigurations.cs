using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;

namespace SmartAndPro.ERP.Domains.Entities.Auth.Configuration;

public class UserConfigurations : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(p => p.UserName).HasMaxLength(200).IsRequired(false);
        builder.Property(p => p.Email).HasMaxLength(200);
        builder.Property(p => p.FirstName).HasMaxLength(100);
        builder.Property(p => p.MiddleName).HasMaxLength(100);
        builder.Property(p => p.LastName).HasMaxLength(100);
        builder.Property(p => p.Password).HasMaxLength(200);
        builder.Property(p => p.PhoneNumber).HasMaxLength(200);
        builder.Property(u => u.WorkId)
            .HasMaxLength(100).IsRequired(false);
        builder.Property(u => u.TwoFactorEnabled)
            .HasDefaultValue(false);

        builder.Property(u => u.CreatedAt)
            .HasColumnType("datetime");
        
        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId);

        builder.HasIndex(u => u.Email);
        builder.HasIndex(u => u.UserName);
        
        builder.Navigation(u => u.UserRoles)
            .AutoInclude();
        builder.HasQueryFilter(u => u.IsActive);
        }
}