using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;

namespace SmartAndPro.ERP.Domains.Entities.Treasuries
{
    public class Treasury : Entity, IEntityTypeConfiguration<Treasury>
    {
        public string Name { get; set; }

        public Guid MainAccountId { get; set; }
        public MainAccount MainAccount { get; set; }

        public bool IsFrontal { get; set; } = false;
        public void Configure(EntityTypeBuilder<Treasury> builder)
        {
            builder
                .ToTable("Treasury")
                .HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.HasOne(x => x.MainAccount)
                .WithMany()
                .HasForeignKey(x => x.MainAccountId)
                .OnDelete(DeleteBehavior.NoAction);
            builder.Navigation(x => x.MainAccount).AutoInclude();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => x.Name)
                .IsUnique();

            builder.HasQueryFilter(p => p.IsActive == true);
        }
    }
}
