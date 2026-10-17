using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.DamagedInventory;
using SmartAndPro.ERP.Application.Common.Dtos.Permits.InventoryPermit;
using SmartAndPro.ERP.Application.Common.Dtos.Permits.InventoryPermitEntry;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.DamagedInventoryManagement.DocumentUtils;
using SmartAndPro.ERP.Application.Services.Permits;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.DamagedInventoryManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Inventory.DamagedInventoryManagement;

public class DamagedInventoryService : ServiceBase<DamagedInventory, DamagedInventoryPostDto, DamagedInventoryPutDto, DamagedInventoryResponseDto, DamagedInventoryFiltersDto>
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly IMapper _localMapper;
    private readonly ServiceBaseFactory _serviceBaseFactory;

    public DamagedInventoryService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper, ServiceBaseFactory serviceBaseFactory) 
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _localMapper = mapper;
        _serviceBaseFactory = serviceBaseFactory;
    }

    public override async Task<OperationResponse<DamagedInventoryResponseDto>> AddAsync(DamagedInventoryPostDto postDto)
    {
        var addResult = await base.AddAsync(postDto);
        if (addResult.ErrorCode != ErrorCodes.Success)
        {
            return addResult;
        }

        var entity = addResult.Data;
        
        try
        {
            // Create inventory operation to deduct damaged quantity from store
            await CreateInventoryOperationForDamage(entity);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            // Continue anyway, damaged item is saved
        }

        return addResult;
    }
    
    private async Task CreateInventoryOperationForDamage(DamagedInventoryResponseDto damagedInventory)
    {
        // Validate we have a valid MainPurchaseClassId
        if (!damagedInventory.MainPurchaseClassId.HasValue || damagedInventory.MainPurchaseClassId == Guid.Empty)
        {
            LogService.LogError("Cannot create inventory permit: MainPurchaseClassId is null or empty");
            return;
        }
        
        if (!damagedInventory.StoreDetailsId.HasValue || damagedInventory.StoreDetailsId == Guid.Empty)
        {
            LogService.LogError("Cannot create inventory permit: StoreDetailsId is null or empty");
            return;
        }

        // Get the store to get its MainAccountId
        var storeRepository = _repositoriesFactory.GetRepository<StoreDetails>();
        var storeResult = await storeRepository.Filter(s => s.Id == damagedInventory.StoreDetailsId.Value);
        var store = storeResult.Data?.FirstOrDefault();
        
        if (store == null || !store.MainAccountId.HasValue)
        {
            LogService.LogError($"Cannot create inventory permit: Store not found or has no MainAccountId. StoreId: {damagedInventory.StoreDetailsId}");
            return;
        }

        var inventoryPermitService = _serviceBaseFactory.GetService<InventoryPermitService>();
        
        // Create an exit permit to deduct the damaged quantity from inventory
        var inventoryPermitDto = new InventoryPermitPostDto
        {
            Description = $"صرف تالف - {damagedInventory.Reason}",
            PermitDate = DateOnly.FromDateTime(damagedInventory.DamageDate),
            StoreDetailsId = damagedInventory.StoreDetailsId.Value,
            MainAccountId = store.MainAccountId.Value,
            PermitType = "Exit", // Exit means deducting from inventory
            PermitEntries = new List<InventoryPermitEntryPostDto>
            {
                new InventoryPermitEntryPostDto
                {
                    MainPurchaseClassId = damagedInventory.MainPurchaseClassId.Value,
                    Quantity = damagedInventory.Quantity,
                    UnitPrice = 0
                }
            }
        };
        
        var permitResult = await inventoryPermitService.AddAsync(inventoryPermitDto);
        
        if (permitResult.ErrorCode != ErrorCodes.Success)
        {
            LogService.LogError($"Failed to create inventory permit for damaged inventory: {permitResult.ErrorCode}");
        }
        else
        {
            LogService.LogInformation($"Successfully created exit permit for damaged inventory. Deducted {damagedInventory.Quantity} from store.");
        }
    }

    public override async Task<OperationResponse<DamagedInventoryResponseDto>> UpdateAsync(DamagedInventoryPutDto putDto)
    {
        return await base.UpdateAsync(putDto);
    }

    public async Task<List<DamagedInventoryResponseDto>> GetDamagedInventoryWithFiltersAsync(DamagedInventoryFiltersDto filters)
    {
        var repository = _repositoriesFactory.GetRepository<DamagedInventory>();
        var query = await repository.Filter(x => x.IsActive == true);
        var items = query.Data.AsQueryable();

        if (filters.StoreDetailsId.HasValue)
        {
            items = items.Where(x => x.StoreDetailsId == filters.StoreDetailsId.Value);
        }

        if (filters.StartDate.HasValue)
        {
            var startDate = filters.StartDate.Value.Date;
            items = items.Where(x => x.DamageDate >= startDate);
        }

        if (filters.EndDate.HasValue)
        {
            var endDate = filters.EndDate.Value.Date.AddDays(1);
            items = items.Where(x => x.DamageDate < endDate);
        }

        var result = await items
            .Include(x => x.MainPurchaseClass)
            .Include(x => x.MainSalesClass)
            .Include(x => x.StoreDetails)
            .ToListAsync();

        return result.Select(x => new DamagedInventoryResponseDto
        {
            Id = x.Id,
            MainPurchaseClassId = x.MainPurchaseClassId,
            MainPurchaseClassName = x.MainPurchaseClass?.ArabicName,
            MainSalesClassId = x.MainSalesClassId,
            MainSalesClassName = x.MainSalesClass?.ArabicName,
            Quantity = x.Quantity,
            DamageDate = x.DamageDate,
            Reason = x.Reason,
            ReportedBy = x.ReportedBy,
            StoreDetailsId = x.StoreDetailsId,
            StoreName = x.StoreDetails?.ArabicName,
            CreatedAt = x.CreatedAt,
            IsActive = x.IsActive
        }).ToList();
    }

    public async Task<byte[]> GetDamagedInventoryReportDocument(Guid? storeId, DateTime? startDate, DateTime? endDate, string? printedBy = null)
    {
        var filters = new DamagedInventoryFiltersDto
        {
            StoreDetailsId = storeId,
            StartDate = startDate,
            EndDate = endDate
        };

        var items = await GetDamagedInventoryWithFiltersAsync(filters);
        
        // Get company info
        var companyRepository = _repositoriesFactory.GetRepository<CompanyDetails>();
        var companyResult = await companyRepository.Filter(c => c.IsActive == true);
        var company = companyResult.Data.FirstOrDefault();

        // Get store info
        string storeName = "جميع المخازن";
        if (storeId.HasValue)
        {
            var storeRepository = _repositoriesFactory.GetRepository<StoreDetails>();
            var storeResult = await storeRepository.Filter(x => x.Id == storeId.Value);
            var store = storeResult.Data.FirstOrDefault();
            storeName = store?.ArabicName ?? "غير محدد";
        }

        var document = new DamagedInventoryReportDocument(
            items,
            company,
            storeName,
            startDate,
            endDate,
            "ar",
            printedBy
        );

        return document.GeneratePdf();
    }
}
