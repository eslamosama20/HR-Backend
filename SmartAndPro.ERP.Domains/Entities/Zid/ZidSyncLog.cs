using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Enums.Zid;

namespace SmartAndPro.ERP.Domains.Entities.Zid;

/// <summary>
/// Audit log for every Zid webhook or sync operation
/// </summary>
public class ZidSyncLog : Entity, IEntityTypeConfiguration<ZidSyncLog>
{
    public string EventType { get; set; } = string.Empty;  // e.g. "order.created"
    public ZidSyncDirection Direction { get; set; }
    public ZidSyncStatus Status { get; set; }
    public string? RequestPayload { get; set; }
    public string? ResponsePayload { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public long? ZidResourceId { get; set; }
    public int DurationMs { get; set; }

    public void Configure(EntityTypeBuilder<ZidSyncLog> builder)
    {
        builder.ToTable("ZidSyncLogs").HasKey(x => x.Id);

        builder.Property(x => x.EventType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);

        builder.HasIndex(x => x.EventType);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.ZidResourceId);
    }
}
