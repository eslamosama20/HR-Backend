using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.SharedKernel.Entities.Attributes;

namespace SmartAndPro.ERP.Domains.Entities.Reservation;

public class SalesRepresentative : Entity, IEntityTypeConfiguration<SalesRepresentative>
{
    [Searchable]
    public string ArabicName { get; set; }
    [Searchable]
    public string EnglishName { get; set; }

    public Guid? MainAccountId { get; set; }
    public MainAccount MainAccount { get; set; }
    public bool IsFrontal { get; set; } = false;

    public void Configure(EntityTypeBuilder<SalesRepresentative> builder)
    {
        builder
            .ToTable("SalesRepresentative")
            .HasKey(x => x.Id);
        builder.Property(x => x.ArabicName)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.EnglishName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsFrontal)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(x => x.MainAccount)
            .WithMany()
            .HasForeignKey(x => x.MainAccountId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Navigation(x => x.MainAccount).AutoInclude();

        builder.HasIndex(x => x.ArabicName).HasDatabaseName("IX_SalesRepresentative_ArabicName").HasFilter("[IsActive] = 1");
        builder.HasIndex(x => x.EnglishName).HasDatabaseName("IX_SalesRepresentative_EnglishName").HasFilter("[IsActive] = 1");

        builder.HasQueryFilter(p => p.IsActive == true);
    }
}