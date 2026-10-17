using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.SharedKernel.Entities.Attributes;

namespace SmartAndPro.ERP.Domains.Entities.Reservation;

public class ReservationService : Entity, IEntityTypeConfiguration<ReservationService>
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
    public Guid? RevenueMainAccountId { get; set; }
    public MainAccount? RevenueMainAccount { get; set; }
    public Guid? ExpenseMainAccountId { get; set; }
    public MainAccount? ExpenseMainAccount { get; set; }
    public bool IsFrontal { get; set; } = false;

    // ربط تلقائي بصنف المبيعات عند إنشاء فاتورة من الحجز
    public Guid? SalesClassId { get; set; }

    public void Configure(EntityTypeBuilder<ReservationService> builder)
    {
        builder
            .ToTable("ReservationService")
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

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.SalesClassId)
            .IsRequired(false);

        builder.Property(x => x.IsFrontal)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(x => x.RevenueMainAccount)
            .WithMany()
            .HasForeignKey(x => x.RevenueMainAccountId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.ExpenseMainAccount)
            .WithMany()
            .HasForeignKey(x => x.ExpenseMainAccountId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Navigation(x => x.RevenueMainAccount).AutoInclude();
        builder.Navigation(x => x.ExpenseMainAccount).AutoInclude();

        builder.HasIndex(x => x.ArabicName).HasDatabaseName("IX_ReservationService_ArabicName").HasFilter("[IsActive] = 1");
        builder.HasIndex(x => x.EnglishName).HasDatabaseName("IX_ReservationService_EnglishName").HasFilter("[IsActive] = 1");
        builder.HasIndex(x => x.TaxNumber).HasDatabaseName("IX_ReservationService_TaxNumber").HasFilter("[IsActive] = 1");

        builder.HasQueryFilter(p => p.IsActive == true);
    }
}
