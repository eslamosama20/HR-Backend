using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.ChannelManager;

public class ChannelReservationPayment : Entity, IEntityTypeConfiguration<ChannelReservationPayment>
{
    public Guid ChannelReservationMappingId { get; set; }
    public string PaymentModel { get; set; } = string.Empty; // 'Prepaid', 'VCC', 'PayAtHotel'
    public string OriginalCurrency { get; set; } = string.Empty; // e.g. "USD"
    public decimal ExchangeRate { get; set; } = 1.000000m;
    public decimal OtaCommissionAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal ServiceCharge { get; set; }
    public decimal CityTax { get; set; }

    // Virtual Credit Card fields (Encrypted using AES-256 in production)
    public string? EncryptedVccCardNumber { get; set; }
    public string? EncryptedVccCvv { get; set; }
    public int? VccExpiryMonth { get; set; }
    public int? VccExpiryYear { get; set; }
    public DateOnly? VccActivationDate { get; set; }
    public decimal? VccBalanceAmount { get; set; }

    // Navigation
    public ChannelReservationMapping ChannelReservationMapping { get; set; } = null!;

    public void Configure(EntityTypeBuilder<ChannelReservationPayment> builder)
    {
        builder.ToTable("ChannelReservationPayments")
            .HasKey(x => x.Id);

        builder.Property(x => x.PaymentModel).HasMaxLength(50).IsRequired();
        builder.Property(x => x.OriginalCurrency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.ExchangeRate).HasColumnType("decimal(18,6)").HasDefaultValue(1.000000m);
        builder.Property(x => x.OtaCommissionAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0.00m);
        builder.Property(x => x.VatAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0.00m);
        builder.Property(x => x.ServiceCharge).HasColumnType("decimal(18,2)").HasDefaultValue(0.00m);
        builder.Property(x => x.CityTax).HasColumnType("decimal(18,2)").HasDefaultValue(0.00m);
        builder.Property(x => x.VccBalanceAmount).HasColumnType("decimal(18,2)");

        builder.Property(x => x.EncryptedVccCardNumber).HasColumnType("nvarchar(max)");
        builder.Property(x => x.EncryptedVccCvv).HasColumnType("nvarchar(max)");

        builder.HasOne(x => x.ChannelReservationMapping)
            .WithOne(m => m.PaymentDetails)
            .HasForeignKey<ChannelReservationPayment>(x => x.ChannelReservationMappingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => x.IsActive == true);
    }
}
