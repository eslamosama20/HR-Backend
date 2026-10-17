namespace SmartAndPro.ERP.SharedKernel.Constants;


public static class Permissions
{
    // System & General
    public static class System
    {
        public const string Read = "System.Read";
        public const string Create = "System.Create";
        public const string Update = "System.Update";
        public const string Delete = "System.Delete";
        public const string Admin = "System.Admin";
        public const string Developer = "System.Developer";
    }

    public static class Dashboard
    {
        public const string Read = "Dashboard.Read";
        public const string SalesTotal = "Dashboard.SalesTotal";
        public const string PurchaseTotal = "Dashboard.PurchaseTotal";
        public const string SalesCount = "Dashboard.SalesCount";
        public const string PurchaseCount = "Dashboard.PurchaseCount";
        public const string ReservationsCount = "Dashboard.ReservationsCount";
        public const string ReservationsConfirmed = "Dashboard.ReservationsConfirmed";
        public const string InventoryItems = "Dashboard.InventoryItems";
        public const string CashIn30 = "Dashboard.CashIn30";
        public const string CashOut30 = "Dashboard.CashOut30";
        public const string Treasuries = "Dashboard.Treasuries";
        public const string StoreCount = "Dashboard.StoreCount";
        public const string SupplierCount = "Dashboard.SupplierCount";
        public const string BankAccounts = "Dashboard.BankAccounts";
    }

    // Projects
    public static class Projects
    {
        public const string Read = "Projects.Read";
        public const string Create = "Projects.Create";
        public const string Update = "Projects.Update";
        public const string Delete = "Projects.Delete";
        
        public static class Assignment
        {
            public const string Read = "Projects.Assignment.Read";
            public const string Update = "Projects.Assignment.Update";
        }

        public static class Execution
        {
            public const string Read = "Projects.Execution.Read";
            public const string Update = "Projects.Execution.Update";
        }

        public static class Documents
        {
            public const string Read = "Projects.Documents.Read";
            public const string Create = "Projects.Documents.Create";
            public const string Delete = "Projects.Documents.Delete";
        }

        public static class Payments
        {
            public const string Read = "Projects.Payments.Read";
            public const string Create = "Projects.Payments.Create";
            public const string Update = "Projects.Payments.Update";
            public const string Delete = "Projects.Payments.Delete";
        }

        public static class PriceQuotes
        {
            public const string Read = "Projects.PriceQuotes.Read";
            public const string Create = "Projects.PriceQuotes.Create";
            public const string Update = "Projects.PriceQuotes.Update";
            public const string Delete = "Projects.PriceQuotes.Delete";
            public const string Accept = "Projects.PriceQuotes.Accept";
        }

        public static class Assessments
        {
            public const string Read = "Projects.Assessments.Read";
            public const string Create = "Projects.Assessments.Create";
            public const string Update = "Projects.Assessments.Update";
            public const string Delete = "Projects.Assessments.Delete";
        }

        public static class Reports
        {
            public const string Read = "Projects.Reports.Read";
        }
    }

    // Smart Booking
    public static class SmartBooking
    {
        public const string Read = "SmartBooking.Read";
    }

    // Sales
    public static class MainSalesClass
    {
        public const string Read = "MainSalesClass.Read";
        public const string Create = "MainSalesClass.Create";
        public const string Update = "MainSalesClass.Update";
        public const string Delete = "MainSalesClass.Delete";
    }

    public static class SalesInvoice
    {
        public const string Read = "SalesInvoice.Read";
        public const string Create = "SalesInvoice.Create";
        public const string Update = "SalesInvoice.Update";
        public const string Delete = "SalesInvoice.Delete";
        public const string Import = "SalesInvoice.Import";
        public const string ExportExcel = "SalesInvoice.ExportExcel";
        public const string ExportSelectedExcel = "SalesInvoice.ExportSelectedExcel";
        public const string ExportPdfZip = "SalesInvoice.ExportPdfZip";
        public const string ExportPdfMerged = "SalesInvoice.ExportPdfMerged";
        public const string PrintSelected = "SalesInvoice.PrintSelected";
        public const string Print3 = "SalesInvoice.Print3";
        public const string DownloadSelectedPdf = "SalesInvoice.DownloadSelectedPdf";
        public const string DownloadSelectedMergedPdf = "SalesInvoice.DownloadSelectedMergedPdf";
        public const string BulkShare = "SalesInvoice.BulkShare";
    }

