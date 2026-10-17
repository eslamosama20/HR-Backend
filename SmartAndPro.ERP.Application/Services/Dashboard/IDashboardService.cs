using SmartAndPro.ERP.Application.Common.Dtos.Dashboard;

namespace SmartAndPro.ERP.Application.Services.Dashboard;

public interface IDashboardService
{
    Task<DashboardKpiDto> GetKpisAsync(Guid userId);
    Task<DashboardChartDataDto> GetSalesPurchaseChartAsync(Guid userId);
    Task<DashboardChartDataDto> GetReservationsChartAsync(Guid userId);
    Task<DashboardChartDataDto> GetTreasuryChartAsync(Guid userId);
    Task<DashboardChartDataDto> GetInventoryChartAsync(Guid userId);
    Task<DashboardOverviewDto> GetOverviewAsync(Guid userId);
}
