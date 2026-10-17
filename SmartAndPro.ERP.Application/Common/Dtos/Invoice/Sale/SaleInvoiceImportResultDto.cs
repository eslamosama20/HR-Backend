using System.Collections.Generic;

namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale;

public class SaleInvoiceImportResultDto
{
    public int Imported { get; set; }
    public int Updated { get; set; }
    public List<SaleInvoiceImportFailureDto> Failed { get; set; } = new();
}
