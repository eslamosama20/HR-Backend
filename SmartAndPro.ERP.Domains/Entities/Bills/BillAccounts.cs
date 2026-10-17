using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Bills
{
    public class BillAccounts : Entity, IEntityTypeConfiguration<BillAccounts>
    {
        public Guid SupplierId { get; set; }
        public string SupplierBankAccountNumber { get; set; }
        public string? Description { get; set; }
        public decimal NetPrice { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalPrice { get; set; }
        public void Configure(EntityTypeBuilder<BillAccounts> builder)
        {
            builder.ToTable("BillsAccounts").HasKey(x => x.Id);
        }
    }
}
