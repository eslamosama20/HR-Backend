using SmartAndPro.ERP.Domains.Models.InputModel.Report;
using SmartAndPro.ERP.Domains.Models.OutputModel;

namespace SmartAndPro.ERP.Infrastructure.Repositories.Report.Supplier
{
    public interface ISupplierReportRepository
    {
        List<SupplierDetailsByReceiptOutputModel> GetSupplerDetailsByReceipt(GetSupplierDetailsInputModel supplierReportRequestDto);

        List<SupplierDetailsByProductOutputModel> GetSupplerDetailsByProduct(GetSupplierDetailsInputModel supplierReportRequestDto);

        List<SupplierDetailsByStoreOutputModel> GetSupplerDetailsByStore(GetSupplierDetailsInputModel supplierReportRequestDto);


    }
}
