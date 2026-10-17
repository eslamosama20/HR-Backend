using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.InventoryOperation.InventoryOperation;
using SmartAndPro.ERP.Application.Common.Utility;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Inventory.InventoryOperation;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Inventory.InventoryOperationManagement;

public class InventoryOperationService
    : ServiceBase
    <
        InventoryOperation,
        InventoryOperationPostDto,
        InventoryOperationPutDto,
        InventoryOperationResponseDto,
        InventoryOperationFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    private readonly ServiceBaseFactory _serviceBaseFactory;

    public InventoryOperationService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper, ServiceBaseFactory serviceBaseFactory)
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
        _serviceBaseFactory = serviceBaseFactory;
    }
    
    public override async Task<OperationResponse<InventoryOperationResponseDto>> UpdateAsync(InventoryOperationPutDto recipePut)
    {
        var recipe = _mapper.Map<InventoryOperation>(recipePut);
        GuidHelper.NormalizeGuids<InventoryOperation>(recipe);

        var result = new OperationResponse<InventoryOperationResponseDto>(ErrorCodes.GeneralFail, null);

        try
        {
            var operationEntries = recipePut.OperationEntries;
            var existingOperationEntriesQuery = await _repositoriesFactory.GetRepository<OperationEntry>()
                .Filter(x => x.InventoryOperationId == recipe.Id);
            var existingOperationEntries = existingOperationEntriesQuery.Data!.ToList();
            
            // Update or Add paycheck receipt accounts
            foreach (var operationEntry in operationEntries)
            {
                var entryEntity = _mapper.Map<OperationEntry>(operationEntry);
                GuidHelper.NormalizeGuids<OperationEntry>(entryEntity);

                var existingEntry = existingOperationEntries.FirstOrDefault(x => x.Id == operationEntry.Id);
                
                if (existingEntry != null)
                {
                    await _repositoriesFactory.GetRepository<OperationEntry>().Update(entryEntity);
                }
                else
                {
                    await _repositoriesFactory.GetRepository<OperationEntry>().Add(entryEntity);
                }
            }
            
            var entriesToDelete = existingOperationEntries
                .Where(x => !operationEntries.Any(pra => pra.Id == x.Id)).ToList();
            
            foreach (var entryToDelete in entriesToDelete)
            {
                await _repositoriesFactory.GetRepository<OperationEntry>().Delete(entryToDelete);
            }

            recipe.OperationEntries = null;
            var operation = await _repositoriesFactory.GetRepository<InventoryOperation>().Update(recipe);
            
            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<InventoryOperationResponseDto>(operation.ErrorCode, null);
            
            var responseData = _mapper.Map<InventoryOperationResponseDto>(operation.Data);
            
            return new OperationResponse<InventoryOperationResponseDto>(ErrorCodes.Success, responseData);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

}

