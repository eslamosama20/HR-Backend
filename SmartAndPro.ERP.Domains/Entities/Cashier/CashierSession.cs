using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Cashier;

public class CashierSession : Entity, IEntityTypeConfiguration<CashierSession>
{
    public int SessionId { get; set; }
    public string SessionNumber { get; set; } = string.Empty;
    public Guid CashierUserGuid { get; set; }
    public string CashierUserName { get; set; } = string.Empty;
    public DateTime OpeningDateUtc { get; set; }
    public DateTime? ClosingDateUtc { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal? ActualBalance { get; set; }
    public int Status { get; set; } // 0=Open, 1=Closed
    public int? TreasuryId { get; set; }
    public Guid? StoreDetailsId { get; set; }
    public string? Notes { get; set; }
    public Guid? ClosedByUserGuid { get; set; }
    public string? ClosedByUserName { get; set; }

    // Terminal locking — one active session per POS terminal
    public string? TerminalId { get; set; }

    // Cash-action totals (for Z-Report reconciliation)
    public decimal TotalCashIn { get; set; }
    public decimal TotalCashOut { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalDiscounts { get; set; }
    
    // Session Statistics
    public decimal TotalSales { get; set; }
    public decimal TotalRefunds { get; set; }
    public decimal TotalCash { get; set; }
    public decimal TotalCard { get; set; }
    public decimal TotalCredit { get; set; }
    public int TransactionsCount { get; set; }

    public int? StockAlertLimit { get; set; }

    public void Configure(EntityTypeBuilder<CashierSession> builder)
    {
        builder.ToTable("CashierSessions").HasKey(x => x.Id);

        builder.Property(x => x.SessionId).IsRequired();
        builder.Property(x => x.SessionNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CashierUserGuid).IsRequired();
        builder.Property(x => x.CashierUserName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.OpeningDateUtc).IsRequired();
        builder.Property(x => x.OpeningBalance).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.ActualBalance).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.Property(x => x.ClosedByUserGuid);
        builder.Property(x => x.ClosedByUserName).HasMaxLength(120);
        builder.Property(x => x.TerminalId).HasMaxLength(50);
        builder.Property(x => x.StockAlertLimit);
        builder.Property(x => x.TotalCashIn).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.TotalCashOut).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.TotalTax).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.TotalDiscounts).HasColumnType("decimal(18,2)").IsRequired();
        
        builder.Property(x => x.TotalSales).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.TotalRefunds).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.TotalCash).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.TotalCard).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.TotalCredit).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.TransactionsCount).IsRequired();

        builder.HasIndex(x => x.SessionId).IsUnique();
        builder.HasIndex(x => x.CashierUserGuid);
        builder.HasIndex(x => x.OpeningDateUtc);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.StoreDetailsId);
        builder.HasIndex(x => x.TerminalId);

        builder.HasQueryFilter(x => x.IsActive == true);
    }
}