    public static class FrontalSalesInvoice
    {
        public const string Read = "FrontalSalesInvoice.Read";
        public const string Create = "FrontalSalesInvoice.Create";
    }

    public static class SalesQuotation
    {
        public const string Read = "SalesQuotation.Read";
        public const string Create = "SalesQuotation.Create";
        public const string Update = "SalesQuotation.Update";
        public const string Delete = "SalesQuotation.Delete";
    }

    public static class SalesReturn
    {
        public const string Read = "SalesReturn.Read";
        public const string Create = "SalesReturn.Create";
        public const string Update = "SalesReturn.Update";
        public const string Delete = "SalesReturn.Delete";
    }

    public static class SalesCreditNote
    {
        public const string Read = "SalesCreditNote.Read";
        public const string Create = "SalesCreditNote.Create";
        public const string Update = "SalesCreditNote.Update";
        public const string Delete = "SalesCreditNote.Delete";
    }

    // Purchases
    public static class MainPurchaseClass
    {
        public const string Read = "MainPurchaseClass.Read";
        public const string Create = "MainPurchaseClass.Create";
        public const string Update = "MainPurchaseClass.Update";
        public const string Delete = "MainPurchaseClass.Delete";
    }

    public static class PurchaseInvoice
    {
        public const string Read = "PurchaseInvoice.Read";
        public const string Create = "PurchaseInvoice.Create";
        public const string Update = "PurchaseInvoice.Update";
        public const string Delete = "PurchaseInvoice.Delete";
    }

    public static class FrontalPurchaseInvoice
    {
        public const string Read = "FrontalPurchaseInvoice.Read";
        public const string Create = "FrontalPurchaseInvoice.Create";
    }

    public static class PurchaseReturn
    {
        public const string Read = "PurchaseReturn.Read";
        public const string Create = "PurchaseReturn.Create";
        public const string Update = "PurchaseReturn.Update";
        public const string Delete = "PurchaseReturn.Delete";
    }

    public static class DebitNote
    {
        public const string Read = "DebitNote.Read";
        public const string Create = "DebitNote.Create";
        public const string Update = "DebitNote.Update";
        public const string Delete = "DebitNote.Delete";
    }

    // Inventory
    public static class InventoryManagement
    {
        public const string Read = "InventoryManagement.Read";
        public const string Create = "InventoryManagement.Create";
        public const string Update = "InventoryManagement.Update";
        public const string Delete = "InventoryManagement.Delete";
    }

    public static class MeasurementUnit
    {
        public const string Read = "MeasurementUnit.Read";
        public const string Create = "MeasurementUnit.Create";
        public const string Update = "MeasurementUnit.Update";
        public const string Delete = "MeasurementUnit.Delete";
    }

    public static class StoreDetails
    {
        public const string Read = "StoreDetails.Read";
        public const string Create = "StoreDetails.Create";
        public const string Update = "StoreDetails.Update";
        public const string Delete = "StoreDetails.Delete";
    }

    public static class InventoryPermit
    {
        public const string Read = "InventoryPermit.Read";
        public const string Create = "InventoryPermit.Create";
        public const string Update = "InventoryPermit.Update";
        public const string Delete = "InventoryPermit.Delete";
    }

    public static class InventoryStocktaking
    {
        public const string Read = "InventoryStocktaking.Read";
        public const string Create = "InventoryStocktaking.Create";
        public const string Update = "InventoryStocktaking.Update";
        public const string Delete = "InventoryStocktaking.Delete";
    }

