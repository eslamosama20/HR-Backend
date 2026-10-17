using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.SharedKernel.Entities.Attributes;

namespace SmartAndPro.ERP.Domains.Entities.Reservation;

public class ReservationCompany : Entity, IEntityTypeConfiguration<ReservationCompany>
{
    [Searchable]
    public string ArabicName { get; set; }
    [Searchable]
    public string EnglishName { get; set; }

    public Guid? MainAccountId { get; set; }
    public MainAccount MainAccount { get; set; }
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
    [Searchable]
    public string? TaxNumber { get; set; }
    public bool IsFrontal { get; set; } = false;

    // حقول العنوان - مطلوبة لزاتكا
    [Searchable]
    public string? Street { get; set; }
    [Searchable]
    public string? District { get; set; }
    [Searchable]
    public string? BuildingNumber { get; set; }
    [Searchable]
    public string? SubNumber { get; set; }
    [Searchable]
    public string? PostalCode { get; set; }
    [Searchable]
    public string? City { get; set; }
    [Searchable]
    public string? Country { get; set; }

    [Searchable]
    public string? ShortAddress { get; set; }
    [Searchable]
    public string? AdditionalContactNumber { get; set; }
    [Searchable]
    public string? Fax { get; set; }
    [Searchable]
    public string? Website { get; set; }
    [Searchable]
    public string? Instructions { get; set; }
    [Searchable]
    public string? GaztNumber { get; set; }
    [Searchable]
    public string? OtherId { get; set; }

    // ربط تلقائي بعميل المبيعات عند إنشاء فاتورة
    public Guid? CustomerProfileId { get; set; }
    public CustomerProfile CustomerProfile { get; set; }

    public void Configure(EntityTypeBuilder<ReservationCompany> builder)
    {
        builder
            .ToTable("ReservationCompany")
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
            .HasMaxLength(200);

        builder.Property(x => x.Street).HasMaxLength(200);
        builder.Property(x => x.District).HasMaxLength(200);
        builder.Property(x => x.BuildingNumber).HasMaxLength(50);
        builder.Property(x => x.SubNumber).HasMaxLength(50);
        builder.Property(x => x.PostalCode).HasMaxLength(20);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.Country).HasMaxLength(100);

        builder.Property(x => x.ShortAddress).HasMaxLength(200);
        builder.Property(x => x.AdditionalContactNumber).HasMaxLength(30);
        builder.Property(x => x.Fax).HasMaxLength(30);
        builder.Property(x => x.Website).HasMaxLength(200);
        builder.Property(x => x.Instructions).HasMaxLength(200);
        builder.Property(x => x.GaztNumber).HasMaxLength(200);
        builder.Property(x => x.OtherId).HasMaxLength(100);

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

        builder.HasOne(x => x.CustomerProfile)
            .WithMany()
            .HasForeignKey(x => x.CustomerProfileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.ArabicName).HasDatabaseName("IX_ReservationCompany_ArabicName").HasFilter("[IsActive] = 1");
        builder.HasIndex(x => x.EnglishName).HasDatabaseName("IX_ReservationCompany_EnglishName").HasFilter("[IsActive] = 1");
        builder.HasIndex(x => x.TaxNumber).HasDatabaseName("IX_ReservationCompany_TaxNumber").HasFilter("[IsActive] = 1");
        builder.HasIndex(x => x.ContactNumber).HasDatabaseName("IX_ReservationCompany_ContactNumber").HasFilter("[IsActive] = 1");
        builder.HasIndex(x => x.Email).HasDatabaseName("IX_ReservationCompany_Email").HasFilter("[IsActive] = 1");

        builder.HasQueryFilter(p => p.IsActive == true);
    }
}