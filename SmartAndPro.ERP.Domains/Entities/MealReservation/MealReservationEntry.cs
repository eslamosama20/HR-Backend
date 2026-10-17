using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Reservation;

namespace SmartAndPro.ERP.Domains.Entities.MealReservation;

public class MealReservationEntry: Entity , IEntityTypeConfiguration<MealReservationEntry>
{
    public DateOnly ArrivalDate { get; set; }
    public DateOnly DepartureDate { get; set; }
    public int NumberOfNights { get; set; }
    
    public Guid ReservationMealId { get; set; }
    public ReservationMeal ReservationMeal { get; set; }
    
    public int NumberOfPersons { get; set; } = 1;
    
    public int Frees { get; set; } = 0;
    
    public decimal MealRate { get; set; }
    public decimal MealCost { get; set; }
    
    public string GuestName { get; set; }
    public string Nationality { get; set; }
    
    public string? EntringMeal { get; set; }
    public string? ExitingMeal { get; set; }
    
    public Guid MealReservationId { get; set; }
    public MealReservation MealReservation { get; set; }

    public void Configure(EntityTypeBuilder<MealReservationEntry> builder)
    {
        builder
            .ToTable("MealReservationEntry")
            .HasKey(x => x.Id);
        
        builder.HasOne(x => x.ReservationMeal)
            .WithMany()
            .HasForeignKey(x => x.ReservationMealId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Navigation(x => x.ReservationMeal).AutoInclude();
        
        
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.HasQueryFilter(p => p.IsActive == true);
    }
}