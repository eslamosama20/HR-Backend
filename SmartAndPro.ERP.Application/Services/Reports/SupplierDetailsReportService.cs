using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Reports.SupplierReport;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Domains.Entities.Inventory.SupplierManagement;
using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.Domains.Models.InputModel.Report;
using SmartAndPro.ERP.Domains.Models.OutputModel;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.Infrastructure.Repositories.Report.Supplier;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Reports
{
    public class SupplierDetailsReportService : ISupplierDetailsReportService
    {
        private readonly ISupplierReportRepository _supplierRepositoty;
        private readonly IMapper _mapper;
        private readonly RepositoriesFactory _repositoriesFactory;

        public SupplierDetailsReportService(ISupplierReportRepository supplierRepositoty, IMapper mapper,RepositoriesFactory repositoriesFactory)
        {
            _supplierRepositoty = supplierRepositoty;
            _mapper = mapper;
            _repositoriesFactory = repositoriesFactory;
        }
        public async Task<OperationResponse<dynamic>> GetSupplierDetails(SupplierReportRequestDto supplierReportRequestDto)
        {
            var response = new OperationResponse<dynamic>
            {
                Message = "Success",
                ErrorCode = SharedKernel.Enums.ErrorCodes.Success,
                IsPaginated = false
            };

            try
            {
                var supplierDetailsModel = _mapper.Map<GetSupplierDetailsInputModel>(supplierReportRequestDto);

                dynamic result = supplierReportRequestDto.ReportType switch
                {
                    1 => await GetSupplerDetailsByProduct(supplierDetailsModel),
                    2 => await GetSupplerDetailsByReceipt(supplierDetailsModel),
                    3 => await GetSupplerDetailsByStore(supplierDetailsModel),
                    _ => throw new ArgumentException("Invalid report type")
                };

                response.Data = result;
                return response;
            }
            catch (Exception)
            {
                response.Message = SharedKernel.Enums.ErrorCodes.GeneralFail.ToString();
                response.ErrorCode = SharedKernel.Enums.ErrorCodes.GeneralFail;
                return response;
            }
        }
        
        private async Task<List<SupplierDetailsByReceiptOutputModel>> GetSupplerDetailsByReceipt(
            GetSupplierDetailsInputModel supplierReportRequestDto)
        {
            try
            {
                var purchaseInvoiceRepository = _repositoriesFactory.GetRepository<PurchaseInvoice>();
                var purchaseInvoicesQuery = await purchaseInvoiceRepository.Filter(p => p.SupplierDetailsId == supplierReportRequestDto.SupplierId);
                var purchaseInvoices = purchaseInvoicesQuery.Data.ToList();
                if (purchaseInvoices.Count == 0)
                {
                    return new List<SupplierDetailsByReceiptOutputModel>();
                }
                
                var result = purchaseInvoices.Select(pi =>
                {
                    var totalAmount = Math.Round(pi.PurchaseItems.Sum(i => i.Price * i.Quantity),2);
                    var vatAmount = pi.TaxAmount > 0 ? Math.Round(totalAmount * 0.15m,2) : 0;

                    return new SupplierDetailsByReceiptOutputModel
                    {
                        CreateDate = pi.IssueDate,
                        ReceiptNo = pi.InvoiceNo,
                        ReceiptAmountBeforeVAT = totalAmount,
                        VAT = vatAmount,
                        TotalAmount = totalAmount + vatAmount
                    };
                }).ToList();
                return result;
            }
            catch (Exception e)
            {
                LogService.LogError(e);
                throw;
            }
        }
        
        private async Task<List<SupplierDetailsByProductOutputModel>> GetSupplerDetailsByProduct(
            GetSupplierDetailsInputModel supplierReportRequestDto)
        {
            try
            {
                var purchaseInvoiceRepository = _repositoriesFactory.GetRepository<PurchaseInvoice>();
                var purchaseInvoicesQuery = await purchaseInvoiceRepository.Filter(p => p.SupplierDetailsId == supplierReportRequestDto.SupplierId);
                var purchaseInvoices = purchaseInvoicesQuery.Data.ToList();
                if (purchaseInvoices.Count == 0)
                {
                    return new List<SupplierDetailsByProductOutputModel>();
                }

                var result = new List<SupplierDetailsByProductOutputModel>();
                purchaseInvoices.ForEach(pi =>
                {
                    pi.PurchaseItems.ForEach(i =>
                    {
                        result.Add(new SupplierDetailsByProductOutputModel
                        {
                            CreateDate = pi.IssueDate,
                            ReceiptNo = pi.InvoiceNo,
                            ProductNameAr = i.MainPurchaseClass.ArabicName,
                            ProductNameEn = i.MainPurchaseClass.EnglishName,
                            ProductPrice = i.Price,
                            ProductQuantity = i.Quantity,
                            TotalAmount = i.Price * i.Quantity
                        });
                    });
                });
                return result;
            }
            catch (Exception e)
            {
                LogService.LogError(e);
                throw;
            }
        }
        
        private async Task<List<SupplierDetailsByStoreOutputModel>> GetSupplerDetailsByStore(
            GetSupplierDetailsInputModel supplierReportRequestDto)
        {
            try
            {
                var purchaseInvoiceRepository = _repositoriesFactory.GetRepository<PurchaseInvoice>();
                var purchaseInvoicesQuery = await purchaseInvoiceRepository.Filter(p => p.SupplierDetailsId == supplierReportRequestDto.SupplierId);
                var purchaseInvoices = purchaseInvoicesQuery.Data.ToList();
                if (purchaseInvoices.Count == 0)
                {
                    return new List<SupplierDetailsByStoreOutputModel>();
                }

                var result = new List<SupplierDetailsByStoreOutputModel>();
                purchaseInvoices.ForEach(pi =>
                {
                    var groupedItemsByStoreAndProduct = pi.PurchaseItems
                        .GroupBy(i => i.StoreDetailsId)
                        .ToList();
                    
                    groupedItemsByStoreAndProduct.ForEach(g =>
                    {
                        var store = g.First().StoreDetails;
                        result.Add(new SupplierDetailsByStoreOutputModel
                        {
                            CreateDate = pi.IssueDate,
                            ReceiptNo = pi.InvoiceNo,
                            StoreNameAr = store.ArabicName,
                            StoreNameEn = store.EnglishName,
                            ProductNameAr = g.First().MainPurchaseClass.ArabicName,
                            ProductNameEn = g.First().MainPurchaseClass.EnglishName,
                            ProductPrice = g.Sum(i => i.Price),
                            ProductQuantity = g.Sum(i => i.Quantity),
                            TotalAmount = g.Sum(i => i.Price * i.Quantity)
                        });
                    });
                });
                return result;
            }
            catch (Exception e)
            {
                LogService.LogError(e);
                throw;
            }
        }
    }
}
