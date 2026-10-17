using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.ValueObjects;

namespace SmartAndPro.ERP.Domains.Entities;

public class PdfTemplateConfiguration : Entity , IEntityTypeConfiguration<PdfTemplateConfiguration>
{
    public PdfHeaderFooterConfig Header { get; set; }
    public PdfHeaderFooterConfig Footer { get; set; }
    public void Configure(EntityTypeBuilder<PdfTemplateConfiguration> builder)
    {
        builder.ToTable("PdfTemplateConfiguration")
            .HasKey(x => x.Id);
        
        builder.OwnsOne(p => p.Header, header =>
        {
            ConfigureHeaderFooter(header,"Header");
        });

        builder.OwnsOne(p => p.Footer, footer =>
        {
            ConfigureHeaderFooter(footer,"Footer");
        });
        
        builder.HasQueryFilter(p => p.IsActive == true);
    }
    
    private void ConfigureHeaderFooter(
        OwnedNavigationBuilder<PdfTemplateConfiguration, PdfHeaderFooterConfig> builder,
        string prefix)
    {
        builder.OwnsMany(h => h.Columns, columns =>
        {
            columns.ToTable($"{prefix}_Columns");
            
            columns.OwnsMany(c => c.Rows, rows =>
            {
                rows.ToTable($"{prefix}_Rows");

                rows.OwnsMany(r => r.Items, items =>
                {
                    items.ToTable($"{prefix}_Items");

                    items.Property(i => i.Type).HasDefaultValue("field");
                    items.Property(i => i.FontSize).HasDefaultValue("inherit");
                    items.Property(i => i.Align).HasDefaultValue("left");
                    items.Property(i => i.Label).HasMaxLength(200);
                    items.Property(i => i.IsBold).HasDefaultValue(false);
                    items.Property(i => i.IsItalic).HasDefaultValue(false);
                    items.Property(i => i.IsUnderline).HasDefaultValue(false);
                    items.Property(i => i.IsStrikethrough).HasDefaultValue(false);
                });
            });
        });

        builder.OwnsMany(h => h.CustomTexts, custom =>
        {
            custom.ToTable($"{prefix}_CustomTexts");
            custom.WithOwner().HasForeignKey("PdfTemplateConfigurationId");
            custom.HasKey("Id");
        });

        builder.Property(h => h.AddressSubfields)
            .HasConversion(
                v => string.Join(";", v),
                v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()
            );
    }
}