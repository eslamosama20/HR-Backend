using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartAndPro.ERP.Application.Common.Dtos.Dashboard;
using SmartAndPro.ERP.Application.Services.Auth.Interfaces;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.Infrastructure.Repositories.PurchaseInvoice;
using SmartAndPro.ERP.Infrastructure.Repositories.SalesInvoice;
using System.Globalization;

namespace SmartAndPro.ERP.Application.Services.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IAuthService _authService;

    public DashboardService(IServiceScopeFactory scopeFactory, IAuthService authService)
    {
        _scopeFactory = scopeFactory;
        _authService = authService;
    }

    public async Task<DashboardKpiDto> GetKpisAsync(Guid userId)
    {
        // Permission lookup runs in parallel with the data queries.
        var allowedTask = _authService.GetUserPermissionNamesAsync(userId);
        var raw = await BuildKpisAsync();
        var allowed = await allowedTask;
        return DashboardKpiAccess.Apply(raw, allowed);
    }

    public async Task<DashboardChartDataDto> GetSalesPurchaseChartAsync(Guid userId)
    {
        var allowedTask = _authService.GetUserPermissionNamesAsync(userId);
        var raw = await BuildSalesPurchaseChartAsync();
        var allowed = await allowedTask;
        return DashboardChartAccess.ApplySalesPurchase(raw, allowed);
    }

    public async Task<DashboardChartDataDto> GetReservationsChartAsync(Guid userId)
    {
        var allowedTask = _authService.GetUserPermissionNamesAsync(userId);
        var raw = await BuildReservationsChartAsync();
        var allowed = await allowedTask;
        return DashboardChartAccess.ApplyReservations(raw, allowed);
    }

    public async Task<DashboardChartDataDto> GetTreasuryChartAsync(Guid userId)
    {
        var allowedTask = _authService.GetUserPermissionNamesAsync(userId);
        var raw = await BuildTreasuryChartAsync();
        var allowed = await allowedTask;
        return DashboardChartAccess.ApplyTreasury(raw, allowed);
    }

    public async Task<DashboardChartDataDto> GetInventoryChartAsync(Guid userId)
    {
        var allowedTask = _authService.GetUserPermissionNamesAsync(userId);
        var raw = await BuildInventoryChartAsync();
        var allowed = await allowedTask;
        return DashboardChartAccess.ApplyInventory(raw, allowed);
    }

    /// <summary>
    /// Single round-trip aggregate: KPIs + all charts, sharing one permission lookup.
    /// </summary>
    public async Task<DashboardOverviewDto> GetOverviewAsync(Guid userId)
    {
        var allowedTask = _authService.GetUserPermissionNamesAsync(userId);
        var kpisTask = BuildKpisAsync();
        var salesPurchaseTask = BuildSalesPurchaseChartAsync();
        var reservationsTask = BuildReservationsChartAsync();
        var treasuryTask = BuildTreasuryChartAsync();
        var inventoryTask = BuildInventoryChartAsync();

        await Task.WhenAll(allowedTask, kpisTask, salesPurchaseTask, reservationsTask, treasuryTask, inventoryTask);

        var allowed = await allowedTask;
        return new DashboardOverviewDto
        {
            Kpis = DashboardKpiAccess.Apply(await kpisTask, allowed),
            SalesPurchase = DashboardChartAccess.ApplySalesPurchase(await salesPurchaseTask, allowed),
            Reservations = DashboardChartAccess.ApplyReservations(await reservationsTask, allowed),
            Treasury = DashboardChartAccess.ApplyTreasury(await treasuryTask, allowed),
            Inventory = DashboardChartAccess.ApplyInventory(await inventoryTask, allowed)
        };
    }

    // ------------------------------------------------------------------
    // Private builders. EF Core contexts are NOT thread-safe, so every
    // parallel query runs on its own child scope (fresh context instance).
    // ------------------------------------------------------------------

    private async Task<DashboardKpiDto> BuildKpisAsync()
    {
        var window = DashboardCashWindow.GetWindow(DateOnly.FromDateTime(DateTime.UtcNow.Date));

        var salesTask = RunScopedAsync(sp =>
            sp.GetRequiredService<ISalesInvoicesRepository>().GetDashboardSummaryAsync());

        var purchaseTask = RunScopedAsync(sp =>
            sp.GetRequiredService<IPurchaseInvoiceRepository>().GetDashboardSummaryAsync());

        var reservationCountsTask = RunScopedAsync(sp =>
            GetReservationCountsAsync(sp.GetRequiredService<ERPContext>()));

        var smallCountsTask = RunScopedAsync(async sp =>
        {
            var ctx = sp.GetRequiredService<ERPContext>();
            var inventoryItems = await ctx.MainSalesClasses.AsNoTracking().CountAsync();
            var treasuries = await ctx.Treasuries.AsNoTracking().CountAsync();
            var stores = await ctx.StoreDetails.AsNoTracking().CountAsync();
            var suppliers = await ctx.SupplierDetails.AsNoTracking().CountAsync();
            var banks = await ctx.GeneralBankAccounts.AsNoTracking().CountAsync();
            return (inventoryItems, treasuries, stores, suppliers, banks);
        });

        var cashInTask = RunScopedAsync(sp =>
        {
            var ctx = sp.GetRequiredService<ERPContext>();
            return ctx.PaycheckReceipts
                .AsNoTracking()
                .Where(x => x.ReceiptDate >= window.From && x.ReceiptDate <= window.To)
                .SumAsync(x => (decimal?)x.Amount);
        });

        var cashOutTask = RunScopedAsync(sp =>
        {
            var ctx = sp.GetRequiredService<ERPContext>();
            return ctx.PaymentReceipts
                .AsNoTracking()
                .Where(x => x.ReceiptDate >= window.From && x.ReceiptDate <= window.To)
                .SumAsync(x => (decimal?)x.Amount);
        });

        await Task.WhenAll(salesTask, purchaseTask, reservationCountsTask, smallCountsTask, cashInTask, cashOutTask);

        var (salesTotal, salesCount) = await salesTask;
        var (purchaseTotal, purchaseCount) = await purchaseTask;
        var (reservationsCount, confirmedCount, _) = await reservationCountsTask;
        var (inventoryItems, treasuries, stores, suppliers, banks) = await smallCountsTask;
        var cashIn30 = await cashInTask ?? 0m;
        var cashOut30 = await cashOutTask ?? 0m;

        return new DashboardKpiDto
        {
            SalesTotal = salesTotal,
            PurchaseTotal = purchaseTotal,
            SalesCount = salesCount,
            PurchaseCount = purchaseCount,
            ReservationsCount = reservationsCount,
            ConfirmedCount = confirmedCount,
            InventoryItems = inventoryItems,
            CashIn30 = cashIn30,
            CashOut30 = cashOut30,
            Treasuries = treasuries + banks,
            StoreCount = stores,
            SupplierCount = suppliers,
            BankAccountCount = banks
        };
    }

    private async Task<DashboardChartDataDto> BuildSalesPurchaseChartAsync()
    {
        var buckets = BuildMonthBuckets(12);
        var from = buckets[0].Date;
        var toExclusive = from.AddMonths(12);

        var salesTask = RunScopedAsync(sp =>
            sp.GetRequiredService<ISalesInvoicesRepository>().GetMonthlyTotalsAsync(from, toExclusive));

        var purchaseTask = RunScopedAsync(sp =>
            sp.GetRequiredService<IPurchaseInvoiceRepository>().GetMonthlyTotalsAsync(from, toExclusive));

        await Task.WhenAll(salesTask, purchaseTask);

        var salesData = await salesTask;
        var purchaseData = await purchaseTask;

        return new DashboardChartDataDto
        {
            Categories = buckets.Select(x => x.Label).ToList(),
            Series =
            [
                new DashboardChartSeriesDto
                {
                    Name = "sales",
                    Data = buckets.Select(x => salesData.TryGetValue((x.Date.Year, x.Date.Month), out var value) ? value : 0m).ToList()
                },
                new DashboardChartSeriesDto
                {
                    Name = "purchases",
                    Data = buckets.Select(x => purchaseData.TryGetValue((x.Date.Year, x.Date.Month), out var value) ? value : 0m).ToList()
                }
            ]
        };
    }

    private async Task<DashboardChartDataDto> BuildReservationsChartAsync()
    {
        var buckets = BuildMonthBuckets(12);
        var fromDate = DateOnly.FromDateTime(buckets[0].Date.Date);
        var toExclusiveDate = DateOnly.FromDateTime(buckets[0].Date.AddMonths(12).Date);

        var monthlyTask = RunScopedAsync(async sp =>
        {
            var ctx = sp.GetRequiredService<ERPContext>();
            return await ctx.Reservations
                .AsNoTracking()
                .Where(x => x.ArrivalDate >= fromDate && x.ArrivalDate < toExclusiveDate)
                .GroupBy(x => new { x.ArrivalDate.Year, x.ArrivalDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => (x.Year, x.Month), x => x.Count);
        });

        var statusCountsTask = RunScopedAsync(sp =>
            GetReservationCountsAsync(sp.GetRequiredService<ERPContext>()));

        await Task.WhenAll(monthlyTask, statusCountsTask);

        var monthlyReservations = await monthlyTask;
        var (reservationsCount, confirmedCount, cancelledCount) = await statusCountsTask;
        var pendingCount = Math.Max(0, reservationsCount - confirmedCount - cancelledCount);

        return new DashboardChartDataDto
        {
            Categories = buckets.Select(x => x.Label).ToList(),
            Series =
            [
                new DashboardChartSeriesDto
                {
                    Name = "reservations",
                    Data = buckets.Select(x => monthlyReservations.TryGetValue((x.Date.Year, x.Date.Month), out var value) ? Convert.ToDecimal(value) : 0m).ToList()
                }
            ],
            StatusCounts = new Dictionary<string, int>
            {
                ["confirmed"] = confirmedCount,
                ["pending"] = pendingCount,
                ["cancelled"] = cancelledCount
            }
        };
    }

    private async Task<DashboardChartDataDto> BuildTreasuryChartAsync()
    {
        var buckets = BuildMonthBuckets(6);
        var fromDate = DateOnly.FromDateTime(buckets[0].Date.Date);
        var toExclusiveDate = DateOnly.FromDateTime(buckets[0].Date.AddMonths(6).Date);

        var cashInTask = RunScopedAsync(async sp =>
        {
            var ctx = sp.GetRequiredService<ERPContext>();
            return await ctx.PaycheckReceipts
                .AsNoTracking()
                .Where(x => x.ReceiptDate >= fromDate && x.ReceiptDate < toExclusiveDate)
                .GroupBy(x => new { x.ReceiptDate.Year, x.ReceiptDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Total = g.Sum(x => x.Amount)
                })
                .ToDictionaryAsync(x => (x.Year, x.Month), x => x.Total);
        });

        var cashOutTask = RunScopedAsync(async sp =>
        {
            var ctx = sp.GetRequiredService<ERPContext>();
            return await ctx.PaymentReceipts
                .AsNoTracking()
                .Where(x => x.ReceiptDate >= fromDate && x.ReceiptDate < toExclusiveDate)
                .GroupBy(x => new { x.ReceiptDate.Year, x.ReceiptDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Total = g.Sum(x => x.Amount)
                })
                .ToDictionaryAsync(x => (x.Year, x.Month), x => x.Total);
        });

        await Task.WhenAll(cashInTask, cashOutTask);

        var cashInData = await cashInTask;
        var cashOutData = await cashOutTask;

        return new DashboardChartDataDto
        {
            Categories = buckets.Select(x => x.Label).ToList(),
            Series =
            [
                new DashboardChartSeriesDto
                {
                    Name = "cashIn",
                    Data = buckets.Select(x => cashInData.TryGetValue((x.Date.Year, x.Date.Month), out var value) ? value : 0m).ToList()
                },
                new DashboardChartSeriesDto
                {
                    Name = "cashOut",
                    Data = buckets.Select(x => cashOutData.TryGetValue((x.Date.Year, x.Date.Month), out var value) ? value : 0m).ToList()
                }
            ]
        };
    }

    private async Task<DashboardChartDataDto> BuildInventoryChartAsync()
    {
        // 1. Purchase-class query: OperationEntry sums (matches inventory management page).
        //    IMPORTANT: the page materializes entities, so EF applies the AutoIncluded
        //    InventoryOperation's IsActive query filter (inner-join). The GroupBy projection
        //    drops unused AutoIncludes, so we must filter the operation explicitly to match.
        var purchaseRowsTask = RunScopedAsync(async sp =>
        {
            var ctx = sp.GetRequiredService<ERPContext>();
            return await ctx.OperationEntries
                .AsNoTracking()
                .Where(x => x.InventoryOperation.IsActive)
                .GroupBy(x => x.MainPurchaseClassId)
                .Select(g => new
                {
                    ClassId = (Guid?)g.Key,
                    Total = g.Sum(x => x.Quantity)
                })
                .ToListAsync();
        });

        // 2. Sales-class query: group by sales class ID, exclude reservation items, sum quantity
        var salesRowsTask = RunScopedAsync(async sp =>
        {
            var ctx = sp.GetRequiredService<ERPContext>();
            return await ctx.ProductUnit
                .AsNoTracking()
                .Where(x => x.MainPurchaseClassId == null
                    && x.MainSalesClassId != null
                    && !x.MainSalesClass!.IsReservationItem)
                .GroupBy(x => x.MainSalesClassId)
                .Select(g => new
                {
                    ClassId = g.Key,
                    Total = g.Sum(x => x.Quantity ?? 0m)
                })
                .ToListAsync();
        });

        // 3. Resolve class names in memory (Arabic-first projection)
        var purchaseClassLookupTask = RunScopedAsync(async sp =>
        {
            var ctx = sp.GetRequiredService<ERPContext>();
            return await ctx.MainPurchaseClasses
                .AsNoTracking()
                .Select(c => new { c.Id, c.ArabicName, c.EnglishName })
                .ToDictionaryAsync(c => c.Id);
        });

        var salesClassLookupTask = RunScopedAsync(async sp =>
        {
            var ctx = sp.GetRequiredService<ERPContext>();
            return await ctx.MainSalesClasses
                .AsNoTracking()
                .Select(c => new { c.Id, c.ArabicName, c.EnglishName })
                .ToDictionaryAsync(c => c.Id);
        });

        await Task.WhenAll(purchaseRowsTask, salesRowsTask, purchaseClassLookupTask, salesClassLookupTask);

        var purchaseRows = await purchaseRowsTask;
        var salesRows = await salesRowsTask;
        var purchaseClassLookup = await purchaseClassLookupTask;
        var salesClassLookup = await salesClassLookupTask;

        var purchaseItems = purchaseRows
            .Where(r => r.ClassId.HasValue)
            .Select(r =>
            {
                var cls = purchaseClassLookup.GetValueOrDefault(r.ClassId!.Value);
                var name = cls?.ArabicName != null && cls.ArabicName != ""
                    ? cls.ArabicName
                    : cls?.EnglishName ?? "";
                return (Name: name, Total: r.Total);
            })
            .ToList();

        var salesItems = salesRows
            .Where(r => r.ClassId.HasValue)
            .Select(r =>
            {
                var cls = salesClassLookup.GetValueOrDefault(r.ClassId!.Value);
                var name = cls?.ArabicName != null && cls.ArabicName != ""
                    ? cls.ArabicName
                    : cls?.EnglishName ?? "";
                return (Name: name, Total: r.Total);
            })
            .ToList();

        // 4. Merge, dedupe, rank via helper
        var ranked = DashboardInventoryAccess.RankTopItems(purchaseItems, salesItems);

        // 5. Build DTO
        return new DashboardChartDataDto
        {
            Categories = ranked.Select(x => x.Name).ToList(),
            Series =
            [
                new DashboardChartSeriesDto
                {
                    Name = "inventory",
                    Data = ranked.Select(x => x.Total).ToList()
                }
            ]
        };
    }

    private static async Task<(int Total, int Confirmed, int Cancelled)> GetReservationCountsAsync(ERPContext ctx)
    {
        var total = await ctx.Reservations.AsNoTracking().CountAsync();
        var confirmed = await ctx.Reservations.AsNoTracking().CountAsync(x => x.IsConfirmed);
        var cancelled = await ctx.Reservations.AsNoTracking().CountAsync(x => x.IsCancelled);
        return (total, confirmed, cancelled);
    }

    private async Task<T> RunScopedAsync<T>(Func<IServiceProvider, Task<T>> operation)
    {
        using var scope = _scopeFactory.CreateScope();
        return await operation(scope.ServiceProvider);
    }

    private static List<(DateTime Date, string Label)> BuildMonthBuckets(int months)
    {
        var now = DateTime.UtcNow;
        var firstMonth = new DateTime(now.Year, now.Month, 1);

        var buckets = new List<(DateTime Date, string Label)>();
        for (var i = months - 1; i >= 0; i--)
        {
            var month = firstMonth.AddMonths(-i);
            buckets.Add((month, month.ToString("MMM yy", CultureInfo.InvariantCulture)));
        }

        return buckets;
    }
}