    // Recipes & Kitchens
    public static class Recipe
    {
        public const string Read = "Recipe.Read";
        public const string Create = "Recipe.Create";
        public const string Update = "Recipe.Update";
        public const string Delete = "Recipe.Delete";
    }

    public static class MealReservation
    {
        public const string Read = "MealReservation.Read";
        public const string Create = "MealReservation.Create";
        public const string Update = "MealReservation.Update";
        public const string Delete = "MealReservation.Delete";
    }

    // Accounting & Finance
    public static class Account
    {
        public const string Read = "Account.Read";
        public const string Create = "Account.Create";
        public const string Update = "Account.Update";
        public const string Delete = "Account.Delete";
    }

    public static class Transaction
    {
        public const string Read = "Transaction.Read";
        public const string Create = "Transaction.Create";
        public const string Update = "Transaction.Update";
        public const string Delete = "Transaction.Delete";
        public const string Import = "Transaction.Import";
        public const string Export = "Transaction.Export";
    }

    public static class FrontalTransaction
    {
        public const string Read = "FrontalTransaction.Read";
        public const string Create = "FrontalTransaction.Create";
    }

    public static class PaymentReceipt
    {
        public const string Read = "PaymentReceipt.Read";
        public const string Create = "PaymentReceipt.Create";
        public const string Update = "PaymentReceipt.Update";
        public const string Delete = "PaymentReceipt.Delete";
    }

    public static class FrontalPaymentReceipt
    {
        public const string Read = "FrontalPaymentReceipt.Read";
        public const string Create = "FrontalPaymentReceipt.Create";
    }

    public static class PaycheckReceipt
    {
        public const string Read = "PaycheckReceipt.Read";
        public const string Create = "PaycheckReceipt.Create";
        public const string Update = "PaycheckReceipt.Update";
        public const string Delete = "PaycheckReceipt.Delete";
    }

    public static class FrontalPaycheckReceipt
    {
        public const string Read = "FrontalPaycheckReceipt.Read";
        public const string Create = "FrontalPaycheckReceipt.Create";
    }

    public static class Treasury
    {
        public const string Read = "Treasury.Read";
        public const string Create = "Treasury.Create";
        public const string Update = "Treasury.Update";
        public const string Delete = "Treasury.Delete";
    }

    public static class FrontalTreasury
    {
        public const string Read = "FrontalTreasury.Read";
        public const string Create = "FrontalTreasury.Create";
    }

    public static class FinancePeriods
    {
        public const string Read = "FinancePeriods.Read";
        public const string Create = "FinancePeriods.Create";
        public const string Update = "FinancePeriods.Update";
        public const string Delete = "FinancePeriods.Delete";
    }

    public static class ExpensesCenter
    {
        public const string Read = "ExpensesCenter.Read";
        public const string Create = "ExpensesCenter.Create";
        public const string Update = "ExpensesCenter.Update";
        public const string Delete = "ExpensesCenter.Delete";
    }

    // Customers & Suppliers
    public static class CustomerProfile
    {
        public const string Read = "CustomerProfile.Read";
        public const string Create = "CustomerProfile.Create";
        public const string Update = "CustomerProfile.Update";
        public const string Delete = "CustomerProfile.Delete";
    }

    public static class FrontalCustomerProfile
    {
        public const string Read = "FrontalCustomerProfile.Read";
        public const string Create = "FrontalCustomerProfile.Create";
    }

    public static class SupplierDetails
    {
        public const string Read = "SupplierDetails.Read";
        public const string Create = "SupplierDetails.Create";
        public const string Update = "SupplierDetails.Update";
        public const string Delete = "SupplierDetails.Delete";
    }

    public static class FrontalSupplierDetails
    {
        public const string Read = "FrontalSupplierDetails.Read";
        public const string Create = "FrontalSupplierDetails.Create";
    }

    // Reservations & Hotels
    public static class Hotels
    {
        public const string Read = "Hotels.Read";
        public const string Create = "Hotels.Create";
        public const string Update = "Hotels.Update";
        public const string Delete = "Hotels.Delete";
    }

