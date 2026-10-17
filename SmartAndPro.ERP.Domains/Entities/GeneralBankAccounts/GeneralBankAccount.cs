using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;


namespace SmartAndPro.ERP.Domains.Entities.GeneralBankAccounts
{
    public class GeneralBankAccount : Entity , IAggregateRoot , IEntityTypeConfiguration<GeneralBankAccount>
    {
        public string BankName { get; set; }

        public string BankAccountNumber { get; set; }

        public string AccountName { get; set; }

        public string? BankBranchName { get; set; }

        public string? SwiftCode { get; set; }
        public string? IBAN { get; set; }

        public Guid MainAccountId { get; set; }
        public MainAccount MainAccount { get; set; }
        public bool IsFrontal { get; set; } = false;


        public void Configure(EntityTypeBuilder<GeneralBankAccount> builder)
        {
            builder.ToTable(nameof(GeneralBankAccount)).HasKey(b=>b.Id);

            builder.Property(b => b.BankName).IsRequired().HasMaxLength(200);
            builder.Property(b => b.BankAccountNumber).IsRequired().HasMaxLength(200);
            builder.Property(b => b.AccountName).IsRequired().HasMaxLength(200);
            builder.Property(b => b.BankBranchName).IsRequired().HasMaxLength(200);

            builder.Property(b => b.SwiftCode).HasMaxLength(100);
            builder.Property(b => b.IBAN).HasMaxLength(200);


            builder.Property(b => b.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(b => b.CreatedAt)
                .IsRequired();

            builder.HasIndex(b => b.BankAccountNumber).IsUnique();

            builder.HasOne(b => b.MainAccount)
             .WithMany()
             .HasForeignKey(b => b.MainAccountId)
             .OnDelete(DeleteBehavior.NoAction);

            builder.Navigation(b => b.MainAccount).AutoInclude();

            builder.HasQueryFilter(p => p.IsActive == true);



        }
    }
}

