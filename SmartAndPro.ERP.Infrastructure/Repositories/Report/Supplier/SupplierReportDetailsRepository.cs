using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Domains.Models.InputModel.Report;
using SmartAndPro.ERP.Domains.Models.OutputModel;
using SmartAndPro.ERP.Infrastructure.Contexts;

namespace SmartAndPro.ERP.Infrastructure.Repositories.Report.Supplier
{
    public class SupplierReportDetailsRepository : ISupplierReportRepository
    {
        private readonly ERPContext _eRPContext;

        public SupplierReportDetailsRepository(ERPContext eRPContext)
        {
            _eRPContext = eRPContext;
        }

        public List<SupplierDetailsByReceiptOutputModel> GetSupplerDetailsByReceipt(GetSupplierDetailsInputModel supplierReportRequestDto)
        {
            try
            {
                var supplierIdParam = new SqlParameter("@SupplierId", supplierReportRequestDto.SupplierId);
                var startDateParam = new SqlParameter("@StartDate", supplierReportRequestDto.StartDate);
                var endDateParam = new SqlParameter("@EndDate", supplierReportRequestDto.EndDate);

                return new List<SupplierDetailsByReceiptOutputModel>();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<SupplierDetailsByProductOutputModel> GetSupplerDetailsByProduct(GetSupplierDetailsInputModel supplierReportRequestDto)
        {
            try
            {
                var result = _eRPContext.InventoryOperations
                    .Join(
                        _eRPContext.OperationEntries,
                        op => op.Id,
                        operationDetails => operationDetails.InventoryOperationId,
                        (op, operationDetails) => new { op, operationDetails }
                    )
                    .Join(
                        _eRPContext.MainPurchaseClasses,
                        combined => combined.operationDetails.MainPurchaseClassId,
                        mainPurchase => mainPurchase.Id,
                        (combined, mainPurchase) => new SupplierDetailsByProductOutputModel
                        {
                            CreateDate = combined.op.OperationDate,
                            ReceiptNo = combined.op.ReferenceNumber,
                            ProductNameAr = mainPurchase.ArabicName,
                            ProductNameEn = mainPurchase.EnglishName,
                            ProductPrice = combined.operationDetails.UnitPrice,
                            ProductQuantity = combined.operationDetails.Quantity,
                            TotalAmount = combined.operationDetails.UnitPrice * combined.operationDetails.Quantity // Calculate total
                        }
                        ).ToList();

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<SupplierDetailsByStoreOutputModel> GetSupplerDetailsByStore(GetSupplierDetailsInputModel supplierReportRequestDto)
        {
            try
            {
                var result = _eRPContext.InventoryOperations
                   .Join(
                       _eRPContext.OperationEntries,
                       op => op.Id,
                       operationDetails => operationDetails.InventoryOperationId,
                       (op, operationDetails) => new { op, operationDetails }
                   )
                   .Join(
                       _eRPContext.MainPurchaseClasses,
                       combined => combined.operationDetails.MainPurchaseClassId,
                       mainPurchase => mainPurchase.Id,
                       (combined, mainPurchase) => new { combined, mainPurchase }
                       )
                   .Join(_eRPContext.StoreDetails,
                       outer => outer.combined.operationDetails.StoreDetailsId,
                       store => store.Id,
                       (outer, stroreDetails) =>
                       new SupplierDetailsByStoreOutputModel
                       {
                           CreateDate = outer.combined.op.OperationDate,
                           ReceiptNo = outer.combined.op.ReferenceNumber,
                           StoreNameAr = stroreDetails.ArabicName,
                           StoreNameEn = stroreDetails.EnglishName,
                           ProductNameAr = outer.mainPurchase.ArabicName,
                           ProductNameEn = outer.mainPurchase.EnglishName,
                           ProductPrice = outer.combined.operationDetails.UnitPrice,
                           ProductQuantity = outer.combined.operationDetails.Quantity,
                           TotalAmount = outer.combined.operationDetails.UnitPrice * outer.combined.operationDetails.Quantity
                       }
                       ).ToList();

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }


}
