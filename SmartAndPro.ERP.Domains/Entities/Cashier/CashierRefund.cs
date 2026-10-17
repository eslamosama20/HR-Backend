using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Cashier
{
    public class CashierRefund : Entity, IEntityTypeConfiguration<CashierRefund>
    {
        public string RefundNo { get; set; } = string.Empty;
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public int SessionId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CashierName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;       // defective, wrong_item, etc.
        public string ReasonText { get; set; } = string.Empty;
        public string RefundMethod { get; set; } = string.Empty;  // cash, card, credit_note, original_method
        public string Status { get; set; } = "completed";         // pending, approved, completed, rejected
        public decimal ItemsCount { get; set; }
        public decimal RefundSubtotal { get; set; }
        public decimal RefundTaxAmount { get; set; }
        public decimal RefundTotalAmount { get; set; }
        public string? Notes { get; set; }
        public DateTime RefundDateUtc { get; set; }
        public Guid? StoreDetailsId { get; set; }

        public virtual ICollection<CashierRefundItem> Items { get; set; } = new List<CashierRefundItem>();

        public void Configure(EntityTypeBuilder<CashierRefund> builder)
        {
            builder.ToTable("CashierRefund");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.RefundNo).HasMaxLength(50).IsRequired();
            builder.Property(e => e.InvoiceNo).HasMaxLength(50).IsRequired();
            builder.Property(e => e.InvoiceId).IsRequired();
            builder.Property(e => e.SessionId).IsRequired();
            builder.Property(e => e.CustomerName).HasMaxLength(200);
            builder.Property(e => e.CashierName).HasMaxLength(120);
            builder.Property(e => e.Reason).HasMaxLength(50).IsRequired();
            builder.Property(e => e.ReasonText).HasMaxLength(200);
            builder.Property(e => e.RefundMethod).HasMaxLength(50).IsRequired();
            builder.Property(e => e.Status).HasMaxLength(20).IsRequired();
            builder.Property(e => e.Notes).HasMaxLength(500);

            builder.Property(e => e.ItemsCount).HasPrecision(18, 2);
            builder.Property(e => e.RefundSubtotal).HasPrecision(18, 2);
            builder.Property(e => e.RefundTaxAmount).HasPrecision(18, 2);
            builder.Property(e => e.RefundTotalAmount).HasPrecision(18, 2);

            builder.HasIndex(e => e.RefundNo).IsUnique();
            builder.HasIndex(e => e.InvoiceId);
            builder.HasIndex(e => e.SessionId);
            builder.HasIndex(e => e.RefundDateUtc);

            builder.HasMany(e => e.Items)
                .WithOne(i => i.CashierRefund)
                .HasForeignKey(i => i.CashierRefundId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(x => x.IsActive == true);
        }
    }
}
