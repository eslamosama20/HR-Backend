using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Reservation;

public class ReservationServiceEntry: Entity , IEntityTypeConfiguration<ReservationServiceEntry>
{
    public Guid ReservationServiceId { get; set; }
    public ReservationService ReservationService { get; set; }
    
    public string? Description { get; set; }
    
    //public DateOnly StartDate { get; set; }
    //public DateOnly EndDate { get; set; }
    
    public decimal Quantity { get; set; }
    
    public decimal SalesPrice { get; set; } = 0;
    public decimal? CostPrice { get; set; } = 0;

    public Guid? ReservationSupplierId { get; set; }
    public ReservationSupplier ReservationSupplier { get; set; }
    
    public Guid ReservationId { get; set; }
    public Reservation Reservation { get; set; }

    public void Configure(EntityTypeBuilder<ReservationServiceEntry> builder)
    {
        builder
            .ToTable("ReservationServiceEntry")
            .HasKey(x => x.Id);
        
        builder.Property(x => x.ReservationServiceId)
            .IsRequired();
        builder.Property(x => x.Description)
            .HasMaxLength(400);
        /*builder.Property(x => x.StartDate)
            .HasColumnType("date");
        builder.Property(x => x.EndDate)
            .HasColumnType("date");*/
        builder.Property(x => x.Quantity)
            .IsRequired();
        builder.Property(x => x.SalesPrice)
            .IsRequired();
        builder.Property(x => x.CostPrice)
            .IsRequired(false);
        builder.Property(x => x.ReservationSupplierId)
            .IsRequired(false);

        builder.HasOne(x => x.ReservationService)
            .WithMany()
            .HasForeignKey(x => x.ReservationServiceId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.ReservationSupplier)
            .WithMany()
            .HasForeignKey(x => x.ReservationSupplierId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.Navigation(x => x.ReservationService).AutoInclude();
        builder.Navigation(x => x.ReservationSupplier).AutoInclude();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.HasQueryFilter(p => p.IsActive == true);
    }
}