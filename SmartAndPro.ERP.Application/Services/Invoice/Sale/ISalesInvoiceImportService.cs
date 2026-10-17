using System.IO;
using System.Threading.Tasks;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale;

namespace SmartAndPro.ERP.Application.Services.Invoice.Sale;

public interface ISalesInvoiceImportService
{
    Task<SaleInvoiceImportResultDto> ImportFromExcelAsync(Stream fileStream);
}
