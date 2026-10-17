using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Cashier
{
    public class CashierPayment : Entity, IEntityTypeConfiguration<CashierPayment>
    {
        public int InvoiceId { get; set; }
        public int SessionId { get; set; }
        public int PaymentMethod { get; set; } // 0=Cash, 1=Card, 2=Credit
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal ChangeAmount { get; set; }
        public decimal CashAmount { get; set; }
        public decimal CardAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public DateTime PaidAtUtc { get; set; }
        public Guid? StoreDetailsId { get; set; }

        /// <summary>Payment reconciliation status: 0=Pending, 1=Paid, 2=Failed, 3=Refunded</summary>
        public int PaymentStatus { get; set; } = 1; // Default: Paid (cash payments are immediately confirmed)

        /// <summary>External payment reference: card terminal transaction ID, gateway reference, etc.</summary>
        public string? PaymentReference { get; set; }

        /// <summary>Reason for the latest status change (for audit trail)</summary>
        public string? StatusReason { get; set; }

        /// <summary>When the payment status last changed (UTC)</summary>
        public DateTime? StatusChangedAtUtc { get; set; }
        
        public void Configure(EntityTypeBuilder<CashierPayment> builder)
        {
            builder.ToTable("CashierPayment");
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.InvoiceId).IsRequired();
            builder.Property(e => e.SessionId).IsRequired();
            builder.Property(e => e.PaymentMethod).IsRequired();
            
            builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
            builder.Property(e => e.PaidAmount).HasPrecision(18, 2);
            builder.Property(e => e.ChangeAmount).HasPrecision(18, 2);
            builder.Property(e => e.CashAmount).HasPrecision(18, 2);
            builder.Property(e => e.CardAmount).HasPrecision(18, 2);
            builder.Property(e => e.CreditAmount).HasPrecision(18, 2);
            
            builder.Property(e => e.PaidAtUtc).IsRequired();
            
            builder.Property(e => e.PaymentStatus).IsRequired();
            builder.Property(e => e.PaymentReference).HasMaxLength(200);
            builder.Property(e => e.StatusReason).HasMaxLength(500);
            builder.Property(e => e.StatusChangedAtUtc);
            
            // Index on InvoiceId for fast lookup
            builder.HasIndex(e => e.InvoiceId);
            builder.HasIndex(e => e.SessionId);
        }
    }
}
