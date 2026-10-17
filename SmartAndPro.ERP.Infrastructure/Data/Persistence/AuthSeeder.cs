using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Domains.Entities.Auth.Authorization;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using SmartAndPro.ERP.Infrastructure.Contexts;

namespace SmartAndPro.ERP.Infrastructure.Data.Persistence;

public static class AuthSeeder
{
    public static void EnsureRoleQuotaColumns(ERPContext context)
    {
        var columns = new[]
        {
            "MaxUsersCount",
            "MaxCompaniesCount",
            "MaxBranchesCount",
            "MaxStoresCount",
            "MaxBanksCount",
            "MaxTreasuriesCount",
            "MaxSuppliersCount",
            "MaxCustomersCount"
        };

        foreach (var col in columns)
        {
            try
            {
                var sql = $@"
                    IF NOT EXISTS (
                        SELECT 1 FROM sys.columns 
                        WHERE object_id = OBJECT_ID(N'[dbo].[Roles]') AND name = '{col}'
                    )
                    BEGIN
                        ALTER TABLE [dbo].[Roles] ADD [{col}] INT NULL;
                    END";
                context.Database.ExecuteSqlRaw(sql);
            }
            catch { }
        }
    }

    public static void SeedOTPTypes(ERPContext context)
    {
        var otpTypes = new List<OTPType> {
            new OTPType { Name = "sign up", IsActive = true },
            new OTPType { Name = "sign in", IsActive = true },
            new OTPType { Name = "reset password", IsActive = true }
        };

        foreach (var otpType in otpTypes)
        {
            if (!context.OTPType.Any(x => x.Name == otpType.Name))
            {
                context.OTPType.Add(otpType);
            }
        }

        context.SaveChanges();
    }

    public static void SeedOTPStatuses(ERPContext context)
    {
        var otpStatuses = new List<OTPStatuses> {
            new OTPStatuses { EnglishDescription = "pending", ArabicDescription = "قيد الانتظار" },
            new OTPStatuses { EnglishDescription = "success", ArabicDescription = "نجاح" },
            new OTPStatuses { EnglishDescription = "blocked", ArabicDescription = "محظور" }
        };

        foreach (var otpStatus in otpStatuses)
        {
            if (!context.OTPStatuses.Any(x => x.EnglishDescription == otpStatus.EnglishDescription))
            {
                context.OTPStatuses.Add(otpStatus);
            }
        }

        context.SaveChanges();
    }

    public static void SeedDomainDetails(ERPContext context)
    {
        var domainDetails = new List<DomainDetails>
        {
            new DomainDetails { Name = "Inventory" },
            new DomainDetails { Name = "Finance" },
            new DomainDetails { Name = "System" },
            new DomainDetails { Name = "Cashier" },
            new DomainDetails { Name = "Reservation" },
        };

        foreach (var domainDetail in domainDetails)
        {
            if (!context.DomainDetails.Any(x => x.Name == domainDetail.Name))
            {
                context.DomainDetails.Add(domainDetail);
            }
        }

        context.SaveChanges();
    }

    public static void SeedRoles(ERPContext context)
    {
        EnsureRoleQuotaColumns(context);
        var inventoryDomain = context.DomainDetails.FirstOrDefault(x => x.Name == "Inventory");
        var financeDomain = context.DomainDetails.FirstOrDefault(x => x.Name == "Finance");
        var systemDomain = context.DomainDetails.FirstOrDefault(x => x.Name == "System");
        var reservationDomain = context.DomainDetails.FirstOrDefault(x => x.Name == "Reservation");
        var cashierDomain = context.DomainDetails.FirstOrDefault(x => x.Name == "Cashier");

        var roles = new List<Role>
        {
            new Role { Name = "System.Admin", DomainDetailsId = systemDomain.Id },
            new Role { Name = "System.Developer", DomainDetailsId = systemDomain.Id },
            new Role { Name = "Reservation", DomainDetailsId = reservationDomain?.Id ?? financeDomain.Id },
            new Role { Name = "Cashier", DomainDetailsId = cashierDomain.Id },
        };

        foreach (var role in roles)
        {
            if (!context.Roles.Any(x => x.Name == role.Name))
            {
                context.Roles.Add(role);
            }
        }

        context.SaveChanges();
    }

