using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Reservation;

public class ReservationRoomEntry: Entity , IEntityTypeConfiguration<ReservationRoomEntry>
{
    public Guid RoomTypeId { get; set; }
    public RoomType RoomType { get; set; }
    
    public int NumberOfRooms { get; set; } = 1;
    
    public decimal RoomRate { get; set; }
    public decimal? RoomCost { get; set; }
    
    public Guid? ReservationMealId { get; set; }
    public ReservationMeal ReservationMeal { get; set; }
    
    public decimal? MealRate { get; set; }
    public decimal? MealCost { get; set; }
    
    public Guid? ReservationViewId { get; set; }
    public ReservationView ReservationView { get; set; }
    
    public decimal? ViewRate { get; set; }
    public decimal? ViewCost { get; set; }
    
    public Guid ReservationId { get; set; }
    public Reservation Reservation { get; set; }

    public void Configure(EntityTypeBuilder<ReservationRoomEntry> builder)
    {
        builder
            .ToTable("ReservationRoomEntry")
            .HasKey(x => x.Id);
        builder.Property(x => x.RoomTypeId)
            .IsRequired();
        builder.Property(x => x.NumberOfRooms)
            .IsRequired();
        builder.Property(x => x.RoomRate)
            .IsRequired();
        builder.Property(x => x.RoomCost)
            .IsRequired(false);
        
        
        builder.HasOne(x => x.RoomType)
            .WithMany()
            .HasForeignKey(x => x.RoomTypeId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne(x => x.ReservationMeal)
            .WithMany()
            .HasForeignKey(x => x.ReservationMealId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne(x => x.ReservationView)
            .WithMany()
            .HasForeignKey(x => x.ReservationViewId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Navigation(x => x.RoomType).AutoInclude();
        builder.Navigation(x => x.ReservationMeal).AutoInclude();
        builder.Navigation(x => x.ReservationView).AutoInclude();
        
        
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.HasQueryFilter(p => p.IsActive == true);
    }
}