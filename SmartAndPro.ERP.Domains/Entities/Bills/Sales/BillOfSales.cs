using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Assets;

namespace SmartAndPro.ERP.Domains.Entities.Bills.Sales
{
    public class BillOfSales :Entity, IEntityTypeConfiguration<BillOfSales>
    {
        public decimal TotalPrice { get; set; }
        public string? Description { get; set; }
        public string BillNo { get; set; }
        public string SellerId { get; set; }
        public Guid? TreasuryId { get; set; }
        public DateTime BillOfSalesCreatedAt { get; set; }
        public Guid? AssetId { get; set; }
        public Asset? Asset { get; set; }
        public virtual ICollection<BillAccounts> BillAccounts { get; set; } = new List<BillAccounts>();
        public void Configure(EntityTypeBuilder<BillOfSales> builder)
        {
            builder.Navigation(x => x.BillAccounts).AutoInclude();
            builder.Navigation(x => x.Asset).AutoInclude();

        }
    }
}
