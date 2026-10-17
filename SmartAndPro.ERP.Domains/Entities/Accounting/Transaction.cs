using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Assets;
using SmartAndPro.ERP.Domains.Entities.HotelAllotmentInvoices;
using SmartAndPro.ERP.Domains.Entities.Inventory.PermitManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.Stocktaking;
using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Domains.Entities.Notes;
using SmartAndPro.ERP.Domains.Entities.Notes.SalesCreditNote;
using SmartAndPro.ERP.Domains.Entities.Receipts;
using SmartAndPro.ERP.Domains.Entities.Return.Purchase;
using SmartAndPro.ERP.Domains.Entities.Return.Sale;
using SmartAndPro.ERP.SharedKernel.Entities.Attributes;


namespace SmartAndPro.ERP.Domains.Entities.Accounting;

public class Transaction : Entity , IEntityTypeConfiguration<Transaction>
{
    [Searchable]
    public string ReferenceNumber { get; set; }
    
    public Guid TransactionTypeId { get; set; }
    public TransactionType TransactionType { get; set; }
    [Searchable]
	public string? Description { get; set; }
    
    public DateOnly TransactionDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    
    public List<TransactionEntry> TransactionEntries { get; set; }
    
    public Guid? ClientProfileId { get; set; }
    public ClientProfile ClientProfile { get; set; }
    
    public int? SaleInvoiceId { get; set; }
    public int? SalesReturnId { get; set; }

    public int? PurchaseInvoiceId { get; set; }
    public int? PurchaseReturnId { get; set; }
    public int? HotelAllotmentInvoiceId { get; set; }

    
    public Guid? PaycheckReceiptId { get; set; }
    public Guid? PaymentReceiptId { get; set; }
    
    public Guid? DebitNoteId { get; set; }
    public Guid? SalesCreditNoteId { get; set; }
    public Guid? InventoryPermitId { get; set; }
    
    public Guid? InventoryStocktakingId { get; set; }
    
    public Guid? ReservationId { get; set; }
    
    public Guid? MealReservationId { get; set; }

    
    public bool IsOpeningTransaction { get; set; } = false;
    
    public List<Attachment> Attachments { get; set; }
    
    public bool IsTransactionPost1 { get; set; } = false;
    
    public bool IsFrontal { get; set; } = false;
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder
            .ToTable("Transaction")
            .HasKey(x => x.Id);
        
        
        builder.Property(x => x.IsTransactionPost1)
            .IsRequired()
            .HasDefaultValue(false);
        
        
        builder.Property(x => x.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(x => x.TransactionDate)
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
        
        builder.Property(x => x.IsOpeningTransaction)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.HasOne(x => x.TransactionType)
            .WithMany()
            .HasForeignKey(x => x.TransactionTypeId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasMany(x => x.TransactionEntries)
            .WithOne(te => te.Transaction)
            .HasForeignKey(te => te.TransactionId)
            ;
        
        builder.HasOne(x => x.ClientProfile)
            .WithMany()
            .HasForeignKey(x => x.ClientProfileId)
            ;
        
        builder.HasOne<SaleInvoice>()
            .WithOne()
            .HasForeignKey<Transaction>(i => i.SaleInvoiceId)
            ;

        builder.HasOne<SalesReturn>()
            .WithOne()
            .HasForeignKey<Transaction>(i => i.SalesReturnId)
            ;

        
        builder.HasOne<PurchaseInvoice>()
            .WithOne()
            .HasForeignKey<Transaction>(i => i.PurchaseInvoiceId)
            ;

        builder.HasOne<HotelAllotmentInvoice>()
         .WithOne()
         .HasForeignKey<Transaction>(i => i.HotelAllotmentInvoiceId)
         ;

        builder.HasOne<PurchaseReturn>()
            .WithOne()
            .HasForeignKey<Transaction>(i => i.PurchaseReturnId)
            ;

        builder.HasOne<PaycheckReceipt>()
            .WithOne()
            .HasForeignKey<Transaction>(i => i.PaycheckReceiptId)
            ;

        builder.HasOne<PaymentReceipt>()
            .WithOne()
            .HasForeignKey<Transaction>(i => i.PaymentReceiptId)
            ;
        
        builder.HasOne<DebitNote>()
            .WithOne()
            .HasForeignKey<Transaction>(i => i.DebitNoteId)
            ;
        
        builder.HasOne<SalesCreditNote>()
            .WithOne()
            .HasForeignKey<Transaction>(i => i.SalesCreditNoteId)
            ;


        builder.HasOne<InventoryPermit>()
            .WithOne()
            .HasForeignKey<Transaction>(i => i.InventoryPermitId)
            ;

        
        builder.HasOne<InventoryStocktaking>()
            .WithOne()
            .HasForeignKey<Transaction>(i => i.InventoryStocktakingId)
            .OnDelete(DeleteBehavior.SetNull);

        
        builder.HasOne<Reservation.Reservation>()
            .WithOne()
            .HasForeignKey<Transaction>(i => i.ReservationId)
            ;

        
        builder.HasOne<MealReservation.MealReservation>()
            .WithOne()
            .HasForeignKey<Transaction>(i => i.MealReservationId)
            ;
        
        builder.HasMany(x => x.Attachments)
            .WithOne()
            .HasForeignKey(x => x.TransactionId)
            .OnDelete(DeleteBehavior.NoAction);

        
        builder.Navigation(x => x.ClientProfile)
            .AutoInclude();
        
        builder.Navigation(x => x.Attachments)
            .AutoInclude();
        
        
        builder.Navigation(x => x.TransactionEntries)
            .AutoInclude();
        
        builder.Navigation(x => x.TransactionType)
            .AutoInclude();
        
        builder.HasQueryFilter(p => p.IsActive == true);

    }
}