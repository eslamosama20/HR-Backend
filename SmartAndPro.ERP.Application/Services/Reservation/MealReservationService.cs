using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.MealReservation.MealReservation;
using SmartAndPro.ERP.Application.Common.Utility;
using SmartAndPro.ERP.Application.Enforcers;
using SmartAndPro.ERP.Application.Services.Accounting;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Inventory.PermitManagement;
using SmartAndPro.ERP.Domains.Entities.MealReservation;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.MealReservation;

public class MealReservationService 
    : ServiceBase
    <
        Domains.Entities.MealReservation.MealReservation,
        MealReservationPostDto,
        MealReservationPutDto,
        MealReservationResponseDto,
        MealReservationFiltersDto
    >

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    private readonly ServiceBaseFactory _serviceBaseFactory;
    
    public MealReservationService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper, ServiceBaseFactory serviceBaseFactory)
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
        _serviceBaseFactory = serviceBaseFactory;
    }
    
    public override async Task<OperationResponse<MealReservationResponseDto>> AddAsync(MealReservationPostDto mealReservation)
    {
        var mealReservationRepo = _repositoriesFactory.GetRepository<Domains.Entities.MealReservation.MealReservation>(true);
        try
        {
            await mealReservationRepo.BeginTransactionAsync();
            var result = await base.AddAsync(mealReservation);

            if (result.ErrorCode != ErrorCodes.Success)
                throw new Exception("Meal reservation not added");

            if (result.Data != null)
            {
                // Call the new abstracted method to create and handle the transaction
                await CreateTransactionForMealReservationAsync(result.Data.Id);
                
            }
            await mealReservationRepo.CommitTransactionAsync();
            return result;


        }
        catch (Exception e)
        {
            await mealReservationRepo.RollbackTransactionAsync();
            LogService.LogError(e);
            throw;
        }

    }

    private async Task CreateTransactionForMealReservationAsync(Guid mealReservationId)
    {
        var mealReservationRepository = _repositoriesFactory.GetRepository<Domains.Entities.MealReservation.MealReservation>(true);
        var transactionRepository = _repositoriesFactory.GetRepository<Transaction>(true);
        var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>(true);

        try
        {
            await mealReservationRepository.BeginTransactionAsync();

            var mealReservationQuery = await mealReservationRepository.Filter(r => r.Id == mealReservationId);
            var mealReservation = mealReservationQuery.Data
                .Include(r => r.CustomerProfile)
                .FirstOrDefault();

            if(mealReservation == null)
            {
                throw new Exception("Meal reservation not found");
            }

            // Fetch the transaction type
            var transactionTypeQuery = await _repositoriesFactory.GetRepository<TransactionType>(true)
                .Filter(tt => tt.ArabicName == "قيد حجز");
            var transactionType = transactionTypeQuery.Data.FirstOrDefault();
            if (transactionType == null)
            {
                throw new Exception("Transaction Type not found");
            }

            var transaction = new Transaction
            {
                TransactionTypeId = transactionType.Id,
                Description = $"Post 1, #{mealReservation.ReferenceNumber}",
                TransactionDate = DateOnly.FromDateTime(mealReservation.CreatedAt),
                TransactionEntries = new List<TransactionEntry>(),
                MealReservationId = mealReservation.Id,
                IsTransactionPost1 = true
            };

            var mealReservationEntriesSalesTotal =
                mealReservation.MealReservationEntries
                    .Sum(re =>
                        re.NumberOfNights * re.MealRate * (re.NumberOfPersons - (re?.Frees ?? 0))
                    );

            var mealReservationEntriesCostTotal =
                mealReservation.MealReservationEntries
                    .Sum(re =>
                        re.NumberOfNights * re.MealCost * (re.NumberOfPersons - (re?.Frees ?? 0))
                    );

            var totalSalesPrice = mealReservationEntriesSalesTotal;
            var totalCostPrice = mealReservationEntriesCostTotal;

            if (mealReservation.CustomerProfile.MainAccountId == null)
            {
                throw new Exception("Client company account not found");
            }

            // client company account
            transaction.TransactionEntries.Add(new TransactionEntry
            {
                CreditAmount = 0,
                DebitAmount = totalSalesPrice + (totalSalesPrice * (decimal)0.15),
                MainAccountId = (Guid)mealReservation.CustomerProfile.MainAccountId,
                Description = "Client Company Account",
            });


            var salesRevenueAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "ايرادات مبيعات");
            var salesRevenueAccount = salesRevenueAccountQuery.Data.FirstOrDefault();
            if (salesRevenueAccount == null)
            {
                throw new Exception("Sales Revenue account not found");
            }
            transaction.TransactionEntries.Add(new TransactionEntry
            {
                CreditAmount = totalSalesPrice,
                DebitAmount = 0,
                MainAccountId = salesRevenueAccount.Id,
                Description = "",
            });


            var vatSalesAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "ضريبة القيمة المضافة  المبيعات");
            var vatSalesAccount = vatSalesAccountQuery.Data.FirstOrDefault();
            if (vatSalesAccount == null)
            {
                throw new Exception("VAT Sales account not found");
            }
            transaction.TransactionEntries.Add(new TransactionEntry
            {
                CreditAmount = totalSalesPrice * (decimal)0.15,
                DebitAmount = 0,
                MainAccountId = vatSalesAccount.Id,
                Description = "",
            });
            /*var vatPurchaseAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "ضريبة القيمة المضافة  المشتريات");
            var vatPurchaseAccount = vatPurchaseAccountQuery.Data.FirstOrDefault();
            if (vatPurchaseAccount == null)
            {
                throw new Exception("Purchase VAT account not found");
            }
            transaction.TransactionEntries.Add(new TransactionEntry
            {
                CreditAmount = 0,
                DebitAmount = totalCostPrice * (decimal)0.15,
                MainAccountId = vatPurchaseAccount.Id,
                Description = "",
            });*/


            Console.WriteLine("Transaction Entries Credit: " + transaction.TransactionEntries.Sum(te => te.CreditAmount));
            Console.WriteLine("Transaction Entries Debit: " + transaction.TransactionEntries.Sum(te => te.DebitAmount));
            var transactionEnforcer = new TransactionEnforcer();
            var enforcerResult = transactionEnforcer.EnforceRules(transaction);
            if (enforcerResult.IsSuccess)
            {
                var result = await transactionRepository.Add(transaction);

                if (result.ErrorCode != ErrorCodes.Success)
                    throw new Exception("Transaction not saved");

                //if (result.ErrorCode == ErrorCodes.Success)
                //{

                mealReservation.TransactionId = result.Data.Id;
                mealReservation.CustomerProfile = null;
                mealReservation.MealReservationEntries = null;

                await mealReservationRepository.Update(mealReservation);

                await mealReservationRepository.CommitTransactionAsync();

                //}

            }
        }
        catch (Exception ex)
        {
            await mealReservationRepository.RollbackTransactionAsync();
            LogService.LogError(ex);
        }

    }
      
    public override async Task<OperationResponse<MealReservationResponseDto>> UpdateAsync(MealReservationPutDto mealReservationPut)
    {
        var mealReservationRepository = _repositoriesFactory.GetRepository<Domains.Entities.MealReservation.MealReservation>(true);

        var mealReservation = _mapper.Map<Domains.Entities.MealReservation.MealReservation>(mealReservationPut);
        GuidHelper.NormalizeGuids<Domains.Entities.MealReservation.MealReservation>(mealReservation);

        var result = new OperationResponse<MealReservationResponseDto>(ErrorCodes.GeneralFail, null);

        try
        {
            await mealReservationRepository.BeginTransactionAsync();

            var mealReservationEntries = mealReservationPut.MealReservationEntries;
            var existingPermitEntriesQuery = await _repositoriesFactory.GetRepository<MealReservationEntry>()
                .Filter(x => x.MealReservationId == mealReservation.Id);
            var existingPermitEntries = existingPermitEntriesQuery.Data!.ToList();
            
            foreach (var mealReservationEntry in mealReservationEntries)
            {
                var entryEntity = _mapper.Map<MealReservationEntry>(mealReservationEntry);
                GuidHelper.NormalizeGuids<MealReservationEntry>(entryEntity);

                var existingEntry = existingPermitEntries.FirstOrDefault(x => x.Id == mealReservationEntry.Id);
                
                if (existingEntry != null)
                {
                    existingEntry.MealReservation = null;
                    existingEntry.ReservationMeal = null;
                    entryEntity.MealReservationId = mealReservation.Id;
                    await _repositoriesFactory.GetRepository<MealReservationEntry>().Update(entryEntity);
                }
                else
                {
                    entryEntity.MealReservationId = mealReservation.Id;
                    await _repositoriesFactory.GetRepository<MealReservationEntry>().Add(entryEntity);
                }
            }
            
            var mealReservationEntriesToDelete = existingPermitEntries
                .Where(x => !mealReservationEntries.Any(pra => pra.Id == x.Id)).ToList();
            
            foreach (var roomEntryToDelete in mealReservationEntriesToDelete)
            {
                await _repositoriesFactory.GetRepository<MealReservationEntry>().Delete(roomEntryToDelete);
            }
            
            
            var existingMealReservationQuery = await _repositoriesFactory.GetRepository<Domains.Entities.MealReservation.MealReservation>().Filter(r => r.Id == mealReservation.Id);
            var existingMealReservation = existingMealReservationQuery.Data.FirstOrDefault();
            if (existingMealReservation == null)
            {
                throw new Exception("MealReservation not found");
            }
            mealReservation.MealReservationEntries = null;
            mealReservation.CustomerProfile = null;
            mealReservation.TransactionId = existingMealReservation.TransactionId;
            var operation = await _repositoriesFactory.GetRepository<Domains.Entities.MealReservation.MealReservation>().Update(mealReservation);
            
            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<MealReservationResponseDto>(operation.ErrorCode, null);
            
            var responseData = _mapper.Map<MealReservationResponseDto>(operation.Data);
            // check if transactionId null create transaction if not update
            if (mealReservation.TransactionId != null)
                await UpdateTransactionForMealReservationAsync(mealReservation.Id);
            else 
                await CreateTransactionForMealReservationAsync(mealReservation.Id);

            await mealReservationRepository.CommitTransactionAsync();

            return new OperationResponse<MealReservationResponseDto>(ErrorCodes.Success, responseData);
        }
        catch (Exception ex)
        {
            await mealReservationRepository.RollbackTransactionAsync();
            LogService.LogError(ex);

        }
        return result;
    }
    
    private async Task UpdateTransactionForMealReservationAsync(Guid mealReservationId)
    {
        var mealReservationRepository = _repositoriesFactory.GetRepository<Domains.Entities.MealReservation.MealReservation>(true);
        var transactionRepository = _repositoriesFactory.GetRepository<Transaction>(true);
        var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>(true);
        var transactionEntryRepository = _repositoriesFactory.GetRepository<TransactionEntry>(true);

        try
        { 
            await mealReservationRepository.BeginTransactionAsync();
            var mealReservationQuery = await mealReservationRepository.Filter((p) => p.Id == mealReservationId);
            var mealReservation = mealReservationQuery.Data
                .Include(r => r.MealReservationEntries)
                .Include(r => r.CustomerProfile)
                .FirstOrDefault();
            if (mealReservation == null)
            {
                throw new Exception("MealReservation not found");
            }

            var transactionQuery = await transactionRepository.Filter(t => t.MealReservationId == mealReservationId);
            var transaction = transactionQuery.Data.FirstOrDefault();
            if (transaction == null)
            {
                throw new Exception("Transaction not found");
            }

            var existingTransactionEntriesQuery = await transactionEntryRepository
                .Filter(te => te.TransactionId == transaction.Id);
            var existingTransactionEntries = existingTransactionEntriesQuery.Data.ToList();

            foreach (var existingEntry in existingTransactionEntries)
            {
                existingEntry.MainAccount = null;
                existingEntry.Transaction = null;
                await transactionEntryRepository.Delete(existingEntry);
            }

            var mealReservationEntriesSalesTotal =
                mealReservation.MealReservationEntries
                    .Sum(re =>
                        re.NumberOfNights * re.MealRate * (re.NumberOfPersons - (re?.Frees ?? 0))
                    );

            var mealReservationEntriesCostTotal =
                mealReservation.MealReservationEntries
                    .Sum(re =>
                        re.NumberOfNights * re.MealCost * (re.NumberOfPersons - (re?.Frees ?? 0))
                    );



            var totalSalesPrice = mealReservationEntriesSalesTotal;
            var totalCostPrice = mealReservationEntriesCostTotal;

            if (mealReservation.CustomerProfile.MainAccountId == null)
            {
                throw new Exception("Client company account not found");
            }
            var transactionEntries = new List<TransactionEntry>();

            if (mealReservation.CustomerProfile.MainAccountId == null)
            {
                throw new Exception("Client company account not found");
            }

            // client company account
            transactionEntries.Add(new TransactionEntry
            {
                CreditAmount = 0,
                DebitAmount = totalSalesPrice + (totalSalesPrice * (decimal)0.15),
                MainAccountId = (Guid)mealReservation.CustomerProfile.MainAccountId,
                Description = "Client Company Account",
                TransactionId = transaction.Id
            });


            var salesRevenueAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "ايرادات مبيعات");
            var salesRevenueAccount = salesRevenueAccountQuery.Data.FirstOrDefault();
            if (salesRevenueAccount == null)
            {
                throw new Exception("Sales Revenue account not found");
            }
            transactionEntries.Add(new TransactionEntry
            {
                CreditAmount = totalSalesPrice,
                DebitAmount = 0,
                MainAccountId = salesRevenueAccount.Id,
                Description = "",
                TransactionId = transaction.Id

            });


            var vatSalesAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "ضريبة القيمة المضافة  المبيعات");
            var vatSalesAccount = vatSalesAccountQuery.Data.FirstOrDefault();
            if (vatSalesAccount == null)
            {
                throw new Exception("VAT Sales account not found");
            }
            transactionEntries.Add(new TransactionEntry
            {
                CreditAmount = totalSalesPrice * (decimal)0.15,
                DebitAmount = 0,
                MainAccountId = vatSalesAccount.Id,
                Description = "",
                TransactionId = transaction.Id

            });

            Console.WriteLine("Transaction Entries Credit: " + transactionEntries.Sum(te => te.CreditAmount));
            Console.WriteLine("Transaction Entries Debit: " + transactionEntries.Sum(te => te.DebitAmount));


            var tempTransaction = transaction;
            tempTransaction.TransactionEntries = transactionEntries;
            var transactionEnforcer = new TransactionEnforcer();
            var enforcerResult = transactionEnforcer.EnforceRules(tempTransaction);
            if (enforcerResult.IsSuccess)
            {
                await transactionEntryRepository.AddRange(transactionEntries);
                await mealReservationRepository.CommitTransactionAsync();

            }
            else
            {
                throw new Exception("Transaction Enforcer Error");
            }
        }
        catch(Exception ex)
        {
            await mealReservationRepository.RollbackTransactionAsync();
            LogService.LogError(ex);
        }
  
    }
    
    public override async Task<OperationResponse<MealReservationResponseDto>> DeleteAsync(Guid mealReservationId)
    {
        var mealReservationEntryRepository = _repositoriesFactory.GetRepository<MealReservationEntry>(true);
        var result = new OperationResponse<MealReservationResponseDto>(ErrorCodes.GeneralFail, null);
        try
        {
            await mealReservationEntryRepository.BeginTransactionAsync();
            var mealReservationRepository = _repositoriesFactory.GetRepository<Domains.Entities.MealReservation.MealReservation>(true);
            
            // Fetch the MealReservation
            var mealReservationQuery = await mealReservationRepository.Filter(r => r.Id == mealReservationId);
            var mealReservation = mealReservationQuery.Data.FirstOrDefault();
            if (mealReservation == null)
            {
                return new OperationResponse<MealReservationResponseDto>(ErrorCodes.NotFound, null);
            }
            
            // Undo related MealReservationEntries
            var mealReservationEntriesToDelete = mealReservation.MealReservationEntries;
            foreach (var mealReservationEntryToDelete in mealReservationEntriesToDelete)
            {
                mealReservationEntryToDelete.MealReservation = null;
                await mealReservationEntryRepository.Delete(mealReservationEntryToDelete);
            }
            
            
            // Delete the Transaction
            var transactionRepository = _repositoriesFactory.GetRepository<Transaction>(true);
            var transactionQuery = await transactionRepository.Filter(t => t.MealReservationId == mealReservationId);
            var transaction = transactionQuery.Data.FirstOrDefault();
            if (transaction != null)
            {
                await _serviceBaseFactory.GetService<TransactionService>().DeleteAsync(transaction.Id);
            }
            
            // Delete the MealReservation itself
            mealReservation.MealReservationEntries = null;
            mealReservation.CustomerProfile = null;
            await mealReservationRepository.Delete(mealReservation);

            await mealReservationEntryRepository.CommitTransactionAsync();
            
            return new OperationResponse<MealReservationResponseDto>(ErrorCodes.Success, null);
        }
        catch (Exception ex)
        {
            await mealReservationEntryRepository.RollbackTransactionAsync();
            LogService.LogError(ex);

        }
        return result;
    }
            

}