namespace SmartAndPro.ERP.Application.Services.Dashboard;

public static class DashboardInventoryAccess
{
    public static List<(string Name, decimal Total)> RankTopItems(
        List<(string Name, decimal Total)> purchaseItems,
        List<(string Name, decimal Total)> salesItems)
    {
        return purchaseItems
            .Concat(salesItems)
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .GroupBy(x => x.Name)
            .Select(g => (Name: g.Key, Total: g.Sum(x => x.Total)))
            .Where(x => x.Total > 0)
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.Name)
            .Take(6)
            .ToList();
    }
}
