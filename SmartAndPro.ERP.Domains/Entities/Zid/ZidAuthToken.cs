using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Zid;

/// <summary>
/// Persists the Zid OAuth access/refresh token so it survives server restarts.
/// </summary>
public class ZidAuthToken : Entity, IEntityTypeConfiguration<ZidAuthToken>
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }

    public void Configure(EntityTypeBuilder<ZidAuthToken> builder)
    {
        builder.ToTable("ZidAuthTokens");
        builder.HasKey(x => x.Id);
    }
}
