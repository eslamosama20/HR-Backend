using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Accounting;

public class TransactionEntry : Entity , IEntityTypeConfiguration<TransactionEntry>
{
    public decimal CreditAmount { get; set; }
    public decimal DebitAmount { get; set; }
    
    public string? Description { get; set; }
    
    public Guid MainAccountId { get; set; }
    public MainAccount MainAccount { get; set; }
    public Guid TransactionId { get; set; }
    public Transaction Transaction { get; set; }
    
    public void Configure(EntityTypeBuilder<TransactionEntry> builder)
    {
        builder
            .ToTable("TransactionEntry")
            .HasKey(x => x.Id);
        builder.Property(x => x.CreditAmount)
            .IsRequired()
            .HasColumnType("money");
        builder.Property(x => x.DebitAmount)
            .IsRequired()
            .HasColumnType("money");
        
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
        
        builder.HasOne(x => x.Transaction)
            .WithMany(t => t.TransactionEntries)
            .HasForeignKey(x => x.TransactionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Navigation(x => x.MainAccount)
            .AutoInclude();
        builder.Navigation(x => x.Transaction)
            .AutoInclude();
        
        builder.HasQueryFilter(p => p.IsActive == true);
        
    }
}

