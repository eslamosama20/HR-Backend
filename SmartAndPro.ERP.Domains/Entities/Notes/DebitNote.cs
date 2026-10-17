using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Assets;
using SmartAndPro.ERP.Domains.Entities.Inventory.SupplierManagement;
using SmartAndPro.ERP.Domains.Entities.Receipts;
using SmartAndPro.ERP.Domains.Entities.Treasuries;

namespace SmartAndPro.ERP.Domains.Entities.Notes;

public class DebitNote : Entity , IEntityTypeConfiguration<DebitNote>
{
    public string ReferenceNumber { get; set; }
	public string? Description { get; set; }
    
    public DateOnly ReceiptDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    
    public decimal Amount { get; set; }
    public decimal Tax { get; set; } = 0;
    
    public Guid SupplierDetailsId { get; set; }
    public SupplierDetails SupplierDetails { get; set; }
    
    public List<DebitNoteAccount>  DebitNoteAccounts{ get; set; }
    
    public Guid? TransactionId { get; set; }
    
    public List<Attachment> Attachments { get; set; } 

    public void Configure(EntityTypeBuilder<DebitNote> builder)
    {
        builder
            .ToTable("DebitNotes")
            .HasKey(x => x.Id);
        
        
        builder.Property(x => x.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(x => x.ReceiptDate)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(255);
        
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.HasIndex(x => x.ReferenceNumber)
            .IsUnique();
        
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.Amount)
            .IsRequired();
        
        builder.Property(x => x.Tax)
            .IsRequired();
        
        
        
        
        builder.HasMany(x => x.DebitNoteAccounts)
            .WithOne(te => te.DebitNote)
            .HasForeignKey(te => te.DebitNoteId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne<Transaction>()
            .WithOne()
            .HasForeignKey<DebitNote>(i => i.TransactionId)
            .OnDelete(DeleteBehavior.SetNull);


        builder.HasMany(x => x.Attachments)
            .WithOne()
            .HasForeignKey(x => x.DebitNoteId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.Navigation(x => x.Attachments)
            .AutoInclude();
        
        
        builder.Navigation(x => x.DebitNoteAccounts)
            .AutoInclude();
        builder.Navigation(x => x.SupplierDetails)
            .AutoInclude();
        
        builder.HasQueryFilter(p => p.IsActive == true);

    }
}