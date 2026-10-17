using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Domains.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement
{

        public class CompanyPolicy : Entity, IAggregateRoot, IEntityTypeConfiguration<CompanyPolicy>
        {
            public sealed override Guid Id { get; set; }

            public string? PolicyParagraph { get; set; }

            public bool IsPrimary { get; set; }

            public Guid CompanyId { get; set; }
            public CompanyDetails Company { get; set; }

        public void Configure(EntityTypeBuilder<CompanyPolicy> builder)
        {
            builder.ToTable(nameof(CompanyPolicy));

            builder.HasKey(p=>p.Id);

            builder.HasOne(p=>p.Company)
                .WithMany(c=>c.Policies)
                .HasForeignKey(p=>p.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);
        }


    }
    }
