using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.SharedKernel.Entities.Attributes;

namespace SmartAndPro.ERP.Domains.Entities.Reservation;

public class ReservationSeason : Entity, IEntityTypeConfiguration<ReservationSeason>
{
    [Searchable]
    public string ArabicName { get; set; }
    [Searchable]
    public string EnglishName { get; set; }

    public Guid? RoomRevenueMainAccountId { get; set; }
    public MainAccount RoomRevenueMainAccount { get; set; }
    public Guid? RoomExpenseMainAccountId { get; set; }
    public MainAccount RoomExpenseMainAccount { get; set; }

    public Guid? MealRevenueMainAccountId { get; set; }
    public MainAccount MealRevenueMainAccount { get; set; }
    public Guid? MealExpenseMainAccountId { get; set; }
    public MainAccount MealExpenseMainAccount { get; set; }

    public Guid? OtherRevenueMainAccountId { get; set; }
    public MainAccount OtherRevenueMainAccount { get; set; }
    public Guid? OtherExpenseMainAccountId { get; set; }
    public MainAccount OtherExpenseMainAccount { get; set; }
    public bool IsFrontal { get; set; } = false;


    public void Configure(EntityTypeBuilder<ReservationSeason> builder)
    {
        builder
            .ToTable("ReservationSeason")
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

        builder.HasOne(x => x.RoomRevenueMainAccount)
            .WithMany()
            .HasForeignKey(x => x.RoomRevenueMainAccountId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.RoomExpenseMainAccount)
            .WithMany()
            .HasForeignKey(x => x.RoomExpenseMainAccountId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.MealRevenueMainAccount)
            .WithMany()
            .HasForeignKey(x => x.MealRevenueMainAccountId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.MealExpenseMainAccount)
            .WithMany()
            .HasForeignKey(x => x.MealExpenseMainAccountId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.OtherRevenueMainAccount)
            .WithMany()
            .HasForeignKey(x => x.OtherRevenueMainAccountId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.OtherExpenseMainAccount)
            .WithMany()
            .HasForeignKey(x => x.OtherExpenseMainAccountId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Navigation(x => x.RoomRevenueMainAccount).AutoInclude();
        builder.Navigation(x => x.RoomExpenseMainAccount).AutoInclude();
        builder.Navigation(x => x.MealRevenueMainAccount).AutoInclude();
        builder.Navigation(x => x.MealExpenseMainAccount).AutoInclude();
        builder.Navigation(x => x.OtherRevenueMainAccount).AutoInclude();
        builder.Navigation(x => x.OtherExpenseMainAccount).AutoInclude();

        builder.HasIndex(x => x.ArabicName).HasDatabaseName("IX_ReservationSeason_ArabicName").HasFilter("[IsActive] = 1");
        builder.HasIndex(x => x.EnglishName).HasDatabaseName("IX_ReservationSeason_EnglishName").HasFilter("[IsActive] = 1");

        builder.HasQueryFilter(p => p.IsActive == true);
    }
}