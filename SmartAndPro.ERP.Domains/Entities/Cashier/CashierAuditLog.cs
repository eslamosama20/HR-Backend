using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Cashier;

public class CashierAuditLog : Entity, IEntityTypeConfiguration<CashierAuditLog>
{
    public string ActionType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityReference { get; set; }
    public int? SessionId { get; set; }
    public Guid? StoreDetailsId { get; set; }
    public Guid UserGuid { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? PayloadJson { get; set; }

    public void Configure(EntityTypeBuilder<CashierAuditLog> builder)
    {
        builder.ToTable("CashierAuditLog").HasKey(x => x.Id);

        builder.Property(x => x.ActionType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityReference).HasMaxLength(120);
        builder.Property(x => x.UserGuid).IsRequired();
        builder.Property(x => x.UserName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.PayloadJson).HasColumnType("nvarchar(max)");

        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.ActionType);
        builder.HasIndex(x => x.SessionId);
        builder.HasIndex(x => x.StoreDetailsId);

        builder.HasQueryFilter(x => x.IsActive == true);
    }
}
