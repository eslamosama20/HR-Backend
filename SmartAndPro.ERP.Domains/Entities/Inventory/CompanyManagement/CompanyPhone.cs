using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement
{
    public class CompanyPhone :  Entity, IAggregateRoot, IEntityTypeConfiguration<CompanyPhone>
    {
        public sealed override Guid Id { get; set; }

        public string? PhoneNumber { get; set; }

        public bool IsPrimary { get; set; } 

        public Guid CompanyId { get; set; }
        public CompanyDetails Company { get; set; }


        public void Configure(EntityTypeBuilder<CompanyPhone> builder)
        {
            builder.ToTable("CompanyPhones");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.PhoneNumber)
                .HasMaxLength(200);

            builder.HasOne(p=>p.Company)
                .WithMany(c=>c.Phones)
                .HasForeignKey(p=>p.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

        }

    }
}