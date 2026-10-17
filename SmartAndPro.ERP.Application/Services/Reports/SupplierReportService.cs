using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Reports.SupplierReport;
using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Reports;

public class SupplierReportService
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    public SupplierReportService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }

    public async Task<List<SupplierReportRecordResponseDto>> GetSupplierReportAsync(SupplierReportRequestDto reportRequest)
    {
        try
        {
            // Get the repository for PurchaseItem
            var purchaseItemRepository = _repositoriesFactory.GetRepository<PurchaseItem>();

            // Filter purchase items based on supplier and the provided date range
            var purchaseItemsQuery = await purchaseItemRepository.Filter(x =>
                x.PurchaseInvoice.SupplierDetailsId == reportRequest.Id &&
                x.CreatedAt >= reportRequest.StartDate && 
                x.CreatedAt <= reportRequest.EndDate);

            var purchaseItems = await purchaseItemsQuery.Data.ToListAsync();

            if (purchaseItems.Count == 0)
            {
                return new List<SupplierReportRecordResponseDto>();
            }

            // Group by MainPurchaseClassId and calculate the total quantity per group
            var groupedPurchaseItems = purchaseItems.GroupBy(pi => pi.MainPurchaseClassId);

            var response = new List<SupplierReportRecordResponseDto>();

            foreach (var group in groupedPurchaseItems)
            {
                var mainPurchaseClass = group.First().MainPurchaseClass;
                var totalQuantity = group.Sum(pi => pi.Quantity);

                // Populate SupplierReportRecordResponseDto
                var reportDto = new SupplierReportRecordResponseDto
                {
                    MainPurchaseClassEnglishName = mainPurchaseClass.EnglishName,
                    MainPurchaseClassArabicName = mainPurchaseClass.ArabicName,
                    Quantity = totalQuantity
                };

                response.Add(reportDto);
            }

            return response;
        }
        catch (Exception e)
        {
            LogService.LogError(e);
            throw;
        }
    }
}