using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.SharedKernel.Entities.Attributes;

namespace SmartAndPro.ERP.Domains.Entities.Accounting;

public class CustomerProfile : Entity , IEntityTypeConfiguration<CustomerProfile>
{
    [Searchable]
    public string ArabicName { get; set; }
    [Searchable]
    public string? EnglishName { get; set; }
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
    public string? Street { get; set; }
    [Searchable]
    public string? EnglishStreet { get; set; }
    [Searchable]
    public string? District { get; set; }
    [Searchable]
    public string? EnglishDistrict { get; set; }
    [Searchable]
    public string? ShortAddress { get; set; }
    [Searchable]
    public string? BuildingNumber { get; set; }
    [Searchable]
    public string? SubNumber { get; set; }
    [Searchable]
    public string? PostalCode { get; set; }
    [Searchable]
    public string? City { get; set; }
    [Searchable]
    public string? EnglishCity { get; set; }
    [Searchable]
    public string? Country { get; set; }
    [Searchable]
    public string? EnglishCountry { get; set; }
    [Searchable]
    public string? AdditionalContactNumber { get; set; }
    [Searchable]
    public string? Fax { get; set; }
    [Searchable]
    public string? Website { get; set; }
    [Searchable]
    public string? Instructions { get; set; }
    [Searchable]
    public string? GaztNumber { get; set; }
    [Searchable]
    public string? OtherId { get; set; }
    public Guid? MainAccountId { get; set; }
    public MainAccount MainAccount { get; set; }
    
    public bool IsFrontal { get; set; } = false;
    public bool IsReservationCompany { get; set; } = false;
    public bool IsForProjects { get; set; } = false;
    public virtual ICollection<CustomerDepartment> Departments { get; set; } = new List<CustomerDepartment>();
    

    public decimal CreditBalance { get; set; } = 0;
    
    public void Configure(EntityTypeBuilder<CustomerProfile> builder)
    {
        builder
            .ToTable("CustomerProfile")
            .HasKey(x => x.Id);
        builder.Property(x => x.ArabicName)
            .IsRequired()
            .HasMaxLength(100);
        //builder.Property(x => x.EnglishName)
        //    .IsRequired()
        //    .HasMaxLength(100);
        builder.Property(x => x.ContactNumber)
            .HasMaxLength(30);
        
        builder.Property(x => x.Email)
            .HasMaxLength(200);
        builder.Property(x => x.BankName)
            .HasMaxLength(200);
        builder.Property(x => x.BankAccountNumber)
            .HasMaxLength(200);
        builder.Property(x => x.IBAN)
            .HasMaxLength(200);
        //builder.Property(x => x.TaxNumber)
        //    .IsRequired()
        //    .HasMaxLength(200);
        builder.Property(x => x.Street)
            .HasMaxLength(200);
        builder.Property(x => x.EnglishStreet)
            .HasMaxLength(200);
        builder.Property(x => x.District)
            .HasMaxLength(200);
        builder.Property(x => x.EnglishDistrict)
            .HasMaxLength(200);
        builder.Property(x => x.ShortAddress)
            .HasMaxLength(200);
        builder.Property(x => x.BuildingNumber)
            .HasMaxLength(200);
        builder.Property(x => x.SubNumber)
            .HasMaxLength(200);
        builder.Property(x => x.PostalCode)
            .HasMaxLength(200);
        builder.Property(x => x.City)
            .HasMaxLength(200);
        builder.Property(x => x.EnglishCity)
            .HasMaxLength(200);
        builder.Property(x => x.Country)
            .HasMaxLength(200);
        builder.Property(x => x.EnglishCountry)
            .HasMaxLength(200);
        builder.Property(x => x.AdditionalContactNumber)
            .HasMaxLength(200);
        builder.Property(x => x.Fax)
            .HasMaxLength(200);
        builder.Property(x => x.Website)
            .HasMaxLength(200);
        builder.Property(x => x.Instructions)
            .HasMaxLength(200);
        builder.Property(x => x.GaztNumber)
            .HasMaxLength(200);
            
        builder.Property(x => x.OtherId)
            .HasMaxLength(100);
        
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.IsForProjects)
            .IsRequired()
            .HasDefaultValue(false);
        builder.Property(x => x.CreditBalance)
            .HasPrecision(18, 2)
            .HasDefaultValue(0m);


        builder.HasOne(x => x.MainAccount)
            .WithMany()
            .HasForeignKey(x => x.MainAccountId);
        builder.Navigation(x => x.MainAccount).AutoInclude();
        
        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.HasIndex(x => new { x.ArabicName, x.IsReservationCompany })
            .IsUnique();
        
        //builder.HasIndex(x => x.EnglishName)
        //    .IsUnique();
        
        builder.HasIndex(x => x.EnglishName).HasDatabaseName("IX_CustomerProfile_EnglishName").HasFilter("[IsActive] = 1");
        builder.HasIndex(x => x.TaxNumber).HasDatabaseName("IX_CustomerProfile_TaxNumber").HasFilter("[IsActive] = 1");
        builder.HasIndex(x => x.ContactNumber).HasDatabaseName("IX_CustomerProfile_ContactNumber").HasFilter("[IsActive] = 1");

        builder.HasQueryFilter(p => p.IsActive == true);

    }
}
