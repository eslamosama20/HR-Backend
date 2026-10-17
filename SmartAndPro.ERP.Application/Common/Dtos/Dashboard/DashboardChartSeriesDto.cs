namespace SmartAndPro.ERP.Application.Common.Dtos.Dashboard;

public class DashboardChartSeriesDto
{
    public string Name { get; set; } = string.Empty;
    public List<decimal> Data { get; set; } = [];
}
