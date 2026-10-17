using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Assets;

namespace SmartAndPro.ERP.Domains.Entities.MealReservation;

public class MealReservation: Entity , IEntityTypeConfiguration<MealReservation>
{
    public string ReferenceNumber { get; set; }
    
    public Guid CustomerProfileId { get; set; }
    public CustomerProfile CustomerProfile { get; set; }
    public string? Description { get; set; }
    public string? CustomerReservationNumber { get; set; }
    public string? Notes { get; set; }
    public List<MealReservationEntry> MealReservationEntries { get; set; }

    
    public Guid? TransactionId { get; set; }
    
    
    public List<Attachment> Attachments { get; set; }

    public void Configure(EntityTypeBuilder<MealReservation> builder)
    {
        builder
            .ToTable("MealReservation")
            .HasKey(x => x.Id);
        builder.Property(x => x.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasMany(x => x.MealReservationEntries)
            .WithOne(x => x.MealReservation)
            .HasForeignKey(x => x.MealReservationId)
            .OnDelete(DeleteBehavior.NoAction);
        
        
        builder.HasOne(x => x.CustomerProfile)
            .WithMany()
            .HasForeignKey(x => x.CustomerProfileId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne<Transaction>()
            .WithOne()
            .HasForeignKey<MealReservation>(i => i.TransactionId)
            .OnDelete(DeleteBehavior.SetNull);

        
        builder.HasMany(x => x.Attachments)
            .WithOne()
            .HasForeignKey(x => x.MealReservationId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Navigation(x => x.Attachments);
        
        builder.Navigation(x => x.MealReservationEntries)
            .AutoInclude();
        
        builder.Navigation(x => x.CustomerProfile).AutoInclude();
        
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.HasQueryFilter(p => p.IsActive == true);
    }
}