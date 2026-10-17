using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.SharedKernel.Entities.Attributes;

namespace SmartAndPro.ERP.Domains.Entities.Reservation;

public class ReservationMeal : Entity, IEntityTypeConfiguration<ReservationMeal>
{
    [Searchable]
    public string ArabicName { get; set; }
    [Searchable]
    public string EnglishName { get; set; }
    public bool IsFrontal { get; set; } = false;
    public decimal Cost { get; set; }

    // ربط تلقائي بصنف المبيعات عند إنشاء فاتورة من الحجز
    public Guid? SalesClassId { get; set; }

    public void Configure(EntityTypeBuilder<ReservationMeal> builder)
    {
        builder
            .ToTable("ReservationMeal")
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

        builder.Property(x => x.SalesClassId)
            .IsRequired(false);

        builder.Property(x => x.IsFrontal)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(x => x.ArabicName).HasDatabaseName("IX_ReservationMeal_ArabicName").HasFilter("[IsActive] = 1");
        builder.HasIndex(x => x.EnglishName).HasDatabaseName("IX_ReservationMeal_EnglishName").HasFilter("[IsActive] = 1");

        builder.HasQueryFilter(p => p.IsActive == true);
    }
}