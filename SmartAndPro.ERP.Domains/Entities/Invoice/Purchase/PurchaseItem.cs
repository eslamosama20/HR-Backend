using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;

namespace SmartAndPro.ERP.Domains.Entities.Invoice.Purchase
{
    public class PurchaseItem : BasePurchaseItem<int> , IEntityTypeConfiguration<PurchaseItem>
    {
        public Guid StoreDetailsId { get; set; }
        public StoreDetails StoreDetails { get; set; }
        public int PurchaseInvoiceId { get; set; }
        //[JsonIgnore]
        public virtual PurchaseInvoice PurchaseInvoice { get; set; }
        
        public string? ItemArabicName { get; set; }
        public string? ItemEnglishName { get; set; }

        // أسعار البيع الثلاثة
        public decimal RetailPrice { get; set; }
        public decimal WholesalePrice { get; set; }
        public decimal BulkPrice { get; set; }

        // Batch / Expiry tracking fields (PDF spec §7)
        public string? BatchNo { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public void Configure(EntityTypeBuilder<PurchaseItem> builder)
        {
            builder.Navigation((x) => x.StoreDetails).AutoInclude();
            builder.Navigation((x) => x.MainPurchaseClass).AutoInclude();

            builder.Property(x => x.BatchNo).HasMaxLength(100);
        }
    }
}
