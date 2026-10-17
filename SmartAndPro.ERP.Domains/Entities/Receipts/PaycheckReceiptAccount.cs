using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;


namespace SmartAndPro.ERP.Domains.Entities.Receipts;

public class PaycheckReceiptAccount : Entity , IEntityTypeConfiguration<PaycheckReceiptAccount>
{
    public decimal Amount { get; set; }
    public decimal Tax { get; set; } = 0;
    public string? Description { get; set; }
    
    public Guid MainAccountId { get; set; }
    public MainAccount MainAccount { get; set; }
    public Guid PaycheckReceiptId { get; set; }
    public PaycheckReceipt PaycheckReceipt { get; set; }
    
    public void Configure(EntityTypeBuilder<PaycheckReceiptAccount> builder)
    {
        builder
            .ToTable("PaycheckReceiptAccounts")
            .HasKey(x => x.Id);
        
        builder.Property(x => x.Amount)
            .IsRequired()
            .HasColumnType("money");
        builder.Property(x => x.Tax)
            .IsRequired();
        
        builder.Property(x => x.Description)
            .HasMaxLength(255);
        
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        
        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.HasOne(x => x.MainAccount)
            .WithMany()
            .HasForeignKey(x => x.MainAccountId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne(x => x.PaycheckReceipt)
            .WithMany(t => t.PaycheckReceiptAccounts)
            .HasForeignKey(x => x.PaycheckReceiptId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Navigation(x => x.MainAccount)
            .AutoInclude();
        
        builder.HasQueryFilter(p => p.IsActive == true);
        
    }
}

