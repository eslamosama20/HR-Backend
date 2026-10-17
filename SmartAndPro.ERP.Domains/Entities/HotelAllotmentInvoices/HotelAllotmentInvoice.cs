using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Assets;
using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.Domains.Entities.Payment_Schedules;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.SharedKernel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Domains.Entities.HotelAllotmentInvoices
{
    public class HotelAllotmentInvoice : BaseInvoice<int>, IEntityTypeConfiguration<HotelAllotmentInvoice>
    {

        public Guid HotelId { get; set; }
        public Hotel Hotel { get; set; }

        public Guid? TransactionId { get; set; }
        public Transaction Transaction { get; set; }
        public int TotalNumberOfRooms { get; set; }
        //public DateTime FromDate { get; set; }
        //public DateTime ToDate { get; set; }
        public int TotalNumberOfDays { get; set; }
        public Guid? MainAccountId { get; set; }
        public MainAccount? MainAccount { get; set; }

        public List<HotelAllotmentRoom> HotelAllotmentRooms { get; set; } = new();
        public List<Attachment>? Attachments { get; set; }
        //public List<PaymentSchedule>? PaymentSchedules { get; set; }
        public List<Domains.Entities.Reservation.Reservation>? Reservations { get; set; }

        public bool IsFrontal { get; set; } = false;
        public bool IsCanceled { get; set; } = false;

        //public Guid? ReservationViewId { get; set; }
        //public ReservationView? ReservationView { get; set; }
        //public Guid? ReservationMealId { get; set; }
        //public ReservationMeal? ReservationMeal { get; set; }

        public decimal TotalCostBeforeVAT { get; set; }

        //public InvoiceTypeEnum InvoiceType { get; set; } = InvoiceTypeEnum.HotelAllotmentInvoice;


        public void Configure(EntityTypeBuilder<HotelAllotmentInvoice> builder)
        {
            builder.ToTable(nameof(HotelAllotmentInvoice));
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Hotel)
                .WithMany()
                .HasForeignKey(x => x.HotelId);


            builder.HasOne(x => x.Transaction)
                  .WithOne()
                  .HasForeignKey<HotelAllotmentInvoice>(x => x.TransactionId)
                  .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(x => x.HotelAllotmentRooms)
                 .WithOne(x => x.HotelAllotmentInvoice)
                 .HasForeignKey(x => x.HotelAllotmentInvoiceId)
                 .OnDelete(DeleteBehavior.NoAction);

            builder.Navigation(x => x.Hotel).AutoInclude();
            builder.Navigation(x => x.HotelAllotmentRooms).AutoInclude();

            builder.HasMany(e => e.Attachments)
           .WithOne()
           .HasForeignKey(e => e.HotelAllotmentInvoiceId)
           .OnDelete(DeleteBehavior.NoAction);

            builder.Navigation(e => e.Attachments).AutoInclude();

           // builder.HasOne(x => x.ReservationView)
           //.WithMany()
           //.HasForeignKey(x => x.ReservationViewId)
           //.OnDelete(DeleteBehavior.NoAction);

           // builder.Navigation(x => x.ReservationView).AutoInclude();

           // builder.HasOne(x => x.ReservationMeal)
           //.WithMany()
           //.HasForeignKey(x => x.ReservationMealId)
           //.OnDelete(DeleteBehavior.NoAction);

           // builder.Navigation(x => x.ReservationMeal).AutoInclude();

            builder.HasOne(x => x.MainAccount)
           .WithMany()
           .HasForeignKey(x => x.MainAccountId)
           .OnDelete(DeleteBehavior.NoAction);
            builder.Navigation(x => x.MainAccount).AutoInclude();

            builder.HasIndex(x => x.InvoiceNo).IsUnique();

            //builder.HasMany(e => e.PaymentSchedules)
            //  .WithOne(e=>e.HotelAllotmentInvoice)
            //  .HasForeignKey(e => e.HotelAllotmentInvoiceId)
            //  .OnDelete(DeleteBehavior.NoAction);   

            //builder.Navigation(e => e.PaymentSchedules).AutoInclude();

            builder.HasMany(e => e.Reservations)
                .WithOne(r => r.HotelAllotmentInvoice)
                .HasForeignKey(r => r.HotelAllotmentInvoiceId) 
                .OnDelete(DeleteBehavior.NoAction);



            //builder.Property(x => x.InvoiceType)
            //.HasConversion<int>() 
            //.IsRequired();

        }
    }
}
