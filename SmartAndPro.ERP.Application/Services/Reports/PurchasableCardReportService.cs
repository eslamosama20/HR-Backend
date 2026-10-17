using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Reports.PurchasableCardReport;
using SmartAndPro.ERP.Domains.Entities.Inventory.InventoryOperation;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Reports;

public class PurchasableCardReportService
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;

    public PurchasableCardReportService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
    
    public async Task<List<PurchasableCardReportResponseDto>> GetReportAsync(PurchasableCardReportRequestDto reportRequest)
    {
        try
        {
            var purchasableCardReportResponse = new List<PurchasableCardReportResponseDto>();
            
            var InventoryOperationRepository = _repositoriesFactory.GetRepository<InventoryOperation>();
            var InventoryOperationQuery = await InventoryOperationRepository.Filter(x =>
                x.OperationEntries.Any(y => y.MainPurchaseClassId == reportRequest.MainPurchaseClassId) &&
                x.CreatedAt >= reportRequest.StartDate &&
                x.CreatedAt <= reportRequest.EndDate);
            if (InventoryOperationQuery.ErrorCode != ErrorCodes.Success || InventoryOperationQuery.Data.Any() == false || InventoryOperationQuery.Data == null)
            {
                return new List<PurchasableCardReportResponseDto>();
            }

            var inventoryOperations = InventoryOperationQuery.Data
                .Include(x => x.SupplierDetails)
                .Include(x => x.OperationEntries)
                .ThenInclude(y => y.StoreDetails)
                .Include(x => x.PurchaseInvoice)
                .Include(x => x.PurchaseReturn)
                .Include(x => x.InventoryStocktaking)
                .Include(x => x.InventoryPermit)
                .Select(x => new
                {
                    x.OperationDate,
                    ReferenceNumber = 
                        x.OperationSource == OperationSource.PurchaseInvoice ? x.PurchaseInvoice.InvoiceNo :
                        x.OperationSource == OperationSource.PurchaseReturn ? x.PurchaseReturn.ReturnNo :
                        x.OperationSource == OperationSource.Stocktaking ? x.InventoryStocktaking.ReferenceNumber :
                        (x.OperationSource == OperationSource.EntryPermit || x.OperationSource == OperationSource.ExitPermit) ? x.InventoryPermit.ReferenceNumber :
                        "-",
                    x.Description,
                    x.OperationSource,
                    SupplierDetails = x.SupplierDetailsId != null
                        ? new
                        {
                            x.SupplierDetails.Id,
                            x.SupplierDetails.EnglishName,
                            x.SupplierDetails.ArabicName,
                        }
                        : null,
                    OperationEntries = x.OperationEntries
                        .Where(y => y.MainPurchaseClassId == reportRequest.MainPurchaseClassId).Select(y => new
                        {
                            y.MainPurchaseClassId,
                            y.Quantity,
                            y.UnitPrice,
                            UnitMeasurementName = y.MainPurchaseClass.MeasurementUnit.ArabicName,
                            StoreDetails = y.StoreDetailsId != null
                                ? new
                                {
                                    y.StoreDetails.Id,
                                    y.StoreDetails.EnglishName,
                                    y.StoreDetails.ArabicName,
                                }
                                : null
                        }).ToList()
                })
                .ToList();

                    

            foreach (var inventoryOperation in inventoryOperations)
            {
                var purchasableCardReport = new PurchasableCardReportResponseDto
                {
                    OperationDate = inventoryOperation.OperationDate,
                    OperationReferenceNumber = inventoryOperation.ReferenceNumber,
                    Description = inventoryOperation?.Description,
                    OperationSource = inventoryOperation.OperationSource.ToString(),
                    SupplierDetailsArabicName = inventoryOperation.SupplierDetails?.ArabicName,
                    SupplierDetailsEnglishName = inventoryOperation.SupplierDetails?.EnglishName,
                };

                var reportItems =new List<PurchasableCardReportItemResponseDto>();
                foreach (var operationEntry in inventoryOperation.OperationEntries)
                {
                    if (operationEntry.MainPurchaseClassId != reportRequest.MainPurchaseClassId)
                        continue;
                    var purchasableCardReportItem = new PurchasableCardReportItemResponseDto
                    {
                        StoreDetailsArabicName = operationEntry.StoreDetails?.ArabicName,
                        StoreDetailsEnglishName = operationEntry.StoreDetails?.EnglishName,
                        UnitMeasurementName = operationEntry.UnitMeasurementName,
                        Quantity = operationEntry.Quantity,
                        UnitPrice = operationEntry.UnitPrice,
                        TotalPrice = operationEntry.UnitPrice * operationEntry.Quantity,

                    };
                    reportItems.Add(purchasableCardReportItem);
                }
                purchasableCardReport.PurchasableCardReportItems = reportItems;
                purchasableCardReportResponse.Add(purchasableCardReport);
            }

            return purchasableCardReportResponse;
        }
        catch (Exception e)
        {
            LogService.LogError(e);
            throw;
        }
    }
}

