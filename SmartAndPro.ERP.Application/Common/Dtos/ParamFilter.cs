using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos;

public class ParamFilter
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? SortField { get; set; }
    public string? SortOrder { get; set; }
    
    public string? Search { get; set; }
    public Dictionary<string, List<QueryFilter>>? Filters { get; set; }
}
