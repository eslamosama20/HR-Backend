using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos;

public class QueryFilter
{
    [JsonPropertyName("matchMode")]
    public string? MatchMode { get; set; }
    [JsonPropertyName("value")]
    public string? Value { get; set; }
    [JsonPropertyName("operator")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Operator { get; set; } = null;
}