    public static class FrontalHotel
    {
        public const string Read = "FrontalHotel.Read";
        public const string Create = "FrontalHotel.Create";
    }

    public static class RoomType
    {
        public const string Read = "RoomType.Read";
        public const string Create = "RoomType.Create";
        public const string Update = "RoomType.Update";
        public const string Delete = "RoomType.Delete";
    }

    public static class FrontalRoomType
    {
        public const string Read = "FrontalRoomType.Read";
        public const string Create = "FrontalRoomType.Create";
    }

    public static class ReservationMeal
    {
        public const string Read = "ReservationMeal.Read";
        public const string Create = "ReservationMeal.Create";
        public const string Update = "ReservationMeal.Update";
        public const string Delete = "ReservationMeal.Delete";
    }

    public static class FrontalReservationMeal
    {
        public const string Read = "FrontalReservationMeal.Read";
        public const string Create = "FrontalReservationMeal.Create";
    }

    public static class SalesRepresentative
    {
        public const string Read = "SalesRepresentative.Read";
        public const string Create = "SalesRepresentative.Create";
        public const string Update = "SalesRepresentative.Update";
        public const string Delete = "SalesRepresentative.Delete";
    }

    public static class FrontalSalesRepresentative
    {
        public const string Read = "FrontalSalesRepresentative.Read";
        public const string Create = "FrontalSalesRepresentative.Create";
        public const string Update = "FrontalSalesRepresentative.Update";
        public const string Delete = "FrontalSalesRepresentative.Delete";
    }

    public static class ReservationSupplier
    {
        public const string Read = "ReservationSupplier.Read";
        public const string Create = "ReservationSupplier.Create";
        public const string Update = "ReservationSupplier.Update";
        public const string Delete = "ReservationSupplier.Delete";
    }

    public static class FrontalReservationSupplier
    {
        public const string Read = "FrontalReservationSupplier.Read";
        public const string Create = "FrontalReservationSupplier.Create";
    }

    public static class ReservationService
    {
        public const string Read = "ReservationService.Read";
        public const string Create = "ReservationService.Create";
        public const string Update = "ReservationService.Update";
        public const string Delete = "ReservationService.Delete";
    }

    public static class FrontalReservationService
    {
        public const string Read = "FrontalReservationService.Read";
        public const string Create = "FrontalReservationService.Create";
    }

    public static class ReservationCompany
    {
        public const string Read = "ReservationCompany.Read";
        public const string Create = "ReservationCompany.Create";
        public const string Update = "ReservationCompany.Update";
        public const string Delete = "ReservationCompany.Delete";
    }

    public static class FrontalReservationCompany
    {
        public const string Read = "FrontalReservationCompany.Read";
        public const string Create = "FrontalReservationCompany.Create";
    }

    public static class ReservationView
    {
        public const string Read = "ReservationView.Read";
        public const string Create = "ReservationView.Create";
        public const string Update = "ReservationView.Update";
        public const string Delete = "ReservationView.Delete";
    }

    public static class FrontalReservationView
    {
        public const string Read = "FrontalReservationView.Read";
        public const string Create = "FrontalReservationView.Create";
    }

    public static class ReservationSeason
    {
        public const string Read = "ReservationSeason.Read";
        public const string Create = "ReservationSeason.Create";
        public const string Update = "ReservationSeason.Update";
        public const string Delete = "ReservationSeason.Delete";
    }

    public static class FrontalReservationSeason
    {
        public const string Read = "FrontalReservationSeason.Read";
        public const string Create = "FrontalReservationSeason.Create";
    }

    public static class Reservation
    {
        public const string Read = "Reservation.Read";
        public const string Create = "Reservation.Create";
        public const string Update = "Reservation.Update";
        public const string Delete = "Reservation.Delete";
    }

    public static class FrontalReservation
    {
        public const string Read = "FrontalReservation.Read";
        public const string Create = "FrontalReservation.Create";
    }

