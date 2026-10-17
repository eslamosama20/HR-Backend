using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.HotelAllotmentInvoices;
using SmartAndPro.ERP.Domains.Entities.Invoice;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.SharedKernel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartAndPro.ERP.Domains.Entities.Receipts;
namespace SmartAndPro.ERP.Domains.Entities.Payment_Schedules
{
    public class PaymentSchedule : Entity ,  IEntityTypeConfiguration<PaymentSchedule>
    {
        public string InvoiceId { get; set; } = string.Empty; // يمكن int أو Guid
        public InvoiceTypeEnum InvoiceType { get; set; }

        public decimal Amount { get; set; }
        public decimal? VAT { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string? EntryNo { get; set; } 
        public string? Notes { get; set; }
        public PaymentMethodType? PaymentMethodType { get; set; }
        public string? ReferenceNumber { get; set; }

        public Guid? PaycheckReceiptId { get; set; }
        public Guid? PaymentReceiptId { get; set; }
        public PaymentReceipt? PaymentReceipt { get; set; }
        public string? ReceiptNo { get; set; }
        public Guid? TreasuryId { get; set; }  // الخزينة المستخدمة في الدفعة


        // رقم الفاتورة في جدولها الأصلي
        //public int? HotelAllotmentInvoiceId { get; set; }
        //public HotelAllotmentInvoice? HotelAllotmentInvoice { get; set; }

        //public short InvoiceTypeId { get; set; }
        //public InvoiceType InvoiceType { get; set; } = null!;


        public void Configure(EntityTypeBuilder<PaymentSchedule> builder)
        {
            builder.ToTable(nameof(PaymentSchedule));
            builder.HasKey(x => x.Id);

  

            builder.Property(x => x.Amount)
                  .HasPrecision(18, 2);

            builder.Property(x => x.VAT)
                   .HasPrecision(5, 2);

            builder.Property(x => x.InvoiceType)
              .HasConversion<int>()  // يخزن كرقم
              .IsRequired();

            builder.HasIndex(x => new { x.InvoiceType, x.InvoiceId })
       .HasDatabaseName("IX_PaymentSchedule_Invoice");

            // TreasuryId - nullable FK (لو حذفت الخزينة بيبقى null)
            builder.Property(x => x.TreasuryId)
                   .IsRequired(false);

            // Optional FK to PaymentReceipt
            builder.HasOne(x => x.PaymentReceipt)
                   .WithMany()
                   .HasForeignKey(x => x.PaymentReceiptId)
                   .OnDelete(DeleteBehavior.SetNull);


        }
    }
}