    public static void SeedPermissions(ERPContext context)
    {
        var inventoryDomain = context.DomainDetails.FirstOrDefault(x => x.Name == "Inventory");
        var financeDomain = context.DomainDetails.FirstOrDefault(x => x.Name == "Finance");
        var systemDomain = context.DomainDetails.FirstOrDefault(x => x.Name == "System");

        var permissions = new List<Permission>
        {
            new Permission { Name = "System.Read",ArabicName = "القراءة على النظام", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "System.Delete", ArabicName = "الحذف على النظام", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "System.Create", ArabicName = "الإنشاء على النظام", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "System.Update", ArabicName = "التحديث على النظام", DomainDetailsId = systemDomain.Id },
            
            // Dashboard
            new Permission { Name = "Dashboard.Read", ArabicName = "لوحة التحكم", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Dashboard.SalesTotal", ArabicName = "إجمالي المبيعات في الصفحة الرئيسية", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Dashboard.PurchaseTotal", ArabicName = "إجمالي المشتريات في الصفحة الرئيسية", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Dashboard.SalesCount", ArabicName = "عدد فواتير المبيعات في الصفحة الرئيسية", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Dashboard.PurchaseCount", ArabicName = "عدد فواتير المشتريات في الصفحة الرئيسية", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Dashboard.ReservationsCount", ArabicName = "عدد الحجوزات في الصفحة الرئيسية", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Dashboard.ReservationsConfirmed", ArabicName = "الحجوزات المؤكدة في الصفحة الرئيسية", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Dashboard.InventoryItems", ArabicName = "عدد أصناف المخزون في الصفحة الرئيسية", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Dashboard.CashIn30", ArabicName = "إجمالي القبض آخر 30 يوم في الصفحة الرئيسية", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Dashboard.CashOut30", ArabicName = "إجمالي الصرف آخر 30 يوم في الصفحة الرئيسية", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Dashboard.Treasuries", ArabicName = "الخزن والمصارف في الصفحة الرئيسية", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Dashboard.StoreCount", ArabicName = "عدد المخازن في الصفحة الرئيسية", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Dashboard.SupplierCount", ArabicName = "عدد الموردين في الصفحة الرئيسية", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Dashboard.BankAccounts", ArabicName = "عدد الحسابات البنكية في الصفحة الرئيسية", DomainDetailsId = systemDomain.Id },
            
            // Projects
            new Permission { Name = "Projects.Read", ArabicName = "قراءة المشاريع", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Projects.Create", ArabicName = "إنشاء المشاريع", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Projects.Update", ArabicName = "تحديث المشاريع", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Projects.Delete", ArabicName = "حذف المشاريع", DomainDetailsId = systemDomain.Id },
            
            // Projects Assignment Phase
            new Permission { Name = "Projects.Assignment.Read", ArabicName = "عرض مراحل إسناد المشاريع", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Projects.Assignment.Update", ArabicName = "إدارة مراحل إسناد المشاريع", DomainDetailsId = systemDomain.Id },
            
            // Projects Execution Phase
            new Permission { Name = "Projects.Execution.Read", ArabicName = "عرض مراحل تنفيذ المشاريع", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Projects.Execution.Update", ArabicName = "إدارة مراحل تنفيذ المشاريع", DomainDetailsId = systemDomain.Id },
            
            // Projects Documents
            new Permission { Name = "Projects.Documents.Read", ArabicName = "عرض مستندات المشاريع", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Projects.Documents.Create", ArabicName = "رفع مستندات المشاريع", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Projects.Documents.Delete", ArabicName = "حذف مستندات المشاريع", DomainDetailsId = systemDomain.Id },
            
            // Projects Payments
            new Permission { Name = "Projects.Payments.Read", ArabicName = "عرض دفعات المشاريع", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Projects.Payments.Create", ArabicName = "إضافة دفعات المشاريع", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Projects.Payments.Update", ArabicName = "تعديل دفعات المشاريع", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Projects.Payments.Delete", ArabicName = "حذف دفعات المشاريع", DomainDetailsId = systemDomain.Id },
            
            // Projects Price Quotes
            new Permission { Name = "Projects.PriceQuotes.Read", ArabicName = "عرض عروض أسعار المشاريع", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Projects.PriceQuotes.Create", ArabicName = "إضافة عروض أسعار المشاريع", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Projects.PriceQuotes.Update", ArabicName = "تعديل عروض أسعار المشاريع", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Projects.PriceQuotes.Delete", ArabicName = "حذف عروض أسعار المشاريع", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Projects.PriceQuotes.Accept", ArabicName = "اعتماد عروض أسعار المشاريع", DomainDetailsId = systemDomain.Id },
            
            // Projects Assessments
            new Permission { Name = "Projects.Assessments.Read", ArabicName = "عرض تقييمات المشاريع", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Projects.Assessments.Create", ArabicName = "إضافة تقييمات المشاريع", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Projects.Assessments.Update", ArabicName = "تعديل تقييمات المشاريع", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Projects.Assessments.Delete", ArabicName = "حذف تقييمات المشاريع", DomainDetailsId = systemDomain.Id },
            
            // Projects Reports
            new Permission { Name = "Projects.Reports.Read", ArabicName = "تحميل تقارير وكشوف حساب المشاريع", DomainDetailsId = systemDomain.Id },
            
            // Smart Booking
            new Permission { Name = "SmartBooking.Read", ArabicName = "الحجز الذكي", DomainDetailsId = financeDomain.Id },
            
            // Measurement Units
            new Permission { Name = "MeasurementUnit.Read", ArabicName = "القراءة على الوحدات القياسية", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "MeasurementUnit.Delete", ArabicName = "الحذف على الوحدات القياسية", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "MeasurementUnit.Create", ArabicName = "الإنشاء على الوحدات القياسية", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "MeasurementUnit.Update", ArabicName = "التحديث على الوحدات القياسية", DomainDetailsId = inventoryDomain.Id },
            
            // Inventory Management
            new Permission { Name = "InventoryManagement.Read", ArabicName = "القراءة على إدارة المخزون", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "InventoryManagement.Delete", ArabicName = "الحذف على إدارة المخزون", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "InventoryManagement.Create", ArabicName = "الإنشاء على إدارة المخزون", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "InventoryManagement.Update", ArabicName = "التحديث على إدارة المخزون", DomainDetailsId = inventoryDomain.Id },
            
            // Main Purchase Class
            new Permission { Name = "MainPurchaseClass.Read", ArabicName = "القراءة على شجرة اصناف المشتريات", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "MainPurchaseClass.Delete", ArabicName = "الحذف على شجرة اصناف المشتريات", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "MainPurchaseClass.Create", ArabicName = "الإنشاء على شجرة اصناف المشتريات", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "MainPurchaseClass.Update", ArabicName = "التحديث على شجرة اصناف المشتريات", DomainDetailsId = inventoryDomain.Id },
            
            // Main Sales Class
            new Permission { Name = "MainSalesClass.Read", ArabicName = "القراءة على شجرة اصناف المبيعات", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "MainSalesClass.Delete", ArabicName = "الحذف على شجرة اصناف المبيعات", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "MainSalesClass.Create", ArabicName = "الإنشاء على شجرة اصناف المبيعات", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "MainSalesClass.Update", ArabicName = "التحديث على شجرة اصناف المبيعات", DomainDetailsId = inventoryDomain.Id },
            
            // Supplier Details
            new Permission { Name = "SupplierDetails.Read", ArabicName = "القراءة على معلومات المورد", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "SupplierDetails.Delete", ArabicName = "الحذف على معلومات المورد", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "SupplierDetails.Create", ArabicName = "الإنشاء على معلومات المورد", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "SupplierDetails.Update", ArabicName = "التحديث على معلومات المورد", DomainDetailsId = inventoryDomain.Id },
            
            // Customer Profile
            new Permission { Name = "CustomerProfile.Read", ArabicName = "القراءة على معلومات العميل", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "CustomerProfile.Delete", ArabicName = "الحذف على معلومات العميل", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "CustomerProfile.Create", ArabicName = "الإنشاء على معلومات العميل", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "CustomerProfile.Update", ArabicName = "التحديث على معلومات العميل", DomainDetailsId = inventoryDomain.Id },
            
            // Store Details
            new Permission { Name = "StoreDetails.Read", ArabicName = "القراءة على معلومات المخزون", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "StoreDetails.Delete", ArabicName = "الحذف على معلومات المخزون", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "StoreDetails.Create", ArabicName = "الإنشاء على معلومات المخزون", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "StoreDetails.Update", ArabicName = "التحديث على معلومات المخزون", DomainDetailsId = inventoryDomain.Id },
            
            // Recipe
            new Permission { Name = "Recipe.Read", ArabicName = "القراءة على الوصفات", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "Recipe.Delete", ArabicName = "الحذف على الوصفات", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "Recipe.Create", ArabicName = "الإنشاء على الوصفات", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "Recipe.Update", ArabicName = "التحديث على الوصفات", DomainDetailsId = inventoryDomain.Id },

            // Expenses Center
            new Permission { Name = "ExpensesCenter.Read", ArabicName = "القراءة على مركز التكلفة", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "ExpensesCenter.Delete", ArabicName = "الحذف على مركز التكلفة", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "ExpensesCenter.Create", ArabicName = "الإنشاء على مركز التكلفة", DomainDetailsId = inventoryDomain.Id },
            new Permission { Name = "ExpensesCenter.Update", ArabicName = "التحديث على مركز التكلفة", DomainDetailsId = inventoryDomain.Id },
            
            // Finance Periods
            new Permission { Name = "FinancePeriods.Read", ArabicName = "القراءة على فترات المالية", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FinancePeriods.Delete", ArabicName = "الحذف على فترات المالية", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FinancePeriods.Create", ArabicName = "الإنشاء على فترات المالية", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FinancePeriods.Update", ArabicName = "التحديث على فترات المالية", DomainDetailsId = financeDomain.Id },
            
            // Treasury
            new Permission { Name = "Treasury.Read", ArabicName = "القراءة على الخزينة", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "Treasury.Delete", ArabicName = "الحذف على الخزينة", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "Treasury.Create", ArabicName = "الإنشاء على الخزينة", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "Treasury.Update", ArabicName = "التحديث على الخزينة", DomainDetailsId = financeDomain.Id },
            
            // Hotels
            new Permission { Name = "Hotels.Read", ArabicName = "القراءة على الفنادق", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "Hotels.Delete", ArabicName = "الحذف على الفنادق", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "Hotels.Create", ArabicName = "الإنشاء على الفنادق", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "Hotels.Update", ArabicName = "التحديث على الفنادق", DomainDetailsId = financeDomain.Id },
            
            // Reservation Meal
            new Permission { Name = "ReservationMeal.Read", ArabicName = "القراءة على وجبات الحجوزات", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationMeal.Delete", ArabicName = "الحذف على وجبات الحجوزات", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationMeal.Create", ArabicName = "الإنشاء على وجبات الحجوزات", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationMeal.Update", ArabicName = "التحديث على وجبات الحجوزات", DomainDetailsId = financeDomain.Id },
            
            // Room Type
            new Permission { Name = "RoomType.Read", ArabicName = "القراءة على أنواع الغرف", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "RoomType.Delete", ArabicName = "الحذف على أنواع الغرف", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "RoomType.Create", ArabicName = "الإنشاء على أنواع الغرف", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "RoomType.Update", ArabicName = "التحديث على أنواع الغرف", DomainDetailsId = financeDomain.Id },
            
            // Sales Representative
            new Permission { Name = "SalesRepresentative.Read", ArabicName = "القراءة على مندوب المبيعات", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesRepresentative.Delete", ArabicName = "الحذف على مندوب المبيعات", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesRepresentative.Create", ArabicName = "الإنشاء على مندوب المبيعات", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesRepresentative.Update", ArabicName = "التحديث على مندوب المبيعات", DomainDetailsId = financeDomain.Id },
            
            // Reservation Supplier
            new Permission { Name = "ReservationSupplier.Read", ArabicName = "القراءة على مورد حجز", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationSupplier.Delete", ArabicName = "الحذف على مورد حجز", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationSupplier.Create", ArabicName = "الإنشاء على مورد حجز", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationSupplier.Update", ArabicName = "التحديث على مورد حجز", DomainDetailsId = financeDomain.Id },
            
            // Reservation Service
            new Permission { Name = "ReservationService.Read", ArabicName = "القراءة على خدمة حجز", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationService.Delete", ArabicName = "الحذف على خدمة حجز", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationService.Create", ArabicName = "الإنشاء على خدمة حجز", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationService.Update", ArabicName = "التحديث على خدمة حجز", DomainDetailsId = financeDomain.Id },
            
            // Reservation Company
            new Permission { Name = "ReservationCompany.Read", ArabicName = "القراءة على شركة حجز", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationCompany.Delete", ArabicName = "الحذف على شركة حجز", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationCompany.Create", ArabicName = "الإنشاء على شركة حجز", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationCompany.Update", ArabicName = "التحديث على شركة حجز", DomainDetailsId = financeDomain.Id },
            
            // Reservation View
            new Permission { Name = "ReservationView.Read", ArabicName = "القراءة على اطلالة حجز", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationView.Delete", ArabicName = "الحذف على اطلالة حجز", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationView.Create", ArabicName = "الإنشاء على اطلالة حجز", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationView.Update", ArabicName = "التحديث على اطلالة حجز", DomainDetailsId = financeDomain.Id },
            
            // Reservation Season
            new Permission { Name = "ReservationSeason.Read", ArabicName = "القراءة على موسم حجز", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationSeason.Delete", ArabicName = "الحذف على موسم حجز", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationSeason.Create", ArabicName = "الإنشاء على موسم حجز", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationSeason.Update", ArabicName = "التحديث على موسم حجز", DomainDetailsId = financeDomain.Id },
            
            // Reservation
            new Permission { Name = "Reservation.Read", ArabicName = "القراءة على حجز", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "Reservation.Delete", ArabicName = "الحذف على حجز", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "Reservation.Create", ArabicName = "الإنشاء على حجز", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "Reservation.Update", ArabicName = "التحديث على حجز", DomainDetailsId = financeDomain.Id },
            
            // Inventory Permit
            new Permission { Name = "InventoryPermit.Read", ArabicName = "القراءة على الإذن المخزني", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "InventoryPermit.Delete", ArabicName = "الحذف على الإذن المخزني", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "InventoryPermit.Create", ArabicName = "الإنشاء على الإذن المخزني", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "InventoryPermit.Update", ArabicName = "التحديث على الإذن المخزني", DomainDetailsId = financeDomain.Id },
            
            // Inventory Stocktaking
            new Permission { Name = "InventoryStocktaking.Read", ArabicName = "القراءة على الجرد المخزني", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "InventoryStocktaking.Delete", ArabicName = "الحذف على الجرد المخزني", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "InventoryStocktaking.Create", ArabicName = "الإنشاء على الجرد المخزني", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "InventoryStocktaking.Update", ArabicName = "التحديث على الجرد المخزني", DomainDetailsId = financeDomain.Id },
            
            // Purchase Invoice
            new Permission { Name = "PurchaseInvoice.Read", ArabicName = "القراءة على فاتورة الشراء", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "PurchaseInvoice.Delete", ArabicName = "الحذف على فاتورة الشراء", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "PurchaseInvoice.Create", ArabicName = "الإنشاء على فاتورة الشراء", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "PurchaseInvoice.Update", ArabicName = "التحديث على فاتورة الشراء", DomainDetailsId = financeDomain.Id },
            
            // Purchase Return
            new Permission { Name = "PurchaseReturn.Read", ArabicName = "القراءة على مرتجع الشراء", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "PurchaseReturn.Delete", ArabicName = "الحذف على مرتجع الشراء", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "PurchaseReturn.Create", ArabicName = "الإنشاء على مرتجع الشراء", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "PurchaseReturn.Update", ArabicName = "التحديث على مرتجع الشراء", DomainDetailsId = financeDomain.Id },
            
            // Reservation Purchases
            new Permission { Name = "ReservationPurchases.Read", ArabicName = "القراءة على مشتريات الحجوزات", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationPurchases.Delete", ArabicName = "الحذف على مشتريات الحجوزات", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationPurchases.Create", ArabicName = "الإنشاء على مشتريات الحجوزات", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationPurchases.Update", ArabicName = "التحديث على مشتريات الحجوزات", DomainDetailsId = financeDomain.Id },

            new Permission { Name = "ReservationInvoice.Read", ArabicName = "القراءة على فاتورة الحجوزات", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationInvoice.Delete", ArabicName = "الحذف على فاتورة الحجوزات", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationInvoice.Create", ArabicName = "الإنشاء على فاتورة الحجوزات", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationInvoice.Update", ArabicName = "التحديث على فاتورة الحجوزات", DomainDetailsId = financeDomain.Id },
            
            // Debit Note
            new Permission { Name = "DebitNote.Read", ArabicName = "القراءة على اشعار مدين", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "DebitNote.Delete", ArabicName = "الحذف على اشعار مدين", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "DebitNote.Create", ArabicName = "الإنشاء على اشعار مدين", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "DebitNote.Update", ArabicName = "التحديث على اشعار مدين", DomainDetailsId = financeDomain.Id },
            
            // Sales Invoice
            new Permission { Name = "SalesInvoice.Read", ArabicName = "القراءة على فاتورة المبيعات", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesInvoice.Delete", ArabicName = "الحذف على فاتورة المبيعات", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesInvoice.Create", ArabicName = "الإنشاء على فاتورة المبيعات", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesInvoice.Update", ArabicName = "التحديث على فاتورة المبيعات", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesInvoice.Import", ArabicName = "رفع فواتير (Excel)", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesInvoice.ExportExcel", ArabicName = "تنزيل Excel", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesInvoice.ExportPdfZip", ArabicName = "تنزيل كل الفواتير (PDF Zip)", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesInvoice.ExportPdfMerged", ArabicName = "تنزيل كل الفواتير (PDF مدمج)", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesInvoice.PrintSelected", ArabicName = "طباعة المختار (PDF)", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesInvoice.Print3", ArabicName = "طباعة 3", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesInvoice.DownloadSelectedPdf", ArabicName = "تنزيل المختار (PDF)", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesInvoice.DownloadSelectedMergedPdf", ArabicName = "تنزيل المختار PDF مدمج", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesInvoice.ExportSelectedExcel", ArabicName = "تصدير المختار (Excel)", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesInvoice.BulkShare", ArabicName = "مشاركة المختار", DomainDetailsId = financeDomain.Id },
            
            // Sales Quotation
            new Permission { Name = "SalesQuotation.Read", ArabicName = "القراءة على عرض سعر", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesQuotation.Delete", ArabicName = "الحذف على عرض سعر", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesQuotation.Create", ArabicName = "الإنشاء على عرض سعر", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesQuotation.Update", ArabicName = "التحديث على عرض سعر", DomainDetailsId = financeDomain.Id },
            
            // Sales Return
            new Permission { Name = "SalesReturn.Read", ArabicName = "القراءة على مرتجع المبيعات", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesReturn.Delete", ArabicName = "الحذف على مرتجع المبيعات", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesReturn.Create", ArabicName = "الإنشاء على مرتجع المبيعات", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesReturn.Update", ArabicName = "التحديث على مرتجع المبيعات", DomainDetailsId = financeDomain.Id },
            
            // Credit Note
            new Permission { Name = "SalesCreditNote.Read", ArabicName = "القراءة على اشعار دائن", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesCreditNote.Delete", ArabicName = "الحذف على اشعار دائن", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesCreditNote.Create", ArabicName = "الإنشاء على اشعار دائن", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "SalesCreditNote.Update", ArabicName = "التحديث على اشعار دائن", DomainDetailsId = financeDomain.Id },
            
            // Account
            new Permission { Name = "Account.Read", ArabicName = "القراءة على الحسابات", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "Account.Delete", ArabicName = "الحذف على الحسابات", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "Account.Create", ArabicName = "الإنشاء على الحسابات", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "Account.Update", ArabicName = "التحديث على الحسابات", DomainDetailsId = financeDomain.Id },
            
            // Payment Receipt
            new Permission { Name = "PaymentReceipt.Read", ArabicName = "القراءة على سند صرف", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "PaymentReceipt.Delete", ArabicName = "الحذف على سند صرف", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "PaymentReceipt.Create", ArabicName = "الإنشاء على سند صرف", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "PaymentReceipt.Update", ArabicName = "التحديث على سند صرف", DomainDetailsId = financeDomain.Id },
            
            // Paycheck Receipt
            new Permission { Name = "PaycheckReceipt.Read", ArabicName = "القراءة على سند قبض", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "PaycheckReceipt.Delete", ArabicName = "الحذف على سند قبض", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "PaycheckReceipt.Create", ArabicName = "الإنشاء على سند قبض", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "PaycheckReceipt.Update", ArabicName = "التحديث على سند قبض", DomainDetailsId = financeDomain.Id },
            
            // Transaction
            new Permission { Name = "Transaction.Read", ArabicName = "القراءة على قيد اليومية", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "Transaction.Delete", ArabicName = "الحذف على قيد اليومية", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "Transaction.Create", ArabicName = "الإنشاء على قيد اليومية", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "Transaction.Update", ArabicName = "التحديث على قيد اليومية", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "Transaction.Import", ArabicName = "رفع/استيراد قيود اليومية", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "Transaction.Export", ArabicName = "تنزيل/تصدير قيود اليومية", DomainDetailsId = financeDomain.Id },
            
            // Inventory Report
            new Permission { Name = "InventoryReport.Read", ArabicName = "القراءة على تقرير المخزن", DomainDetailsId = financeDomain.Id },
            
            // Supplier Report
            new Permission { Name = "SupplierReport.Read", ArabicName = "القراءة على تقرير المورد", DomainDetailsId = financeDomain.Id },
            
            // Purchasable Card Report
            new Permission { Name = "PurchasableCardReport.Read", ArabicName = "القراءة على تقرير بطاقة الصنف", DomainDetailsId = financeDomain.Id },
            
            // Purchase Invoice Report
            new Permission { Name = "PurchasableReport.Read", ArabicName = "القراءة على تقرير المشتريات", DomainDetailsId = financeDomain.Id },
            
            // Sales Invoice Report
            new Permission { Name = "SalesInvoiceReport.Read", ArabicName = "القراءة على تقرير المبيعات", DomainDetailsId = financeDomain.Id },
            
            // Review Balance
            new Permission { Name = "ReviewBalance.Read", ArabicName = "القراءة على ميزان المراجعة", DomainDetailsId = financeDomain.Id },
            
            // Profit Loss Account
            new Permission { Name = "ProfitLossAccount.Read", ArabicName = "القراءة على حساب الأرباح والخسائر", DomainDetailsId = financeDomain.Id },
            
            // Budget Assessment
            new Permission { Name = "BudgetAssessment.Read", ArabicName = "القراءة على حساب الميزانية", DomainDetailsId = financeDomain.Id },
            
            // Account Statement
            new Permission { Name = "AccountStatement.Read", ArabicName = "القراءة على كشف الحساب", DomainDetailsId = financeDomain.Id },
            
            // User Management
            new Permission { Name = "UserManagement.Read", ArabicName = "القراءة على إدارة المستخدمين", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "UserManagement.Delete", ArabicName = "الحذف على إدارة المستخدمين", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "UserManagement.Create", ArabicName = "الإنشاء على إدارة المستخدمين", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "UserManagement.Update", ArabicName = "التحديث على إدارة المستخدمين", DomainDetailsId = systemDomain.Id },
            
            new Permission { Name = "FrontalUserManagement.Read", ArabicName = "القراءة على إدارة المستخدمين frontal", DomainDetailsId = systemDomain.Id },
            
            // Role Management
            new Permission { Name = "RoleManagement.Read", ArabicName = "القراءة على إدارة الأدوار", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "RoleManagement.Delete", ArabicName = "الحذف على إدارة الأدوار", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "RoleManagement.Create", ArabicName = "الإنشاء على إدارة الأدوار", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "RoleManagement.Update", ArabicName = "التحديث على إدارة الأدوار", DomainDetailsId = systemDomain.Id },
            
            new Permission { Name = "FrontalRoleManagement.Read", ArabicName = "القراءة على إدارة الأدوار frontal", DomainDetailsId = systemDomain.Id },
            
            
            // Company Details
            new Permission { Name = "CompanyDetails.Read", ArabicName = "القراءة على تفاصيل الشركة", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "CompanyDetails.Delete", ArabicName = "الحذف على تفاصيل الشركة", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "CompanyDetails.Create", ArabicName = "الإنشاء على تفاصيل الشركة", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "CompanyDetails.Update", ArabicName = "التحديث على تفاصيل الشركة", DomainDetailsId = systemDomain.Id },
            
            // Meal Reservation
            new Permission { Name = "MealReservation.Read", ArabicName = "القراءة على حجز وجبة", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "MealReservation.Delete", ArabicName = "الحذف على حجز وجبة", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "MealReservation.Create", ArabicName = "الإنشاء على حجز وجبة", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "MealReservation.Update", ArabicName = "التحديث على حجز وجبة", DomainDetailsId = systemDomain.Id },
            
            // SystemSetting
            new Permission { Name = "SystemSetting.Read", ArabicName = "القراءة على إعدادات النظام", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "SystemSetting.Delete", ArabicName = "الحذف على إعدادات النظام", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "SystemSetting.Create", ArabicName = "الإنشاء على إعدادات النظام", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "SystemSetting.Update", ArabicName = "التحديث على إعدادات النظام", DomainDetailsId = systemDomain.Id },
            
            // PdfTemplateConfigurator
            new Permission { Name = "PdfTemplateConfigurator.Read", ArabicName = "القراءة على إعدادات قوالب PDF", DomainDetailsId = systemDomain.Id },

            new Permission { Name = "FrontalSalesInvoice.Read", ArabicName = "القراءة على فاتورة المبيعات frontal", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FrontalSalesInvoice.Create", ArabicName = "الإنشاء على فاتورة المبيعات frontal", DomainDetailsId = financeDomain.Id },

            new Permission { Name = "FrontalCustomerProfile.Read", ArabicName = "القراءة على معلومات العميل frontal", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FrontalCustomerProfile.Create", ArabicName = "الإنشاء على معلومات العميل frontal", DomainDetailsId = financeDomain.Id },

            new Permission { Name = "FrontalSupplierDetails.Read", ArabicName = "القراءة على معلومات المورد frontal", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FrontalSupplierDetails.Create", ArabicName = "الإنشاء على معلومات المورد frontal", DomainDetailsId = financeDomain.Id },

            new Permission { Name = "FrontalPurchaseInvoice.Read", ArabicName = "القراءة على فاتورة الشراء frontal", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FrontalPurchaseInvoice.Create", ArabicName = "الإنشاء على فاتورة الشراء frontal", DomainDetailsId = financeDomain.Id },
            
            // FrontalTransaction
            new Permission { Name = "FrontalTransaction.Read", ArabicName = "القراءة على قيد اليومية frontal", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FrontalTransaction.Create", ArabicName = "الإنشاء على قيد اليومية frontal", DomainDetailsId = financeDomain.Id },
            
            // FrontalPaymentReceipt
            new Permission { Name = "FrontalPaymentReceipt.Read", ArabicName = "القراءة على سند صرف frontal", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FrontalPaymentReceipt.Create", ArabicName = "الإنشاء على سند صرف frontal", DomainDetailsId = financeDomain.Id },
            
            // FrontalPaycheckReceipt
            new Permission { Name = "FrontalPaycheckReceipt.Read", ArabicName = "القراءة على سند قبض frontal", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FrontalPaycheckReceipt.Create", ArabicName = "الإنشاء على سند قبض frontal", DomainDetailsId = financeDomain.Id },
            
            // FrontalAccountStatement
            new Permission { Name = "FrontalAccountStatement.Read", ArabicName = "القراءة على كشف الحساب frontal", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FrontalAccountStatement.Create", ArabicName = "الإنشاء على كشف الحساب frontal", DomainDetailsId = financeDomain.Id },
            
            // FrontalTreasury
            new Permission { Name = "FrontalTreasury.Read", ArabicName = "القراءة على الخزينة frontal", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FrontalTreasury.Create", ArabicName = "الإنشاء على الخزينة frontal", DomainDetailsId = financeDomain.Id },
            
            // FrontalHotel
            new Permission { Name = "FrontalHotel.Read", ArabicName = "القراءة على الفنادق frontal", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FrontalHotel.Create", ArabicName = "الإنشاء على الفنادق frontal", DomainDetailsId = financeDomain.Id },
            
            // FrontalRoomType
            new Permission { Name = "FrontalRoomType.Read", ArabicName = "القراءة على أنواع الغرف frontal", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FrontalRoomType.Create", ArabicName = "الإنشاء على أنواع الغرف frontal", DomainDetailsId = financeDomain.Id },
            
            // FrontalSalesRepresentative
            new Permission { Name = "FrontalSalesRepresentative.Read", ArabicName = "القراءة على مندوبي المبيعات frontal", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FrontalSalesRepresentative.Create", ArabicName = "الإنشاء على مندوبي المبيعات frontal", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FrontalSalesRepresentative.Update", ArabicName = "التحديث على مندوبي المبيعات frontal", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FrontalSalesRepresentative.Delete", ArabicName = "الحذف على مندوبي المبيعات frontal", DomainDetailsId = financeDomain.Id },
            
            // FrontalReservationSupplier
            new Permission { Name = "FrontalReservationSupplier.Read", ArabicName = "القراءة على موردي الحجز frontal", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FrontalReservationSupplier.Create", ArabicName = "الإنشاء على موردي الحجز frontal", DomainDetailsId = financeDomain.Id },
            
            // FrontalReservationService
            new Permission { Name = "FrontalReservationService.Read", ArabicName = "القراءة على خدمات الحجز frontal", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FrontalReservationService.Create", ArabicName = "الإنشاء على خدمات الحجز frontal", DomainDetailsId = financeDomain.Id },
            
            // FrontalReservationCompany
            new Permission { Name = "FrontalReservationCompany.Read", ArabicName = "القراءة على شركات الحجز frontal", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FrontalReservationCompany.Create", ArabicName = "الإنشاء على شركات الحجز frontal", DomainDetailsId = financeDomain.Id },
            
            // FrontalReservationView
            new Permission { Name = "FrontalReservationView.Read", ArabicName = "القراءة على إطلالات الحجز frontal", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FrontalReservationView.Create", ArabicName = "الإنشاء على إطلالات الحجز frontal", DomainDetailsId = financeDomain.Id },
            
            // FrontalReservationSeason
            new Permission { Name = "FrontalReservationSeason.Read", ArabicName = "القراءة على مواسم الحجز frontal", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FrontalReservationSeason.Create", ArabicName = "الإنشاء على مواسم الحجز frontal", DomainDetailsId = financeDomain.Id },
            
            // FrontalReservationMeal
            new Permission { Name = "FrontalReservationMeal.Read", ArabicName = "القراءة على وجبات الحجز frontal", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FrontalReservationMeal.Create", ArabicName = "الإنشاء على وجبات الحجز frontal", DomainDetailsId = financeDomain.Id },
            
            // FrontalReservation
            new Permission { Name = "FrontalReservation.Read", ArabicName = "القراءة على الحجوزات frontal", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FrontalReservation.Create", ArabicName = "الإنشاء على الحجوزات frontal", DomainDetailsId = financeDomain.Id },
            
            // ReservationAdvancedSearch
            new Permission { Name = "ReservationAdvancedSearch.Read", ArabicName = "القراءة على البحث المتقدم للحجز frontal", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "FrontalReservationAdvancedSearch.Read", ArabicName = "القراءة على البحث المتقدم للحجز frontal", DomainDetailsId = financeDomain.Id },
            
            // ReservationPrintTableTemplates
            new Permission { Name = "ReservationPrintTableTemplates.Read", ArabicName = "القراءة على قوالب طباعة الحجز", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationPrintTableTemplates.Create", ArabicName = "الإنشاء على قوالب طباعة الحجز", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationPrintTableTemplates.Update", ArabicName = "التحديث على قوالب طباعة الحجز", DomainDetailsId = financeDomain.Id },
            new Permission { Name = "ReservationPrintTableTemplates.Delete", ArabicName = "الحذف على قوالب طباعة الحجز", DomainDetailsId = financeDomain.Id },
            
            // HotelAllotment
            new Permission { Name = "HotelAllotment.Read", ArabicName = "القراءة على تخصيص الفندق", DomainDetailsId = financeDomain.Id },
            
            // RoomAvailability
            new Permission { Name = "RoomAvailability.Read", ArabicName = "القراءة على توافر الغرف", DomainDetailsId = financeDomain.Id },

            // Branch Management
            new Permission { Name = "Branch.Read", ArabicName = "عرض الفروع", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Branch.Create", ArabicName = "إضافة وإنشاء الفروع", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Branch.Update", ArabicName = "تحديث الفروع", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "Branch.Delete", ArabicName = "حذف الفروع", DomainDetailsId = systemDomain.Id },

            // Branch Details
            new Permission { Name = "BranchDetails.Read", ArabicName = "عرض تفاصيل الفرع", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "BranchDetails.Create", ArabicName = "إضافة تفاصيل الفرع", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "BranchDetails.Update", ArabicName = "تحديث تفاصيل الفرع", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "BranchDetails.Delete", ArabicName = "حذف تفاصيل الفرع", DomainDetailsId = systemDomain.Id },

            // License & Quota Management
            new Permission { Name = "LicenseManagement.Read", ArabicName = "القراءة على قيود الترخيص والباقت", DomainDetailsId = systemDomain.Id },
            new Permission { Name = "LicenseManagement.Update", ArabicName = "تحديث قيود الترخيص والباقت", DomainDetailsId = systemDomain.Id },
        };

        // ── Cashier Permissions ────────────────────────────────────────
        var cashierDomain = context.DomainDetails.FirstOrDefault(x => x.Name == "Cashier");
        if (cashierDomain != null)
        {
            var cashierPermissions = new List<Permission>
            {
                new Permission { Name = "Cashier.Session.Open",                   ArabicName = "فتح جلسة الكاشير",                      DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.Session.Close",                  ArabicName = "إغلاق جلسة الكاشير",                     DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.Session.SupervisorClose",         ArabicName = "تجاوز مشرف عند إغلاق الجلسة",            DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.Session.CashAction",             ArabicName = "إدخال / سحب نقدي",                       DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.Invoice.Refund",                 ArabicName = "إصدار مرتجع فاتورة",                      DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.Invoice.Void",                   ArabicName = "إلغاء فاتورة",                            DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.Invoice.Discount",               ArabicName = "منح خصم استثنائي",                        DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.Reports.View",                   ArabicName = "عرض تقارير الجلسة المالية",               DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.Reports.ZReport",                ArabicName = "تقرير نهاية اليوم Z",                    DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.Session.MultiTerminalOverride",  ArabicName = "استخدام الجلسة على طرفية أخرى",           DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.POS",                           ArabicName = "نقطة البيع",                              DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.Pharmacy",                      ArabicName = "الصيدلية",                               DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.Restaurant",                    ArabicName = "المطعم",                                 DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.Pricing.Retail",                ArabicName = "سعر البيع القطاعي",                       DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.Pricing.Wholesale",             ArabicName = "سعر البيع الجملة",                        DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.Pricing.Bulk",                  ArabicName = "سعر بيع جملة الجملة",                     DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.Pricing.Purchase",              ArabicName = "سعر الشراء",                              DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.Session.Manage",                ArabicName = "إدارة الجلسة",                            DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.Session.CloseAny",              ArabicName = "إغلاق أي جلسة كاشير / وردية",             DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.Session.ViewAll",               ArabicName = "عرض جميع الورديات",                       DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.Session.SetCustomNumber",        ArabicName = "إدخال رقم وردية يدوي عند الفتح",           DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.Invoice.ViewAll",                ArabicName = "عرض فواتير جميع الورديات",                 DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.Invoice.EditPrice",              ArabicName = "تعديل السعر / المبلغ عند الدفع",           DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.Invoice.ShiftInvoiceCounter",    ArabicName = "تفعيل عداد وتسلسل فواتير الوردية (Shift Counter)", DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.Session.SetAlertLimit",          ArabicName = "تعديل حد تنبيهات المخزون",                  DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.DineIn",                         ArabicName = "إنشاء طلبات محلي في الكاشير",               DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Cashier.AssignTable",                    ArabicName = "ربط طلب الكاشير بطاولة الصالة",            DomainDetailsId = cashierDomain.Id },
            };

            foreach (var p in cashierPermissions)
            {
                permissions.Add(p);
            }
        }

        // ── Pharmacy Permissions ────────────────────────────────────────
        if (cashierDomain != null)
        {
            var pharmacyPermissions = new List<Permission>
            {
                new Permission { Name = "Pharmacy.Read",       ArabicName = "القراءة على الصيدلية",    DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Pharmacy.Create",     ArabicName = "الإنشاء على الصيدلية",    DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Pharmacy.Update",     ArabicName = "التحديث على الصيدلية",    DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Pharmacy.Delete",     ArabicName = "الحذف على الصيدلية",      DomainDetailsId = cashierDomain.Id },

                // 1. الوصفات الطبية
                new Permission { Name = "Pharmacy.Prescriptions.Create",      ArabicName = "الوصفات الطبية - تسجيل وإنشاء وصفة جديدة",                    DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Pharmacy.Prescriptions.Dispense",    ArabicName = "الوصفات الطبية - صرف وتجهيز أدوية الوصفة وتحويلها لنقطة البيع", DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Pharmacy.Prescriptions.Cancel",      ArabicName = "الوصفات الطبية - إلغاء أو تعديل الوصفات المسجلة",               DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Pharmacy.Prescriptions.WasfatySync", ArabicName = "الوصفات الطبية - المزامنة والسحب من منصة وصفتي الإلكترونية",  DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Pharmacy.Prescriptions.ViewHistory", ArabicName = "الوصفات الطبية - استعراض سجل وأرشيف وصفات المرضى",           DomainDetailsId = cashierDomain.Id },

                // 2. الرقابة الدوائية والأدوية المقيدة
                new Permission { Name = "Pharmacy.ControlledDrugs.View",          ArabicName = "الرقابة الدوائية - استعراض سجل وجداول الأدوية المقيدة والمخدرة",   DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Pharmacy.ControlledDrugs.Dispense",      ArabicName = "الرقابة الدوائية - اعتماد وصرف الأدوية الخاضعة للرقابة وإصدار الأذونات", DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Pharmacy.ControlledDrugs.ApproveReject",  ArabicName = "الرقابة الدوائية - قبول أو رفض طلبات صرف الأدوية المقيدة",       DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Pharmacy.ControlledDrugs.ExportReports",  ArabicName = "الرقابة الدوائية - تصدير وطباعة تقارير وكشوفات التفتيش والرقابة",   DomainDetailsId = cashierDomain.Id },

                // 3. ملفات المرضى
                new Permission { Name = "Pharmacy.Patients.CreateEdit",           ArabicName = "ملفات المرضى - فتح وتعديل بيانات ملفات المرضى والتأمين",       DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Pharmacy.Patients.ManageClinicalAlerts", ArabicName = "ملفات المرضى - تسجيل وتعديل التنبيهات الإكلينيكية والحساسيات والأمراض المزمنة", DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Pharmacy.Patients.Delete",               ArabicName = "ملفات المرضى - حذف وأرشفة ملف مريض",                           DomainDetailsId = cashierDomain.Id },

                // 4. التأمين الصيدلي
                new Permission { Name = "Pharmacy.Insurance.EligibilityCheck", ArabicName = "التأمين الصيدلي - التحقق من أهلية وبطاقات التأمين الطبي",        DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Pharmacy.Insurance.SubmitClaim",      ArabicName = "التأمين الصيدلي - رفع وإرسال المطالبات لشركات التأمين",            DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Pharmacy.Insurance.SettleClaims",     ArabicName = "التأمين الصيدلي - تسوية وتأكيد سداد مبالغ المطالبات التأمينية",    DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Pharmacy.Insurance.ManagePolicies",   ArabicName = "التأمين الصيدلي - إدارة بوالص وفئات وخصومات شركات التأمين",        DomainDetailsId = cashierDomain.Id },

                // 5. المخزون والتشغيلات والصلاحيات
                new Permission { Name = "Pharmacy.Batches.View",           ArabicName = "المخزون الدوائي - استعراض تشغيلات الأدوية وتواريخ الصلاحية والباركود", DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Pharmacy.Batches.Manage",         ArabicName = "المخزون الدوائي - إدخال وتعديل أرقام الباتشات وتواريخ انتهاء الصلاحية", DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Pharmacy.Pricing.Manage",         ArabicName = "المخزون الدوائي - تعديل أسعار بيع الأدوية (قطاعي، جملة، تأمين)",     DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Pharmacy.ExpiryAlerts.Manage",    ArabicName = "المخزون الدوائي - إدارة تنبيهات الأدوية قريبة الانتهاء والتوالف",    DomainDetailsId = cashierDomain.Id },

                // 6. كاشير ونقاط بيع الصيدلية
                new Permission { Name = "Pharmacy.POS.ApplyDiscount",     ArabicName = "كاشير الصيدلية - منح خصم يدوي إضافي على الفاتورة",                   DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Pharmacy.POS.ReturnDrugs",        ArabicName = "كاشير الصيدلية - عمل مرتجع مبيعات للأدوية",                          DomainDetailsId = cashierDomain.Id },
                new Permission { Name = "Pharmacy.POS.CloseShiftSummary",  ArabicName = "كاشير الصيدلية - إغلاق الوردية والاطلاع على التقرير المالي",           DomainDetailsId = cashierDomain.Id },
            };

            foreach (var p in pharmacyPermissions)
            {
                permissions.Add(p);
            }
        }

        foreach (var permission in permissions)
        {
            if (!context.Permissions.Any(x => x.Name == permission.Name))
            {
                context.Permissions.Add(permission);
            }
        }

        foreach (var devPermission in DeveloperPermissions.All)
        {
            var existing = context.Permissions.FirstOrDefault(x => x.Name == devPermission);
            DeveloperPermissions.ArabicNames.TryGetValue(devPermission, out var arName);
            if (existing == null)
            {
                context.Permissions.Add(new Permission 
                { 
                    Name = devPermission, 
                    ArabicName = arName ?? devPermission, 
                    DomainDetailsId = systemDomain.Id 
                });
            }
            else if (existing.ArabicName != arName)
            {
                existing.ArabicName = arName;
            }
        }

        context.SaveChanges();
    }

    public static void SeedRolePermissions(ERPContext context)
    {
        // ── Remove obsolete/deprecated permissions from DB ──
        var obsoleteNames = new[] { "SalesInvoice.Export", "SalesInvoice.Share" };
        var obsoletePermissions = context.Permissions.Where(x => obsoleteNames.Contains(x.Name)).ToList();
        if (obsoletePermissions.Any())
        {
            var obsoleteIds = obsoletePermissions.Select(x => x.Id).ToList();
            var obsoleteRolePerms = context.RolePermissions.Where(rp => obsoleteIds.Contains(rp.PermissionId)).ToList();
            context.RolePermissions.RemoveRange(obsoleteRolePerms);
            context.Permissions.RemoveRange(obsoletePermissions);
            context.SaveChanges();
        }

        var roles = context.Roles.ToList();
        var permissions = context.Permissions.ToList();

        // ── Backward-compat: assign ALL permissions to ALL active roles ──
        // This ensures existing users are not locked out when new permissions
        // (e.g. Dashboard.Read) become required for previously-open pages.
        var newPermissionNames = new[]
        {
            "Dashboard.Read",
            "RoomAvailability.Read",
            "ReservationPurchases.Read",
            "ReservationReports.Read",
            "Cashier.POS", "Cashier.Pharmacy", "Cashier.Restaurant", "Cashier.Session.Manage", "Cashier.Session.CloseAny", "Cashier.Session.ViewAll", "Cashier.Session.SetCustomNumber", "Cashier.Session.SetAlertLimit",
            "Pharmacy.Read", "Pharmacy.Create", "Pharmacy.Update", "Pharmacy.Delete",
            "SalesInvoice.Import", "Transaction.Import", "Transaction.Export",
            "SalesInvoice.ExportExcel", "SalesInvoice.ExportPdfZip", "SalesInvoice.ExportPdfMerged",
            "SalesInvoice.PrintSelected", "SalesInvoice.Print3", "SalesInvoice.DownloadSelectedPdf", "SalesInvoice.DownloadSelectedMergedPdf", "SalesInvoice.ExportSelectedExcel", "SalesInvoice.BulkShare",
            "BranchDetails.Read", "BranchDetails.Create", "BranchDetails.Update", "BranchDetails.Delete",
            "FrontalSalesRepresentative.Update", "FrontalSalesRepresentative.Delete",
        };
        var newPermissions = permissions.Where(p => newPermissionNames.Contains(p.Name)).ToList();
        foreach (var role in roles.Where(r => r.IsActive))
        {
            var missing = newPermissions
                .Where(np => !context.RolePermissions.Any(rp => rp.RoleId == role.Id && rp.PermissionId == np.Id))
                .Select(np => new RolePermission { RoleId = role.Id, PermissionId = np.Id })
                .ToList();
            if (missing.Any())
            {
                context.RolePermissions.AddRange(missing);
            }
        }
        context.SaveChanges();

        foreach (var role in roles)
        {
            if (role.Name.Contains("System"))
            {
                var rolePermissions = permissions
                    .Where(x => role.Name == "System.Developer" || !DeveloperPermissions.All.Contains(x.Name))
                    .Select(x => new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = x.Id
                    })
                    .Where(rp => !context.RolePermissions.Any(r => r.RoleId == rp.RoleId && r.PermissionId == rp.PermissionId))
                    .ToList();

                context.RolePermissions.AddRange(rolePermissions);
            }
            if (role.Name.Contains("Inventory"))
            {
                if (role.Name.Contains("Admin"))
                {
                    var rolePermissions = permissions
                        .Where(x => x.Name.Contains("Inventory"))
                        .Select(x => new RolePermission
                        {
                            RoleId = role.Id,
                            PermissionId = x.Id
                        })
                        .Where(rp => !context.RolePermissions.Any(r => r.RoleId == rp.RoleId && r.PermissionId == rp.PermissionId))
                        .ToList();

                    context.RolePermissions.AddRange(rolePermissions);
                }
                else if (role.Name.Contains("User"))
                {
                    var readPermission = permissions.FirstOrDefault(x => x.Name.Contains("Inventory") && x.Name.Contains("Read"));
                    if (readPermission != null && !context.RolePermissions.Any(r => r.RoleId == role.Id && r.PermissionId == readPermission.Id))
                    {
                        context.RolePermissions.Add(new RolePermission
                        {
                            RoleId = role.Id,
                            PermissionId = readPermission.Id
                        });
                    }
                }
            }
            if (role.Name.Contains("Finance"))
            {
                if (role.Name.Contains("Admin"))
                {
                    var rolePermissions = permissions
                        .Where(x => x.Name.Contains("Finance"))
                        .Select(x => new RolePermission
                        {
                            RoleId = role.Id,
                            PermissionId = x.Id
                        })
                        .Where(rp => !context.RolePermissions.Any(r => r.RoleId == rp.RoleId && r.PermissionId == rp.PermissionId))
                        .ToList();

                    context.RolePermissions.AddRange(rolePermissions);
                }
                else if (role.Name.Contains("User"))
                {
                    var readPermission = permissions.FirstOrDefault(x => x.Name.Contains("Finance") && x.Name.Contains("Read"));
                    if (readPermission != null && !context.RolePermissions.Any(r => r.RoleId == role.Id && r.PermissionId == readPermission.Id))
                    {
                        context.RolePermissions.Add(new RolePermission
                        {
                            RoleId = role.Id,
                            PermissionId = readPermission.Id
                        });
                    }
                }
            }
            if (role.Name == "Reservation")
            {
                var excludedPrefixes = new[] { "Projects.", "SmartBooking.", "Cashier.POS", "Cashier.Pharmacy", "Pharmacy." };
                var reservationPermissionPrefixes = new[]
                {
                    "Hotels", "ReservationMeal", "RoomType", "ReservationSupplier",
                    "ReservationService", "ReservationCompany", "ReservationView",
                    "ReservationSeason", "Reservation", "ReservationPurchases",
                    "ReservationInvoice", "ReservationPrintTableTemplates",
                    "HotelAllotment", "RoomAvailability", "ReservationAdvancedSearch",
                    "FrontalHotel", "FrontalRoomType", "FrontalReservationSupplier",
                    "FrontalReservationService", "FrontalReservationCompany",
                    "FrontalReservationView", "FrontalReservationSeason",
                    "FrontalReservationMeal", "FrontalReservation",
                    "FrontalReservationAdvancedSearch", "Dashboard.Read"
                };
                var rolePermissions = permissions
                    .Where(x => reservationPermissionPrefixes.Any(p => x.Name.StartsWith(p))
                                && !excludedPrefixes.Any(ep => x.Name.StartsWith(ep)))
                    .Select(x => new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = x.Id
                    })
                    .Where(rp => !context.RolePermissions.Any(r => r.RoleId == rp.RoleId && r.PermissionId == rp.PermissionId))
                    .ToList();

                context.RolePermissions.AddRange(rolePermissions);
            }
            if (role.Name == "Cashier")
            {
                var rolePermissions = permissions
                    .Where(x => x.Name.StartsWith("Cashier") || x.Name.StartsWith("Pharmacy"))
                    .Select(x => new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = x.Id
                    })
                    .Where(rp => !context.RolePermissions.Any(r => r.RoleId == rp.RoleId && r.PermissionId == rp.PermissionId))
                    .ToList();

                context.RolePermissions.AddRange(rolePermissions);
            }
        }

        context.SaveChanges();
    }

    public static void SeedAdminUser(ERPContext context)
    {
        if (!context.Users.Any(x => x.UserName == "admin"))
        {
            var user = new User
            {
                UserName = "admin",
                WorkId = "00000",
                FirstName = "admin",
                MiddleName = "admin",
                LastName = "admin",
                Password = "1994053a606a8f0714002f50561b52c0064d203c91c8349dc9af2233ac4917c6", // hashed 11223344 
                Email = "admin@admin.com",
                TwoFactorEnabled = false,
                EmailVerified = true,
                DateOfBirth = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };

            context.Users.Add(user);
            context.SaveChanges();

            var roles = context.Roles.ToList();
            var userRoles = roles.Select(role => new UserRole { UserId = user.Id, RoleId = role.Id }).ToList();

            context.UserRoles.AddRange(userRoles);
            context.SaveChanges();
        }

        // Reservation user - only has Reservation domain permissions
        if (!context.Users.Any(x => x.UserName == "reservation_user"))
        {
            var reservationRole = context.Roles.FirstOrDefault(x => x.Name == "Reservation");
            if (reservationRole != null)
            {
                var reservationUser = new User
                {
                    UserName = "reservation_user",
                    WorkId = "00001",
                    FirstName = "reservation",
                    MiddleName = "user",
                    LastName = "test",
                    Password = "1994053a606a8f0714002f50561b52c0064d203c91c8349dc9af2233ac4917c6", // hashed 11223344
                    Email = "reservation@gmail.com",
                    TwoFactorEnabled = false,
                    EmailVerified = true,
                    DateOfBirth = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                };

                context.Users.Add(reservationUser);
                context.SaveChanges();

                context.UserRoles.Add(new UserRole { UserId = reservationUser.Id, RoleId = reservationRole.Id });
                context.SaveChanges();
            }
        }

        // Cashier user - only has Cashier domain permissions
        if (!context.Users.Any(x => x.UserName == "cashier_user"))
        {
            var cashierRole = context.Roles.FirstOrDefault(x => x.Name == "Cashier");
            if (cashierRole != null)
            {
                var cashierUser = new User
                {
                    UserName = "cashier_user",
                    WorkId = "00002",
                    FirstName = "cashier",
                    MiddleName = "user",
                    LastName = "test",
                    Password = "1994053a606a8f0714002f50561b52c0064d203c91c8349dc9af2233ac4917c6", // hashed 11223344
                    Email = "cashier@gmail.com",
                    TwoFactorEnabled = false,
                    EmailVerified = true,
                    DateOfBirth = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                };

                context.Users.Add(cashierUser);
                context.SaveChanges();

                context.UserRoles.Add(new UserRole { UserId = cashierUser.Id, RoleId = cashierRole.Id });
                context.SaveChanges();
            }
        }

        // Developer user
        if (!context.Users.Any(x => x.UserName == "sys_developer"))
        {
            var devRole = context.Roles.FirstOrDefault(x => x.Name == "System.Developer");
            if (devRole != null)
            {
                var devUser = new User
                {
                    UserName = "sys_developer",
                    WorkId = "00003",
                    FirstName = "System",
                    MiddleName = "Developer",
                    LastName = "User",
                    Password = "7275bf1f048257989f2b2b5beee3c440f904325c37628a1d8a073616a3c3c1c1", // HMAC hashed Dev@SmartPro2026!
                    Email = "developer@smartapro.com",
                    TwoFactorEnabled = false,
                    EmailVerified = true,
                    DateOfBirth = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                };

                context.Users.Add(devUser);
                context.SaveChanges();

                context.UserRoles.Add(new UserRole { UserId = devUser.Id, RoleId = devRole.Id });
                context.SaveChanges();
            }
        }
    }
}