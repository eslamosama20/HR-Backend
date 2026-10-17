using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Reports.InventoryReport;
using SmartAndPro.ERP.Domains.Entities.Inventory.InventoryOperation;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;
using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Reports
{
    public class InventoryReportService
    {
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;

        public InventoryReportService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        {
            _repositoriesFactory = repositoriesFactory;
            _appSettings = appSettings;
            _mapper = mapper;
        }
        public async Task<List<InventoryReportResponseDto>> GetInventoryReportAsync(InventoryReportRequestDto reportRequest)
        { 
            try
            {
                var inventoryOperationRepository = _repositoriesFactory.GetRepository<InventoryOperation>();
                var parameters = new Dictionary<string, dynamic>
                {
                    { "StartDate", reportRequest.StartDate },
                    { "EndDate", reportRequest.EndDate },
                    { "StoreDetailsId", reportRequest.Id }
                };

               return await inventoryOperationRepository.ExecuteStored<InventoryReportResponseDto>("sp_get_inventory_report", parameters);
            }
            catch (Exception e)
            {
               LogService.LogError(e);
               throw;
            }
        }
    }
}
