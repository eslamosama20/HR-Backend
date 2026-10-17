using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Salla;

/// <summary>
/// Persists the Salla OAuth access/refresh token so it survives server restarts.
/// Only one active record is expected at a time (IsSingleton pattern).
/// </summary>
public class SallaAuthToken : Entity, IEntityTypeConfiguration<SallaAuthToken>
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }

    public void Configure(EntityTypeBuilder<SallaAuthToken> builder)
    {
        builder.ToTable("SallaAuthTokens");
        builder.HasKey(x => x.Id);
    }
}
