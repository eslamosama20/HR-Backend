namespace SmartAndPro.ERP.Application.Common.Dtos.Dashboard;

public class DashboardChartDataDto
{
    public List<string> Categories { get; set; } = [];
    public List<DashboardChartSeriesDto> Series { get; set; } = [];
    public Dictionary<string, int>? StatusCounts { get; set; }
}
