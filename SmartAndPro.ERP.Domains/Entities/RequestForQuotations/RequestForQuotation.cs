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
    public class RequestForQuotation : Entity, IEntityTypeConfiguration<RequestForQuotation>
    {
        public List<RequestForQuotationItem> Items { get; set; } = new();

        public void Configure(EntityTypeBuilder<RequestForQuotation> builder)
        {
            builder.ToTable(nameof(RequestForQuotation))
                .HasKey(r=>r.Id);

            builder.HasMany(r => r.Items)
                .WithOne(i => i.RequestForQuotation)
                .HasForeignKey(i => i.RequestForQuotationId)
                .OnDelete(DeleteBehavior.NoAction);
            builder.Navigation(r => r.Items).AutoInclude();

        }
    }
}
