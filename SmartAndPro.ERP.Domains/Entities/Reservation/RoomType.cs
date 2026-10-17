using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.SharedKernel.Entities.Attributes;

namespace SmartAndPro.ERP.Domains.Entities.Reservation;

public class RoomType : Entity, IEntityTypeConfiguration<RoomType>
{
    [Searchable]
    public string ArabicName { get; set; } = string.Empty;
    [Searchable]
    public string EnglishName { get; set; } = string.Empty;
    [NotSearchable]
    public byte[]? Logo { get; set; }
    [NotSearchable]
    public string? LogoUrl { get; set; }
    [NotSearchable]
    public string? LogoPublicId { get; set; }
    [NotSearchable]
    public string? Images { get; set; } // JSON array of base64 images
    [NotSearchable]
    public string? ImageUrls { get; set; } // JSON array of cloudinary image URLs

    public int PersonNo { get; set; } = 1;
    public bool IsFrontal { get; set; } = false;

    // ربط تلقائي بصنف المبيعات عند إنشاء فاتورة من الحجز
    public Guid? SalesClassId { get; set; }

    public void Configure(EntityTypeBuilder<RoomType> builder)
    {
        builder
            .ToTable("RoomType")
            .HasKey(x => x.Id);
        builder.Property(x => x.ArabicName)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.EnglishName)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.Logo)
            .HasColumnType("varbinary(max)")
            .IsRequired(false);

        builder.Property(x => x.LogoUrl)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(x => x.LogoPublicId)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.Images)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(x => x.ImageUrls)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(x => x.PersonNo)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsFrontal)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.SalesClassId)
            .IsRequired(false);

        builder.HasIndex(x => x.ArabicName).HasDatabaseName("IX_RoomType_ArabicName").HasFilter("[IsActive] = 1");
        builder.HasIndex(x => x.EnglishName).HasDatabaseName("IX_RoomType_EnglishName").HasFilter("[IsActive] = 1");

        builder.HasQueryFilter(p => p.IsActive == true);
    }
}
