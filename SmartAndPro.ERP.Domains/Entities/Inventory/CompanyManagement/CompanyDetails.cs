using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.ValueObjects;
using SmartAndPro.ERP.SharedKernel.Entities.Attributes;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;

public class CompanyDetails : Entity, IAggregateRoot, IEntityTypeConfiguration<CompanyDetails>
{
    public sealed override Guid Id { get; set; }
    [Searchable]
    public string ArabicName { get; set; }
    [Searchable]
    public string EnglishName { get; set; }
    [Searchable]
    public string? Email { get; set; }
    
    public Address? ArabicAddress { get; set; }
    
    public Address? Address { get; set; }

    [Searchable]
    public string? ShortAddress { get; set; }
    [Searchable]
    public string? BuildingNumber { get; set; }
    [Searchable]
    public string? SubNumber { get; set; }
    [Searchable]
    public string? District { get; set; }
    [Searchable]
    public string? EnglishDistrict { get; set; }

    public override bool IsActive { get; set; } = true;
    public override Guid CreatedBy { get; set; }


    public List<CompanyPhone> Phones { get; set; } = new ();

    public List<BankAccount> BankAccounts { get; set; } = new();
    [Searchable]
    public string TaxNumber { get; set; }
    [Searchable]
    public string? GaztNumber { get; set; }

    [Searchable]
    public string? BusinessCategory { get; set; }
    public CompanyStatus? CompanyStatus { get; set; }
    public Guid? CompanyStatusId { get; set; }

    public List<BranchDetails> BranchDetails { get; set; }

    [NotSearchable]
    public byte[]? Logo { get; set; }
    [NotSearchable]
    public byte[]? Stamp { get; set; }
    public string? LogoUrl { get; set; }
    public string? StampUrl { get; set; }


    public bool IsZatcaIntegrated { get; set; } = false;

    public bool IsOnBoarded { get; set; } = false;

    [NotSearchable]
    public string? Csr { get; set; }
    [NotSearchable]
    public string? PrivateKey { get; set; }


    public long? RequestId { get; set; }
    public string? DispositionMessage { get; set; }
    [NotSearchable]
    public string? BinarySecurityToken { get; set; }
    [NotSearchable]
    public string? Secret { get; set; }

    public long? ProdRequestId { get; set; }
    public string? ProdDispositionMessage { get; set; }
    [NotSearchable]
    public string? ProdBinarySecurityToken { get; set; }
    [NotSearchable]
    public string? ProdSecret { get; set; }

    [Searchable]
    public string? Website { get; set; }

    [Searchable]
    public string? UnifiedNumber { get; set; }
    [Searchable]
    public string? RegistrationNumber { get; set; }

    public List<CompanyPolicy> Policies { get; set; } = new();

    public void Configure(EntityTypeBuilder<CompanyDetails> builder)
    {
        builder.ToTable("CompanyDetails");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.ArabicName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.EnglishName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Email).HasMaxLength(200);

        builder.Property(b => b.IsZatcaIntegrated).IsRequired()
            .HasDefaultValue(false);


        builder.Property(b => b.Address).HasMaxLength(400).HasConversion(
            address => address.ToString(),
            addressString => (Address)addressString
        );
        builder.Property(b => b.ArabicAddress).HasMaxLength(400).HasConversion(
            address => address.ToString(),
            addressString => (Address)addressString
        );

        builder.Property(b => b.ShortAddress).HasMaxLength(200);
        builder.Property(b => b.BuildingNumber).HasMaxLength(200);
        builder.Property(b => b.SubNumber).HasMaxLength(200);
        builder.Property(b => b.District).HasMaxLength(200);
        builder.Property(b => b.EnglishDistrict).HasMaxLength(200);
        builder.Property(b => b.LogoUrl).HasMaxLength(1000);
        builder.Property(b => b.StampUrl).HasMaxLength(1000);
        

        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.CreatedBy).IsRequired();


        builder.HasMany(c => c.Phones)
            .WithOne(p => p.Company)
            .HasForeignKey(p => p.CompanyId)
             .OnDelete(DeleteBehavior.NoAction);


        builder.HasMany(c=>c.BankAccounts)
            .WithOne(b=>b.Company)
            .HasForeignKey(b=>b.CompanyId)
            .OnDelete(DeleteBehavior.NoAction);

        
        builder.Property(b => b.TaxNumber)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.GaztNumber)
            .HasMaxLength(200);

        builder.HasOne(p => p.CompanyStatus)
            .WithMany()
            .HasForeignKey(p => p.CompanyStatusId);

        builder.HasMany(c=>c.Policies)
            .WithOne(p=>p.Company)
            .HasForeignKey(p=>p.CompanyId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasQueryFilter(p => p.IsActive);

        builder.HasMany<BranchDetails>(b => b.BranchDetails)
            .WithOne()
            .HasForeignKey(x => x.CompanyDetailsId)
            .OnDelete(DeleteBehavior.NoAction);


        builder.Navigation(e => e.CompanyStatus).AutoInclude();
        builder.Navigation(e => e.BranchDetails).AutoInclude();
        builder.Navigation(e => e.Phones).AutoInclude();
        builder.Navigation(e=>e.Policies).AutoInclude();
        builder.Navigation(e => e.BankAccounts).AutoInclude();

        //builder.Navigation(e => e.Stores).AutoInclude();
    }
}
