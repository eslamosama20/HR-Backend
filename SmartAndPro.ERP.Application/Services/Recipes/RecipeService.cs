using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Recipes.Recipe;
using SmartAndPro.ERP.Application.Common.Utility;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Inventory.PermitManagement;
using SmartAndPro.ERP.Domains.Entities.Recipes;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Recipes;

public class RecipeService
    : ServiceBase
    <
        Recipe,
        RecipePostDto,
        RecipePutDto,
        RecipeResponseDto,
        RecipeFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    private readonly ServiceBaseFactory _serviceBaseFactory;

    public RecipeService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper, ServiceBaseFactory serviceBaseFactory)
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
        _serviceBaseFactory = serviceBaseFactory;
    }
    
    public override async Task<OperationResponse<RecipeResponseDto>> UpdateAsync(RecipePutDto recipePut)
    {
        var recipe = _mapper.Map<Recipe>(recipePut);
        GuidHelper.NormalizeGuids<Recipe>(recipe);

        var result = new OperationResponse<RecipeResponseDto>(ErrorCodes.GeneralFail, null);

        try
        {
            var recipeEntries = recipePut.RecipeEntries;
            var existingRecipeEntriesQuery = await _repositoriesFactory.GetRepository<RecipeEntry>()
                .Filter(x => x.RecipeId == recipe.Id);
            var existingRecipeEntries = existingRecipeEntriesQuery.Data!.ToList();
            
            // Update or Add paycheck receipt accounts
            foreach (var recipeEntry in recipeEntries)
            {
                var entryEntity = _mapper.Map<RecipeEntry>(recipeEntry);
                GuidHelper.NormalizeGuids<RecipeEntry>(entryEntity);

                var existingEntry = existingRecipeEntries.FirstOrDefault(x => x.Id == recipeEntry.Id);
                
                if (existingEntry != null)
                {
                    await _repositoriesFactory.GetRepository<RecipeEntry>().Update(entryEntity);
                }
                else
                {
                    await _repositoriesFactory.GetRepository<RecipeEntry>().Add(entryEntity);
                }
            }
            
            var entriesToDelete = existingRecipeEntries
                .Where(x => !recipeEntries.Any(pra => pra.Id == x.Id)).ToList();
            
            foreach (var entryToDelete in entriesToDelete)
            {
                await _repositoriesFactory.GetRepository<RecipeEntry>().Delete(entryToDelete);
            }

            recipe.RecipeEntries = null;
            var operation = await _repositoriesFactory.GetRepository<Recipe>().Update(recipe);
            
            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<RecipeResponseDto>(operation.ErrorCode, null);
            
            var responseData = _mapper.Map<RecipeResponseDto>(operation.Data);
            
            return new OperationResponse<RecipeResponseDto>(ErrorCodes.Success, responseData);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

}

