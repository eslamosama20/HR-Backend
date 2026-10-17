using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.ValueObjects;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;

public class BranchDetails : Entity, IAggregateRoot, IEntityTypeConfiguration<BranchDetails>
{
    public sealed override Guid Id { get; set; }
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public string? Email { get; set; }
    
    public string? Phone { get; set; }
    
    public string? BankName { get; set; }
    
    public string? BankAccountNumber { get; set; }
    
    public string? IBAN { get; set; }
    
    public string? TaxNumber { get; set; }
    public Address? Address { get; set; }
    public override bool IsActive { get; set; } = true;
    public override Guid CreatedBy { get; set; }


    public BranchStatus? BranchStatus { get; set; }
    public Guid? BranchStatusId { get; set; }

    public Guid CompanyDetailsId { get; set; }


    public void Configure(EntityTypeBuilder<BranchDetails> builder)
    {
        builder.ToTable("BranchDetails");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.ArabicName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.EnglishName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Email).HasMaxLength(200);
        
        builder.Property(b => b.Phone).HasMaxLength(200);
        builder.Property(b => b.BankName).HasMaxLength(200);
        builder.Property(b => b.BankAccountNumber).HasMaxLength(200);
        builder.Property(b => b.IBAN).HasMaxLength(200);
        builder.Property(b => b.TaxNumber).HasMaxLength(200);

        builder.Property(b => b.Address).HasMaxLength(400).HasConversion(
            address => address.ToString(),
            addressString => (Address)addressString
        );

        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.CreatedBy).IsRequired();

        builder.HasOne(p => p.BranchStatus)
            .WithMany()
            .HasForeignKey(p => p.BranchStatusId);

        builder.HasQueryFilter(p => p.IsActive);

        builder.Navigation(e => e.BranchStatus).AutoInclude();
        //builder.Navigation(e => e.Stores).AutoInclude();
    }
}