    public static class ReservationPurchases
    {
        public const string Read = "ReservationPurchases.Read";
        public const string Create = "ReservationPurchases.Create";
        public const string Update = "ReservationPurchases.Update";
        public const string Delete = "ReservationPurchases.Delete";
    }

    public static class ReservationInvoice
    {
        public const string Read = "ReservationInvoice.Read";
        public const string Create = "ReservationInvoice.Create";
        public const string Update = "ReservationInvoice.Update";
        public const string Delete = "ReservationInvoice.Delete";
    }

    public static class RoomAvailability
    {
        public const string Read = "RoomAvailability.Read";
    }

    public static class HotelAllotment
    {
        public const string Read = "HotelAllotment.Read";
    }

    public static class ReservationReports
    {
        public const string Read = "ReservationReports.Read";
    }

    public static class ReservationAdvancedSearch
    {
        public const string Read = "ReservationAdvancedSearch.Read";
    }

    public static class FrontalReservationAdvancedSearch
    {
        public const string Read = "FrontalReservationAdvancedSearch.Read";
    }

    // Reports
    public static class InventoryReport
    {
        public const string Read = "InventoryReport.Read";
    }

    public static class SupplierReport
    {
        public const string Read = "SupplierReport.Read";
    }

    public static class PurchasableCardReport
    {
        public const string Read = "PurchasableCardReport.Read";
    }

    public static class PurchasableReport
    {
        public const string Read = "PurchasableReport.Read";
    }

    public static class SalesInvoiceReport
    {
        public const string Read = "SalesInvoiceReport.Read";
    }

    public static class ReviewBalance
    {
        public const string Read = "ReviewBalance.Read";
    }

    public static class ProfitLossAccount
    {
        public const string Read = "ProfitLossAccount.Read";
    }

    public static class BudgetAssessment
    {
        public const string Read = "BudgetAssessment.Read";
    }

    public static class AccountStatement
    {
        public const string Read = "AccountStatement.Read";
    }

    public static class FrontalAccountStatement
    {
        public const string Read = "FrontalAccountStatement.Read";
        public const string Create = "FrontalAccountStatement.Create";
    }

    // Management & Settings
    public static class UserManagement
    {
        public const string Read = "UserManagement.Read";
        public const string Create = "UserManagement.Create";
        public const string Update = "UserManagement.Update";
        public const string Delete = "UserManagement.Delete";
    }

    public static class FrontalUserManagement
    {
        public const string Read = "FrontalUserManagement.Read";
    }

    public static class RoleManagement
    {
        public const string Read = "RoleManagement.Read";
        public const string Create = "RoleManagement.Create";
        public const string Update = "RoleManagement.Update";
        public const string Delete = "RoleManagement.Delete";
    }

    public static class FrontalRoleManagement
    {
        public const string Read = "FrontalRoleManagement.Read";
    }

    public static class CompanyDetails
    {
        public const string Read = "CompanyDetails.Read";
        public const string Create = "CompanyDetails.Create";
        public const string Update = "CompanyDetails.Update";
        public const string Delete = "CompanyDetails.Delete";
    }

    public static class SystemSetting
    {
        public const string Read = "SystemSetting.Read";
        public const string Create = "SystemSetting.Create";
        public const string Update = "SystemSetting.Update";
        public const string Delete = "SystemSetting.Delete";
    }

    public static class PdfTemplateConfigurator
    {
        public const string Read = "PdfTemplateConfigurator.Read";
    }

    public static class LicenseManagement
    {
        public const string Read = "LicenseManagement.Read";
        public const string Update = "LicenseManagement.Update";
    }

    public static class Branch
    {
        public const string Read = "Branch.Read";
        public const string Create = "Branch.Create";
        public const string Update = "Branch.Update";
        public const string Delete = "Branch.Delete";
    }

    public static class ReservationPrintTableTemplates
    {
        public const string Read = "ReservationPrintTableTemplates.Read";
    }

    // Cashier & POS
    public static class Cashier
    {
        public const string POS = "Cashier.POS";
        public const string Pharmacy = "Cashier.Pharmacy";
        public const string Restaurant = "Cashier.Restaurant";

