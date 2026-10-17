using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.Domains.Entities.Projects;
using SmartAndPro.ERP.SharedKernel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Domains.Entities.Receipts
{
    public class ReceiptConnection : Entity , IEntityTypeConfiguration<ReceiptConnection> 
    {
        public  Guid? PaycheckReceiptId { get; set; }
        public PaycheckReceipt PaycheckReceipt { get; set; } 

        public Guid? PaymentReceiptId { get; set; }
        public PaymentReceipt PaymentReceipt { get; set; }

        public int? SaleInvoiceId { get; set; }
        public SaleInvoice SaleInvoice { get; set; }

        public int? PurchaseInvoiceId { get; set; }
        public PurchaseInvoice PurchaseInvoice { get; set; }

         public Guid? ReservationId { get; set; }
         public SmartAndPro.ERP.Domains.Entities.Reservation.Reservation Reservation { get; set; }

        public Guid? ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        public decimal LinkedAmount { get; set; }

        public ReceiptConnectionType ReceiptConnectionType { get; set; }
        public void Configure(EntityTypeBuilder<ReceiptConnection> builder)
        {
            builder.ToTable(nameof(ReceiptConnection)).HasKey(r=>r.Id);

            builder.HasOne(r=>r.PaycheckReceipt)
                .WithMany(r=>r.ReceiptConnections)
                .HasForeignKey(r=>r.PaycheckReceiptId)
                .OnDelete(DeleteBehavior.NoAction);


            builder.HasOne(r => r.PaymentReceipt)
                .WithMany(p=>p.ReceiptConnections)
                .HasForeignKey(r => r.PaymentReceiptId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(r => r.SaleInvoice)
                .WithMany(p=>p.ReceiptConnections)
                .HasForeignKey(r => r.SaleInvoiceId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(r => r.PurchaseInvoice)
                .WithMany(p=>p.ReceiptConnections)
                .HasForeignKey(r => r.PurchaseInvoiceId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(r => r.Reservation)
                .WithMany(r=>r.ReceiptConnections)
                .HasForeignKey(r => r.ReservationId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(r => r.Project)
                .WithMany()
                .HasForeignKey(r => r.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Property(r => r.LinkedAmount).IsRequired();

            builder.Property(r => r.ReceiptConnectionType)
                .IsRequired()
               .HasConversion<int>(); 

        }
    }
}
