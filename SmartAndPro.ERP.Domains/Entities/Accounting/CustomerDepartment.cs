using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using System;

namespace SmartAndPro.ERP.Domains.Entities.Accounting
{
    public class CustomerDepartment : Entity, IEntityTypeConfiguration<CustomerDepartment>
    {
        public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public Guid CustomerProfileId { get; set; }
        public virtual CustomerProfile CustomerProfile { get; set; }

        public void Configure(EntityTypeBuilder<CustomerDepartment> builder)
        {
            builder.ToTable("CustomerDepartments")
                .HasKey(x => x.Id);
                
            builder.Property(x => x.NameAr)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.NameEn)
                .HasMaxLength(200);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.HasOne(x => x.CustomerProfile)
                .WithMany(c => c.Departments)
                .HasForeignKey(x => x.CustomerProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(p => p.IsActive == true);
        }
    }
}
