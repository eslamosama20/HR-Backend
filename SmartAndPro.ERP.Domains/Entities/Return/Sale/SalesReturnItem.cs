using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Return.Sale

{
    public class SalesReturnItem : BaseSalesReturnItem<int>, IEntityTypeConfiguration<SalesReturnItem>
    {
        public int SalesReturnId { get; set; }
        //[JsonIgnore]
        public virtual SalesReturn SalesReturn { get; set; }
        public void Configure(EntityTypeBuilder<SalesReturnItem> builder)
        {
            builder.Navigation(x => x.MainSalesClass).AutoInclude();

        }
    }
}
