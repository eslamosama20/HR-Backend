using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.SharedKernel.Entities.Attributes;

namespace SmartAndPro.ERP.Domains.Entities.Reservation;

public class Hotel : Entity, IEntityTypeConfiguration<Hotel>
{
    [Searchable]
    public string ArabicName { get; set; } = string.Empty;
    [Searchable]
    public string? EnglishName { get; set; }
    [NotSearchable]
    public byte[]? Logo { get; set; }
    [NotSearchable]
    public string? LogoUrl { get; set; }
    [NotSearchable]
    public string? LogoPublicId { get; set; }
    [Searchable]
    public string? ContactNumber { get; set; }

    [Searchable]
    public string? Email { get; set; }
    [Searchable]
    public string? BankName { get; set; }
    [Searchable]
    public string? BankAccountNumber { get; set; }
    [Searchable]
    public string? IBAN { get; set; }

    //public string TaxNumber { get; set; } = string.Empty;
    [Searchable]
    public string? TaxNumber { get; set; }

    public Guid? MainAccountId { get; set; }
    public MainAccount? MainAccount { get; set; }

    public bool IsFrontal { get; set; } = false;

    [Searchable]
    public string? Street { get; set; }
    [Searchable]
    public string? District { get; set; }
    [Searchable]
    public string? City { get; set; }
    [Searchable]
    public string? Country { get; set; }
    [Searchable]
    public string? ShortAddress { get; set; }
    [Searchable]
    public string? BuildingNumber { get; set; }
    [Searchable]
    public string? PostalCode { get; set; }
    [Searchable]
    public string? Website { get; set; }
    [Searchable]
    public string? AdditionalContactNumber { get; set; }
    [Searchable]
    public string? Fax { get; set; }
    [Searchable]
    public string? GaztNumber { get; set; }
    [NotSearchable]
    public string? SubNumber { get; set; }
    [NotSearchable]
    public string? Instructions { get; set; }
    [NotSearchable]
    public string? OtherId { get; set; }

    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder
            .ToTable("Hotel")
            .HasKey(x => x.Id);
        builder.Property(x => x.ArabicName)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.EnglishName)
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

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ContactNumber)
            .HasMaxLength(30);

        builder.Property(x => x.Email)
            .HasMaxLength(200);

        builder.Property(x => x.BankName)
            .HasMaxLength(200);

        builder.Property(x => x.BankAccountNumber)
            .HasMaxLength(200);

        builder.Property(x => x.IBAN)
            .HasMaxLength(200);

        builder.Property(x => x.TaxNumber)
            //.IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Street).HasMaxLength(200);
        builder.Property(x => x.District).HasMaxLength(200);
        builder.Property(x => x.City).HasMaxLength(200);
        builder.Property(x => x.Country).HasMaxLength(200);
        builder.Property(x => x.ShortAddress).HasMaxLength(200);
        builder.Property(x => x.BuildingNumber).HasMaxLength(200);
        builder.Property(x => x.PostalCode).HasMaxLength(200);
        builder.Property(x => x.Website).HasMaxLength(200);
        builder.Property(x => x.AdditionalContactNumber).HasMaxLength(200);
        builder.Property(x => x.Fax).HasMaxLength(200);
        builder.Property(x => x.GaztNumber).HasMaxLength(200);
        builder.Property(x => x.SubNumber).HasMaxLength(50);
        builder.Property(x => x.Instructions).HasMaxLength(200);
        builder.Property(x => x.OtherId).HasMaxLength(100);

        builder.HasOne(x => x.MainAccount)
            .WithMany()
            .HasForeignKey(x => x.MainAccountId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Navigation(x => x.MainAccount).AutoInclude();

        builder.HasIndex(x => x.ArabicName).HasDatabaseName("IX_Hotel_ArabicName").HasFilter("[IsActive] = 1");
        builder.HasIndex(x => x.EnglishName).HasDatabaseName("IX_Hotel_EnglishName").HasFilter("[IsActive] = 1");
        builder.HasIndex(x => x.TaxNumber).HasDatabaseName("IX_Hotel_TaxNumber").HasFilter("[IsActive] = 1");
        builder.HasIndex(x => x.ContactNumber).HasDatabaseName("IX_Hotel_ContactNumber").HasFilter("[IsActive] = 1");
        builder.HasIndex(x => x.Email).HasDatabaseName("IX_Hotel_Email").HasFilter("[IsActive] = 1");

        builder.HasQueryFilter(p => p.IsActive == true);
    }
}
