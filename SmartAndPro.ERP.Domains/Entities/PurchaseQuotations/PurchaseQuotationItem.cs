using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;

namespace SmartAndPro.ERP.Domains.Entities.PurchaseQuotations
{
    public class PurchaseQuotationItem : BasePurchaseItem<int> , IEntityTypeConfiguration<PurchaseQuotationItem>
    {
        //public Guid? StoreDetailsId { get; set; }
        //public StoreDetails? StoreDetails { get; set; }
        public int PurchaseQuotationId { get; set; }
        //[JsonIgnore]
        //public virtual PurchaseInvoice PurchaseInvoice { get; set; }

        public virtual PurchaseQuotation PurchaseQuotation { get; set; }
        public void Configure(EntityTypeBuilder<PurchaseQuotationItem> builder)
        {
            //builder.Navigation((x) => x.StoreDetails).AutoInclude();
            builder.Navigation((x) => x.MainPurchaseClass).AutoInclude();

        }
    }
}
