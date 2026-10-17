using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Assets;
using SmartAndPro.ERP.Domains.Entities.HotelAllotmentInvoices;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Domains.Entities.Receipts;
using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Domains.Entities.Reservation;

public class Reservation: Entity , IEntityTypeConfiguration<Reservation>
{
    public string ReferenceNumber { get; set; }
    public string GuestName { get; set; }
    public string? GuestPhone { get; set; }
    public string? GuestEmail { get; set; }
    public string Nationality { get; set; }
    
    public DateOnly ArrivalDate { get; set; }
    public DateOnly DepartureDate { get; set; }
    
    public int NumberOfNights { get; set; }
    
    public Guid? ReservationCompanyId { get; set; }
    public ReservationCompany ReservationCompany { get; set; }
    public Guid ReservationClientCompanyId { get; set; }
    public ReservationCompany ReservationClientCompany { get; set; }
    
    public Guid? SalesRepresentativeId { get; set; }
    public SalesRepresentative SalesRepresentative { get; set; }
    
    public Guid HotelId { get; set; }
    public Hotel Hotel { get; set; }
    public Guid ReservationSeasonId { get; set; }
    public ReservationSeason ReservationSeason { get; set; }
    
    public ReservationType ReservationType { get; set; } = ReservationType.HotelDirect;
    
    public List<ReservationRoomEntry> ReservationRoomEntries { get; set; }
    
    public List<ReservationServiceEntry> ReservationServiceEntries { get; set; }
    
    public Guid? TransactionId { get; set; }
    
    public bool IsSentToZatca  { get; set; } = false;
    public bool IsCancelled  { get; set; } = false;
    
    public string? InvoiceHash { get; set; }
    public string? GeneratedQR { get; set; }
    
    public bool IsFrontal { get; set; } = false;
    public bool IsConfirmed { get; set; } = false;

    public Guid? BankAccountId { get; set; }
    public BankAccount BankAccount{ get; set; }
    public Guid? CompanyPhoneId { get; set; }
    public CompanyPhone CompanyPhone{ get; set; }
    public Guid? CompanyPolicyId { get; set; }
    public CompanyPolicy CompanyPolicy { get; set; }

    public List<Attachment> Attachments { get; set; }
    public List<ReceiptConnection> ReceiptConnections { get; set; } = new();

    public int? SaleInvoiceId { get; set; }
    public SaleInvoice SaleInvoice { get; set; }

    public List<AllotmentUsage>? AllotmentUsages { get; set; }

    public int? HotelAllotmentInvoiceId { get; set; }
    public HotelAllotmentInvoice? HotelAllotmentInvoice { get; set; }
    public string? HotelRemarks { get; set; }
    public string? ClientRemarks { get; set; }

    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder
            .ToTable("Reservation")
            .HasKey(x => x.Id);
        builder.Property(x => x.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.GuestName)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.GuestPhone)
            .HasMaxLength(100);
        builder.Property(x => x.GuestEmail)
            .HasMaxLength(255);
        
        builder.Property(x => x.Nationality)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.ArrivalDate)
            .HasColumnType("date");
        builder.Property(x => x.DepartureDate)
            .HasColumnType("date");
        builder.Property(x => x.NumberOfNights)
            .IsRequired();
        builder.Property(x => x.ReservationClientCompanyId)
            .IsRequired();
        
        builder.Property(x => x.HotelId)
            .IsRequired();
        builder.Property(x => x.ReservationSeasonId)
            .IsRequired();
        builder.Property(x => x.ReservationType)
            .IsRequired()
            .HasConversion<string>(
                v => v.ToString(),
                v => (ReservationType)Enum.Parse(typeof(ReservationType), v));
        
        builder.HasOne(x => x.ReservationCompany)
            .WithMany()
            .HasForeignKey(x => x.ReservationCompanyId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne(x => x.ReservationClientCompany)
            .WithMany()
            .HasForeignKey(x => x.ReservationClientCompanyId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne(x => x.SalesRepresentative)
            .WithMany()
            .HasForeignKey(x => x.SalesRepresentativeId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne(x => x.Hotel)
            .WithMany()
            .HasForeignKey(x => x.HotelId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne(x => x.ReservationSeason)
            .WithMany()
            .HasForeignKey(x => x.ReservationSeasonId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasMany(x => x.ReservationRoomEntries)
            .WithOne(x => x.Reservation)
            .HasForeignKey(x => x.ReservationId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasMany(x => x.ReservationServiceEntries)
            .WithOne(x => x.Reservation)
            .HasForeignKey(x => x.ReservationId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne<Transaction>()
            .WithOne()
            .HasForeignKey<Reservation>(i => i.TransactionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.HotelAllotmentInvoice)
               .WithMany(i=>i.Reservations)
               .HasForeignKey(x => x.HotelAllotmentInvoiceId)
               .OnDelete(DeleteBehavior.NoAction);


        builder.HasMany(x => x.Attachments)
            .WithOne()
            .HasForeignKey(x => x.ReservationId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(x => x.ReceiptConnections)
         .WithOne(rc=>rc.Reservation)
         .HasForeignKey(x => x.ReservationId)
         .OnDelete(DeleteBehavior.NoAction);

        builder.Navigation(x => x.ReceiptConnections)
            .AutoInclude();

        builder.HasOne(x => x.BankAccount)
            .WithMany()
            .HasForeignKey(x => x.BankAccountId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.CompanyPhone)
            .WithMany()
            .HasForeignKey(x => x.CompanyPhoneId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.CompanyPolicy)
            .WithMany()
            .HasForeignKey(x => x.CompanyPolicyId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Navigation(x => x.Attachments);
        
        builder.Navigation(x => x.ReservationRoomEntries)
            .AutoInclude();


        builder.HasMany(x => x.AllotmentUsages)
            .WithOne(x=>x.Reservation)
            .HasForeignKey(x => x.ReservationId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Navigation(x => x.BankAccount).AutoInclude();
        builder.Navigation(x => x.CompanyPhone).AutoInclude();
        builder.Navigation(x => x.CompanyPolicy).AutoInclude();

        builder.Navigation(x => x.ReservationServiceEntries)
            .AutoInclude();
        
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.IsFrontal)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsConfirmed)
          .IsRequired()
          .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();
            
        builder.HasIndex(x => x.CreatedAt);

        builder.HasIndex(x => x.ArrivalDate);
        builder.HasIndex(x => x.IsConfirmed);
        builder.HasIndex(x => x.IsCancelled);

        builder.HasQueryFilter(p => p.IsActive == true);


        builder.Property(x=>x.IsCancelled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Navigation(x => x.SaleInvoice).AutoInclude();

        builder.HasOne(x => x.SaleInvoice)
         .WithMany(x=>x.Reservations)
         .HasForeignKey(x => x.SaleInvoiceId)
         .OnDelete(DeleteBehavior.NoAction);


    }
}