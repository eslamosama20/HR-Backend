using SmartAndPro.ERP.Application.Common.Dtos.Reports.SupplierReport;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;

namespace SmartAndPro.ERP.Application.IServices
{
    public interface ISupplierDetailsReportService
    {
        Task<OperationResponse<dynamic>> GetSupplierDetails(SupplierReportRequestDto supplierReportRequestDto);
    }
}
