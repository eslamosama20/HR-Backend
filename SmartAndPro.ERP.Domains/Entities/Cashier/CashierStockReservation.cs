using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Cashier;

public enum CashierReservationStatus
{
    Reserved = 0,
    Confirmed = 1,
    Released = 2
}

public class CashierStockReservation : Entity, IEntityTypeConfiguration<CashierStockReservation>
{
    public int InvoiceId { get; set; }
    public int? SessionId { get; set; }
    public Guid StoreDetailsId { get; set; }
    public Guid MainSalesClassId { get; set; }
    public decimal Quantity { get; set; }
    public CashierReservationStatus Status { get; set; } = CashierReservationStatus.Reserved;
    public DateTime ReservedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAtUtc { get; set; }
    public DateTime? ReleasedAtUtc { get; set; }
    public string? ReleaseReason { get; set; }
    public string? Source { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public void Configure(EntityTypeBuilder<CashierStockReservation> builder)
    {
        builder.ToTable("CashierStockReservation").HasKey(x => x.Id);

        builder.Property(x => x.InvoiceId).IsRequired();
        builder.Property(x => x.StoreDetailsId).IsRequired();
        builder.Property(x => x.MainSalesClassId).IsRequired();
        builder.Property(x => x.Quantity).HasPrecision(18, 3).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.ReservedAtUtc).IsRequired();
        builder.Property(x => x.ReleaseReason).HasMaxLength(500);
        builder.Property(x => x.Source).HasMaxLength(64);
        builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();

        builder.HasIndex(x => new { x.InvoiceId, x.MainSalesClassId }).IsUnique();
        builder.HasIndex(x => new { x.StoreDetailsId, x.MainSalesClassId, x.Status });
        builder.HasIndex(x => x.SessionId);

        builder.HasQueryFilter(x => x.IsActive == true);
    }
}
