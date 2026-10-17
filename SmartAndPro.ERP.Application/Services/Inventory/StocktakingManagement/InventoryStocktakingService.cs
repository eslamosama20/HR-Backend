using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.InventoryOperation.InventoryOperation;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.InventoryOperation.OperationEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.Stocktaking.InventoryStocktaking;
using SmartAndPro.ERP.Application.Common.Utility;
using SmartAndPro.ERP.Application.Enforcers;
using SmartAndPro.ERP.Application.Services.Accounting;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.InventoryOperationManagement;
using SmartAndPro.ERP.Application.Services.Reports;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.InventoryOperation;
using SmartAndPro.ERP.Domains.Entities.Inventory.Stocktaking;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Inventory.StocktakingManagement;

public class InventoryStocktakingService
    : ServiceBase
    <
        InventoryStocktaking,
        InventoryStocktakingPostDto,
        InventoryStocktakingPutDto,
        InventoryStocktakingResponseDto,
        InventoryStocktakingFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    private readonly ServiceBaseFactory _serviceBaseFactory;

    public InventoryStocktakingService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper, ServiceBaseFactory serviceBaseFactory)
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
        _serviceBaseFactory = serviceBaseFactory;
    }

    public override async Task<OperationResponse<InventoryStocktakingResponseDto>> AddAsync(
        InventoryStocktakingPostDto recipePost)
    {
        var result = await base.AddAsync(recipePost);

        try
        {
            if (result.Data != null)
            {
                // Call the new abstracted method to create and handle the transaction
                await CreateTransactionForInventoryStocktakingAsync(result.Data.Id);
                var stocktakingRepository = _repositoriesFactory.GetRepository<InventoryStocktaking>();
                var stocktakingQuery = await stocktakingRepository.Filter(p => p.Id == result.Data.Id);
                var stocktaking = stocktakingQuery.Data.FirstOrDefault();
                
                if (stocktaking == null)
                {
                    throw new Exception("Stocktaking not found");
                }

                if (stocktaking.StocktakingEntries.Count < 1)
                {
                    throw new Exception("Stocktaking Entries not found");
                }
                
                
                var stocksService = _serviceBaseFactory.GetService<InventoryStocksService>();
                var stocks = await stocksService.GetAllStocks(stocktaking.StoreDetailsId);

                var inventoryOperationService = _serviceBaseFactory.GetService<InventoryOperationService>();
                
                var operationEntries = stocktaking.StocktakingEntries
                    .Select(pe => new OperationEntryPostDto
                    {
                        MainPurchaseClassId = pe.MainPurchaseClassId,
                        Quantity = pe.ActualQuantity - pe.ReportedQuantity, // Calculate the difference
                        UnitPrice = stocks.FirstOrDefault()?.StoredItems.FirstOrDefault(x => x.MainPurchaseClassId == pe.MainPurchaseClassId)?.LastPrice ?? 0,
                        StoreDetailsId = stocktaking.StoreDetailsId
                    })
                    .Where(entry => entry.Quantity != 0) // Optionally filter out entries with no change
                    .ToList();

                var inventoryOperation = new InventoryOperationPostDto
                {
                    Description = $"Inventory Stocktaking #{stocktaking.ReferenceNumber}",
                    OperationDate = stocktaking.StocktakingDate,
                    OperationSource = OperationSource.Stocktaking.ToString(),
                    OperationEntries = operationEntries,
                    InventoryStocktakingId = stocktaking.Id,
                    TransactionId = stocktaking.TransactionId
                };

                await inventoryOperationService.AddAsync(inventoryOperation);
            }
        }
        catch (Exception e)
        {
            LogService.LogError(e);
            return new OperationResponse<InventoryStocktakingResponseDto>(ErrorCodes.GeneralFail, null);
        }
        
        return result;
    }


    private async Task CreateTransactionForInventoryStocktakingAsync(Guid inventoryStocktakingId)
    {
        
        var stocktakingRepository = _repositoriesFactory.GetRepository<InventoryStocktaking>();
        var stocktakingQuery = await stocktakingRepository.Filter(p => p.Id == inventoryStocktakingId);
        var stocktaking = stocktakingQuery.Data.FirstOrDefault();
        if (stocktaking == null)
        {
            return; // Stocktaking not found
        }
        
        var stocksService = _serviceBaseFactory.GetService<InventoryStocksService>();
        var stocks = await stocksService.GetAllStocks(stocktaking.StoreDetailsId);

        var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();
        var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();
        var storeDetailsRepository = _repositoriesFactory.GetRepository<StoreDetails>();

        // Fetch the transaction type for stocktaking adjustments
        var transactionTypeQuery = await _repositoriesFactory.GetRepository<TransactionType>()
            .Filter(tt => tt.ArabicName.Contains("جرد المخزون"));
        var transactionTypeId = transactionTypeQuery.Data?.FirstOrDefault()?.Id;
        if (transactionTypeId == null)
        {
            return; // Transaction Type not found
        }

        var mainAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName.Contains("تسوية المخزون"));
        var mainAccount = mainAccountQuery.Data.FirstOrDefault();
        if (mainAccount == null)
        {
            return; // Main account not found
        }

        var storeDetailsQuery = await storeDetailsRepository.Filter(sd => sd.Id == stocktaking.StoreDetailsId);
        var storeDetails = storeDetailsQuery.Data.FirstOrDefault();
        if (storeDetails == null)
        {
            return; // Store account not found
        }

        var transaction = new Transaction
        {
            TransactionTypeId = transactionTypeId.Value,
            Description = $"Stocktaking Adjustment for {stocktaking.ReferenceNumber}",
            TransactionDate = DateOnly.FromDateTime(stocktaking.StocktakingDate),
            TransactionEntries = new List<TransactionEntry>(),
            InventoryStocktakingId = stocktaking.Id
        };


        decimal totalIncreaseAmount = 0;
        decimal totalDecreaseAmount = 0;

        foreach (var entry in stocktaking.StocktakingEntries)
        {
            var quantityDifference = entry.ActualQuantity - entry.ReportedQuantity;
            var lastPrice = stocks.FirstOrDefault()?.StoredItems.FirstOrDefault(x => x.MainPurchaseClassId == entry.MainPurchaseClassId)?.LastPrice ?? 0;
            var amount = Math.Abs(quantityDifference * lastPrice);

            if (quantityDifference > 0)
            {
                // Inventory Increase - Accumulate amount for the increase
                totalIncreaseAmount += amount;
            }
            else if (quantityDifference < 0)
            {
                // Inventory Decrease - Accumulate amount for the decrease
                totalDecreaseAmount += amount;
            }
        }

        // After calculating totals, create grouped entries

        // Inventory Increase - Debit Main Account, Credit Store Account
        if (totalIncreaseAmount > 0)
        {
            transaction.TransactionEntries.Add(new TransactionEntry
            {
                DebitAmount = totalIncreaseAmount,
                CreditAmount = 0,
                MainAccountId = mainAccount.Id,
                Description = stocktaking.Description
            });
            transaction.TransactionEntries.Add(new TransactionEntry
            {
                DebitAmount = 0,
                CreditAmount = totalIncreaseAmount,
                MainAccountId = (Guid)storeDetails.MainAccountId,
                Description = stocktaking.Description
            });
        }

        // Inventory Decrease - Credit Main Account, Debit Store Account
        if (totalDecreaseAmount > 0)
        {
            transaction.TransactionEntries.Add(new TransactionEntry
            {
                DebitAmount = 0,
                CreditAmount = totalDecreaseAmount,
                MainAccountId = mainAccount.Id,
                Description = stocktaking.Description
            });
            transaction.TransactionEntries.Add(new TransactionEntry
            {
                DebitAmount = totalDecreaseAmount,
                CreditAmount = 0,
                MainAccountId = (Guid)storeDetails.MainAccountId,
                Description = stocktaking.Description
            });
        }

        // Validate and save the transaction
        var transactionEnforcer = new TransactionEnforcer();
        var enforcerResult = transactionEnforcer.EnforceRules(transaction);
        if (enforcerResult.IsSuccess)
        {
            var result = await transactionRepository.Add(transaction);
            if (result.ErrorCode == ErrorCodes.Success)
            {
                stocktaking.TransactionId = result.Data.Id;
                stocktaking.StocktakingEntries = null; // Prevent re-serialization issues
                await stocktakingRepository.Update(stocktaking);
            }
        }
    }

    
    public override async Task<OperationResponse<InventoryStocktakingResponseDto>> UpdateAsync(InventoryStocktakingPutDto recipePut)
    {
        var recipe = _mapper.Map<InventoryStocktaking>(recipePut);
        GuidHelper.NormalizeGuids<InventoryStocktaking>(recipe);

        var result = new OperationResponse<InventoryStocktakingResponseDto>(ErrorCodes.GeneralFail, null);

        try
        {
            var operationEntries = recipePut.StocktakingEntries;
            var existingStocktakingEntriesQuery = await _repositoriesFactory.GetRepository<StocktakingEntry>()
                .Filter(x => x.InventoryStocktakingId == recipe.Id);
            var existingStocktakingEntries = existingStocktakingEntriesQuery.Data!.ToList();
            
            // Update or Add paycheck receipt accounts
            foreach (var operationEntry in operationEntries)
            {
                var entryEntity = _mapper.Map<StocktakingEntry>(operationEntry);
                GuidHelper.NormalizeGuids<StocktakingEntry>(entryEntity);

                var existingEntry = existingStocktakingEntries.FirstOrDefault(x => x.Id == operationEntry.Id);
                
                if (existingEntry != null)
                {
                    await _repositoriesFactory.GetRepository<StocktakingEntry>().Update(entryEntity);
                }
                else
                {
                    await _repositoriesFactory.GetRepository<StocktakingEntry>().Add(entryEntity);
                }
            }
            
            var entriesToDelete = existingStocktakingEntries
                .Where(x => !operationEntries.Any(pra => pra.Id == x.Id)).ToList();
            
            foreach (var entryToDelete in entriesToDelete)
            {
                await _repositoriesFactory.GetRepository<StocktakingEntry>().Delete(entryToDelete);
            }

            recipe.StocktakingEntries = null;
            var operation = await _repositoriesFactory.GetRepository<InventoryStocktaking>().Update(recipe);
            
            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<InventoryStocktakingResponseDto>(operation.ErrorCode, null);
            
            var responseData = _mapper.Map<InventoryStocktakingResponseDto>(operation.Data);
            
            return new OperationResponse<InventoryStocktakingResponseDto>(ErrorCodes.Success, responseData);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    public override async Task<OperationResponse<InventoryStocktakingResponseDto>> DeleteAsync(Guid stocktakingId)
    {
        var result = new OperationResponse<InventoryStocktakingResponseDto>(ErrorCodes.GeneralFail, null);
        try
        {
            // Repositories
            var stocktakingRepository = _repositoriesFactory.GetRepository<InventoryStocktaking>();
            var inventoryOperationRepository = _repositoriesFactory.GetRepository<InventoryOperation>();
            var operationEntryRepository = _repositoriesFactory.GetRepository<OperationEntry>();
            var stocktakingEntryRepository = _repositoriesFactory.GetRepository<StocktakingEntry>();
            // Step 1: Fetch the stocktaking
            var stocktakingQuery = await stocktakingRepository.Filter(p => p.Id == stocktakingId);
            var stocktaking = stocktakingQuery.Data.FirstOrDefault();
            if (stocktaking == null)
                return new OperationResponse<InventoryStocktakingResponseDto>(ErrorCodes.NotFound, null);
            
            
            
            // Step 3: Delete related inventory operations
            var inventoryOperationQuery = await inventoryOperationRepository.Filter(op => op.InventoryStocktakingId == stocktakingId);
            var inventoryOperationsToDelete = inventoryOperationQuery.Data.ToList();
            foreach (var operation in inventoryOperationsToDelete)
            {
                var operationEntriesToDelete = operation?.OperationEntries?.ToList() ?? new List<OperationEntry>();
                foreach (var operationEntry in operationEntriesToDelete)
                {
                    await operationEntryRepository.Delete(operationEntry);  
                }
                await inventoryOperationRepository.Delete(operation);
            }

            var stocktakingEntriesToDelete = stocktaking?.StocktakingEntries?.ToList() ?? new List<StocktakingEntry>();
            foreach (var stocktakingEntry in stocktakingEntriesToDelete)
            {
                stocktakingEntry.MainPurchaseClass = null;
                await stocktakingEntryRepository.Delete(stocktakingEntry);
            }
            
            // Delete the Transaction
            var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();
            var transactionQuery = await transactionRepository.Filter(t => t.InventoryStocktakingId == stocktakingId);
            var transaction = transactionQuery.Data
                .FirstOrDefault();
            if (transaction != null)
            {
                await _serviceBaseFactory.GetService<TransactionService>().DeleteAsync(transaction.Id);
            }

            stocktaking.StocktakingEntries = null;
            stocktaking.StoreDetails = null;
            await stocktakingRepository.Delete(stocktaking);
            return new OperationResponse<InventoryStocktakingResponseDto>(ErrorCodes.Success, null);
        }
        catch (Exception e)
        {
            LogService.LogError(e);
        }

        return result;
    }

    public async Task<byte[]> GetStocktakingReportDocument(DateTime? startDate, DateTime? endDate, string? printedBy = null)
    {
        var filters = new InventoryStocktakingFiltersDto();

        var result = await GetAllAsync(filters);
        var items = result.Data ?? new List<InventoryStocktakingResponseDto>();
        
        // Filter by date if provided
        if (startDate.HasValue)
        {
            items = items.Where(x => x.StocktakingDate >= startDate.Value).ToList();
        }
        if (endDate.HasValue)
        {
            items = items.Where(x => x.StocktakingDate <= endDate.Value).ToList();
        }
        
        // Get company info
        var companyRepository = _repositoriesFactory.GetRepository<CompanyDetails>();
        var companyResult = await companyRepository.Filter(c => c.IsActive == true);
        var company = companyResult.Data.FirstOrDefault();

        var document = new DocumentUtils.StocktakingReportDocument(
            items,
            company,
            startDate,
            endDate,
            "ar",
            printedBy
        );

        return document.GeneratePdf();
    }

}


