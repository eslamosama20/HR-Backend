using SmartAndPro.ERP.Application.Common.Dtos.Dashboard;
using SmartAndPro.ERP.SharedKernel.Constants;

namespace SmartAndPro.ERP.Application.Services.Dashboard;

public static class DashboardChartAccess
{
    private static readonly Dictionary<string, string> SalesPurchaseSeriesPermissionMap = new()
    {
        ["sales"] = Permissions.Dashboard.SalesTotal,
        ["purchases"] = Permissions.Dashboard.PurchaseTotal
    };

    private static readonly Dictionary<string, string> ReservationsSeriesPermissionMap = new()
    {
        ["reservations"] = Permissions.Dashboard.ReservationsCount
    };

    private static readonly Dictionary<string, string> TreasurySeriesPermissionMap = new()
    {
        ["cashIn"] = Permissions.Dashboard.CashIn30,
        ["cashOut"] = Permissions.Dashboard.CashOut30
    };

    private static readonly Dictionary<string, string> InventorySeriesPermissionMap = new()
    {
        ["inventory"] = Permissions.Dashboard.InventoryItems
    };

    public static DashboardChartDataDto ApplySalesPurchase(DashboardChartDataDto raw, HashSet<string> allowed)
    {
        return ApplySeries(raw, SalesPurchaseSeriesPermissionMap, allowed);
    }

    public static DashboardChartDataDto ApplyReservations(DashboardChartDataDto raw, HashSet<string> allowed)
    {
        var result = ApplySeries(raw, ReservationsSeriesPermissionMap, allowed);

        if (result.StatusCounts != null)
        {
            result.StatusCounts = allowed.Contains(Permissions.Dashboard.ReservationsConfirmed)
                ? raw.StatusCounts
                : new Dictionary<string, int>();
        }

        return result;
    }

    public static DashboardChartDataDto ApplyTreasury(DashboardChartDataDto raw, HashSet<string> allowed)
    {
        return ApplySeries(raw, TreasurySeriesPermissionMap, allowed);
    }

    public static DashboardChartDataDto ApplyInventory(DashboardChartDataDto raw, HashSet<string> allowed)
    {
        return ApplySeries(raw, InventorySeriesPermissionMap, allowed);
    }

    private static DashboardChartDataDto ApplySeries(
        DashboardChartDataDto raw,
        Dictionary<string, string> seriesPermissionMap,
        HashSet<string> allowed)
    {
        return new DashboardChartDataDto
        {
            Categories = raw.Categories,
            Series = raw.Series.Select(s =>
            {
                if (seriesPermissionMap.TryGetValue(s.Name, out var permission) && !allowed.Contains(permission))
                {
                    return new DashboardChartSeriesDto { Name = s.Name, Data = [] };
                }
                return s;
            }).ToList(),
            StatusCounts = raw.StatusCounts
        };
    }
}
