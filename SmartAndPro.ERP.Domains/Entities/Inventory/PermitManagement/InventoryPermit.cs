using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Assets;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;
using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.PermitManagement;

public class InventoryPermit : Entity , IEntityTypeConfiguration<InventoryPermit>
{
    public string ReferenceNumber { get; set; }
    public string? Description { get; set; }
    
    public DateOnly PermitDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public Guid StoreDetailsId { get; set; }
    public Guid MainAccountId { get; set; }
    
    public StoreDetails StoreDetails { get; set; }
    public MainAccount MainAccount { get; set; }

    public PermitType PermitType { get; set; } = PermitType.Entry;
    
    public decimal ShippingCost { get; set; } = 0;
    
    public List<PermitEntry> PermitEntries { get; set; }
    
    public Guid? TransactionId { get; set; }
    
    public List<Attachment> Attachments { get; set; }
    
    // حقول التذكير بتاريخ الصلاحية
    public bool ReminderEnabled { get; set; } = false;
    public ReminderType? ReminderFrequency { get; set; }
    public int? CustomReminderDays { get; set; }
    public DateTime? LastReminderDate { get; set; }
    public DateTime? NextReminderDate { get; set; }

    public void Configure(EntityTypeBuilder<InventoryPermit> builder)
    {
        builder
            .ToTable("InventoryPermit")
            .HasKey(x => x.Id);
        
        builder.Property(x => x.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(x => x.PermitDate)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(255);
        
        builder.Property(b => b.PermitType)
            .IsRequired()
            .HasDefaultValue(PermitType.Entry)
            .HasConversion(
                permitType => permitType.ToString(),
                permitTypeString => Enum.Parse<PermitType>(permitTypeString)
            );
        
        builder.HasOne(x=>x.MainAccount)
            .WithMany()
            .HasForeignKey(x => x.MainAccountId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne(x=>x.StoreDetails)
            .WithMany()
            .HasForeignKey(x => x.StoreDetailsId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasMany(x => x.PermitEntries)
            .WithOne()
            .HasForeignKey(te => te.InventoryPermitId)
            .OnDelete(DeleteBehavior.NoAction);
        
        
        builder.HasOne<Transaction>()
            .WithOne()
            .HasForeignKey<InventoryPermit>(i => i.TransactionId)
            .OnDelete(DeleteBehavior.SetNull);


        
        builder.HasMany(x => x.Attachments)
            .WithOne()
            .HasForeignKey(x => x.InventoryPermitId)
            .OnDelete(DeleteBehavior.NoAction);
        
        // إعدادات حقول التذكير
        builder.Property(x => x.ReminderEnabled)
            .HasDefaultValue(false);
        
        builder.Property(x => x.ReminderFrequency)
            .HasConversion(
                reminderType => reminderType.HasValue ? reminderType.ToString() : null,
                reminderTypeString => string.IsNullOrEmpty(reminderTypeString) ? null : Enum.Parse<ReminderType>(reminderTypeString)
            );
        
        builder.Property(x => x.CustomReminderDays);
        builder.Property(x => x.LastReminderDate);
        builder.Property(x => x.NextReminderDate);
        
        
        builder.Navigation(x => x.PermitEntries)
            .AutoInclude();
        
        builder.Navigation(x => x.StoreDetails)
            .AutoInclude();
        builder.Navigation(x => x.MainAccount)
            .AutoInclude();
        
        builder.HasQueryFilter(p => p.IsActive == true);
    }

}