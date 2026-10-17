using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.ClientProfile;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.CustomerProfile;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.DetailAccount;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.HeaderAccount;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.MainAccount;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.RootAccount;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.Transaction;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.TransactionEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.TransactionType;
using SmartAndPro.ERP.Application.Common.Dtos.Assets;
using SmartAndPro.ERP.Application.Common.Dtos.Attachments;
using SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization;
using SmartAndPro.ERP.Application.Common.Dtos.Bills.Accounts;
using SmartAndPro.ERP.Application.Common.Dtos.Bills.Exchange;
using SmartAndPro.ERP.Application.Common.Dtos.Bills.Sale;
using SmartAndPro.ERP.Application.Common.Dtos.GeneralBankAccount;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BankAccount;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BatchDetails;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BatchStatus;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BatchUnitStatus;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BranchDetails;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BranchStatus;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.DetailPurchaseClass;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.DetailSalesClass;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.HeaderPurchaseClass;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.HeaderSalesClass;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainPurchaseClass;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainSalesClass;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.RootPurchaseClass;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.RootSalesClass;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyDetails;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyPhone;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyPolicy;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyStatus;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ExpensesCenter;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.FinancePeriods;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.InventoryOperation.InventoryOperation;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.InventoryOperation.OperationEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.MeasurementUnits;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ProductUnits;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ProductUnitStatus;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.Stocktaking.InventoryStocktaking;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.Stocktaking.StocktakingEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.StagnantInventory;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.StoreDetails;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.StoreStatus;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.SupplierDetails;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.SupplierStatus;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Purchase;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale;
using SmartAndPro.ERP.Application.Common.Dtos.MealReservation.MealReservation;
using SmartAndPro.ERP.Application.Common.Dtos.MealReservation.MealReservationEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Notes.DebitNoteAccount;
using SmartAndPro.ERP.Application.Common.Dtos.Notes.DebitNotes;
using SmartAndPro.ERP.Application.Common.Dtos.Notes.SalesCreditNote;
using SmartAndPro.ERP.Application.Common.Dtos.Notes.SalesCreditNoteItem;
using SmartAndPro.ERP.Application.Common.Dtos.Notifications;
using SmartAndPro.ERP.Application.Common.Dtos.PdfTemplateConfiguration;
using SmartAndPro.ERP.Application.Common.Dtos.Permits.InventoryPermit;
using SmartAndPro.ERP.Application.Common.Dtos.Permits.InventoryPermitEntry;
using SmartAndPro.ERP.Application.Common.Dtos.ReceiptConnections;
using SmartAndPro.ERP.Application.Common.Dtos.Receipts.PaycheckReceiptAccountResponse;
using SmartAndPro.ERP.Application.Common.Dtos.Receipts.PaycheckReceipts;
using SmartAndPro.ERP.Application.Common.Dtos.Receipts.PaymentReceiptAccountResponse;
using SmartAndPro.ERP.Application.Common.Dtos.Receipts.PaymentReceipts;
using SmartAndPro.ERP.Application.Common.Dtos.Recipes.Recipe;
using SmartAndPro.ERP.Application.Common.Dtos.Recipes.RecipeEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Reports.SupplierReport;
using SmartAndPro.ERP.Application.Common.Dtos.RequestForQuotations;
using SmartAndPro.ERP.Application.Common.Dtos.RequestForQuotations.RequestForQuotationItems;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.Hotel;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.Reservation;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationCompany;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationMeal;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationPrintTableTemplate;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationRoomEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationSeason;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationService;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationServiceEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationSupplier;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationView;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.RoomType;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.SalesRepresentative;
using SmartAndPro.ERP.Application.Common.Dtos.Return.Purchase;
using SmartAndPro.ERP.Application.Common.Dtos.Return.Sale;
using SmartAndPro.ERP.Application.Common.Dtos.SystemSetting;
using SmartAndPro.ERP.Application.Common.Dtos.Treasuries;
using SmartAndPro.ERP.Application.Common.Dtos.UserNotifications;
using SmartAndPro.ERP.Application.Services.Auth.Security;
using SmartAndPro.ERP.Domains.Entities;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Assets;
using SmartAndPro.ERP.Domains.Entities.Auth.Authorization;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using SmartAndPro.ERP.Domains.Entities.Bills;
using SmartAndPro.ERP.Domains.Entities.Bills.Exchange;
using SmartAndPro.ERP.Domains.Entities.Bills.Sales;
using SmartAndPro.ERP.Domains.Entities.GeneralBankAccounts;
using SmartAndPro.ERP.Domains.Entities.Inventory;
using SmartAndPro.ERP.Domains.Entities.Inventory.StagnantInventoryManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.InventoryOperation;
using SmartAndPro.ERP.Domains.Entities.Inventory.PermitManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.Stocktaking;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.SupplierManagement;
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
using SmartAndPro.ERP.Domains.Entities.Return.Purchase;
using SmartAndPro.ERP.Domains.Entities.Return.Sale;
using SmartAndPro.ERP.Domains.Entities.Treasuries;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Domains.Models.InputModel;
using SmartAndPro.ERP.Domains.Models.InputModel.Report;
using SmartAndPro.ERP.Domains.Models.OutputModel;
using SmartAndPro.ERP.Domains.ValueObjects;
using SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;
using SmartAndPro.ERP.Domains.Entities.PublicBooking;
using SmartAndPro.ERP.Application.Common.Dtos.HotelAllotmentInvoices;
using SmartAndPro.ERP.Domains.Entities.HotelAllotmentInvoices;
using SmartAndPro.ERP.Application.Common.Dtos.HotelAllotmentInvoices.HotelAllotmentRooms;
using SmartAndPro.ERP.Application.Common.Dtos.PaymentSchedules;
using SmartAndPro.ERP.Domains.Entities.Payment_Schedules;
using SmartAndPro.ERP.Application.Common.Dtos.PurchaseQuotations;
using SmartAndPro.ERP.Domains.Entities.PurchaseQuotations;
using SmartAndPro.ERP.Application.Common.Dtos.PurchaseQuotations.PurchaseQuotationItems;

