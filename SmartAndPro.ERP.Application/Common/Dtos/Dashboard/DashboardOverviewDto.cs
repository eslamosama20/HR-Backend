namespace SmartAndPro.ERP.Application.Common.Dtos.Dashboard;

public class DashboardOverviewDto
{
    public DashboardKpiDto Kpis { get; set; } = new();
    public DashboardChartDataDto SalesPurchase { get; set; } = new();
    public DashboardChartDataDto Reservations { get; set; } = new();
    public DashboardChartDataDto Treasury { get; set; } = new();
    public DashboardChartDataDto Inventory { get; set; } = new();
}
