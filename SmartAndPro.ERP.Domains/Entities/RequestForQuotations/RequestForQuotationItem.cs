using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Domains.Entities.RequestForQuotations
{
    public class RequestForQuotationItem : Entity, IEntityTypeConfiguration<RequestForQuotationItem>
    {
        public string ItemName{ get; set; }
        public decimal Quantity{ get; set; }

        public Guid RequestForQuotationId { get; set; }  
        public RequestForQuotation RequestForQuotation { get; set; }  
        public void Configure(EntityTypeBuilder<RequestForQuotationItem> builder)
        {
           builder.ToTable(nameof(RequestForQuotationItem))
                .HasKey(i=>i.Id);

            builder.Property(i => i.ItemName)
                .IsRequired();

            builder.Property(i => i.Quantity)
               .IsRequired();

            builder.HasOne(i=>i.RequestForQuotation)
                .WithMany(r=>r.Items)
                .HasForeignKey(i=>i.RequestForQuotationId)
                .OnDelete(DeleteBehavior.NoAction);
            
        }
    }
}