namespace SmartAndPro.ERP.Application.Common.AutoMapper;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // INVENTORY
        CreateMap<BatchUnitStatusPostDto, BatchUnitStatus>();
        CreateMap<BatchUnitStatusPutDto, BatchUnitStatus>();
        CreateMap<BatchUnitStatus, BatchUnitStatusResponseDto>();

        CreateMap<ProductUnitStatusPostDto, ProductUnitStatus>();
        CreateMap<ProductUnitStatusPutDto, ProductUnitStatus>();
        CreateMap<ProductUnitStatus, ProductUnitStatusResponseDto>();

        CreateMap<BatchStatusPostDto, BatchStatus>();
        CreateMap<BatchStatusPutDto, BatchStatus>();
        CreateMap<BatchStatus, BatchStatusResponseDto>();

        CreateMap<MeasurementUnitPostDto, MeasurementUnit>();
        CreateMap<MeasurementUnitPutDto, MeasurementUnit>();
        CreateMap<MeasurementUnit, MeasurementUnitResponseDto>();


        CreateMap<ProductUnit, ProductUnitResponseDto>()
            .ForMember(dest => dest.MainPurchaseClass, opt => opt.MapFrom(src => src.MainPurchaseClass))
            .ForMember(dst => dst.MainSalesClass, opt => opt.MapFrom(src => src.MainSalesClass))
            .ForMember(dest => dest.StoreDetails, opt => opt.MapFrom(src => src.StoreDetails));
        CreateMap<ProductUnitPostDto, ProductUnit>();
        CreateMap<ProductUnitPutDto, ProductUnit>();
        CreateMap<ProductUnitResponseDto, ProductUnit>();

        CreateMap<ProductBatchDetails, BatchDetailsResponseDto>()
            .ForMember(dest => dest.BatchStatus, opt => opt.MapFrom(src => src.BatchStatus))
            .ForMember(dest => dest.ProductBatchToUnitAssignments,
                opt => opt.MapFrom(src => src.ProductBatchToUnitAssignments));

        CreateMap<BatchDetailsPutDto, ProductBatchDetails>();
        CreateMap<BatchDetailsPostDto, ProductBatchDetails>();

        CreateMap<ProductBatchToUnitAssignmentsDto, ProductBatchToUnitAssignments>();
        CreateMap<ProductBatchToUnitAssignments, ProductBatchToUnitAssignmentsResponseDto>()
            .ForMember(dest => dest.BatchUnitStatus, opt => opt.MapFrom(src => src.BatchUnitStatus));

        CreateMap<StoreStatusPostDto, StoreStatus>();
        CreateMap<StoreStatusPutDto, StoreStatus>();
        CreateMap<StoreStatus, StoreStatusResponseDto>();

        CreateMap<CompanyStatusPostDto, CompanyStatus>();
        CreateMap<CompanyStatusPutDto, CompanyStatus>();
        CreateMap<CompanyStatus, CompanyStatusResponseDto>();

        CreateMap<StoreDetailsPostDto, StoreDetails>()
            .ForMember(dest => dest.StoreType, opt => opt.MapFrom(src => Enum.Parse<StoreType>(src.StoreType)));
        CreateMap<StoreDetailsPutDto, StoreDetails>()
            .ForMember(dest => dest.StoreType, opt => opt.MapFrom(src => Enum.Parse<StoreType>(src.StoreType)));


        CreateMap<StoreDetails, StoreDetailsResponseDto>()
            .ForMember(dest => dest.StoreStatus, opt => opt.MapFrom(src => src.StoreStatus))
            .ForMember(dest => dest.MainAccount, opt => opt.MapFrom(src => src.MainAccount))
            .ForMember(dest => dest.StoreType, opt => opt.MapFrom(src => src.StoreType.ToString()));

        CreateMap<SupplierDetailsPostDto, SupplierDetails>()
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => (Address)(src.Address ?? string.Empty)));
        CreateMap<SupplierDetailsPutDto, SupplierDetails>()
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => (Address)(src.Address ?? string.Empty)));
        CreateMap<SupplierDetails, SupplierDetailsResponseDto>()
            .ForMember(dest => dest.SupplierStatus, opt => opt.MapFrom(src => src.SupplierStatus))
            .ForMember(dest => dest.MainAccount, opt => opt.MapFrom(src => src.MainAccount));



        CreateMap<SupplierStatusPostDto, SupplierStatus>();
        CreateMap<SupplierStatusPutDto, SupplierStatus>();
        CreateMap<SupplierStatus, SupplierStatusResponseDto>();

        CreateMap<CompanyDetailsPostDto, CompanyDetails>()
            .ForMember(dest => dest.Logo, opt => opt.MapFrom(src => SafeBase64ToBytes(src.Logo)))
            .ForMember(dest => dest.Stamp, opt => opt.MapFrom(src => SafeBase64ToBytes(src.Stamp)))
            .ForMember(dest => dest.LogoUrl, opt => opt.MapFrom(src => ResolveInputImageUrl(src.LogoUrl, src.Logo)))
            .ForMember(dest => dest.StampUrl, opt => opt.MapFrom(src => ResolveInputImageUrl(src.StampUrl, src.Stamp)))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => (Address)(src.Address ?? string.Empty)))
            .ForMember(dest => dest.ArabicAddress,
                opt => opt.MapFrom(src => (Address)(src.ArabicAddress ?? string.Empty)))
            .ForMember(dest=>dest.Policies,opt=>opt.Ignore())
            .ForMember(dest => dest.Phones, opt => opt.Ignore())
            .ForMember(dest=>dest.BankAccounts , opt=>opt.Ignore());
          


        CreateMap<CompanyDetailsPutDto, CompanyDetails>()
            .ForMember(dest => dest.Logo, opt => opt.MapFrom(src => SafeBase64ToBytes(src.Logo)))
            .ForMember(dest => dest.Stamp, opt => opt.MapFrom(src => SafeBase64ToBytes(src.Stamp)))
            .ForMember(dest => dest.LogoUrl, opt => opt.MapFrom(src => ResolveInputImageUrl(src.LogoUrl, src.Logo)))
            .ForMember(dest => dest.StampUrl, opt => opt.MapFrom(src => ResolveInputImageUrl(src.StampUrl, src.Stamp)))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => (Address)(src.Address ?? string.Empty)))
            .ForMember(dest => dest.ArabicAddress,
                opt => opt.MapFrom(src => (Address)(src.ArabicAddress ?? string.Empty)))
            .ForMember(dest => dest.Policies, opt => opt.Ignore())
            .ForMember(dest=>dest.Phones , opt => opt.Ignore())
            .ForMember(dest=>dest.BankAccounts , opt=>opt.Ignore());

        CreateMap<CompanyDetails, CompanyDetailsResponseDto>()
            .ForMember(dest => dest.CompanyStatus, opt => opt.MapFrom(src => src.CompanyStatus))
            .ForMember(dest => dest.LogoUrl, opt => opt.MapFrom(src => ResolveImageUrl(src.LogoUrl, src.Logo)))
            .ForMember(dest => dest.Policies, opt => opt.MapFrom(src => src.Policies))
            .ForMember(dest => dest.Phones, opt => opt.MapFrom(src => src.Phones))
            .ForMember(dest => dest.StampUrl, opt => opt.MapFrom(src => ResolveImageUrl(src.StampUrl, src.Stamp)))
            .ForMember(dest=>dest.BankAccounts , opt=>opt.MapFrom(src=>src.BankAccounts));


        CreateMap<CompanyPolicyPostDto, CompanyPolicy>();
        CreateMap<CompanyPolicyPutDto, CompanyPolicy>();
        CreateMap<CompanyPolicy, CompanyPolicyResponseDto>();

        //CompanyPhone
        CreateMap<CompanyPhonePostDto, CompanyPhone>();
        CreateMap<CompanyPhonePutDto, CompanyPhone>();
        CreateMap<CompanyPhone, CompanyPhoneResponseDto>();

        CreateMap<BankAccountPostDto, BankAccount>();
        CreateMap<BankAccountPutDto, BankAccount>();
        CreateMap<BankAccount, BankAccountResponseDto>();

        CreateMap<BranchStatusPostDto, BranchStatus>();
        CreateMap<BranchStatusPutDto, BranchStatus>();
        CreateMap<BranchStatus, BranchStatusResponseDto>();

        CreateMap<BranchDetailsPostDto, BranchDetails>();
        CreateMap<BranchDetailsPutDto, BranchDetails>();
        CreateMap<BranchDetails, BranchDetailsResponseDto>()
            .ForMember(dest => dest.BranchStatus, opt => opt.MapFrom(src => src.BranchStatus));


        // AUTH
        CreateMap<UserDTO, User>()
            .ForMember(d => d.UserRoles, o => o.MapFrom(s => s.UserRoles));


        CreateMap<RegisterUserDTO, User>()
            .ForMember(d => d.CreatedAt, o => o.MapFrom(s => DateTime.UtcNow))
            .ForMember(d => d.IsActive, o => o.MapFrom(s => true))
            .ForMember(d => d.Password, o => o.MapFrom(s => SecurityService.ComputeHMAC(s.Password, "UserID")))
            .ForMember(d => d.UserRoles, o => o.MapFrom(s => s.UserRoles))
            .ForMember(d=>d.ProfilePicture,o=>o.MapFrom(s=>!string.IsNullOrEmpty(s.ProfilePicture)
            ? Convert.FromBase64String(s.ProfilePicture):null));
       
        CreateMap<User, UserAccountDto>()
            .ForMember(d => d.UserRoles, o => o.MapFrom(s => s.UserRoles))
            .ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src =>src.ProfilePicture!=null
            ?Convert.ToBase64String(src.ProfilePicture):null));


        CreateMap<RoleDto, Role>().ReverseMap()
            .ForMember(d => d.RolePermissions, o => o.MapFrom(s => s.RolePermissions))
            .ForMember(d => d.DomainDetails, o => o.MapFrom(s => s.DomainDetails));

        CreateMap<RolePermission, RolePermissionDto>()
            .ForMember(d => d.Permission, o => o.MapFrom(s => s.Permission));


        CreateMap<RolePermissionDto, RolePermission>()
            .ForMember(d => d.Permission, o => o.MapFrom(s => s.Permission));


        CreateMap<DomainDetailsDto, DomainDetails>();
        CreateMap<DomainDetails, DomainDetailsDto>();

        CreateMap<PermissionDto, Permission>()
            .ForMember(d => d.DomainDetails, o => o.MapFrom(s => s.DomainDetails));
        CreateMap<Permission, PermissionDto>()
            .ForMember(d => d.DomainDetails, o => o.MapFrom(s => s.DomainDetails));
        CreateMap<UserRole, UserRoleDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role))
            .ReverseMap();

        // ACCOUNTING
        CreateMap<RootAccountPostDto, RootAccount>();
        CreateMap<RootAccountPutDto, RootAccount>();
        CreateMap<RootAccount, RootAccountResponseDto>()
            .ForMember(x => x.HeaderAccounts, opt => opt.MapFrom(src => src.HeaderAccounts));


        CreateMap<HeaderAccountPostDto, HeaderAccount>();
        CreateMap<HeaderAccountPutDto, HeaderAccount>();
        CreateMap<HeaderAccount, HeaderAccountResponseDto>()
            .ForMember(x => x.ClientProfile, opt => opt.MapFrom(src => src.ClientProfile))
            .ForMember(x => x.DetailAccounts, opt => opt.MapFrom(src => src.DetailAccounts));

        CreateMap<DetailAccountPostDto, DetailAccount>();
        CreateMap<DetailAccountPutDto, DetailAccount>();
        CreateMap<DetailAccount, DetailAccountResponseDto>()
            .ForMember(x => x.ClientProfile, opt => opt.MapFrom(src => src.ClientProfile))
            .ForMember(x => x.MainAccounts, opt => opt.MapFrom(src => src.MainAccounts));

        CreateMap<MainAccountPostDto, MainAccount>();
        CreateMap<MainAccountPutDto, MainAccount>();
        CreateMap<MainAccount, MainAccountResponseDto>()
            .ForMember(x => x.ClientProfile, opt => opt.MapFrom(src => src.ClientProfile));


        CreateMap<RootPurchaseClassPostDto, RootPurchaseClass>();
        CreateMap<RootPurchaseClassPutDto, RootPurchaseClass>();
        CreateMap<RootPurchaseClass, RootPurchaseClassResponseDto>()
            .ForMember(x => x.HeaderPurchaseClasses, opt => opt.MapFrom(src => src.HeaderPurchaseClasses));

        CreateMap<HeaderPurchaseClassPostDto, HeaderPurchaseClass>();
        CreateMap<HeaderPurchaseClassPutDto, HeaderPurchaseClass>();
        CreateMap<HeaderPurchaseClass, HeaderPurchaseClassResponseDto>()
            .ForMember(x => x.DetailPurchaseClasses, opt => opt.MapFrom(src => src.DetailPurchaseClasses))
            .ForMember(x => x.ClientProfile, opt => opt.MapFrom(src => src.ClientProfile));

        CreateMap<DetailPurchaseClassPostDto, DetailPurchaseClass>();
        CreateMap<DetailPurchaseClassPutDto, DetailPurchaseClass>();
        CreateMap<DetailPurchaseClass, DetailPurchaseClassResponseDto>()
            .ForMember(x => x.MainPurchaseClasses, opt => opt.MapFrom(src => src.MainPurchaseClasses))
            .ForMember(x => x.ClientProfile, opt => opt.MapFrom(src => src.ClientProfile));

        CreateMap<MainPurchaseClassPostDto, MainPurchaseClass>();
        CreateMap<MainPurchaseClassPutDto, MainPurchaseClass>();
        CreateMap<MainPurchaseClass, MainPurchaseClassResponseDto>()
            .ForMember(x => x.ClientProfile, opt => opt.MapFrom(src => src.ClientProfile))
            .ForMember(x => x.MeasurementUnit, opt => opt.MapFrom(src => src.MeasurementUnit));


        CreateMap<RootSalesClassPostDto, RootSalesClass>();
        CreateMap<RootSalesClassPutDto, RootSalesClass>();
        CreateMap<RootSalesClass, RootSalesClassResponseDto>()
            .ForMember(x => x.HeaderSalesClasses, opt => opt.MapFrom(src => src.HeaderSalesClasses));

        CreateMap<HeaderSalesClassPostDto, HeaderSalesClass>();
        CreateMap<HeaderSalesClassPutDto, HeaderSalesClass>();
        CreateMap<HeaderSalesClass, HeaderSalesClassResponseDto>()
            .ForMember(x => x.DetailSalesClasses, opt => opt.MapFrom(src => src.DetailSalesClasses))
            .ForMember(x => x.ClientProfile, opt => opt.MapFrom(src => src.ClientProfile));

        CreateMap<DetailSalesClassPostDto, DetailSalesClass>();
        CreateMap<DetailSalesClassPutDto, DetailSalesClass>();
        CreateMap<DetailSalesClass, DetailSalesClassResponseDto>()
            .ForMember(x => x.MainSalesClasses, opt => opt.MapFrom(src => src.MainSalesClasses))
            .ForMember(x => x.ClientProfile, opt => opt.MapFrom(src => src.ClientProfile));

        CreateMap<MainSalesClassPostDto, MainSalesClass>()
            .ForMember(x => x.ClassType, opt => opt.MapFrom(src => src.ClassType));
        CreateMap<MainSalesClassPutDto, MainSalesClass>()
            .ForMember(x => x.ClassType, opt => opt.MapFrom(src => src.ClassType));
        CreateMap<MainSalesClass, MainSalesClassResponseDto>()
            .ForMember(x => x.ClientProfile, opt => opt.MapFrom(src => src.ClientProfile))
            .ForMember(x => x.ClassType, opt => opt.MapFrom(src => src.ClassType))
            .ForMember(x => x.MeasurementUnit, opt => opt.MapFrom(src => src.MeasurementUnit));

        CreateMap<ClassType, string>().ConvertUsing(x => x.ToString());
        CreateMap<string, ClassType>().ConvertUsing(x => Enum.Parse<ClassType>(x));

        CreateMap<ClientProfilePostDto, ClientProfile>();
        CreateMap<ClientProfilePutDto, ClientProfile>();
        CreateMap<ClientProfile, ClientProfileResponseDto>();

        CreateMap<CustomerProfilePostDto, CustomerProfile>()
            .ForMember(dest => dest.Departments, opt => opt.Ignore());
        CreateMap<CustomerProfilePutDto, CustomerProfile>()
            .ForMember(dest => dest.Departments, opt => opt.Ignore());
        CreateMap<CustomerProfile, CustomerProfileResponseDto>()
            .ForMember(dest => dest.MainAccount, opt => opt.MapFrom(src => src.MainAccount));

        CreateMap<CustomerDepartment, CustomerDepartmentDto>().ReverseMap();


        CreateMap<TransactionTypePostDto, TransactionType>();
        CreateMap<TransactionTypePutDto, TransactionType>();
        CreateMap<TransactionType, TransactionTypeResponseDto>();


        CreateMap<TransactionPostDto, Transaction>()
            .ForMember(x => x.TransactionEntries, opt => opt.MapFrom(src => src.TransactionEntries));
        CreateMap<TransactionPutDto, Transaction>();
        CreateMap<Transaction, TransactionResponseDto>()
            .ForMember(x => x.ClientProfile, opt => opt.MapFrom(src => src.ClientProfile))
            .ForMember(x => x.TransactionEntries, opt => opt.MapFrom(src => src.TransactionEntries))
            .ForMember(x => x.Attachments, opt => opt.MapFrom(src => src.Attachments))
            .ForMember(x => x.TransactionType, opt => opt.MapFrom(src => src.TransactionType));
        CreateMap<Transaction, TransactionChildResponseDto>()
            .ForMember(x => x.ClientProfile, opt => opt.MapFrom(src => src.ClientProfile))
            .ForMember(x => x.Attachments, opt => opt.MapFrom(src => src.Attachments))
            .ForMember(x => x.TransactionType, opt => opt.MapFrom(src => src.TransactionType));


        CreateMap<TransactionEntryPostDto, TransactionEntry>();

        CreateMap<TransactionEntryPutDto, TransactionEntry>();

        CreateMap<TransactionEntry, TransactionEntryResponseDto>()
            .ForMember(x => x.MainAccount, opt => opt.MapFrom(src => src.MainAccount))
            .ForMember(x => x.Transaction, opt => opt.MapFrom(src => src.Transaction));
        CreateMap<TransactionEntry, TransactionEntryChildResponseDto>()
            .ForMember(x => x.MainAccount, opt => opt.MapFrom(src => src.MainAccount));


        // INVOICE
        CreateMap<PurchaseInvoicePostDto, PurchaseInvoice>();
        CreateMap<PurchaseInvoicePutDto, PurchaseInvoice>();

        CreateMap<PurchaseInvoice, PurchaseInvoiceDto>()
            .ForMember((x) => x.SupplierDetails, opt => opt.MapFrom(src => src.SupplierDetails))
            .ForMember(x => x.Attachments, opt => opt.MapFrom(src => src.Attachments))
            .ReverseMap();

        CreateMap<PurchaseItemPostDto, PurchaseItem>();
        CreateMap<PurchaseItem, PurchaseItemDto>()
            .ForMember((x) => x.MainPurchaseClass, opt => opt.MapFrom(src => src.MainPurchaseClass))
            .ForMember((x) => x.StoreDetails, opt => opt.MapFrom(src => src.StoreDetails))
            .ReverseMap();

        CreateMap<SaleInvoicePostDto, SaleInvoice>()
            .ForMember(dest => dest.IsQuotation, opt => opt.MapFrom(src => src.IsQuotation ?? false));
        CreateMap<SaleInvoicePutDto, SaleInvoice>()
            .ForMember(dest => dest.IsFrontal, opt => opt.Condition(src => src.IsFrontal.HasValue));

        CreateMap<SaleInvoice, SaleInvoiceDto>()
            .ForMember((x) => x.CustomerProfile, opt => opt.MapFrom(src => src.CustomerProfile))
            .ForMember(x => x.Attachments, opt => opt.MapFrom(src => src.Attachments))
            .ForMember(dest => dest.ReservationIds, opt => opt.MapFrom(src => src.Reservations.Select(r => r.Id).ToList()))
            .ForMember(dest => dest.Reservations, opt => opt.MapFrom(src => src.Reservations))
            .ReverseMap();

        CreateMap<SaleItemPostDto, SaleItem>();
        CreateMap<SaleItem, SaleItemDto>()
            .ForMember((x) => x.MainSalesClass, opt => opt.MapFrom(src => src.MainSalesClass))
            .ReverseMap();

        // Return
        CreateMap<PurchaseReturnPostDto, PurchaseReturn>();
        CreateMap<PurchaseReturnPutDto, PurchaseReturn>();

        CreateMap<PurchaseReturn, PurchaseReturnDto>()
            .ForMember((x) => x.SupplierDetails, opt => opt.MapFrom(src => src.SupplierDetails))
            .ForMember(x => x.Attachments, opt => opt.MapFrom(src => src.Attachments))
            .ReverseMap();

        CreateMap<PurchaseReturnItemPostDto, PurchaseReturnItem>();
        CreateMap<PurchaseReturnItem, PurchaseReturnItemDto>()
            .ForMember((x) => x.MainPurchaseClass, opt => opt.MapFrom(src => src.MainPurchaseClass))
            .ForMember((x) => x.StoreDetails, opt => opt.MapFrom(src => src.StoreDetails))
            .ReverseMap();

        CreateMap<SalesReturnPostDto, SalesReturn>();
        CreateMap<SalesReturnPutDto, SalesReturn>();

        CreateMap<SalesReturn, SalesReturnDto>()
            .ForMember((x) => x.CustomerProfile, opt => opt.MapFrom(src => src.CustomerProfile))
            .ForMember(x => x.Attachments, opt => opt.MapFrom(src => src.Attachments))
            .ReverseMap();

        CreateMap<SalesReturnItemPostDto, SalesReturnItem>();
        CreateMap<SalesReturnItem, SalesReturnItemDto>()
            .ForMember((x) => x.MainSalesClass, opt => opt.MapFrom(src => src.MainSalesClass))
            .ReverseMap();

        // Bill
        CreateMap<Asset, AssetResponseDto>().ReverseMap();


        CreateMap<BillAccounts, BillAccountsDto>().ReverseMap();
        CreateMap<BillAccounts, BillAccountPostDto>().ReverseMap();
        CreateMap<BillAccounts, BillAccountsPutDto>().ReverseMap();

        CreateMap<BillOfExchange, BillOfExchangeDto>()
            .ForMember(x => x.BillAccounts, y => y.MapFrom(src => src.BillAccounts))
            .ReverseMap();
        CreateMap<BillOfExchangePostDto, BillOfExchange>()
            .ForMember(dest => dest.Asset, opt => opt.MapFrom(src => src.Asset != null ? src.Asset : null))
            .ForMember(x => x.BillAccounts, y => y.MapFrom(src => src.BillAccounts))
            .ReverseMap();
        CreateMap<BillOfExchange, BillOfExchangePutDto>()
            .ForMember(dest => dest.Asset, opt => opt.MapFrom(src => src.Asset != null ? src.Asset : null))
            .ForMember(x => x.BillAccounts, y => y.MapFrom(src => src.BillAccounts))
            .ReverseMap();
        CreateMap<BillOfExchange, BillOfExchangeResponseDto>()
             .ForMember(dest => dest.Asset, opt => opt.MapFrom(src => src.Asset != null ? src.Asset : null))
            .ForMember(dest => dest.AssetUrl, opt => opt.MapFrom(src => src.Asset != null ? src.Asset.URL : null))
            .ForMember(x => x.BillAccounts, y => y.MapFrom(src => src.BillAccounts))
            .ReverseMap();

        CreateMap<BillOfSales, BillOfSaleDto>()
            .ForMember(x => x.BillAccounts, y => y.MapFrom(src => src.BillAccounts))
            .ReverseMap();
        CreateMap<BillOfSales, BillOfSalePostDto>()
            .ForMember(x => x.BillAccounts, y => y.MapFrom(src => src.BillAccounts))
            .ReverseMap();
        CreateMap<BillOfSales, BillOfSalePutDto>()
            .ForMember(x => x.BillAccounts, y => y.MapFrom(src => src.BillAccounts))
            .ReverseMap();
        CreateMap<BillOfSales, BillOfSaleResponseDto>()
            .ForMember(x => x.BillAccounts, y => y.MapFrom(src => src.BillAccounts))
            .ReverseMap();

        CreateMap<TreasuryPostDto, Treasury>();
        CreateMap<TreasuryPutDto, Treasury>();
        CreateMap<Treasury, TreasuryResponseDto>();

        CreateMap<GeneralBankAccountPostDto, GeneralBankAccount>();
        CreateMap<GeneralBankAccountPutDto, GeneralBankAccount>();
        CreateMap<GeneralBankAccount, GeneralBankAccountResponseDto>();



        //RECEIPTS
        CreateMap<PaymentReceiptPostDto, PaymentReceipt>()
            .ForMember(x => x.PaymentReceiptAccounts, opt => opt.MapFrom(src => src.PaymentReceiptAccounts))
            .ForMember(x => x.ReceiptConnections, opt => opt.MapFrom(src => src.ReceiptConnections));
        CreateMap<PaymentReceiptPutDto, PaymentReceipt>()
            .ForMember(x => x.ReceiptConnections, opt => opt.MapFrom(src => src.ReceiptConnections));
        CreateMap<PaymentReceipt, PaymentReceiptResponseDto>()
            .ForMember(x => x.PaymentReceiptAccounts, opt => opt.MapFrom(src => src.PaymentReceiptAccounts))
            .ForMember(x => x.Attachments, opt => opt.MapFrom(src => src.Attachments))
            .ForMember(x => x.Treasury, opt => opt.MapFrom(src => src.Treasury))
            .ForMember(x => x.ReceiptConnections, opt => opt.MapFrom(src => src.ReceiptConnections));


        CreateMap<PaycheckReceiptPostDto, PaycheckReceipt>()
            .ForMember(x => x.PaycheckReceiptAccounts, opt => opt.MapFrom(src => src.PaycheckReceiptAccounts))
            .ForMember(x => x.ReceiptConnections, opt => opt.Ignore());
        CreateMap<PaycheckReceiptPutDto, PaycheckReceipt>()
            .ForMember(x => x.ReceiptConnections, opt => opt.MapFrom(src => src.ReceiptConnections));
        CreateMap<PaycheckReceipt, PaycheckReceiptResponseDto>()
            .ForMember(x => x.PaycheckReceiptAccounts, opt => opt.MapFrom(src => src.PaycheckReceiptAccounts))
            .ForMember(x => x.Attachments, opt => opt.MapFrom(src => src.Attachments))
            .ForMember(x => x.Treasury, opt => opt.MapFrom(src => src.Treasury))
            .ForMember(x => x.ReceiptConnections, opt => opt.MapFrom(src => src.ReceiptConnections));


        CreateMap<PaymentReceiptAccountPostDto, PaymentReceiptAccount>();
        CreateMap<PaymentReceiptAccountPutDto, PaymentReceiptAccount>();
        CreateMap<PaymentReceiptAccount, PaymentReceiptAccountResponseDto>()
            .ForMember(x => x.MainAccount, opt => opt.MapFrom(src => src.MainAccount));

        CreateMap<PaycheckReceiptAccountPostDto, PaycheckReceiptAccount>();
        CreateMap<PaycheckReceiptAccountPutDto, PaycheckReceiptAccount>();
        CreateMap<PaycheckReceiptAccount, PaycheckReceiptAccountResponseDto>()
            .ForMember(x => x.MainAccount, opt => opt.MapFrom(src => src.MainAccount));

        CreateMap<ReceiptConnectionPostDto, ReceiptConnection>();
        CreateMap<ReceiptConnectionPutDto, ReceiptConnection>();
        CreateMap<ReceiptConnection, ReceiptConnectionResponseDto>();

        //NOTES
        CreateMap<DebitNotePostDto, DebitNote>()
            .ForMember(x => x.DebitNoteAccounts, opt => opt.MapFrom(src => src.DebitNoteAccounts));
        CreateMap<DebitNotePutDto, DebitNote>()
            .ForMember(x => x.DebitNoteAccounts, opt => opt.MapFrom(src => src.DebitNoteAccounts));
        CreateMap<DebitNote, DebitNoteResponseDto>()
            .ForMember(x => x.DebitNoteAccounts, opt => opt.MapFrom(src => src.DebitNoteAccounts))
            .ForMember(x => x.Attachments, opt => opt.MapFrom(src => src.Attachments))
            .ForMember(x => x.SupplierDetails, opt => opt.MapFrom(src => src.SupplierDetails));

        CreateMap<SalesCreditNotePostDto, SalesCreditNote>()
            .ForMember(x => x.SalesCreditNoteItems, opt => opt.MapFrom(src => src.SalesCreditNoteItems));
        CreateMap<SalesCreditNotePutDto, SalesCreditNote>()
            .ForMember(x => x.SalesCreditNoteItems, opt => opt.MapFrom(src => src.SalesCreditNoteItems));
        CreateMap<SalesCreditNote, SalesCreditNoteResponseDto>()
            .ForMember(x => x.SalesCreditNoteItems, opt => opt.MapFrom(src => src.SalesCreditNoteItems))
            .ForMember(x => x.Attachments, opt => opt.MapFrom(src => src.Attachments))
            .ForMember(x => x.CustomerProfile, opt => opt.MapFrom(src => src.CustomerProfile))
            .ForMember(x => x.SaleInvoice, opt => opt.MapFrom(src => src.SaleInvoice));

        CreateMap<DebitNoteAccountPostDto, DebitNoteAccount>();
        CreateMap<DebitNoteAccountPutDto, DebitNoteAccount>();
        CreateMap<DebitNoteAccount, DebitNoteAccountResponseDto>()
            .ForMember(x => x.MainAccount, opt => opt.MapFrom(src => src.MainAccount));

        CreateMap<SalesCreditNoteItemPostDto, SalesCreditNoteItem>();
        CreateMap<SalesCreditNoteItemPutDto, SalesCreditNoteItem>();
        CreateMap<SalesCreditNoteItem, SalesCreditNoteItemResponseDto>()
            .ForMember(x => x.MainSalesClass, opt => opt.MapFrom(src => src.MainSalesClass));

        CreateMap<InventoryPermitPostDto, InventoryPermit>()
            .ForMember(x => x.PermitEntries, opt => opt.MapFrom(src => src.PermitEntries))
            .ForMember(dest => dest.PermitType, opt => opt.MapFrom(src => Enum.Parse<PermitType>(src.PermitType)))
            .ForMember(dest => dest.ShippingCost, opt => opt.MapFrom(src => src.ShippingCost ?? 0));
        CreateMap<InventoryPermitPutDto, InventoryPermit>()
            .ForMember(x => x.PermitEntries, opt => opt.MapFrom(src => src.PermitEntries))
            .ForMember(dest => dest.PermitType, opt => opt.MapFrom(src => Enum.Parse<PermitType>(src.PermitType)))
            .ForMember(dest => dest.ShippingCost, opt => opt.MapFrom(src => src.ShippingCost ?? 0));
        CreateMap<InventoryPermit, InventoryPermitResponseDto>()
            .ForMember(x => x.PermitEntries, opt => opt.MapFrom(src => src.PermitEntries))
            .ForMember(x => x.MainAccount, opt => opt.MapFrom(src => src.MainAccount))
            .ForMember(x => x.StoreDetails, opt => opt.MapFrom(src => src.StoreDetails))
            .ForMember(x => x.Attachments, opt => opt.MapFrom(src => src.Attachments));


        CreateMap<InventoryPermitEntryPostDto, PermitEntry>();
        CreateMap<InventoryPermitEntryPutDto, PermitEntry>();
        CreateMap<PermitEntry, InventoryPermitEntryResponseDto>()
            .ForMember(x => x.MainPurchaseClass, opt => opt.MapFrom(src => src.MainPurchaseClass))
            .ForMember(x => x.MainSalesClass, opt => opt.MapFrom(src => src.MainSalesClass));

        CreateMap<ExpensesCenterPostDto, ExpensesCenter>();
        CreateMap<ExpensesCenterPutDto, ExpensesCenter>();
        CreateMap<ExpensesCenter, ExpensesCenterResponseDto>();

        CreateMap<FinancePeriodsPostDto, FinancePeriods>();
        CreateMap<FinancePeriodsPutDto, FinancePeriods>();
        CreateMap<FinancePeriods, FinancePeriodsResponseDto>();

        CreateMap<RecipePostDto, Recipe>()
            .ForMember(x => x.RecipeEntries, opt => opt.MapFrom(src => src.RecipeEntries));
        CreateMap<RecipePutDto, Recipe>()
            .ForMember(x => x.RecipeEntries, opt => opt.MapFrom(src => src.RecipeEntries));
        CreateMap<Recipe, RecipeResponseDto>()
            .ForMember(x => x.MainSalesClass, opt => opt.MapFrom(src => src.MainSalesClass))
            .ForMember(x => x.RecipeEntries, opt => opt.MapFrom(src => src.RecipeEntries));

        CreateMap<RecipeEntryPostDto, RecipeEntry>();
        CreateMap<RecipeEntryPutDto, RecipeEntry>();
        CreateMap<RecipeEntry, RecipeEntryResponseDto>()
            .ForMember(x => x.MainPurchaseClass, opt => opt.MapFrom(src => src.MainPurchaseClass));


        CreateMap<InventoryOperationPostDto, InventoryOperation>()
            .ForMember(x => x.OperationEntries, opt => opt.MapFrom(src => src.OperationEntries))
            .ForMember(dest => dest.OperationSource, opt => opt.MapFrom(src => Enum.Parse<OperationSource>(src.OperationSource)));
        CreateMap<InventoryOperationPutDto, InventoryOperation>()
            .ForMember(x => x.OperationEntries, opt => opt.MapFrom(src => src.OperationEntries))
            .ForMember(dest => dest.OperationSource, opt => opt.MapFrom(src => Enum.Parse<OperationSource>(src.OperationSource)));
        CreateMap<InventoryOperation, InventoryOperationResponseDto>()
            .ForMember(x => x.OperationEntries, opt => opt.MapFrom(src => src.OperationEntries))
            .ForMember(x => x.PurchaseInvoice, opt => opt.MapFrom(src => src.PurchaseInvoice))
            .ForMember(x => x.InventoryPermit, opt => opt.MapFrom(src => src.InventoryPermit))
            .ForMember(x => x.PurchaseReturn, opt => opt.MapFrom(src => src.PurchaseReturn))
            .ForMember(x => x.SupplierDetails, opt => opt.MapFrom(src => src.SupplierDetails));

        CreateMap<OperationEntryPostDto, OperationEntry>();
        CreateMap<OperationEntryPutDto, OperationEntry>();
        CreateMap<OperationEntry, OperationEntryResponseDto>()
            .ForMember(x => x.MainPurchaseClass, opt => opt.MapFrom(src => src.MainPurchaseClass))
            .ForMember(x => x.StoreDetails, opt => opt.MapFrom(src => src.StoreDetails));

        CreateMap<Domains.Models.OutputModel.ItemDetails, Dtos.Invoice.Purchase.ItemDetails>();

        CreateMap<GetPurchaseInvoiceDetailsRequest, GetPurchaseInvoiceDetailsInputModel>();

        CreateMap<PurchaseInvoiceDetailsOutputModel, PurchaseInvoiceDetailsResponse>();

        CreateMap<GetSalesInvoiceDetailsRequest, GetSalesInvoiceDetailsInputModel>();
        CreateMap<SalesInvoiceDetailsOutputModel, SalesInvoiceDetailsResponse>();

        // VAT Mappings
        CreateMap<GetSalesVATDetailsRequest, GetSalesVATDetailsInputModel>();
        CreateMap<GetSalesVATDetailsRequest, GetSalesInvoiceDetailsInputModel>();

        //Stocktaking
        CreateMap<InventoryStocktakingPostDto, InventoryStocktaking>()
            .ForMember(x => x.StocktakingEntries, opt => opt.MapFrom(src => src.StocktakingEntries));
        CreateMap<InventoryStocktakingPutDto, InventoryStocktaking>()
            .ForMember(x => x.StocktakingEntries, opt => opt.MapFrom(src => src.StocktakingEntries));
        CreateMap<InventoryStocktaking, InventoryStocktakingResponseDto>()
            .ForMember(x => x.StocktakingEntries, opt => opt.MapFrom(src => src.StocktakingEntries))
            .ForMember(x => x.Attachments, opt => opt.MapFrom(src => src.Attachments))
            .ForMember(x => x.StoreDetails, opt => opt.MapFrom(src => src.StoreDetails));

        //StocktakingEntry
        CreateMap<StocktakingEntryPostDto, StocktakingEntry>();
        CreateMap<StocktakingEntryPutDto, StocktakingEntry>();
        CreateMap<StocktakingEntry, StocktakingEntryResponseDto>()
            .ForMember(x => x.MainPurchaseClass, opt => opt.MapFrom(src => src.MainPurchaseClass));

        //DamagedInventory
        CreateMap<Dtos.Inventory.DamagedInventory.DamagedInventoryPostDto, Domains.Entities.Inventory.DamagedInventoryManagement.DamagedInventory>();
        CreateMap<Dtos.Inventory.DamagedInventory.DamagedInventoryPutDto, Domains.Entities.Inventory.DamagedInventoryManagement.DamagedInventory>();
        CreateMap<Domains.Entities.Inventory.DamagedInventoryManagement.DamagedInventory, Dtos.Inventory.DamagedInventory.DamagedInventoryResponseDto>()
            .ForMember(x => x.MainPurchaseClassName, opt => opt.MapFrom(src => src.MainPurchaseClass != null ? src.MainPurchaseClass.ArabicName : null))
            .ForMember(x => x.MainSalesClassName, opt => opt.MapFrom(src => src.MainSalesClass != null ? src.MainSalesClass.ArabicName : null))
            .ForMember(x => x.StoreName, opt => opt.MapFrom(src => src.StoreDetails != null ? src.StoreDetails.ArabicName : null));

        //Reservation
        CreateMap<SalesRepresentativePostDto, SalesRepresentative>();
        CreateMap<SalesRepresentativePutDto, SalesRepresentative>();
        CreateMap<SalesRepresentative, SalesRepresentativeResponseDto>()
            .ForMember(x => x.MainAccount, opt => opt.MapFrom(x => x.MainAccount));

        CreateMap<HotelPostDto, Hotel>()
            .ForMember(dest => dest.Logo, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Logo) ? null : Convert.FromBase64String(src.Logo)));
        CreateMap<HotelPutDto, Hotel>()
            .ForMember(dest => dest.Logo, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Logo) ? null : Convert.FromBase64String(src.Logo)));
        CreateMap<Hotel, HotelResponseDto>()
            .ForMember(x => x.MainAccount, opt => opt.MapFrom(x => x.MainAccount))
            .ForMember(dest => dest.LogoUrl, opt => opt.MapFrom(src => ResolveImageUrl(src.LogoUrl, src.Logo)));

        CreateMap<RoomTypePostDto, RoomType>()
            .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.ImageUrls))
            .ForMember(dest => dest.Logo, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Logo) ? null : Convert.FromBase64String(src.Logo)));
        CreateMap<RoomTypePutDto, RoomType>()
            .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.ImageUrls))
            .ForMember(dest => dest.Logo, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Logo) ? null : Convert.FromBase64String(src.Logo)));
        CreateMap<RoomType, RoomTypeResponseDto>()
            .ForMember(dest => dest.LogoUrl, opt => opt.MapFrom(src => ResolveImageUrl(src.LogoUrl, src.Logo)))
            .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => ResolveImageUrls(src.ImageUrls, src.Images)));

        CreateMap<ReservationMealPostDto, ReservationMeal>();
        CreateMap<ReservationMealPutDto, ReservationMeal>();
        CreateMap<ReservationMeal, ReservationMealResponseDto>();

        CreateMap<ReservationViewPostDto, ReservationView>();
        CreateMap<ReservationViewPutDto, ReservationView>();
        CreateMap<ReservationView, ReservationViewResponseDto>();

        CreateMap<ReservationSeasonPostDto, ReservationSeason>();
        CreateMap<ReservationSeasonPutDto, ReservationSeason>();
        CreateMap<ReservationSeason, ReservationSeasonResponseDto>()
            .ForMember(x => x.RoomRevenueMainAccount, opt => opt.MapFrom(x => x.RoomRevenueMainAccount))
            .ForMember(x => x.RoomExpenseMainAccount, opt => opt.MapFrom(x => x.RoomExpenseMainAccount))
            .ForMember(x => x.MealRevenueMainAccount, opt => opt.MapFrom(x => x.MealRevenueMainAccount))
            .ForMember(x => x.MealExpenseMainAccount, opt => opt.MapFrom(x => x.MealExpenseMainAccount))
            .ForMember(x => x.OtherRevenueMainAccount, opt => opt.MapFrom(x => x.OtherRevenueMainAccount))
            .ForMember(x => x.OtherExpenseMainAccount, opt => opt.MapFrom(x => x.OtherExpenseMainAccount));

        CreateMap<ReservationSupplierPostDto, ReservationSupplier>();
        CreateMap<ReservationSupplierPutDto, ReservationSupplier>();
        CreateMap<ReservationSupplier, ReservationSupplierResponseDto>()
            .ForMember(x => x.MainAccount, opt => opt.MapFrom(x => x.MainAccount));

        CreateMap<ReservationServicePostDto, ReservationService>()
            .ForMember(dest => dest.Logo, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Logo) ? null : Convert.FromBase64String(src.Logo)));
        CreateMap<ReservationServicePutDto, ReservationService>()
            .ForMember(dest => dest.Logo, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Logo) ? null : Convert.FromBase64String(src.Logo)));
        CreateMap<ReservationService, ReservationServiceResponseDto>()
            .ForMember(x => x.RevenueMainAccount, opt => opt.MapFrom(x => x.RevenueMainAccount))
            .ForMember(x => x.ExpenseMainAccount, opt => opt.MapFrom(x => x.ExpenseMainAccount))
            .ForMember(dest => dest.LogoUrl, opt => opt.MapFrom(src => ResolveImageUrl(src.LogoUrl, src.Logo)));

        CreateMap<ReservationCompanyPostDto, ReservationCompany>();
        CreateMap<ReservationCompanyPutDto, ReservationCompany>();
        CreateMap<ReservationCompany, ReservationCompanyResponseDto>()
            .ForMember(x => x.MainAccount, opt => opt.MapFrom(x => x.MainAccount));

        CreateMap<ReservationRoomEntryPostDto, ReservationRoomEntry>();
        CreateMap<ReservationRoomEntryPutDto, ReservationRoomEntry>();
        CreateMap<ReservationRoomEntry, ReservationRoomEntryResponseDto>()
            .ForMember(x => x.RoomType, opt => opt.MapFrom(x => x.RoomType))
            .ForMember(x => x.RoomTypeArabicName, opt => opt.MapFrom(src => src.RoomType != null ? src.RoomType.ArabicName : null))
            .ForMember(x => x.RoomTypeEnglishName, opt => opt.MapFrom(src => src.RoomType != null ? src.RoomType.EnglishName : null))
            .ForMember(x => x.ReservationMealArabicName, opt => opt.MapFrom(src => src.ReservationMeal != null ? src.ReservationMeal.ArabicName : null))
            .ForMember(x => x.ReservationMealEnglishName, opt => opt.MapFrom(src => src.ReservationMeal != null ? src.ReservationMeal.EnglishName : null))
            .ForMember(x => x.ReservationViewArabicName, opt => opt.MapFrom(src => src.ReservationView != null ? src.ReservationView.ArabicName : null))
            .ForMember(x => x.ReservationViewEnglishName, opt => opt.MapFrom(src => src.ReservationView != null ? src.ReservationView.EnglishName : null));


        CreateMap<ReservationServiceEntryPostDto, ReservationServiceEntry>();
        CreateMap<ReservationServiceEntryPutDto, ReservationServiceEntry>();
        CreateMap<ReservationServiceEntry, ReservationServiceEntryResponseDto>()
            .ForMember(x => x.ReservationServiceArabicName, opt => opt.MapFrom(src => src.ReservationService != null ? src.ReservationService.ArabicName : null))
            .ForMember(x => x.ReservationServiceEnglishName, opt => opt.MapFrom(src => src.ReservationService != null ? src.ReservationService.EnglishName : null))
            .ForMember(x => x.ReservationSupplierArabicName, opt => opt.MapFrom(src => src.ReservationSupplier != null ? src.ReservationSupplier.ArabicName : null))
            .ForMember(x => x.ReservationSupplierEnglishName, opt => opt.MapFrom(src => src.ReservationSupplier != null ? src.ReservationSupplier.EnglishName : null));

        CreateMap<ReservationPostDto, Reservation>()
            .ForMember(x => x.ReservationRoomEntries, opt => opt.MapFrom(src => src.ReservationRoomEntries))
            .ForMember(x => x.ReservationServiceEntries, opt => opt.MapFrom(src => src.ReservationServiceEntries))
            .ForMember(x => x.ReservationType, opt => opt.MapFrom(src => Enum.Parse<ReservationType>(src.ReservationType)));
        CreateMap<ReservationPutDto, Reservation>()
            .ForMember(x => x.ReservationRoomEntries, opt => opt.MapFrom(src => src.ReservationRoomEntries))
            .ForMember(x => x.ReservationServiceEntries, opt => opt.MapFrom(src => src.ReservationServiceEntries))
            .ForMember(x => x.ReservationType, opt => opt.MapFrom(src => Enum.Parse<ReservationType>(src.ReservationType)));
        CreateMap<Reservation, ReservationResponseDto>()
            .ForMember(x => x.ReservationRoomEntries, opt => opt.MapFrom(src => src.ReservationRoomEntries))
            .ForMember(x => x.Attachments, opt => opt.MapFrom(src => src.Attachments))
            .ForMember(x => x.ReservationServiceEntries, opt => opt.MapFrom(src => src.ReservationServiceEntries))
            .ForMember(x => x.HotelArabicName, opt => opt.MapFrom(src => src.Hotel != null ? src.Hotel.ArabicName : null))
            .ForMember(x => x.HotelEnglishName, opt => opt.MapFrom(src => src.Hotel != null ? src.Hotel.EnglishName : null))
            .ForMember(x => x.ReservationCompanyArabicName, opt => opt.MapFrom(src => src.ReservationCompany != null ? src.ReservationCompany.ArabicName : null))
            .ForMember(x => x.ReservationCompanyEnglishName, opt => opt.MapFrom(src => src.ReservationCompany != null ? src.ReservationCompany.EnglishName : null))
            .ForMember(x => x.ReservationClientCompanyArabicName, opt => opt.MapFrom(src => src.ReservationClientCompany != null ? src.ReservationClientCompany.ArabicName : null))
            .ForMember(x => x.ReservationClientCompanyEnglishName, opt => opt.MapFrom(src => src.ReservationClientCompany != null ? src.ReservationClientCompany.EnglishName : null))
            .ForMember(x => x.SalesRepresentativeArabicName, opt => opt.MapFrom(src => src.SalesRepresentative != null ? src.SalesRepresentative.ArabicName : null))
            .ForMember(x => x.SalesRepresentativeEnglishName, opt => opt.MapFrom(src => src.SalesRepresentative != null ? src.SalesRepresentative.EnglishName : null))
            .ForMember(x => x.ReservationSeasonArabicName, opt => opt.MapFrom(src => src.ReservationSeason != null ? src.ReservationSeason.ArabicName : null))
            .ForMember(x => x.ReservationSeasonEnglishName, opt => opt.MapFrom(src => src.ReservationSeason != null ? src.ReservationSeason.EnglishName : null))
            .ForMember(x => x.BankAccountName, opt => opt.MapFrom(src => src.BankAccount != null ? src.BankAccount.BankName : null))
            .ForMember(x => x.CompanyPhoneNumber, opt => opt.MapFrom(src => src.CompanyPhone != null ? src.CompanyPhone.PhoneNumber : null))
            .ForMember(x => x.CompanyPolicyParagraph, opt => opt.MapFrom(src => src.CompanyPolicy != null ? src.CompanyPolicy.PolicyParagraph : null));

        // ReservationPrintTableTemplate mappings
        CreateMap<ReservationPrintTableTemplatePostDto, ReservationPrintTableTemplate>()
            .ForMember(dest => dest.Columns, opt => opt.MapFrom(src => src.Columns))
            .ForMember(dest=>dest.PageOrientation,opt=>opt.MapFrom(src=>Enum.Parse<PageOrientation>(src.PageOrientation,true)));
        CreateMap<ReservationPrintTableTemplatePutDto, ReservationPrintTableTemplate>()
            .ForMember(dest => dest.Columns, opt => opt.MapFrom(src => src.Columns))
            .ForMember(dest => dest.PageOrientation, opt => opt.MapFrom(src => Enum.Parse<PageOrientation>(src.PageOrientation, true)));
        CreateMap<ReservationPrintTableTemplate, ReservationPrintTableTemplateResponseDto>()
            .ForMember(dest => dest.Columns, opt => opt.MapFrom(src => src.Columns))
            .ForMember(dest=>dest.PageOrientation,opt=>opt.MapFrom(src=>src.PageOrientation.ToString()));

        // PrintTableColumn mappings
        CreateMap<PrintTableColumnDto, SmartAndPro.ERP.Domains.ValueObjects.PrintTableColumn>();
        CreateMap<SmartAndPro.ERP.Domains.ValueObjects.PrintTableColumn, PrintTableColumnDto>();

        CreateMap<SupplierReportRequestDto, GetSupplierDetailsInputModel>()
            .ForMember(dest => dest.SupplierId, src => src.MapFrom(opt => opt.Id));


        CreateMap<AttachmentPostDto, Attachment>();
        CreateMap<AttachmentPutDto, Attachment>();
        CreateMap<Attachment, AttachmentResponseDto>();
        CreateMap<Attachment, AttachmentDownloadDto>()
            .ForMember(dest => dest.FileData, opt => opt.MapFrom(src => Convert.ToBase64String(src.FileData)));

        CreateMap<MealReservationPostDto, MealReservation>()
            .ForMember(dest => dest.MealReservationEntries, src => src.MapFrom(opt => opt.MealReservationEntries));
        CreateMap<MealReservationPutDto, MealReservation>()
            .ForMember(dest => dest.MealReservationEntries, src => src.MapFrom(opt => opt.MealReservationEntries));
        CreateMap<MealReservation, MealReservationResponseDto>()
            .ForMember(dest => dest.MealReservationEntries, src => src.MapFrom(opt => opt.MealReservationEntries))
            .ForMember(dest => dest.CustomerProfile, src => src.MapFrom(opt => opt.CustomerProfile));

        CreateMap<MealReservationEntryPostDto, MealReservationEntry>();
        CreateMap<MealReservationEntryPutDto, MealReservationEntry>();
        CreateMap<MealReservationEntry, MealReservationEntryResponseDto>()
            .ForMember(dest => dest.ReservationMeal, src => src.MapFrom(opt => opt.ReservationMeal));

        CreateMap<SystemSettingPostDto, SystemSetting>();
        CreateMap<SystemSettingPutDto, SystemSetting>();
        CreateMap<SystemSetting, SystemSettingResponseDto>();

        // PDF TEMPLATE CONFIGURATION
        CreateMap<PdfTemplateConfigurationPostDto, PdfTemplateConfiguration>();
        CreateMap<PdfTemplateConfigurationPutDto, PdfTemplateConfiguration>();
        CreateMap<PdfTemplateConfiguration, PdfTemplateConfigurationResponseDto>();

        CreateMap<PdfHeaderFooterConfigDto, PdfHeaderFooterConfig>();
        CreateMap<PdfHeaderFooterConfig, PdfHeaderFooterConfigDto>();

        CreateMap<PdfColumnGroupDto, PdfColumnGroup>();
        CreateMap<PdfColumnGroup, PdfColumnGroupDto>();

        CreateMap<PdfRowGroupDto, PdfRowGroup>();
        CreateMap<PdfRowGroup, PdfRowGroupDto>();

        CreateMap<PdfItemDto, PdfItem>();
        CreateMap<PdfItem, PdfItemDto>();

        CreateMap<CustomTextDto, CustomText>();
        CreateMap<CustomText, CustomTextDto>();

        //Notifications
        CreateMap<NotificationPostDto, Notification>();
        CreateMap<NotificationPutDto, Notification>();
        CreateMap<Notification, NotificationResponseDto>();

        //UserNotifications
        CreateMap<UserNotificationPostDto, UserNotification>();
        CreateMap<UserNotificationPutDto, UserNotification>();
        CreateMap<UserNotification, UserNotificationResponseDto>();

        //RequestForQuotations
        CreateMap<RequestForQuotationPostDto, RequestForQuotation>();
        CreateMap<RequestForQuotationPutDto, RequestForQuotation>();
        CreateMap<RequestForQuotation, RequestForQuotationResponseDto>();

        //RequestForQuotationItems
        CreateMap<RequestForQuotationItemPostDto, RequestForQuotationItem>();
        CreateMap<RequestForQuotationItemPutDto, RequestForQuotationItem>();
        CreateMap<RequestForQuotationItem, RequestForQuotationItemResponseDto>();

        // Public Booking System
        CreateMap<PublicHotelPostDto, PublicHotel>();
        CreateMap<PublicHotelPutDto, PublicHotel>()
            .ForMember(dest => dest.HotelPaidServices, opt => opt.Ignore()); // Handle manually in service
        CreateMap<PublicHotel, PublicHotelResponseDto>();
        CreateMap<PublicHotelPaidServiceDto, PublicHotelPaidService>();

        CreateMap<PublicRoomTypePostDto, PublicRoomType>();
        CreateMap<PublicRoomTypePutDto, PublicRoomType>();
        CreateMap<PublicRoomType, PublicRoomTypeResponseDto>();

        CreateMap<PublicBookingPostDto, Domains.Entities.PublicBooking.PublicBooking>();
        CreateMap<PublicBookingPutDto, Domains.Entities.PublicBooking.PublicBooking>();
        CreateMap<Domains.Entities.PublicBooking.PublicBooking, PublicBookingResponseDto>();

        CreateMap<HotelAllotmentInvoicePostDto, HotelAllotmentInvoice>();
        CreateMap<HotelAllotmentInvoicePutDto, HotelAllotmentInvoice>();
        CreateMap<HotelAllotmentInvoice, HotelAllotmentInvoiceResponseDto>()
            .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel.ArabicName));
            //.ForMember(dest=>dest.InvoiceType,
            //           opt=>opt.MapFrom(src=>src.InvoiceType.ToString())
                      //);

        CreateMap<HotelAllotmentRoomPostDto, HotelAllotmentRoom>();
        CreateMap<HotelAllotmentRoomPutDto, HotelAllotmentRoom>();
        CreateMap<HotelAllotmentRoom, HotelAllotmentRoomResponseDto>();

        CreateMap<PaymentSchedulePostDto, PaymentSchedule>();

        CreateMap<PaymentSchedulePutDto, PaymentSchedule>();

        CreateMap<PaymentSchedule, PaymentScheduleResponseDto>()
             .ForMember(dest => dest.InvoiceType,
                       opt => opt.MapFrom(src => src.InvoiceType.ToString()));
                 //.ForMember(dest => dest.PaymentDay, opt => opt.Ignore())
                //.ForMember(dest => dest.TotalAmount, opt => opt.Ignore()); ;

        // Public Paid Services
        CreateMap<PublicPaidServicePostDto, PublicPaidService>();
        CreateMap<PublicPaidServicePutDto, PublicPaidService>();
        CreateMap<PublicPaidService, PublicPaidServiceResponseDto>();
        
        CreateMap<PublicHotelPaidService, PublicHotelPaidServiceDto>();
        CreateMap<PublicBookingPaidService, PublicBookingPaidServiceDto>();

        CreateMap<PurchaseQuotationPostDto, PurchaseQuotation>();
        CreateMap<PurchaseQuotationPutDto, PurchaseQuotation>();

        CreateMap<PurchaseQuotation, PurchaseQuotationDto>()
            .ReverseMap();

        CreateMap<PurchaseQuotationItemPostDto, PurchaseQuotationItem>();

        CreateMap<PurchaseQuotationItem, PurchaseQuotationItemDto>()
            .ReverseMap();


        CreateMap<PurchaseQuotation, PurchaseInvoice>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.PurchaseItems,
        opt => opt.MapFrom(src => src.PurchaseQuotationItems))
    .ForMember(dest => dest.Attachments, opt => opt.Ignore());

        CreateMap<PurchaseQuotationItem, PurchaseItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PurchaseInvoiceId, opt => opt.Ignore())
            .ForMember(dest => dest.PurchaseInvoice, opt => opt.Ignore())
            .ForMember(dest => dest.StoreDetails, opt => opt.Ignore());

        // Stagnant Inventory (الرواكد)
        CreateMap<StagnantInventoryPostDto, StagnantInventory>();
        CreateMap<StagnantInventoryPutDto, StagnantInventory>();
        CreateMap<StagnantInventory, StagnantInventoryResponseDto>()
            .ForMember(dest => dest.MainPurchaseClassName, opt => opt.MapFrom(src => src.MainPurchaseClass != null ? src.MainPurchaseClass.ArabicName : null))
            .ForMember(dest => dest.MainSalesClassName, opt => opt.MapFrom(src => src.MainSalesClass != null ? src.MainSalesClass.ArabicName : null))
            .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => src.StoreDetails != null ? src.StoreDetails.ArabicName : null))
            .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.MainPurchaseClass != null ? src.MainPurchaseClass.ReferenceCode : (src.MainSalesClass != null ? src.MainSalesClass.ReferenceCode : null)));

    }

    private static bool IsHttpUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;
        return value.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }

    private static string? ResolveInputImageUrl(string? directUrl, string? legacyString)
    {
        if (!string.IsNullOrWhiteSpace(directUrl))
        {
            return directUrl;
        }

        if (IsHttpUrl(legacyString))
        {
            return legacyString;
        }

        return null;
    }

    private static byte[]? SafeBase64ToBytes(string? base64OrUrl)
    {
        if (string.IsNullOrWhiteSpace(base64OrUrl))
            return null;

        if (IsHttpUrl(base64OrUrl))
            return null;

        var cleaned = base64OrUrl;
        var commaIndex = base64OrUrl.IndexOf(',');
        if (commaIndex >= 0)
            cleaned = base64OrUrl[(commaIndex + 1)..];

        cleaned = cleaned.Trim().Replace("\r", "").Replace("\n", "").Replace(" ", "");

        if (string.IsNullOrWhiteSpace(cleaned))
            return null;

        try
        {
            return Convert.FromBase64String(cleaned);
        }
        catch
        {
            return null;
        }
    }

    private static string? ResolveImageUrl(string? cloudinaryUrl, byte[]? legacyBytes)
    {
        if (!string.IsNullOrWhiteSpace(cloudinaryUrl))
        {
            return cloudinaryUrl;
        }

        if (legacyBytes is null || legacyBytes.Length == 0)
        {
            return null;
        }

        return $"data:image/*;base64,{Convert.ToBase64String(legacyBytes)}";
    }

    private static string? ResolveImageUrls(string? cloudinaryImageUrls, string? legacyImagesJson)
    {
        if (!string.IsNullOrWhiteSpace(cloudinaryImageUrls))
        {
            return cloudinaryImageUrls;
        }

        if (string.IsNullOrWhiteSpace(legacyImagesJson))
        {
            return null;
        }

        try
        {
            var legacyImages = System.Text.Json.JsonSerializer.Deserialize<List<string>>(legacyImagesJson);
            if (legacyImages is null || legacyImages.Count == 0)
            {
                return null;
            }

            var dataUriImages = legacyImages
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => $"data:image/*;base64,{x.Trim()}")
                .ToList();

            return dataUriImages.Count == 0
                ? null
                : System.Text.Json.JsonSerializer.Serialize(dataUriImages);
        }
        catch
        {
            return null;
        }
    }
}
