using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Cashier;

public class HeldOrder : Entity, IEntityTypeConfiguration<HeldOrder>
{
    public Guid StoreDetailsId { get; set; }
    public string TerminalId { get; set; } = string.Empty;
    public int SessionId { get; set; }
    public Guid RequestedByUserGuid { get; set; }
    public DateTime HeldAtUtc { get; set; }
    public string? CustomerName { get; set; }
    public string? Notes { get; set; }
    public decimal TotalAmount { get; set; }

    public virtual ICollection<HeldOrderLine> Lines { get; set; } = new List<HeldOrderLine>();

    public void Configure(EntityTypeBuilder<HeldOrder> builder)
    {
        builder.ToTable("HeldOrders").HasKey(x => x.Id);

        builder.Property(x => x.StoreDetailsId).IsRequired();
        builder.Property(x => x.TerminalId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.SessionId).IsRequired();
        builder.Property(x => x.RequestedByUserGuid).IsRequired();
        builder.Property(x => x.HeldAtUtc).IsRequired();
        builder.Property(x => x.CustomerName).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)").IsRequired();

        builder.HasMany(x => x.Lines)
            .WithOne(l => l.HeldOrder)
            .HasForeignKey(l => l.HeldOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => x.IsActive == true);
    }
}
