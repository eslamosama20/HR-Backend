using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Reports.InventoryReport;
using SmartAndPro.ERP.Application.Common.Dtos.Reports.InventoryStocks;
using SmartAndPro.ERP.Domains.Entities.Inventory.PermitManagement;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Reports
{
    public class InventoryStocksService
    {
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;

        public InventoryStocksService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        {
            _repositoriesFactory = repositoriesFactory;
            _appSettings = appSettings;
            _mapper = mapper;
        }

        // Get all inventory stocks, reading from PermitEntry to include BatchNo and ExpiryDate
        public async Task<List<InventoryStocksResponseDto>> GetAllStocks(Guid? storeDetailsId = null)
        {
            var permitRepository = _repositoriesFactory.GetRepository<InventoryPermit>();

            var permitQuery = await permitRepository.GetAll();
            if (storeDetailsId.HasValue)
            {
                permitQuery.Data = permitQuery.Data.Where(p => p.StoreDetailsId == storeDetailsId);
            }

            // Materialize permits with their auto-included PermitEntries, StoreDetails and
            // MainPurchaseClass (with MeasurementUnit) so that in-memory grouping is straightforward
            // and EF Core does not have to translate nested GroupBy + SelectMany to SQL.
            var permits = await permitQuery.Data.ToListAsync();

            var flatEntries = permits
                .SelectMany(permit => permit.PermitEntries.Select(entry => new
                {
                    permit.StoreDetailsId,
                    StoreEnglishName = permit.StoreDetails?.EnglishName,
                    StoreArabicName  = permit.StoreDetails?.ArabicName,
                    entry.MainPurchaseClassId,
                    ReferenceCode  = entry.MainPurchaseClass?.ReferenceCode,
                    EnglishName    = entry.MainPurchaseClass?.EnglishName,
                    ArabicName     = entry.MainPurchaseClass?.ArabicName,
                    UnitArabicName = entry.MainPurchaseClass?.MeasurementUnit?.ArabicName,
                    UnitEnglishName= entry.MainPurchaseClass?.MeasurementUnit?.EnglishName,
                    // Entries add stock; Exits reduce it
                    SignedQuantity = permit.PermitType == PermitType.Entry ? entry.Quantity : -entry.Quantity,
                    entry.UnitPrice,
                    entry.BatchNo,
                    entry.ExpiryDate,
                    permit.PermitDate
                }))
                .ToList();

            var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));

            var inventoryStocksResponse = flatEntries
                .GroupBy(x => new { x.StoreDetailsId, x.StoreEnglishName, x.StoreArabicName })
                .Select(storeGroup => new InventoryStocksResponseDto
                {
                    StoreDetailsId           = storeGroup.Key.StoreDetailsId,
                    StoreDetailsEnglishName  = storeGroup.Key.StoreEnglishName,
                    StoreDetailsArabicName   = storeGroup.Key.StoreArabicName,

                    StoredItems = storeGroup
                        .GroupBy(x => new
                        {
                            x.MainPurchaseClassId,
                            x.ReferenceCode,
                            x.EnglishName,
                            x.ArabicName,
                            x.UnitArabicName,
                            x.UnitEnglishName,
                            x.BatchNo,
                            x.ExpiryDate
                        })
                        .Select(itemGroup => new StoredItemsResponseDto
                        {
                            MainPurchaseClassId           = itemGroup.Key.MainPurchaseClassId,
                            MainPurchaseClassReferenceCode= itemGroup.Key.ReferenceCode,
                            MainPurchaseClassEnglishName  = itemGroup.Key.EnglishName,
                            MainPurchaseClassArabicName   = itemGroup.Key.ArabicName,
                            UnitArabicName  = itemGroup.Key.UnitArabicName,
                            UnitEnglishName = itemGroup.Key.UnitEnglishName,
                            BatchNo     = itemGroup.Key.BatchNo,
                            ExpiryDate  = itemGroup.Key.ExpiryDate,
                            Quantity    = itemGroup.Sum(x => x.SignedQuantity),
                            LastPrice   = itemGroup
                                .OrderByDescending(x => x.PermitDate)
                                .Select(x => x.UnitPrice)
                                .FirstOrDefault(),
                            LastMonthAverage = itemGroup
                                .Where(x => x.PermitDate >= cutoff)
                                .Average(x => (decimal?)x.UnitPrice) ?? 0
                        })
                        .Where(x => x.Quantity > 0)
                        .ToList()
                })
                .Where(x => x.StoredItems.Any())
                .ToList();

            return inventoryStocksResponse;
        }
        
        
    }
}
