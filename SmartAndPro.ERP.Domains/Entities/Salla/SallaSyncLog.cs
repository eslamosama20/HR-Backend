using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Enums.Salla;

namespace SmartAndPro.ERP.Domains.Entities.Salla;

/// <summary>
/// Audit log for every Salla webhook or sync operation
/// </summary>
public class SallaSyncLog : Entity, IEntityTypeConfiguration<SallaSyncLog>
{
    public string EventType { get; set; } = string.Empty;  // e.g. "order.created"
    public SallaSyncDirection Direction { get; set; }
    public SallaSyncStatus Status { get; set; }
    public string? RequestPayload { get; set; }
    public string? ResponsePayload { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public long? SallaResourceId { get; set; }
    public int DurationMs { get; set; }

    public void Configure(EntityTypeBuilder<SallaSyncLog> builder)
    {
        builder.ToTable("SallaSyncLogs").HasKey(x => x.Id);

        builder.Property(x => x.EventType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);

        builder.HasIndex(x => x.EventType);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.SallaResourceId);
    }
}
