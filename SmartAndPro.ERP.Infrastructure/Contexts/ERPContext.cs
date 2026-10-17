using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Assets;
using SmartAndPro.ERP.Domains.Entities.Auth.Authorization;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using SmartAndPro.ERP.Domains.Entities.Bills;
using SmartAndPro.ERP.Domains.Entities.Bills.Exchange;
using SmartAndPro.ERP.Domains.Entities.Bills.Sales;
using SmartAndPro.ERP.Domains.Entities.GeneralBankAccounts;
using SmartAndPro.ERP.Domains.Entities.Inventory;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.InventoryOperation;
using SmartAndPro.ERP.Domains.Entities.Inventory.PermitManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.Stocktaking;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.SupplierManagement;
using SmartAndPro.ERP.Domains.Entities.Invoice;
using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Domains.Entities.MealReservation;
using SmartAndPro.ERP.Domains.Entities.Notes;
using SmartAndPro.ERP.Domains.Entities.Notes.SalesCreditNote;
using SmartAndPro.ERP.Domains.Entities.Notifications;
using SmartAndPro.ERP.Domains.Entities.Receipts;
using SmartAndPro.ERP.Domains.Entities.Recipes;
using SmartAndPro.ERP.Domains.Entities.RequestForQuotations;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.Domains.Entities.Return;
using SmartAndPro.ERP.Domains.Entities.Return.Purchase;
using SmartAndPro.ERP.Domains.Entities.Return.Sale;
using SmartAndPro.ERP.Domains.Entities.Treasuries;
using SmartAndPro.ERP.Domains.Entities.PublicBooking;
using SmartAndPro.ERP.Domains.Entities.Payment;
using SmartAndPro.ERP.Domains.Entities.Projects;
using SmartAndPro.ERP.Domains.Entities.Cashier;
using SmartAndPro.ERP.Infrastructure.Contexts.Utility;
using System.Reflection;
using SmartAndPro.ERP.Domains.Entities.HotelAllotmentInvoices;
using SmartAndPro.ERP.Domains.Entities.Payment_Schedules;
using SmartAndPro.ERP.Domains.Entities.PurchaseQuotations;
using SmartAndPro.ERP.Domains.Entities.Cashier;
using SmartAndPro.ERP.Domains.Entities.Pharmacy;
using SmartAndPro.ERP.Domains.Entities.Salla;
using SmartAndPro.ERP.Domains.Entities.Zid;
using SmartAndPro.ERP.Domains.Entities.Wasafaty;
using SmartAndPro.ERP.Domains.Entities.ChannelManager;
using SmartAndPro.ERP.Domains.Entities.HR;
using SmartAndPro.ERP.Domains.Entities;


namespace SmartAndPro.ERP.Infrastructure.Contexts;

public class ERPContext : DbContext
{
    private readonly IAutoNumberGenerator _autoNumberGenerator;

    public ERPContext(DbContextOptions<ERPContext> dbContextOptions, IAutoNumberGenerator autoNumberGenerator) : base(dbContextOptions)
    {
        _autoNumberGenerator = autoNumberGenerator;
    }
    // INVENTORY
    //public DbSet<ProductDetails> ProductDetails { get; set; }
    public DbSet<ProductUnit> ProductUnit { get; set; }
    public DbSet<MeasurementUnit> MeasurementUnit { get; set; }
    public DbSet<ProductUnitStatus> ProductUnitStatus { get; set; }
    public DbSet<BatchStatus> BatchStatus { get; set; }
    public DbSet<ProductBatchDetails> ProductBatchDetails { get; set; }
    public DbSet<ProductBatchToUnitAssignments> ProductBatchToUnitAssignments { get; set; }
    public DbSet<StoreDetails> StoreDetails { get; set; }
    public DbSet<StoreStatus> StoreStatus { get; set; }
    public DbSet<CompanyDetails> CompanyDetails { get; set; }
    public DbSet<CompanyPhone> CompanyPhones{ get; set; }
    public DbSet<BankAccount> BankAccounts { get; set; }
    public DbSet<CompanyStatus> CompanyStatus { get; set; }
    public DbSet<BranchStatus> BranchStatus { get; set; }
    public DbSet<CompanyPolicy> CompanyPolicies { get; set; }
    public DbSet<BranchDetails> BranchDetails { get; set; }
    public DbSet<SupplierDetails> SupplierDetails { get; set; }
    public DbSet<SupplierStatus> SupplierStatus { get; set; }

