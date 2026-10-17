using SmartAndPro.ERP.Application.Common.Dtos.Dashboard;
using SmartAndPro.ERP.SharedKernel.Constants;

namespace SmartAndPro.ERP.Application.Services.Dashboard;

public static class DashboardKpiAccess
{
    public static DashboardKpiDto Apply(DashboardKpiDto raw, HashSet<string> allowed)
    {
        return new DashboardKpiDto
        {
            SalesTotal = allowed.Contains(Permissions.Dashboard.SalesTotal) ? raw.SalesTotal : null,
            PurchaseTotal = allowed.Contains(Permissions.Dashboard.PurchaseTotal) ? raw.PurchaseTotal : null,
            SalesCount = allowed.Contains(Permissions.Dashboard.SalesCount) ? raw.SalesCount : null,
            PurchaseCount = allowed.Contains(Permissions.Dashboard.PurchaseCount) ? raw.PurchaseCount : null,
            ReservationsCount = allowed.Contains(Permissions.Dashboard.ReservationsCount) ? raw.ReservationsCount : null,
            ConfirmedCount = allowed.Contains(Permissions.Dashboard.ReservationsConfirmed) ? raw.ConfirmedCount : null,
            InventoryItems = allowed.Contains(Permissions.Dashboard.InventoryItems) ? raw.InventoryItems : null,
            CashIn30 = allowed.Contains(Permissions.Dashboard.CashIn30) ? raw.CashIn30 : null,
            CashOut30 = allowed.Contains(Permissions.Dashboard.CashOut30) ? raw.CashOut30 : null,
            Treasuries = allowed.Contains(Permissions.Dashboard.Treasuries) ? raw.Treasuries : null,
            StoreCount = allowed.Contains(Permissions.Dashboard.StoreCount) ? raw.StoreCount : null,
            SupplierCount = allowed.Contains(Permissions.Dashboard.SupplierCount) ? raw.SupplierCount : null,
            BankAccountCount = allowed.Contains(Permissions.Dashboard.BankAccounts) ? raw.BankAccountCount : null,
        };
    }
}
