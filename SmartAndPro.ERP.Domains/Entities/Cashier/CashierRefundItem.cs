using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Cashier
{
    public class CashierRefundItem : Entity, IEntityTypeConfiguration<CashierRefundItem>
    {
        public Guid CashierRefundId { get; set; }
        public int SaleItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string? ItemBarcode { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal ReturnQty { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public string? UnitName { get; set; }

        public virtual CashierRefund CashierRefund { get; set; } = null!;

        public void Configure(EntityTypeBuilder<CashierRefundItem> builder)
        {
            builder.ToTable("CashierRefundItem");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.CashierRefundId).IsRequired();
            builder.Property(e => e.SaleItemId).IsRequired();
            builder.Property(e => e.ItemCode).HasMaxLength(100);
            builder.Property(e => e.ItemName).HasMaxLength(200);
            builder.Property(e => e.ItemBarcode).HasMaxLength(100);
            builder.Property(e => e.UnitName).HasMaxLength(50);

            builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
            builder.Property(e => e.ReturnQty).HasPrecision(18, 2);
            builder.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            builder.Property(e => e.TaxPercentage).HasPrecision(18, 2);
            builder.Property(e => e.TaxAmount).HasPrecision(18, 2);
            builder.Property(e => e.RefundAmount).HasPrecision(18, 2);

            builder.HasIndex(e => e.CashierRefundId);

            builder.HasQueryFilter(x => x.IsActive == true);
        }
    }
}
