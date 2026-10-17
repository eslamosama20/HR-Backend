namespace SmartAndPro.ERP.Application.Services.Dashboard;

/// <summary>
/// Provides a pure, testable 30-day cash window computation for dashboard KPIs.
/// Ensures cashIn30/cashOut30 cover only [today − 30d, today] (inclusive).
/// </summary>
public static class DashboardCashWindow
{
    /// <summary>
    /// The number of days in the cash window (30 days).
    /// </summary>
    public const int WindowDays = 30;

    /// <summary>
    /// Computes the 30-day cash window boundaries.
    /// </summary>
    /// <param name="today">The reference date (typically UTC today).</param>
    /// <returns>
    /// A tuple (From, To) where:
    /// - From = today.AddDays(-WindowDays) (inclusive lower bound)
    /// - To = today (inclusive upper bound)
    /// The window spans exactly WindowDays + 1 days (inclusive of both endpoints).
    /// </returns>
    public static (DateOnly From, DateOnly To) GetWindow(DateOnly today)
    {
        var from = today.AddDays(-WindowDays);
        return (from, today);
    }
}
