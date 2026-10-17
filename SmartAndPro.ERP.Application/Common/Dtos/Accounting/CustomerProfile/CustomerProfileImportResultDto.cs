namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.CustomerProfile;

/// <summary>
/// Returned by POST /api/accounting/customerProfile/import
/// </summary>
public class CustomerProfileImportResultDto
{
    /// <summary>Number of rows that were newly created.</summary>
    public int Imported { get; set; }

    /// <summary>Number of rows that matched an existing customer and were updated.</summary>
    public int Updated { get; set; }

    /// <summary>Per-row failures — rows that could not be imported or updated.</summary>
    public List<CustomerProfileImportFailureDto> Failed { get; set; } = new();
}

/// <summary>
/// Describes a single row that failed during import.
/// </summary>
public class CustomerProfileImportFailureDto
{
    /// <summary>1-based row index in the Excel file (header row = 1).</summary>
    public int Row { get; set; }

    /// <summary>The Arabic name read from that row (for user-friendly display).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Human-readable reason why the row failed.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Optional translation key/category for frontend-friendly error rendering.</summary>
    public string? ErrorCode { get; set; }
}
