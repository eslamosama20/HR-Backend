using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Assets;
using SmartAndPro.ERP.Domains.Entities.Treasuries;

namespace SmartAndPro.ERP.Domains.Entities.Receipts;

public class PaymentReceipt : Entity , IEntityTypeConfiguration<PaymentReceipt>
{
    public string ReferenceNumber { get; set; }
	public string? Description { get; set; }
    
    public DateOnly ReceiptDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    
    public decimal Amount { get; set; }
    public decimal Tax { get; set; } = 0;
    
    public Guid TreasuryId { get; set; }
    public Treasury Treasury { get; set; }
    
    public List<PaymentReceiptAccount>  PaymentReceiptAccounts{ get; set; }
    
    
    public Guid? TransactionId { get; set; }
    
    public List<Attachment> Attachments { get; set; }
    public bool IsFrontal { get; set; } = false;
    public Enums.PaymentMethodType? PaymentMethodType { get; set; }

    public string? CheckNumber { get; set; }
    public string? BankName { get; set; }
    public string? BankReferenceNumber { get; set; }
    public string? VoucherNumber { get; set; }
    public string? TransactionNumber { get; set; }

    public List<ReceiptConnection> ReceiptConnections { get; set; } = new();

    public void Configure(EntityTypeBuilder<PaymentReceipt> builder)
    {
        builder
            .ToTable("PaymentReceipts")
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
            .IsUnique()
            .HasFilter("[IsActive] = 1");

        builder.HasIndex(x => x.ReceiptDate);
        
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.Amount)
            .IsRequired();
        
        builder.Property(x => x.Tax)
            .IsRequired();
        
        
        
        builder.HasMany(x => x.PaymentReceiptAccounts)
            .WithOne(te => te.PaymentReceipt)
            .HasForeignKey(te => te.PaymentReceiptId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne<Transaction>()
            .WithOne()
            .HasForeignKey<PaymentReceipt>(i => i.TransactionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Attachments)
            .WithOne()
            .HasForeignKey(x => x.PaymentReceiptId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Navigation(x => x.Attachments)
            .AutoInclude();


        builder.HasMany(x => x.ReceiptConnections)
            .WithOne(x=>x.PaymentReceipt)
            .HasForeignKey(x => x.PaymentReceiptId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Navigation(x => x.Treasury)
            .AutoInclude();
        
        builder.Navigation(x => x.PaymentReceiptAccounts)
            .AutoInclude();
        
        builder.Property(x => x.PaymentMethodType)
            .IsRequired(false);

        builder.HasQueryFilter(p => p.IsActive == true);

    }
}