using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.ValueObjects;
using SmartAndPro.ERP.SharedKernel.Entities.Attributes;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.SupplierManagement;

public class SupplierDetails : Entity, IEntityTypeConfiguration<SupplierDetails>
{
    public sealed override Guid Id { get; set; }
    [Searchable]
    public string ArabicName { get; set; }
    [Searchable]
    public string? EnglishName { get; set; }
    public Address? Address { get; set; }
    [Searchable]
    public string? ContactNumber { get; set; }
    [Searchable]
    public string? Email { get; set; }
    
    [Searchable]
    public string? BankName { get; set; }
    
    [Searchable]
    public string? BankAccountNumber { get; set; }
    
    [Searchable]
    public string? IBAN { get; set; }
    
    [Searchable]
    public string? TaxNumber { get; set; }
    
    [Searchable]
    public string? AdditionalContactNumber { get; set; }
    [Searchable]
    public string? District { get; set; }
    [Searchable]
    public string? ShortAddress { get; set; }
    [Searchable]
    public string? BuildingNumber { get; set; }
    [Searchable]
    public string? PostalCode { get; set; }
    [Searchable]
    public string? Fax { get; set; }
    [Searchable]
    public string? Website { get; set; }
    [Searchable]
    public string? Instructions { get; set; }
    [Searchable]
    public string? GaztNumber { get; set; }
    public string? OtherId { get; set; }

    public SupplierStatus SupplierStatus { get; set; }
    public Guid? SupplierStatusId { get; set; }

    public override bool IsActive { get; set; } = true;
    public override Guid CreatedBy { get; set; }
    
    public Guid? MainAccountId { get; set; }
    public MainAccount MainAccount { get; set; }
    
    public bool IsFrontal { get; set; } = false;


    public void Configure(EntityTypeBuilder<SupplierDetails> builder)
    {
        builder.ToTable("SupplierDetails");
        builder.HasKey(b => b.Id);


        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.CreatedBy).IsRequired();
        builder.Property(b => b.ArabicName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Email).HasMaxLength(200);
        builder.Property(b => b.ContactNumber).HasMaxLength(50);
        //builder.Property(b => b.EnglishName).IsRequired().HasMaxLength(200);
        
        builder.Property(b => b.BankName).HasMaxLength(200);
        builder.Property(b => b.BankAccountNumber).HasMaxLength(200);
        builder.Property(b => b.IBAN).HasMaxLength(200);
        builder.Property(b => b.TaxNumber).HasMaxLength(200);
        builder.Property(b => b.AdditionalContactNumber).HasMaxLength(200);
        builder.Property(b => b.District).HasMaxLength(200);
        builder.Property(b => b.ShortAddress).HasMaxLength(200);
        builder.Property(b => b.BuildingNumber).HasMaxLength(200);
        builder.Property(b => b.PostalCode).HasMaxLength(200);
        builder.Property(b => b.Fax).HasMaxLength(200);
        builder.Property(b => b.Website).HasMaxLength(200);
        builder.Property(b => b.Instructions).HasMaxLength(200);
        builder.Property(b => b.GaztNumber).HasMaxLength(200);
        builder.Property(b => b.OtherId).HasMaxLength(100);
        
        builder.Property(b => b.Address).HasMaxLength(400).HasConversion(
            address => address != null ? address.ToString() : null,
            addressString => addressString != null ? (SmartAndPro.ERP.Domains.ValueObjects.Address)addressString : null,
            new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<Address?>(
                (c1, c2) => c1 == c2 || (c1 != null && c2 != null && c1.ToString() == c2.ToString()),
                c => c == null ? 0 : c.ToString().GetHashCode(),
                c => c == null ? null : (Address)c.ToString()
            )
        );

        builder.HasOne(s => s.SupplierStatus)
            .WithMany()
            .HasForeignKey(s => s.SupplierStatusId);


        builder.HasOne(x => x.MainAccount)
            .WithMany()
            .HasForeignKey(x => x.MainAccountId);
        builder.Navigation(x => x.MainAccount).AutoInclude();
        
        builder.HasIndex(b => b.ArabicName).HasDatabaseName("IX_SupplierDetails_ArabicName").HasFilter("[IsActive] = 1");
        builder.HasIndex(b => b.EnglishName).HasDatabaseName("IX_SupplierDetails_EnglishName").HasFilter("[IsActive] = 1");
        builder.HasIndex(b => b.TaxNumber).HasDatabaseName("IX_SupplierDetails_TaxNumber").HasFilter("[IsActive] = 1");
        builder.HasIndex(b => b.ContactNumber).HasDatabaseName("IX_SupplierDetails_ContactNumber").HasFilter("[IsActive] = 1");

        builder.HasQueryFilter(p => p.IsActive);
        
        builder.Navigation(e => e.SupplierStatus).AutoInclude();
    }
}
