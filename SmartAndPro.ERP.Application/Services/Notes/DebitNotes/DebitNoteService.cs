using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Notes.DebitNotes;
using SmartAndPro.ERP.Application.Common.Utility;
using SmartAndPro.ERP.Application.Enforcers;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Notes;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Notes.DebitNotes;


public class DebitNoteService
    : ServiceBase
    <
        DebitNote,
        DebitNotePostDto,
        DebitNotePutDto,
        DebitNoteResponseDto,
        DebitNoteFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public DebitNoteService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper)
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }

    public override async Task<OperationResponse<DebitNoteResponseDto>> AddAsync(DebitNotePostDto dto)
    {
        var result = await base.AddAsync(dto);

        if (result.Data != null)
        {
            // Call the new abstracted method to create and handle the transaction
            await CreateTransactionForDebitNoteAsync(result.Data.Id);
        }

        return result;
    }
    private async Task CreateTransactionForDebitNoteAsync(
        Guid debitNoteId)
    {
        var debitNoteRepository = _repositoriesFactory.GetRepository<DebitNote>();
        var debitNoteQuery =  await debitNoteRepository
            .Filter(p => p.Id == debitNoteId);
        var debitNote = debitNoteQuery.Data.FirstOrDefault();
        if (debitNote == null)
        {
            return;
        }
        
        var debitNoteAccounts = debitNote.DebitNoteAccounts;
        var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();
        var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();

        // Fetch the transaction type
        var transactionTypeQuery = await _repositoriesFactory.GetRepository<TransactionType>()
            .Filter(tt => tt.ArabicName.Contains("قيد يوميه"));
        var transactionTypeId = transactionTypeQuery.Data.FirstOrDefault()?.Id;
        if (transactionTypeId == null)
        {
            return; // Transaction Type not found
        }

        // Retrieve the "Purchase Tax" and "Main Vault" accounts
        var purchaseTaxAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "ضريبة القيمة المضافة  المشتريات");
        var purchaseTaxAccount = purchaseTaxAccountQuery.Data.FirstOrDefault();
        if (purchaseTaxAccount == null)
        {
            return; // Purchase Tax account not found
        }


        var transaction = new Transaction
        {
            TransactionTypeId = transactionTypeId.Value,
            Description = $"{debitNote.Description}",
            TransactionDate = debitNote.ReceiptDate,
            TransactionEntries = new List<TransactionEntry>(),
            DebitNoteId = debitNote.Id
        };

        // Add paycheck receipt accounts to the transaction
        foreach (var account in debitNoteAccounts)
        {
            var entry = new TransactionEntry
            {
                CreditAmount = account.Amount,
                DebitAmount = 0,
                MainAccountId = account.MainAccountId,
                Description = account.Description,
            };
            transaction.TransactionEntries.Add(entry);
        }

        // Add tax entry if applicable
        if (debitNote.Tax > 0)
        {
            var taxEntry = new TransactionEntry
            {
                DebitAmount = 0, // Credit the tax amount
                CreditAmount = debitNote.Tax * debitNote.Amount,
                MainAccountId = purchaseTaxAccount.Id,
                Description = "Tax"
            };
            transaction.TransactionEntries.Add(taxEntry);
        }

        if (debitNote.SupplierDetails.MainAccountId == null)
        {
            throw new Exception("Supplier Details Main Account not found");
        }
        // Add the main entry for the vault account
        var totalAmount = debitNote.Amount + (debitNote.Tax * debitNote.Amount);
        var mainEntry = new TransactionEntry
        {
            CreditAmount = 0, // Debit the total amount
            DebitAmount = totalAmount,
            MainAccountId = (Guid)debitNote.SupplierDetails.MainAccountId,
            Description = debitNote.Description
        };
        transaction.TransactionEntries.Add(mainEntry);

        // Validate and save the transaction
        var transactionEnforcer = new TransactionEnforcer();
        var enforcerResult = transactionEnforcer.EnforceRules(transaction);
        if (enforcerResult.IsSuccess)
        {
            var result = await transactionRepository.Add(transaction);
            if (result.ErrorCode == ErrorCodes.Success)
            {
                debitNote.TransactionId = result.Data.Id;
                debitNote.SupplierDetails = null;
                debitNote.DebitNoteAccounts = null;
                await debitNoteRepository.Update(debitNote);
            }
        }
    }

    public override async Task<OperationResponse<DebitNoteResponseDto>> UpdateAsync(DebitNotePutDto debitNotePut)
    {
    var debitNote = _mapper.Map<DebitNote>(debitNotePut);
    GuidHelper.NormalizeGuids<DebitNote>(debitNote);

    var result = new OperationResponse<DebitNoteResponseDto>(ErrorCodes.GeneralFail, null);

    try
    {
        var debitNoteAccounts = debitNotePut.DebitNoteAccounts;
        var existingDebitNoteAccountsQuery = await _repositoriesFactory.GetRepository<DebitNoteAccount>()
            .Filter(x => x.DebitNoteId == debitNote.Id);
        var existingDebitNoteAccounts = existingDebitNoteAccountsQuery.Data!.ToList();
        
        // Update or Add paycheck receipt accounts
        foreach (var debitNoteAccount in debitNoteAccounts)
        {
            var accountEntity = _mapper.Map<DebitNoteAccount>(debitNoteAccount);
            GuidHelper.NormalizeGuids<DebitNoteAccount>(accountEntity);
            accountEntity.MainAccount = null; // Avoid updating related MainAccount
            accountEntity.DebitNote = null; // Avoid updating the parent DebitNote
            
            var existingAccount = existingDebitNoteAccounts.FirstOrDefault(x => x.Id == debitNoteAccount.Id);
            
            if (existingAccount != null)
            {
                existingAccount.MainAccount = null;
                existingAccount.DebitNote = null;
                
                if (
                    existingAccount.Amount == accountEntity.Amount &&
                    existingAccount.Tax == accountEntity.Tax &&
                    existingAccount.Description == accountEntity.Description &&
                    existingAccount.MainAccountId == accountEntity.MainAccountId
                )
                    continue;
                
                // Update existing account
                await _repositoriesFactory.GetRepository<DebitNoteAccount>().Update(accountEntity);
            }
            else
            {
                // Add new account
                await _repositoriesFactory.GetRepository<DebitNoteAccount>().Add(accountEntity);
            }
        }
        
        // Handle deletion of accounts not present in the current list
        var accountsToDelete = existingDebitNoteAccounts
            .Where(x => !debitNoteAccounts.Any(pra => pra.Id == x.Id)).ToList();
        
        foreach (var accountToDelete in accountsToDelete)
        {
            accountToDelete.MainAccount = null;
            accountToDelete.DebitNote = null;
            await _repositoriesFactory.GetRepository<DebitNoteAccount>().Delete(accountToDelete);
        }

        // Update the paycheck receipt itself
        debitNote.DebitNoteAccounts = null; // Avoid updating accounts in the main entity update
        var operation = await _repositoriesFactory.GetRepository<DebitNote>().Update(debitNote);
        
        if (operation.ErrorCode != ErrorCodes.Success)
            return new OperationResponse<DebitNoteResponseDto>(operation.ErrorCode, null);
        
        var responseData = _mapper.Map<DebitNoteResponseDto>(operation.Data);
        
        await UpdateTransactionForDebitNoteAsync(debitNote.Id);

        return new OperationResponse<DebitNoteResponseDto>(ErrorCodes.Success, responseData);
    }
    catch (Exception ex)
    {
        LogService.LogError(ex);
    }
    return result;
    }
    
    private async Task UpdateTransactionForDebitNoteAsync(Guid debitNoteId)
    {
        var debitNoteRepository = _repositoriesFactory.GetRepository<DebitNote>();
        var debitNoteQuery = await debitNoteRepository.Filter((p) => p.Id == debitNoteId);
        var debitNote = debitNoteQuery.Data.FirstOrDefault();
        if (debitNote ==null | debitNote.TransactionId is null) return;

        var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();

        // Fetch the existing transaction associated with the DebitNote
        var transactionQuery = await transactionRepository.Filter(t => t.DebitNoteId == debitNote.Id);
        var transaction = transactionQuery.Data.FirstOrDefault();

        if (transaction == null)
            return; // Transaction not found

        var transactionEntryRepository = _repositoriesFactory.GetRepository<TransactionEntry>();
        var existingTransactionEntriesQuery = await transactionEntryRepository
            .Filter(te => te.TransactionId == debitNote.TransactionId);
        var existingTransactionEntries = existingTransactionEntriesQuery.Data.ToList();

        foreach (var existingEntry in existingTransactionEntries)
        {
            existingEntry.MainAccount = null;
            existingEntry.Transaction = null;
            await transactionEntryRepository.Delete(existingEntry);
        }

        // Recalculate and add new transaction entries
        var debitNoteAccounts = debitNote.DebitNoteAccounts;
        var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();

        // Retrieve the "Purchase Tax" and "Main Vault" accounts
        var purchaseTaxAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "ضريبة القيمة المضافة  المشتريات");
        var purchaseTaxAccount = purchaseTaxAccountQuery.Data.FirstOrDefault();

        var transactionEntries = new System.Collections.Generic.List<TransactionEntry>();
        
        // Add paycheck receipt accounts to the transaction
        foreach (var account in debitNoteAccounts)
        {
            var entry = new TransactionEntry
            {
                CreditAmount = account.Amount,
                DebitAmount = 0,
                MainAccountId = account.MainAccountId,
                Description = account.Description,
                TransactionId = (Guid)debitNote.TransactionId
            };
            transactionEntries.Add(entry);
        }

        // Add tax entry if applicable
        if (debitNote.Tax > 0)
        {
            var taxEntry = new TransactionEntry
            {
                DebitAmount = 0, // Credit the tax amount
                CreditAmount = debitNote.Tax * debitNote.Amount,
                MainAccountId = purchaseTaxAccount.Id,
                Description = "Tax",
                TransactionId = (Guid)debitNote.TransactionId
            };
            transactionEntries.Add(taxEntry);
        }
        
        if (debitNote.SupplierDetails.MainAccountId == null)
        {
            throw new Exception("Supplier Details Main Account not found");
        }

        // Add the main entry for the vault account
        var totalAmount = debitNote.Amount + (debitNote.Tax  * debitNote.Amount);
        var mainEntry = new TransactionEntry
        {
            CreditAmount = 0, // Debit the total amount
            DebitAmount = totalAmount,
            MainAccountId = (Guid)debitNote.SupplierDetails.MainAccountId,
            Description = debitNote.Description,
            TransactionId = (Guid)debitNote.TransactionId
        };
        transactionEntries.Add(mainEntry);
        
        await transactionEntryRepository.AddRange(transactionEntries);
    }
        
}