    // AUTH
    public DbSet<User> Users { get; set; }
    public DbSet<PasswordsLog> passwordsLogs { get; set; }
    public DbSet<LoginActionLogs> LoginActionLogs { get; set; }
    public DbSet<OTPs> OTPs { get; set; }
    public DbSet<OTPStatuses> OTPStatuses { get; set; }
    public DbSet<OTPType> OTPType { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<DomainDetails> DomainDetails { get; set; }

    // ACCOUNTING
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionType> TransactionTypes { get; set; }
    public DbSet<TransactionEntry> TransactionEntries { get; set; }

    public DbSet<MainAccount> MainAccounts { get; set; }
    public DbSet<DetailAccount> DetailAccounts { get; set; }
    public DbSet<RootAccount> RootAccounts { get; set; }
    public DbSet<HeaderAccount> HeaderAccounts { get; set; }
    public DbSet<RootPurchaseClass> RootPurchaseClasses { get; set; }
    public DbSet<HeaderPurchaseClass> HeaderPurchaseClasses { get; set; }
    public DbSet<DetailPurchaseClass> DetailPurchaseClasses { get; set; }
    public DbSet<MainPurchaseClass> MainPurchaseClasses { get; set; }

    public DbSet<RootSalesClass> RootSalesClasses { get; set; }
    public DbSet<HeaderSalesClass> HeaderSalesClasses { get; set; }
    public DbSet<DetailSalesClass> DetailSalesClasses { get; set; }
    public DbSet<MainSalesClass> MainSalesClasses { get; set; }

    public DbSet<ClientProfile> ClientProfiles { get; set; }
    public DbSet<CustomerProfile> CustomerProfiles { get; set; }
    public DbSet<CustomerDepartment> CustomerDepartments { get; set; }

    // INVOICE
    public DbSet<PurchaseInvoice> PurchaseInvoices { get; set; }
    public DbSet<SaleInvoice> SaleInvoices { get; set; }
    public DbSet<InvoiceType> InvoiceTypes { get; set; }
    public DbSet<PurchaseItem> PurchaseItems { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }
    public DbSet<CostCenter> CostCenters { get; set; }

    // RETURN
    public DbSet<PurchaseReturn> PurchaseReturns { get; set; }
    public DbSet<SalesReturn> SalesReturns { get; set; }
    public DbSet<ReturnType> ReturnTypes { get; set; }
    public DbSet<PurchaseReturnItem> PurchaseReturnItems { get; set; }
    public DbSet<SalesReturnItem> SalesReturnItems { get; set; }

    // Bonds
    public DbSet<Treasury> Treasuries { get; set; }
    public DbSet<GeneralBankAccount> GeneralBankAccounts { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<BillOfSales> BillOfSales { get; set; }
    public DbSet<BillOfExchange> BillOfExchanges { get; set; }
    public DbSet<BillAccounts> BillsAccounts { get; set; }


    //Receipts
    public DbSet<PaycheckReceipt> PaycheckReceipts { get; set; }
    public DbSet<PaycheckReceiptAccount> PaycheckReceiptAccounts { get; set; }
    public DbSet<PaymentReceipt> PaymentReceipts { get; set; }
    public DbSet<PaymentReceiptAccount> PaymentReceiptAccounts { get; set; }
    public DbSet<ReceiptConnection> ReceiptConnections { get; set; }

    //Notes
    public DbSet<DebitNote> DebitNotes { get; set; }
    public DbSet<DebitNoteAccount> DebitNoteAccounts { get; set; }
    public DbSet<SalesCreditNote> SalesCreditNotes { get; set; }
    public DbSet<SalesCreditNoteItem> SalesCreditNoteItems { get; set; }
    //Permits
    public DbSet<InventoryPermit> InventoryPermits { get; set; }
    public DbSet<PermitEntry> PermitEntries { get; set; }

    //Expenses Center
    public DbSet<ExpensesCenter> ExpensesCenters { get; set; }
    //Finance Periods
    public DbSet<FinancePeriods> FinancePeriods { get; set; }

    // Recipes
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<RecipeEntry> RecipeEntries { get; set; }

    //Inventory Operation
    public DbSet<InventoryOperation> InventoryOperations { get; set; }
    public DbSet<OperationEntry> OperationEntries { get; set; }

    //Inventory Stocktaking
    public DbSet<InventoryStocktaking> InventoryStocktakings { get; set; }
    public DbSet<StocktakingEntry> StocktakingEntries { get; set; }

    //Damaged Inventory
    public DbSet<Domains.Entities.Inventory.DamagedInventoryManagement.DamagedInventory> DamagedInventories { get; set; }

    //Reservation
    public DbSet<SalesRepresentative> SalesRepresentatives { get; set; }
    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<RoomType> RoomTypes { get; set; }
    public DbSet<ReservationMeal> ReservationMeals { get; set; }
    public DbSet<ReservationService> ReservationServices { get; set; }
    public DbSet<ReservationCompany> ReservationCompanies { get; set; }
    public DbSet<ReservationView> ReservationViews { get; set; }
    public DbSet<ReservationSeason> ReservationSeasons { get; set; }
    public DbSet<ReservationSupplier> ReservationSuppliers { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<ReservationActionLog> ReservationActionLogs { get; set; }
    public DbSet<ReservationRoomEntry> ReservationRoomsEntries { get; set; }
    public DbSet<ReservationServiceEntry> ReservationServiceEntries { get; set; }
    public DbSet<ReservationPrintTableTemplate> ReservationPrintTableTemplates { get; set; }

    // MealReservation
    public DbSet<MealReservation> MealReservations { get; set; }
    public DbSet<MealReservationEntry> MealReservationEntries { get; set; }
    // Attachments
    public DbSet<Attachment> Attachments { get; set; }

    //Notifications
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<UserNotification> UserNotifications { get; set; }

    //RequestForQuotations
    public DbSet<RequestForQuotation> RequestForQuotations { get; set; }
    public DbSet<RequestForQuotationItem> RequestForQuotationItems { get; set; }

    // Public Booking System (independent from ERP)
    public DbSet<PublicHotel> PublicHotels { get; set; }
    public DbSet<PublicRoomType> PublicRoomTypes { get; set; }
    public DbSet<PublicRoomPricing> PublicRoomPricings { get; set; }
    public DbSet<PublicBooking> PublicBookings { get; set; }
    public DbSet<PublicBookingRoom> PublicBookingRooms { get; set; }
    public DbSet<PublicPaidService> PublicPaidServices { get; set; }
    public DbSet<PublicHotelPaidService> PublicHotelPaidServices { get; set; }
    public DbSet<PublicBookingPaidService> PublicBookingPaidServices { get; set; }
    public DbSet<SystemNotification> SystemNotifications { get; set; }
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
    public DbSet<RoomTypeSearchLog> RoomTypeSearchLogs { get; set; }
    public DbSet<HotelReview> HotelReviews { get; set; }
    public DbSet<HotelAllotmentInvoice> HotelAllotmentInvoices { get; set; }
    public DbSet<HotelAllotmentRoom> HotelAllotmentRooms { get; set; }
    public DbSet<PaymentSchedule> PaymentSchedules { get; set; }
    public DbSet<AllotmentUsage> AllotmentUsages { get; set; }
    public DbSet<PurchaseQuotation> PurchaseQuotations { get; set; }
    public DbSet<PurchaseQuotationItem> PurchaseQuotationItems { get; set; }
    public DbSet<CashierStockReservation> CashierStockReservations { get; set; }
    public DbSet<CashierAuditLog> CashierAuditLogs { get; set; }
    public DbSet<CashierSession> CashierSessions { get; set; }
    public DbSet<CashierPayment> CashierPayments { get; set; }
    public DbSet<CashierRefund> CashierRefunds { get; set; }
    public DbSet<CashierRefundItem> CashierRefundItems { get; set; }
    public DbSet<CheckoutIdempotencyRecord> CheckoutIdempotencyRecords { get; set; }
    public DbSet<CashierInventoryMovement> CashierInventoryMovements { get; set; }
    public DbSet<HeldOrder> HeldOrders { get; set; }
    public DbSet<HeldOrderLine> HeldOrderLines { get; set; }
    public DbSet<PaymentProviderEvent> PaymentProviderEvents { get; set; }

    // Pharmacy Module
    public DbSet<PharmacyPatient> PharmacyPatients { get; set; }
    public DbSet<PharmacyDoctor> PharmacyDoctors { get; set; }
    public DbSet<PharmacyInsuranceCompany> PharmacyInsuranceCompanies { get; set; }
    public DbSet<PharmacyPrescription> PharmacyPrescriptions { get; set; }
    public DbSet<PharmacyPrescriptionItem> PharmacyPrescriptionItems { get; set; }
    public DbSet<PharmacyAttachment> PharmacyAttachments { get; set; }
    public DbSet<PharmacyInsuranceClaim> PharmacyInsuranceClaims { get; set; }
    public DbSet<PharmacyControlledDrugRecord> PharmacyControlledDrugRecords { get; set; }
    public DbSet<PharmacyAlert> PharmacyAlerts { get; set; }
    public DbSet<PharmacyReturn> PharmacyReturns { get; set; }
    public DbSet<PharmacyDrugInteraction> PharmacyDrugInteractions { get; set; }
    public DbSet<PharmacyBatch> PharmacyBatches { get; set; }
    public DbSet<RestrictedDrugDispense> RestrictedDrugDispenses { get; set; }

    // Payment Orchestration System
    public DbSet<Payment> Payments { get; set; }

    // HR System
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Shift> Shifts { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<CheckInLog> CheckInLogs { get; set; }
    public DbSet<FingerprintDeviceSettings> FingerprintDeviceSettings { get; set; }
    public DbSet<EmploymentType> EmploymentTypes { get; set; }
    public DbSet<MonthlyLoan> MonthlyLoans { get; set; }
    public DbSet<OvertimeHours> OvertimeHours { get; set; }
    public DbSet<Penalty> Penalties { get; set; }
    public DbSet<Bonus> Bonuses { get; set; }
    public DbSet<BankDocument> BankDocuments { get; set; }
    public DbSet<CompanyDocument> CompanyDocuments { get; set; }
    public DbSet<EmploymentApplication> EmploymentApplications { get; set; }
    public DbSet<HRTrustReceipt> HRTrustReceipts { get; set; }
    public DbSet<InsuranceRecord> InsuranceRecords { get; set; }
    public DbSet<LeaveApplication> LeaveApplications { get; set; }
    public DbSet<ResignationRequest> ResignationRequests { get; set; }
    public DbSet<SalarySlip> SalarySlips { get; set; }

    // Pharmacy Allocation & POS Bridge (PDF spec §12-§16)
    public DbSet<PharmacyAllocation> PharmacyAllocations { get; set; }
    public DbSet<PharmacyAllocationLine> PharmacyAllocationLines { get; set; }
    public DbSet<PharmacyPickup> PharmacyPickups { get; set; }

    // Wasafaty (وصفتي) E-Prescription Integration
    public DbSet<WasafatyPrescription> WasafatyPrescriptions { get; set; }
    public DbSet<WasafatyPrescriptionItem> WasafatyPrescriptionItems { get; set; }
    public DbSet<WasafatySyncLog> WasafatySyncLogs { get; set; }
    public DbSet<WasafatyAuthToken> WasafatyAuthTokens { get; set; }

    // Salla E-Commerce Integration
    public DbSet<SallaOrder> SallaOrders { get; set; }
    public DbSet<SallaOrderItem> SallaOrderItems { get; set; }
    public DbSet<SallaProductMapping> SallaProductMappings { get; set; }
    public DbSet<SallaSyncLog> SallaSyncLogs { get; set; }
    public DbSet<SallaAuthToken> SallaAuthTokens { get; set; }

    // Zid E-Commerce Integration
    public DbSet<ZidOrder> ZidOrders { get; set; }
    public DbSet<ZidOrderItem> ZidOrderItems { get; set; }
    public DbSet<ZidProductMapping> ZidProductMappings { get; set; }
    public DbSet<ZidSyncLog> ZidSyncLogs { get; set; }
    public DbSet<ZidAuthToken> ZidAuthTokens { get; set; }

    // Channel Manager Integration
    public DbSet<ChannelConnection> ChannelConnections { get; set; }
    public DbSet<ChannelRoomMapping> ChannelRoomMappings { get; set; }
    public DbSet<ChannelRateMapping> ChannelRateMappings { get; set; }
    public DbSet<ChannelReservationMapping> ChannelReservationMappings { get; set; }
    public DbSet<ChannelReservationPayment> ChannelReservationPayments { get; set; }
    public DbSet<ChannelOutboxEvent> ChannelOutboxEvents { get; set; }
    public DbSet<ChannelWebhookQueue> ChannelWebhookQueues { get; set; }
    public DbSet<ChannelDeadLetterQueue> ChannelDeadLetterQueues { get; set; }
    public DbSet<ChannelSyncLog> ChannelSyncLogs { get; set; }


    // Project Management System
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectAssignmentPhase> ProjectAssignmentPhases { get; set; }
    public DbSet<ProjectExecutionPhase> ProjectExecutionPhases { get; set; }
    public DbSet<Company> ProjectCompanies { get; set; }
    public DbSet<Department> ProjectDepartments { get; set; }
    public DbSet<ProjectDocument> ProjectDocuments { get; set; }
    public DbSet<ProjectAssessment> ProjectAssessments { get; set; }
    public DbSet<ProjectPriceQuote> ProjectPriceQuotes { get; set; }
    public DbSet<ProjectPayment> ProjectPayments { get; set; }
    public DbSet<ProjectHistory> ProjectHistories { get; set; }
    public DbSet<ProjectExecutionMilestone> ProjectExecutionMilestones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var domain_assembly = Assembly.Load("SmartAndPro.ERP.Domains");
        modelBuilder.ApplyConfigurationsFromAssembly(domain_assembly);
        
        // Configure PublicBookingRoom to prevent cascade delete conflict
        // SQL Server doesn't allow multiple cascade paths to the same table
        modelBuilder.Entity<PublicBookingRoom>()
            .HasOne(br => br.RoomType)
            .WithMany(rt => rt.BookingRooms)
            .HasForeignKey(br => br.RoomTypeId)
            .OnDelete(DeleteBehavior.NoAction);

        // Configure Project Management System relationships
        
        // ProjectAssignmentPhase - Company relationships
        modelBuilder.Entity<ProjectAssignmentPhase>()
            .HasOne(p => p.ExecutingEntity)
            .WithMany(c => c.ExecutingProjects)
            .HasForeignKey(p => p.ExecutingEntityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProjectAssignmentPhase>()
            .HasOne(p => p.SupervisingEntity)
            .WithMany(c => c.SupervisingProjects)
            .HasForeignKey(p => p.SupervisingEntityId)
            .OnDelete(DeleteBehavior.Restrict);

        // ProjectExecutionPhase - Company relationship
        modelBuilder.Entity<ProjectExecutionPhase>()
            .HasOne(p => p.Company)
            .WithMany(c => c.ExecutionPhases)
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // ProjectPriceQuote - Company relationship
        modelBuilder.Entity<ProjectPriceQuote>()
            .HasOne(q => q.Company)
            .WithMany(c => c.PriceQuotes)
            .HasForeignKey(q => q.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        AddAutoGeneratedNumbers();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddAutoGeneratedNumbers();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void AddAutoGeneratedNumbers()
    {
        var transactions = ChangeTracker.Entries<Transaction>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();
        foreach (var transaction in transactions)
        {
            if (string.IsNullOrEmpty(transaction.Entity.ReferenceNumber))
            {
                transaction.Entity.ReferenceNumber =
                    transaction.Entity.IsOpeningTransaction ?
                        " " :
                        _autoNumberGenerator.GenerateAsync<Transaction>(
                            this,
                            nameof(Transaction.ReferenceNumber),
                            "TRN",
                            5,
                            "-"
                            ).Result;

            }
        }

        var RootAccounts = ChangeTracker.Entries<RootAccount>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();
        foreach (var rootAccount in RootAccounts)
        {
            if (string.IsNullOrEmpty(rootAccount.Entity.ReferenceCode))
            {
                rootAccount.Entity.ReferenceCode =
                    _autoNumberGenerator.GenerateAsync<RootAccount>(
                        this,
                        nameof(RootAccount.ReferenceCode),
                        "",
                        0
                        ).Result;

            }
        }

        var HeaderAccounts = ChangeTracker.Entries<HeaderAccount>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();
        foreach (var headerAccount in HeaderAccounts)
        {
            if (string.IsNullOrEmpty(headerAccount.Entity.ReferenceCode))
            {
                var rootRefCode = this.RootAccounts
                    .Where(ra => ra.Id == headerAccount.Entity.RootAccountId)
                    .Select(ra => ra.ReferenceCode)
                    .FirstOrDefault();
                headerAccount.Entity.ReferenceCode =
                    _autoNumberGenerator.GenerateAsync<HeaderAccount>(
                        this,
                        nameof(HeaderAccount.ReferenceCode),
                        rootRefCode,
                        2,
                        ""
                        ).Result;

            }
        }

        var DetailAccounts = ChangeTracker.Entries<DetailAccount>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();
        foreach (var detailAccount in DetailAccounts)
        {
            if (string.IsNullOrEmpty(detailAccount.Entity.ReferenceCode))
            {
                var headerRefCode = this.HeaderAccounts
                    .Where(ha => ha.Id == detailAccount.Entity.HeaderAccountId)
                    .Select(ha => ha.ReferenceCode)
                    .FirstOrDefault();
                detailAccount.Entity.ReferenceCode =
                    _autoNumberGenerator.GenerateAsync<DetailAccount>(
                        this,
                        nameof(DetailAccount.ReferenceCode),
                        headerRefCode,
                        1,
                        ""
                        ).Result;
            }
        }

        var MainAccounts = ChangeTracker.Entries<MainAccount>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();
        foreach (var mainAccount in MainAccounts)
        {
            if (string.IsNullOrEmpty(mainAccount.Entity.ReferenceCode))
            {
                var detailRefCode = this.DetailAccounts
                    .Where(da => da.Id == mainAccount.Entity.DetailAccountId)
                    .Select(da => da.ReferenceCode)
                    .FirstOrDefault();
                mainAccount.Entity.ReferenceCode =
                    _autoNumberGenerator.GenerateAsync<MainAccount>(
                        this,
                        nameof(MainAccount.ReferenceCode),
                        detailRefCode,
                        4,
                        ""
                        ).Result;
            }
        }

        var RootPurchaseClasses = ChangeTracker.Entries<RootPurchaseClass>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();
        foreach (var rootPurchaseClass in RootPurchaseClasses)
        {
            if (string.IsNullOrEmpty(rootPurchaseClass.Entity.ReferenceCode))
            {
                rootPurchaseClass.Entity.ReferenceCode =
                    _autoNumberGenerator.GenerateAsync<RootPurchaseClass>(
                        this,
                        nameof(RootPurchaseClass.ReferenceCode),
                        "",
                        0
                        ).Result;

            }
        }

        var HeaderPurchaseClasses = ChangeTracker.Entries<HeaderPurchaseClass>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();
        foreach (var headerPurchaseClass in HeaderPurchaseClasses)
        {
            if (string.IsNullOrEmpty(headerPurchaseClass.Entity.ReferenceCode))
            {
                var rootRefCode = this.RootPurchaseClasses
                    .Where(ra => ra.Id == headerPurchaseClass.Entity.RootPurchaseClassId)
                    .Select(ra => ra.ReferenceCode)
                    .FirstOrDefault();
                headerPurchaseClass.Entity.ReferenceCode =
                    _autoNumberGenerator.GenerateAsync<HeaderPurchaseClass>(
                        this,
                        nameof(HeaderPurchaseClass.ReferenceCode),
                        rootRefCode,
                        2,
                        ""
                        ).Result;

            }
        }

        var DetailPurchaseClasses = ChangeTracker.Entries<DetailPurchaseClass>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();
        foreach (var detailPurchaseClass in DetailPurchaseClasses)
        {
            if (string.IsNullOrEmpty(detailPurchaseClass.Entity.ReferenceCode))
            {
                var headerRefCode = this.HeaderPurchaseClasses
                    .Where(ha => ha.Id == detailPurchaseClass.Entity.HeaderPurchaseClassId)
                    .Select(ha => ha.ReferenceCode)
                    .FirstOrDefault();
                detailPurchaseClass.Entity.ReferenceCode =
                    _autoNumberGenerator.GenerateAsync<DetailPurchaseClass>(
                        this,
                        nameof(DetailPurchaseClass.ReferenceCode),
                        headerRefCode,
                        1,
                        ""
                        ).Result;
            }
        }

        var MainPurchaseClasses = ChangeTracker.Entries<MainPurchaseClass>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();
        foreach (var mainPurchaseClass in MainPurchaseClasses)
        {
            if (string.IsNullOrEmpty(mainPurchaseClass.Entity.ReferenceCode))
            {
                var detailRefCode = this.DetailPurchaseClasses
                    .Where(da => da.Id == mainPurchaseClass.Entity.DetailPurchaseClassId)
                    .Select(da => da.ReferenceCode)
                    .FirstOrDefault();
                mainPurchaseClass.Entity.ReferenceCode =
                    _autoNumberGenerator.GenerateAsync<MainPurchaseClass>(
                        this,
                        nameof(MainPurchaseClass.ReferenceCode),
                        detailRefCode,
                        3,
                        ""
                        ).Result;
            }
        }

        var RootSalesClasses = ChangeTracker.Entries<RootSalesClass>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();
        foreach (var rootSalesClass in RootSalesClasses)
        {
            if (string.IsNullOrEmpty(rootSalesClass.Entity.ReferenceCode))
            {
                rootSalesClass.Entity.ReferenceCode =
                    _autoNumberGenerator.GenerateAsync<RootSalesClass>(
                        this,
                        nameof(RootSalesClass.ReferenceCode),
                        "",
                        0
                        ).Result;

            }
        }

        var HeaderSalesClasses = ChangeTracker.Entries<HeaderSalesClass>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();
        foreach (var headerSalesClass in HeaderSalesClasses)
        {
            if (string.IsNullOrEmpty(headerSalesClass.Entity.ReferenceCode))
            {
                var rootRefCode = this.RootSalesClasses
                    .Where(ra => ra.Id == headerSalesClass.Entity.RootSalesClassId)
                    .Select(ra => ra.ReferenceCode)
                    .FirstOrDefault();
                headerSalesClass.Entity.ReferenceCode =
                    _autoNumberGenerator.GenerateAsync<HeaderSalesClass>(
                        this,
                        nameof(HeaderSalesClass.ReferenceCode),
                        rootRefCode,
                        2,
                        ""
                        ).Result;

            }
        }

        var DetailSalesClasses = ChangeTracker.Entries<DetailSalesClass>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();
        foreach (var detailSalesClass in DetailSalesClasses)
        {
            if (string.IsNullOrEmpty(detailSalesClass.Entity.ReferenceCode))
            {
                var headerRefCode = this.HeaderSalesClasses
                    .Where(ha => ha.Id == detailSalesClass.Entity.HeaderSalesClassId)
                    .Select(ha => ha.ReferenceCode)
                    .FirstOrDefault();
                detailSalesClass.Entity.ReferenceCode =
                    _autoNumberGenerator.GenerateAsync<DetailSalesClass>(
                        this,
                        nameof(DetailSalesClass.ReferenceCode),
                        headerRefCode,
                        1,
                        ""
                        ).Result;
            }
        }

        var MainSalesClasses = ChangeTracker.Entries<MainSalesClass>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();
        foreach (var mainSalesClass in MainSalesClasses)
        {
            if (string.IsNullOrEmpty(mainSalesClass.Entity.ReferenceCode))
            {
                var detailRefCode = this.DetailSalesClasses
                    .Where(da => da.Id == mainSalesClass.Entity.DetailSalesClassId)
                    .Select(da => da.ReferenceCode)
                    .FirstOrDefault();
                mainSalesClass.Entity.ReferenceCode =
                    _autoNumberGenerator.GenerateAsync<MainSalesClass>(
                        this,
                        nameof(MainSalesClass.ReferenceCode),
                        detailRefCode,
                        3,
                        ""
                        ).Result;
            }
        }

        var paymentReceipts = ChangeTracker.Entries<PaymentReceipt>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();
        foreach (var paymentReceipt in paymentReceipts)
        {
            if (string.IsNullOrEmpty(paymentReceipt.Entity.ReferenceNumber))
            {
                paymentReceipt.Entity.ReferenceNumber =
                        _autoNumberGenerator.GenerateAsync<PaymentReceipt>(
                            this,
                            nameof(PaymentReceipt.ReferenceNumber),
                            "M",
                            5,
                            "-"
                        ).Result;

            }
        }
        var paycheckReceipts = ChangeTracker.Entries<PaycheckReceipt>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();
        foreach (var paycheckReceipt in paycheckReceipts)
        {
            if (string.IsNullOrEmpty(paycheckReceipt.Entity.ReferenceNumber))
            {
                paycheckReceipt.Entity.ReferenceNumber =
                    _autoNumberGenerator.GenerateAsync<PaycheckReceipt>(
                        this,
                        nameof(PaycheckReceipt.ReferenceNumber),
                        "C",
                        5,
                        "-"
                    ).Result;

            }
        }
        // Notes
        var debitNotes = ChangeTracker.Entries<DebitNote>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();
        foreach (var debitNote in debitNotes)
        {
            if (string.IsNullOrEmpty(debitNote.Entity.ReferenceNumber))
            {
                debitNote.Entity.ReferenceNumber =
                    _autoNumberGenerator.GenerateAsync<DebitNote>(
                        this,
                        nameof(DebitNote.ReferenceNumber),
                        "DN",
                        5,
                        "-"
                    ).Result;

            }
        }
        var salesCreditNotes = ChangeTracker.Entries<SalesCreditNote>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();
        foreach (var salesCreditNote in salesCreditNotes)
        {
            if (string.IsNullOrEmpty(salesCreditNote.Entity.SalesCreditNoteNo))
            {
                salesCreditNote.Entity.SalesCreditNoteNo =
                    _autoNumberGenerator.GenerateAsync<SalesCreditNote>(
                        this,
                        nameof(SalesCreditNote.SalesCreditNoteNo),
                        "CN",
                        5,
                        "-"
                    ).Result;

            }
        }
        // Permits
        var inventoryPermits = ChangeTracker.Entries<InventoryPermit>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();
        foreach (var inventoryPermit in inventoryPermits)
        {
            if (string.IsNullOrEmpty(inventoryPermit.Entity.ReferenceNumber))
            {
                inventoryPermit.Entity.ReferenceNumber =
                    _autoNumberGenerator.GenerateAsync<InventoryPermit>(
                        this,
                        nameof(InventoryPermit.ReferenceNumber),
                        "IP",
                        5,
                        "-"
                    ).Result;

            }
        }

        // Recipes
        var recipes = ChangeTracker.Entries<Recipe>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();
        foreach (var recipe in recipes)
        {
            if (string.IsNullOrEmpty(recipe.Entity.ReferenceNumber))
            {
                recipe.Entity.ReferenceNumber =
                    _autoNumberGenerator.GenerateAsync<Recipe>(
                        this,
                        nameof(Recipe.ReferenceNumber),
                        "REC",
                        5,
                        "/"
                    ).Result;

            }
        }

        // Inventory Operation
        var inventoryOperations = ChangeTracker.Entries<InventoryOperation>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();
        foreach (var inventoryOperation in inventoryOperations)
        {
            if (string.IsNullOrEmpty(inventoryOperation.Entity.ReferenceNumber))
            {
                inventoryOperation.Entity.ReferenceNumber =
                    _autoNumberGenerator.GenerateAsync<Recipe>(
                        this,
                        nameof(InventoryOperation.ReferenceNumber),
                        "IOP",
                        8,
                        "/"
                    ).Result;

            }
        }

        //Inventory Stocktaking
        var inventoryStocktakings = ChangeTracker.Entries<InventoryStocktaking>()
            .Where(x => x.State == EntityState.Added)
            .ToList();

        foreach (var stocktaking in inventoryStocktakings)
        {
            if (string.IsNullOrEmpty(stocktaking.Entity.ReferenceNumber))
            {
                stocktaking.Entity.ReferenceNumber =
                    _autoNumberGenerator.GenerateAsync<InventoryStocktaking>(
                        this,
                        nameof(InventoryStocktaking.ReferenceNumber),
                        "IST",
                        8,
                        "-"
                    ).Result;
            }
        }

        // Reservation
        var reservations = ChangeTracker.Entries<Reservation>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();
        foreach (var reservation in reservations)
        {
            if (string.IsNullOrEmpty(reservation.Entity.ReferenceNumber))
            {
                reservation.Entity.ReferenceNumber =
                    _autoNumberGenerator.GenerateAsync<Reservation>(
                        this,
                        nameof(Reservation.ReferenceNumber),
                        "RES",
                        5,
                        "-"
                    ).Result;

            }
        }

        // MealReservation
        var mealReservations = ChangeTracker.Entries<MealReservation>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();
        foreach (var mealReservation in mealReservations)
        {
            if (string.IsNullOrEmpty(mealReservation.Entity.ReferenceNumber))
            {
                mealReservation.Entity.ReferenceNumber =
                    _autoNumberGenerator.GenerateAsync<MealReservation>(
                        this,
                        nameof(MealReservation.ReferenceNumber),
                        "MR",
                        6,
                        "-"
                    ).Result;

            }
        }

        // Attachments
        var attachments = ChangeTracker.Entries<Attachment>()
            .Where(x => x.State == EntityState.Added)
            .ToList();

        foreach (var attachment in attachments)
        {
            if (string.IsNullOrEmpty(attachment.Entity.ReferenceNumber))
            {
                attachment.Entity.ReferenceNumber =
                    _autoNumberGenerator.GenerateAsync<Attachment>(
                        this,
                        nameof(Attachment.ReferenceNumber),
                        "PDF",
                        6,
                        "-"
                    ).Result;
            }
        }

        // Employee
        var employees = ChangeTracker.Entries<Employee>()
            .Where(x => x.State == EntityState.Added)
            .ToList();

        foreach (var employee in employees)
        {
            if (string.IsNullOrEmpty(employee.Entity.ReferenceNumber))
            {
                employee.Entity.ReferenceNumber =
                    _autoNumberGenerator.GenerateAsync<Employee>(
                        this,
                        nameof(Employee.ReferenceNumber),
                        "EMP",
                        6,
                        "-"
                    ).Result;
            }
        }

        // Shift
        var shifts = ChangeTracker.Entries<Shift>()
            .Where(x => x.State == EntityState.Added)
            .ToList();

        foreach (var shift in shifts)
        {
            if (string.IsNullOrEmpty(shift.Entity.ReferenceNumber))
            {
                shift.Entity.ReferenceNumber =
                    _autoNumberGenerator.GenerateAsync<Shift>(
                        this,
                        nameof(Shift.ReferenceNumber),
                        "SFT",
                        4,
                        "-"
                    ).Result;
            }
        }

        // CompanyDocument
        var companyDocuments = ChangeTracker.Entries<CompanyDocument>()
            .Where(x => x.State == EntityState.Added)
            .ToList();

        foreach (var companyDocument in companyDocuments)
        {
            if (string.IsNullOrEmpty(companyDocument.Entity.ReferenceNumber))
            {
                companyDocument.Entity.ReferenceNumber =
                    _autoNumberGenerator.GenerateAsync<CompanyDocument>(
                        this,
                        nameof(CompanyDocument.ReferenceNumber),
                        "CD",
                        5,
                        "-"
                    ).Result;
            }
        }

        // ResignationRequest
        var resignationRequests = ChangeTracker.Entries<ResignationRequest>()
            .Where(x => x.State == EntityState.Added)
            .ToList();

        foreach (var resignationRequest in resignationRequests)
        {
            if (string.IsNullOrEmpty(resignationRequest.Entity.ReferenceNumber))
            {
                resignationRequest.Entity.ReferenceNumber =
                    _autoNumberGenerator.GenerateAsync<ResignationRequest>(
                        this,
                        nameof(ResignationRequest.ReferenceNumber),
                        "RR",
                        5,
                        "-"
                    ).Result;
            }
        }

        // HRTrustReceipt
        var hrTrustReceipts = ChangeTracker.Entries<HRTrustReceipt>()
            .Where(x => x.State == EntityState.Added)
            .ToList();

        foreach (var hrTrustReceipt in hrTrustReceipts)
        {
            if (string.IsNullOrEmpty(hrTrustReceipt.Entity.ReferenceNumber))
            {
                hrTrustReceipt.Entity.ReferenceNumber =
                    _autoNumberGenerator.GenerateAsync<HRTrustReceipt>(
                        this,
                        nameof(HRTrustReceipt.ReferenceNumber),
                        "HTR",
                        5,
                        "-"
                    ).Result;
            }
        }

        // EmploymentApplication
        var employmentApplications = ChangeTracker.Entries<EmploymentApplication>()
            .Where(x => x.State == EntityState.Added)
            .ToList();

        foreach (var employmentApplication in employmentApplications)
        {
            if (string.IsNullOrEmpty(employmentApplication.Entity.ReferenceNumber))
            {
                employmentApplication.Entity.ReferenceNumber =
                    _autoNumberGenerator.GenerateAsync<EmploymentApplication>(
                        this,
                        nameof(EmploymentApplication.ReferenceNumber),
                        "EA",
                        5,
                        "-"
                    ).Result;
            }
        }

        // BankDocument
        var bankDocuments = ChangeTracker.Entries<BankDocument>()
            .Where(x => x.State == EntityState.Added)
            .ToList();

        foreach (var bankDocument in bankDocuments)
        {
            if (string.IsNullOrEmpty(bankDocument.Entity.ReferenceNumber))
            {
                bankDocument.Entity.ReferenceNumber =
                    _autoNumberGenerator.GenerateAsync<BankDocument>(
                        this,
                        nameof(BankDocument.ReferenceNumber),
                        "BD",
                        5,
                        "-"
                    ).Result;
            }
        }

        // InsuranceRecord
        var insuranceRecords = ChangeTracker.Entries<InsuranceRecord>()
            .Where(x => x.State == EntityState.Added)
            .ToList();

        foreach (var insuranceRecord in insuranceRecords)
        {
            if (string.IsNullOrEmpty(insuranceRecord.Entity.ReferenceNumber))
            {
                insuranceRecord.Entity.ReferenceNumber =
                    _autoNumberGenerator.GenerateAsync<InsuranceRecord>(
                        this,
                        nameof(InsuranceRecord.ReferenceNumber),
                        "IR",
                        5,
                        "-"
                    ).Result;
            }
        }

        // SalarySlip
        var salarySlips = ChangeTracker.Entries<SalarySlip>()
            .Where(x => x.State == EntityState.Added)
            .ToList();

        foreach (var salarySlip in salarySlips)
        {
            if (string.IsNullOrEmpty(salarySlip.Entity.ReferenceNumber))
            {
                salarySlip.Entity.ReferenceNumber =
                    _autoNumberGenerator.GenerateAsync<SalarySlip>(
                        this,
                        nameof(SalarySlip.ReferenceNumber),
                        "SS",
                        5,
                        "-"
                    ).Result;
            }
        }

        // LeaveApplication
        var leaveApplications = ChangeTracker.Entries<LeaveApplication>()
            .Where(x => x.State == EntityState.Added)
            .ToList();

        foreach (var leaveApplication in leaveApplications)
        {
            if (string.IsNullOrEmpty(leaveApplication.Entity.ReferenceNumber))
            {
                leaveApplication.Entity.ReferenceNumber =
                    _autoNumberGenerator.GenerateAsync<LeaveApplication>(
                        this,
                        nameof(LeaveApplication.ReferenceNumber),
                        "LA",
                        5,
                        "-"
                    ).Result;
            }
        }
    }

}