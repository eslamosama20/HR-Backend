using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Domains.Entities;

public class SystemSetting : Entity , IEntityTypeConfiguration<SystemSetting>
{
    // timezone
    public string TimeZone { get; set; } = "Asia/Riyadh";
    public bool IsLinkedToInventory { get; set; } = false;
    public ReservationSerialNumberFormat? ReservationSerialNumberFormat { get; set; }
    public long? ReservationSerialStartNumber { get; set; } = 1;
    public string? ReservationSerialPrefix { get; set; }
    public bool ShowSignaturesInInvoicePrint { get; set; } = true;
    public bool ShowTermsInPrint2 { get; set; } = false;

    // Reservation print settings
    public bool ShowStampInReservationPrint { get; set; } = true;
    public bool ShowCompanySignatureInReservationPrint { get; set; } = true;
    public bool ShowCustomerSignatureInReservationPrint { get; set; } = true;
    public List<string> DefaultReservationUncheckedPolicyIds { get; set; } = new();
    public List<string> DefaultReservationUncheckedBankIds { get; set; } = new();
    public int SessionTimeoutMinutes { get; set; } = 20;

    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("SystemSetting")
            .HasKey(x => x.Id);
        
        
        builder.HasQueryFilter(p => p.IsActive == true);

        builder.Property(h => h.DefaultReservationUncheckedPolicyIds)
            .HasConversion(
                v => string.Join(";", v),
                v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()
            )
            .HasMaxLength(4000);

        builder.Property(h => h.DefaultReservationUncheckedBankIds)
            .HasConversion(
                v => string.Join(";", v),
                v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()
            )
            .HasMaxLength(4000);
    }
}
