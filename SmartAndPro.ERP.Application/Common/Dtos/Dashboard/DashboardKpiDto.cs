namespace SmartAndPro.ERP.Application.Common.Dtos.Dashboard;

public class DashboardKpiDto
{
    public decimal? SalesTotal { get; set; }
    public decimal? PurchaseTotal { get; set; }
    public int? SalesCount { get; set; }
    public int? PurchaseCount { get; set; }
    public int? ReservationsCount { get; set; }
    public int? ConfirmedCount { get; set; }
    public int? InventoryItems { get; set; }
    public decimal? CashIn30 { get; set; }
    public decimal? CashOut30 { get; set; }
    public int? Treasuries { get; set; }
    public int? StoreCount { get; set; }
    public int? SupplierCount { get; set; }
    public int? BankAccountCount { get; set; }
}
