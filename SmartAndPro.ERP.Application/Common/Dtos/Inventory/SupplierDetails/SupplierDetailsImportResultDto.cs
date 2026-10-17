namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.SupplierDetails;

/// <summary>
/// Returned by POST /api/inventory/supplierDetails/import
/// </summary>
public class SupplierDetailsImportResultDto
{
    public int Imported { get; set; }
    public int Updated  { get; set; }
    public List<SupplierDetailsImportFailureDto> Failed { get; set; } = new();
}

public class SupplierDetailsImportFailureDto
{
    public int    Row    { get; set; }
    public string Name   { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
}