        public static class Pricing
        {
            public const string Retail = "Cashier.Pricing.Retail";
            public const string Wholesale = "Cashier.Pricing.Wholesale";
            public const string Bulk = "Cashier.Pricing.Bulk";
            public const string Purchase = "Cashier.Pricing.Purchase";
        }

        public static class Session
        {
            public const string Manage = "Cashier.Session.Manage";
            public const string CloseAnySession = "Cashier.Session.CloseAny";
            public const string SetCustomNumber = "Cashier.Session.SetCustomNumber";
            public const string ViewAll = "Cashier.Session.ViewAll";
            public const string ViewAllSessions = "Cashier.Session.ViewAll";
        }

        public static class Invoice
        {
            public const string ViewAll = "Cashier.Invoice.ViewAll";
            public const string Discount = "Cashier.Invoice.Discount";
            public const string EditPrice = "Cashier.Invoice.EditPrice";
            public const string ShiftInvoiceCounter = "Cashier.Invoice.ShiftInvoiceCounter";
        }
    }

    // Pharmacy
    public static class Pharmacy
    {
        public const string Read = "Pharmacy.Read";
        public const string Create = "Pharmacy.Create";
        public const string Update = "Pharmacy.Update";
        public const string Delete = "Pharmacy.Delete";

        // 1. Prescriptions
        public static class Prescriptions
        {
            public const string Create = "Pharmacy.Prescriptions.Create";
            public const string Dispense = "Pharmacy.Prescriptions.Dispense";
            public const string Cancel = "Pharmacy.Prescriptions.Cancel";
            public const string WasfatySync = "Pharmacy.Prescriptions.WasfatySync";
            public const string ViewHistory = "Pharmacy.Prescriptions.ViewHistory";
        }

        // 2. Controlled & Restricted Drugs
        public static class ControlledDrugs
        {
            public const string View = "Pharmacy.ControlledDrugs.View";
            public const string Dispense = "Pharmacy.ControlledDrugs.Dispense";
            public const string ApproveReject = "Pharmacy.ControlledDrugs.ApproveReject";
            public const string ExportReports = "Pharmacy.ControlledDrugs.ExportReports";
        }

        // 3. Patients & Clinical Profiles
        public static class Patients
        {
            public const string CreateEdit = "Pharmacy.Patients.CreateEdit";
            public const string ManageClinicalAlerts = "Pharmacy.Patients.ManageClinicalAlerts";
            public const string Delete = "Pharmacy.Patients.Delete";
        }

        // 4. Insurance & Claims
        public static class Insurance
        {
            public const string EligibilityCheck = "Pharmacy.Insurance.EligibilityCheck";
            public const string SubmitClaim = "Pharmacy.Insurance.SubmitClaim";
            public const string SettleClaims = "Pharmacy.Insurance.SettleClaims";
            public const string ManagePolicies = "Pharmacy.Insurance.ManagePolicies";
        }

        // 5. Batches, Expiry & Pricing
        public static class Inventory
        {
            public const string BatchesView = "Pharmacy.Batches.View";
            public const string BatchesManage = "Pharmacy.Batches.Manage";
            public const string PricingManage = "Pharmacy.Pricing.Manage";
            public const string ExpiryAlertsManage = "Pharmacy.ExpiryAlerts.Manage";
        }

        // 6. POS Controls
        public static class POS
        {
            public const string ApplyDiscount = "Pharmacy.POS.ApplyDiscount";
            public const string ReturnDrugs = "Pharmacy.POS.ReturnDrugs";
            public const string CloseShiftSummary = "Pharmacy.POS.CloseShiftSummary";
        }
    }

    // Company, Branch & System Settings
    public static class BranchDetails
    {
        public const string Read = "BranchDetails.Read";
        public const string Create = "BranchDetails.Create";
        public const string Update = "BranchDetails.Update";
        public const string Delete = "BranchDetails.Delete";
    }
}
