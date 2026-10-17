using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement
{
    public class BankAccount : Entity, IAggregateRoot, IEntityTypeConfiguration<BankAccount>
    {
        public sealed override Guid Id { get; set; }

        public string? BankName { get; set; }

        public string? BankAccountNumber { get; set; }

        public string? AccountName { get; set; }

        public string? BankBranchName { get; set; }

        public string? SwiftCode { get; set; }
        public string? IBAN { get; set; }

        public bool IsPrimary { get; set; }

        public Guid CompanyId { get; set; }

        public CompanyDetails Company {  get; set; }

        public void Configure(EntityTypeBuilder<BankAccount> builder)
        {
            builder.ToTable(nameof(BankAccount));

            builder.HasKey(b => b.Id);

            builder.Property(b => b.BankName).HasMaxLength(200);
            builder.Property(b => b.BankAccountNumber).HasMaxLength(200);
            builder.Property(b => b.IBAN).HasMaxLength(200);

            builder.HasOne(b=>b.Company)
                .WithMany(c=>c.BankAccounts)
                .HasForeignKey(b=>b.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
