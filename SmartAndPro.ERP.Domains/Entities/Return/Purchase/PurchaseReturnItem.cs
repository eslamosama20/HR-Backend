using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;

namespace SmartAndPro.ERP.Domains.Entities.Return.Purchase
{
    public class PurchaseReturnItem : BasePurchaseReturnItem<int> , IEntityTypeConfiguration<PurchaseReturnItem>
    {
        public Guid StoreDetailsId { get; set; }
        public StoreDetails StoreDetails { get; set; }
        
        public int PurchaseReturnId { get; set; }
        //[JsonIgnore]
        public virtual PurchaseReturn PurchaseReturn { get; set; }
        public void Configure(EntityTypeBuilder<PurchaseReturnItem> builder)
        {
            builder.Navigation((x) => x.MainPurchaseClass).AutoInclude();
            builder.Navigation((x) => x.StoreDetails).AutoInclude();
        }
    }
}
