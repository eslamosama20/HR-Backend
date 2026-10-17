namespace SmartAndPro.ERP.Application.Common.Dtos.Treasuries;

/// <summary>
/// Returned by POST /api/treasuries/import
/// </summary>
public class TreasuryImportResultDto
{
    public int Imported { get; set; }
    public int Updated { get; set; }
    public List<TreasuryImportFailureDto> Failed { get; set; } = new();
}

public class TreasuryImportFailureDto
{
    public int Row { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
}
