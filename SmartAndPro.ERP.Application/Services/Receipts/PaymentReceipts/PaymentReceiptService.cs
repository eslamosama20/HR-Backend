using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Receipts.PaymentReceipts;
using SmartAndPro.ERP.Application.Common.Utility;
using SmartAndPro.ERP.Application.Enforcers;
using SmartAndPro.ERP.Application.Services.Accounting;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Receipts;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.Application.Common.Dtos.ReceiptConnections;
using SmartAndPro.ERP.Domains.Entities.Projects;

namespace SmartAndPro.ERP.Application.Services.Receipts.PaymentReceipts;


public class PaymentReceiptService
    : ServiceBase
    <
        PaymentReceipt,
        PaymentReceiptPostDto,
        PaymentReceiptPutDto,
        PaymentReceiptResponseDto,
        PaymentReceiptFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    private readonly ServiceBaseFactory _serviceBaseFactory;
    
    public PaymentReceiptService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper, ServiceBaseFactory serviceBaseFactory)
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
        _serviceBaseFactory = serviceBaseFactory;
    }
    
    public async Task<OperationResponse<List<PaymentReceiptResponseDto>>> GetAllFrontalAsync()
    {
        var receiptRepository = _repositoriesFactory.GetRepository<PaymentReceipt>();
        var frontalReceipts = await receiptRepository.Filter(s => s.IsFrontal);
        
        if (frontalReceipts.Data == null || !frontalReceipts.Data.Any())
        {
            return new OperationResponse<List<PaymentReceiptResponseDto>>(ErrorCodes.NoContent, null);
        }
        var frontalReceiptsMapped = _mapper.Map<List<PaymentReceiptResponseDto>>(frontalReceipts.Data);

        return new OperationResponse<List<PaymentReceiptResponseDto>>(ErrorCodes.Success, frontalReceiptsMapped);
    }
    
    public async Task<OperationResponse<PaymentReceiptResponseDto>> MakeFrontalAsync(Guid receiptId)
    {
        var receiptRepository = _repositoriesFactory.GetRepository<PaymentReceipt>();
        var receiptQuery = await receiptRepository.Filter(x => x.Id == receiptId);
        var receipt = receiptQuery.Data?
            .IgnoreAutoIncludes()
            .FirstOrDefault();
        if (receipt == null)
        {
            return new OperationResponse<PaymentReceiptResponseDto>(ErrorCodes.NotFound, null);
        }
        if (receipt.TransactionId != null)
        {
            var transactionService = _serviceBaseFactory.GetService<TransactionService>();
            var updateTransactionResult = await transactionService.MakeFrontalAsync(receipt.TransactionId.Value);
            if (updateTransactionResult.ErrorCode != ErrorCodes.Success)
            {
                return new OperationResponse<PaymentReceiptResponseDto>(ErrorCodes.GeneralFail, null);
            }
        }
        

        receipt.IsFrontal = true;
        var lastFrontalReceiptQuery = await receiptRepository
            .Filter(r => r.IsFrontal && r.ReferenceNumber.StartsWith("FM-"));

        var lastFrontalReceipt = lastFrontalReceiptQuery.Data?
            .OrderByDescending(r => r.ReferenceNumber)
            .FirstOrDefault();

        int nextNumber = 1;
        if (lastFrontalReceipt != null)
        {
            var numberPart = lastFrontalReceipt.ReferenceNumber.Substring("FM-".Length);
            if (int.TryParse(numberPart, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        receipt.ReferenceNumber = $"FM-{nextNumber}";



        var updateResult = await receiptRepository.Update(receipt);
        
        if (updateResult.Data == null)
        {
            return new OperationResponse<PaymentReceiptResponseDto>(ErrorCodes.GeneralFail, null);
        }

        var mappedReceipt = _mapper.Map<PaymentReceiptResponseDto>(updateResult.Data);
        return new OperationResponse<PaymentReceiptResponseDto>(ErrorCodes.Success, mappedReceipt);
    }

    public override async Task<OperationResponse<PaymentReceiptResponseDto>> AddAsync(PaymentReceiptPostDto dto)
    {
        // Validate ReceiptConnections before processing
        if (dto.ReceiptConnections != null && dto.ReceiptConnections.Any())
        {
            foreach (var connection in dto.ReceiptConnections)
            {
                decimal currentTotal = 0;

                if (connection.SaleInvoiceId.HasValue)
                {
                    var saleInvoiceQuery = await _repositoriesFactory.GetRepository<SaleInvoice>()
                        .Filter(si => si.Id == connection.SaleInvoiceId.Value);
                    var saleInvoice = saleInvoiceQuery.Data.FirstOrDefault();
                    if (saleInvoice == null)
                    {
                        var response = new OperationResponse<PaymentReceiptResponseDto>(ErrorCodes.GeneralFail, null);
                        response.Message = $"SaleInvoice with ID {connection.SaleInvoiceId.Value} not found in DB.";
                        return response;
                    }

                    var receiptConnectionRepo = _repositoriesFactory.GetRepository<ReceiptConnection>();
                    var existingConnections = await receiptConnectionRepo.Filter(r => r.SaleInvoiceId == connection.SaleInvoiceId.Value);
                    currentTotal = existingConnections.Data.Sum(r => r.LinkedAmount);

                    if (currentTotal + connection.LinkedAmount > saleInvoice.TotalPrice)
                    {
                        var response = new OperationResponse<PaymentReceiptResponseDto>(ErrorCodes.GeneralFail, null);
                        response.Message = $"Cannot add payment receipt: linked amount ({connection.LinkedAmount}) + current total ({currentTotal}) exceeds sale invoice total ({saleInvoice.TotalPrice}).";
                        return response;
                    }
                }

                if (connection.PurchaseInvoiceId.HasValue)
                {
                    var purchaseInvoiceQuery = await _repositoriesFactory.GetRepository<PurchaseInvoice>()
                        .Filter(pi => pi.Id == connection.PurchaseInvoiceId.Value);
                    var purchaseInvoice = purchaseInvoiceQuery.Data.FirstOrDefault();

                    if (purchaseInvoice == null)
                    {
                        var response = new OperationResponse<PaymentReceiptResponseDto>(ErrorCodes.GeneralFail, null);
                        response.Message = $"PurchaseInvoice with ID {connection.PurchaseInvoiceId.Value} not found in DB.";
                        return response;
                    }

                    var receiptConnectionRepo = _repositoriesFactory.GetRepository<ReceiptConnection>();
                    var existingConnections = await receiptConnectionRepo.Filter(r => r.PurchaseInvoiceId == connection.PurchaseInvoiceId.Value);
                    currentTotal = existingConnections.Data.Sum(r => r.LinkedAmount);

                    if (currentTotal + connection.LinkedAmount > purchaseInvoice.TotalPrice)
                    {
                        var response = new OperationResponse<PaymentReceiptResponseDto>(ErrorCodes.GeneralFail, null);
                        response.Message = $"Cannot add payment receipt: linked amount ({connection.LinkedAmount}) + current total ({currentTotal}) exceeds purchase invoice total ({purchaseInvoice.TotalPrice}).";
                        return response;
                    }
                }

                if (connection.ReservationId.HasValue)
                {
                    var reservationQuery = await _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Reservation>()
                        .Filter(r => r.Id == connection.ReservationId.Value);
                    var reservation = reservationQuery.Data.FirstOrDefault();
                    if (reservation == null)
                    {
                        var response = new OperationResponse<PaymentReceiptResponseDto>(ErrorCodes.GeneralFail, null);
                        response.Message = $"Reservation with ID {connection.ReservationId.Value} not found in DB.";
                        return response;
                    }

                    // Note: Reservation total calculation may need to be implemented based on business logic
                    var receiptConnectionRepo = _repositoriesFactory.GetRepository<ReceiptConnection>();
                    var existingConnections = await receiptConnectionRepo.Filter(r => r.ReservationId == connection.ReservationId.Value);
                    currentTotal = existingConnections.Data.Sum(r => r.LinkedAmount);

                    // Basic validation - may need to adjust based on reservation total calculation
                    if (currentTotal + connection.LinkedAmount < 0)
                    {
                        var response = new OperationResponse<PaymentReceiptResponseDto>(ErrorCodes.GeneralFail, null);
                        response.Message = $"Cannot add payment receipt: invalid linked amount for reservation.";
                        return response;
                    }
                }
            }
        }

        var dbContext = _repositoriesFactory.GetRepository<PaymentReceipt>().GetDbContext();
        var isTreasuryValid = await dbContext.Set<SmartAndPro.ERP.Domains.Entities.Treasuries.Treasury>()
            .AnyAsync(t => t.Id == dto.TreasuryId);

        if (!isTreasuryValid)
        {
            var bankAcc = await dbContext.Set<SmartAndPro.ERP.Domains.Entities.GeneralBankAccounts.GeneralBankAccount>()
                .FindAsync(dto.TreasuryId);

            if (bankAcc != null && string.IsNullOrWhiteSpace(dto.BankName))
            {
                dto.BankName = string.IsNullOrWhiteSpace(bankAcc.BankName)
                    ? bankAcc.AccountName
                    : $"{bankAcc.AccountName} - {bankAcc.BankName}";
            }

            var fallbackTreasuryId = await dbContext.Set<SmartAndPro.ERP.Domains.Entities.Treasuries.Treasury>()
                .Where(t => t.IsActive == true)
                .Select(t => t.Id)
                .FirstOrDefaultAsync();

            if (fallbackTreasuryId != Guid.Empty)
            {
                dto.TreasuryId = fallbackTreasuryId;
            }
        }

        var result = await base.AddAsync(dto);

        if (result.Data != null)
        {
            // Call the new abstracted method to create and handle the transaction
            await CreateTransactionForPaymentReceiptAsync(result.Data.Id);
            
            
            if (result.Data.IsFrontal == true)
            {
                var makerFrontalResult = await MakeFrontalAsync(result.Data.Id);
                if (makerFrontalResult.ErrorCode == ErrorCodes.Success)
                {
                    result.Data.ReferenceNumber = makerFrontalResult.Data.ReferenceNumber;
                }
                else
                {
                    // If making frontal fails, we can log or handle it as needed
                    LogService.LogError($"Failed to make frontal for PaymentReceipt with ID: {result.Data.Id}");
                }
            }
        }

        return result;
    }
    private async Task CreateTransactionForPaymentReceiptAsync(
        Guid paymentReceiptId)
    {
        var paymentReceiptRepository = _repositoriesFactory.GetRepository<PaymentReceipt>();
        var paymentReceiptQuery =  await paymentReceiptRepository
            .Filter(p => p.Id == paymentReceiptId);
        var paymentReceipt = paymentReceiptQuery.Data.FirstOrDefault();
        if (paymentReceipt == null)
        {
            return;
        }
        
        var paymentReceiptAccounts = paymentReceipt.PaymentReceiptAccounts;
        var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();
        var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();

        // Fetch the transaction type
        var transactionTypeQuery = await _repositoriesFactory.GetRepository<TransactionType>()
            .Filter(tt => tt.ArabicName.Contains("سند صرف"));
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
            Description = $"{paymentReceipt.Description}",
            TransactionDate = paymentReceipt.ReceiptDate,
            TransactionEntries = new List<TransactionEntry>(),
            PaymentReceiptId = paymentReceipt.Id,
            IsFrontal = paymentReceipt.IsFrontal,
        };

        // Add payment receipt accounts to the transaction
        foreach (var account in paymentReceiptAccounts)
        {
            var entry = new TransactionEntry
            {
                CreditAmount = 0,
                DebitAmount = account.Amount,
                MainAccountId = account.MainAccountId,
                Description = account.Description,
            };
            transaction.TransactionEntries.Add(entry);
        }

        // Add tax entry if applicable
        if (paymentReceipt.Tax > 0)
        {
            var taxEntry = new TransactionEntry
            {
                DebitAmount = paymentReceipt.Tax * paymentReceipt.Amount, // Credit the tax amount
                CreditAmount = 0,
                MainAccountId = purchaseTaxAccount.Id,
                Description = "Tax"
            };
            transaction.TransactionEntries.Add(taxEntry);
        }

        // Add the main entry for the vault account
        var totalAmount = paymentReceipt.Amount + (paymentReceipt.Tax * paymentReceipt.Amount);

        Guid vaultMainAccountId = Guid.Empty;
        var dbContext = _repositoriesFactory.GetRepository<PaymentReceipt>().GetDbContext();

        if (paymentReceipt.PaymentMethodType != Domains.Enums.PaymentMethodType.Cash && !string.IsNullOrWhiteSpace(paymentReceipt.BankName))
        {
            var matchedBank = await dbContext.Set<SmartAndPro.ERP.Domains.Entities.GeneralBankAccounts.GeneralBankAccount>()
                .FirstOrDefaultAsync(b => (b.AccountName + " - " + b.BankName) == paymentReceipt.BankName || b.AccountName == paymentReceipt.BankName || b.BankName == paymentReceipt.BankName);
            if (matchedBank != null && matchedBank.MainAccountId != Guid.Empty)
            {
                vaultMainAccountId = matchedBank.MainAccountId;
            }
        }

        if (vaultMainAccountId == Guid.Empty)
        {
            if (paymentReceipt.Treasury != null && paymentReceipt.Treasury.MainAccountId != Guid.Empty)
            {
                vaultMainAccountId = paymentReceipt.Treasury.MainAccountId;
            }
            else
            {
                var treasury = await dbContext.Set<SmartAndPro.ERP.Domains.Entities.Treasuries.Treasury>().FindAsync(paymentReceipt.TreasuryId);
                if (treasury != null && treasury.MainAccountId != Guid.Empty)
                {
                    vaultMainAccountId = treasury.MainAccountId;
                }
                else
                {
                    var bankAccount = await dbContext.Set<SmartAndPro.ERP.Domains.Entities.GeneralBankAccounts.GeneralBankAccount>().FindAsync(paymentReceipt.TreasuryId);
                    if (bankAccount != null && bankAccount.MainAccountId != Guid.Empty)
                    {
                        vaultMainAccountId = bankAccount.MainAccountId;
                    }
                }
            }
        }

        if (vaultMainAccountId == Guid.Empty)
        {
            var defaultVault = await mainAccountRepository.Filter(ma => ma.ArabicName.Contains("الخزينة") || ma.ArabicName.Contains("البنك"));
            vaultMainAccountId = defaultVault.Data?.FirstOrDefault()?.Id ?? Guid.Empty;
        }

        var mainEntry = new TransactionEntry
        {
            CreditAmount = totalAmount, // Debit the total amount
            DebitAmount = 0,
            MainAccountId = vaultMainAccountId,
            Description = paymentReceipt.Description
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
                paymentReceipt.TransactionId = result.Data.Id;
                paymentReceipt.Treasury = null;
                paymentReceipt.PaymentReceiptAccounts = null;
                await paymentReceiptRepository.Update(paymentReceipt);
            }
        }
    }

    public override async Task<OperationResponse<PaymentReceiptResponseDto>> UpdateAsync(PaymentReceiptPutDto paymentReceiptPut)
    {
    var paymentReceipt = _mapper.Map<PaymentReceipt>(paymentReceiptPut);
    GuidHelper.NormalizeGuids<PaymentReceipt>(paymentReceipt);

    var result = new OperationResponse<PaymentReceiptResponseDto>(ErrorCodes.GeneralFail, null);

    try
    {
        var paymentReceiptAccounts = paymentReceiptPut.PaymentReceiptAccounts;
        var existingPaymentReceiptAccountsQuery = await _repositoriesFactory.GetRepository<PaymentReceiptAccount>()
            .Filter(x => x.PaymentReceiptId == paymentReceipt.Id);
        var existingPaymentReceiptAccounts = existingPaymentReceiptAccountsQuery.Data!.ToList();
        
        // Update or Add payment receipt accounts
        foreach (var paymentReceiptAccount in paymentReceiptAccounts)
        {
            var accountEntity = _mapper.Map<PaymentReceiptAccount>(paymentReceiptAccount);
            GuidHelper.NormalizeGuids<PaymentReceiptAccount>(accountEntity);
            accountEntity.MainAccount = null; // Avoid updating related MainAccount
            accountEntity.PaymentReceipt = null; // Avoid updating the parent PaymentReceipt
            
            var existingAccount = existingPaymentReceiptAccounts.FirstOrDefault(x => x.Id == paymentReceiptAccount.Id);
            
            if (existingAccount != null)
            {
                existingAccount.MainAccount = null;
                existingAccount.PaymentReceipt = null;
                
                if (
                    existingAccount.Amount == accountEntity.Amount &&
                    existingAccount.Tax == accountEntity.Tax &&
                    existingAccount.Description == accountEntity.Description &&
                    existingAccount.MainAccountId == accountEntity.MainAccountId
                )
                    continue;
                
                // Update existing account
                await _repositoriesFactory.GetRepository<PaymentReceiptAccount>().Update(accountEntity);
            }
            else
            {
                // Add new account
                await _repositoriesFactory.GetRepository<PaymentReceiptAccount>().Add(accountEntity);
            }
        }
        
        // Handle deletion of accounts not present in the current list
        var accountsToDelete = existingPaymentReceiptAccounts
            .Where(x => !paymentReceiptAccounts.Any(pra => pra.Id == x.Id)).ToList();
        
        foreach (var accountToDelete in accountsToDelete)
        {
            accountToDelete.MainAccount = null;
            accountToDelete.PaymentReceipt = null;
            await _repositoriesFactory.GetRepository<PaymentReceiptAccount>().Delete(accountToDelete);
        }

        // Update the payment receipt itself
        paymentReceipt.PaymentReceiptAccounts = null; // Avoid updating accounts in the main entity update
        paymentReceipt.ReceiptConnections = null; // Handle connections separately
        var operation = await _repositoriesFactory.GetRepository<PaymentReceipt>().Update(paymentReceipt);
        
        // Handle ReceiptConnections update
        if (paymentReceiptPut.ReceiptConnections != null)
        {
            var receiptConnectionRepo = _repositoriesFactory.GetRepository<ReceiptConnection>();
            var existingConnectionsQuery = await receiptConnectionRepo.Filter(x => x.PaymentReceiptId == paymentReceipt.Id);
            var existingConnections = existingConnectionsQuery.Data!.ToList();

            // Update or Add receipt connections
            foreach (var connectionDto in paymentReceiptPut.ReceiptConnections)
            {
                var connectionEntity = _mapper.Map<ReceiptConnection>(connectionDto);
                connectionEntity.PaymentReceiptId = paymentReceipt.Id;
                connectionEntity.PaymentReceipt = null; // Avoid circular reference
                connectionEntity.SaleInvoice = null;
                connectionEntity.PurchaseInvoice = null;
                connectionEntity.Reservation = null;

                if (connectionEntity.Id == Guid.Empty)
                {
                    connectionEntity.Id = Guid.NewGuid();
                }

                var existingConnection = existingConnections.FirstOrDefault(x => x.Id == connectionDto.Id);

                if (existingConnection != null)
                {
                    // Update existing connection
                    await receiptConnectionRepo.Update(connectionEntity);
                }
                else
                {
                    // Add new connection
                    await receiptConnectionRepo.Add(connectionEntity);
                }
            }

            // Delete connections not present in the current list
            var connectionsToDelete = existingConnections
                .Where(x => !paymentReceiptPut.ReceiptConnections.Any(dto => dto.Id == x.Id)).ToList();

            foreach (var connectionToDelete in connectionsToDelete)
            {
                connectionToDelete.PaymentReceipt = null;
                connectionToDelete.SaleInvoice = null;
                connectionToDelete.PurchaseInvoice = null;
                connectionToDelete.Reservation = null;
                await receiptConnectionRepo.Delete(connectionToDelete);
            }
        }
        
        if (operation.ErrorCode != ErrorCodes.Success)
            return new OperationResponse<PaymentReceiptResponseDto>(operation.ErrorCode, null);
        
        var responseData = _mapper.Map<PaymentReceiptResponseDto>(operation.Data);
        
        await UpdateTransactionForPaymentReceiptAsync(paymentReceipt.Id);

        return new OperationResponse<PaymentReceiptResponseDto>(ErrorCodes.Success, responseData);
    }
    catch (Exception ex)
    {
        LogService.LogError(ex);
    }
    return result;
    }
    
    private async Task UpdateTransactionForPaymentReceiptAsync(Guid paymentReceiptId)
    {
        var paymentReceiptRepository = _repositoriesFactory.GetRepository<PaymentReceipt>();
        var paymentReceiptQuery = await paymentReceiptRepository.Filter((p) => p.Id == paymentReceiptId);
        var paymentReceipt = paymentReceiptQuery.Data.FirstOrDefault();
        if (paymentReceipt ==null | paymentReceipt.TransactionId is null) return;

        var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();

        // Fetch the existing transaction associated with the PaymentReceipt
        var transactionQuery = await transactionRepository.Filter(t => t.PaymentReceiptId == paymentReceipt.Id);
        var transaction = transactionQuery.Data.FirstOrDefault();

        if (transaction == null)
            return; // Transaction not found

        var transactionEntryRepository = _repositoriesFactory.GetRepository<TransactionEntry>();
        var existingTransactionEntriesQuery = await transactionEntryRepository
            .Filter(te => te.TransactionId == paymentReceipt.TransactionId);
        var existingTransactionEntries = existingTransactionEntriesQuery.Data.ToList();

        foreach (var existingEntry in existingTransactionEntries)
        {
            existingEntry.MainAccount = null;
            existingEntry.Transaction = null;
            await transactionEntryRepository.Delete(existingEntry);
        }

        // Recalculate and add new transaction entries
        var paymentReceiptAccounts = paymentReceipt.PaymentReceiptAccounts;
        var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();

        // Retrieve the "Purchase Tax" and "Main Vault" accounts
        var purchaseTaxAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "ضريبة القيمة المضافة  المشتريات");
        var purchaseTaxAccount = purchaseTaxAccountQuery.Data.FirstOrDefault();

        var transactionEntries = new System.Collections.Generic.List<TransactionEntry>();
        
        // Add payment receipt accounts to the transaction
        foreach (var account in paymentReceiptAccounts)
        {
            var entry = new TransactionEntry
            {
                CreditAmount = 0,
                DebitAmount = account.Amount,
                MainAccountId = account.MainAccountId,
                Description = account.Description,
                TransactionId = (Guid)paymentReceipt.TransactionId
            };
            transactionEntries.Add(entry);
        }

        // Add tax entry if applicable
        if (paymentReceipt.Tax > 0)
        {
            var taxEntry = new TransactionEntry
            {
                DebitAmount =  paymentReceipt.Tax * paymentReceipt.Amount, // Credit the tax amount
                CreditAmount =0,
                MainAccountId = purchaseTaxAccount.Id,
                Description = "Tax",
                TransactionId = (Guid)paymentReceipt.TransactionId
            };
            transactionEntries.Add(taxEntry);
        }

        // Add the main entry for the vault account
        var totalAmount = paymentReceipt.Amount + (paymentReceipt.Tax  * paymentReceipt.Amount);
        var mainEntry = new TransactionEntry
        {
            CreditAmount = totalAmount, // Debit the total amount
            DebitAmount = 0,
            MainAccountId = paymentReceipt.Treasury.MainAccountId,
            Description = paymentReceipt.Description,
            TransactionId = (Guid)paymentReceipt.TransactionId
        };
        transactionEntries.Add(mainEntry);
        
        await transactionEntryRepository.AddRange(transactionEntries);
    }

    public override async Task<OperationResponse<PaymentReceiptResponseDto>> GetAsync(Guid id)
    {
        try
        {
            var paymentReceiptRepo = _repositoriesFactory.GetRepository<PaymentReceipt>();
            var receiptQuery = await paymentReceiptRepo.Filter(x => x.Id == id);
            
            var receipt = receiptQuery.Data?
                .Include(x => x.ReceiptConnections)
                .Include(x => x.PaymentReceiptAccounts)
                    .ThenInclude(pra => pra.MainAccount)
                .Include(x => x.Treasury)
                    .ThenInclude(t => t.MainAccount)
                .Include(x => x.Attachments)
                .FirstOrDefault();
            
            if (receipt == null)
                return new OperationResponse<PaymentReceiptResponseDto>(ErrorCodes.NotFound, null);
            
            var mappedReceipt = _mapper.Map<PaymentReceiptResponseDto>(receipt);
            await PopulateReceiptConnectionMetadataAsync(mappedReceipt);

            if (mappedReceipt != null && mappedReceipt.PaymentMethodType != Domains.Enums.PaymentMethodType.Cash && !string.IsNullOrWhiteSpace(mappedReceipt.BankName))
            {
                var dbContext = _repositoriesFactory.GetRepository<PaymentReceipt>().GetDbContext();
                var bankAccountsList = await dbContext.Set<SmartAndPro.ERP.Domains.Entities.GeneralBankAccounts.GeneralBankAccount>()
                    .AsNoTracking()
                    .ToListAsync();

                var matchedBank = bankAccountsList.FirstOrDefault(b => 
                    (b.AccountName + " - " + b.BankName) == mappedReceipt.BankName || b.AccountName == mappedReceipt.BankName || b.BankName == mappedReceipt.BankName
                );

                if (matchedBank != null)
                {
                    mappedReceipt.TreasuryId = matchedBank.Id;
                }
            }
            
            return new OperationResponse<PaymentReceiptResponseDto>(ErrorCodes.Success, mappedReceipt);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<PaymentReceiptResponseDto>(ErrorCodes.GeneralFail, null);
        }
    }

    private async Task PopulateReceiptConnectionMetadataAsync(PaymentReceiptResponseDto receipt)
    {
        if (receipt?.ReceiptConnections == null || receipt.ReceiptConnections.Count == 0)
            return;

        var saleInvoiceIds = receipt.ReceiptConnections
            .Where(x => x.SaleInvoiceId.HasValue)
            .Select(x => x.SaleInvoiceId!.Value)
            .Distinct()
            .ToList();

        var purchaseInvoiceIds = receipt.ReceiptConnections
            .Where(x => x.PurchaseInvoiceId.HasValue)
            .Select(x => x.PurchaseInvoiceId!.Value)
            .Distinct()
            .ToList();

        var reservationIds = receipt.ReceiptConnections
            .Where(x => x.ReservationId.HasValue)
            .Select(x => x.ReservationId!.Value)
            .Distinct()
            .ToList();

        var projectIds = receipt.ReceiptConnections
            .Where(x => x.ProjectId.HasValue)
            .Select(x => x.ProjectId!.Value)
            .Distinct()
            .ToList();

        var projectsDict = new Dictionary<Guid, Domains.Entities.Projects.Project>();
        if (projectIds.Count > 0)
        {
            var projectRepo = _repositoriesFactory.GetRepository<Domains.Entities.Projects.Project>();
            var projectQuery = await projectRepo.Filter(p => projectIds.Contains(p.Id));
            var projects = projectQuery.Data?.ToList() ?? new List<Domains.Entities.Projects.Project>();
            projectsDict = projects.ToDictionary(x => x.Id, x => x);
        }

        var saleInvoicesDict = new Dictionary<int, SaleInvoice>();
        if (saleInvoiceIds.Count > 0)
        {
            var saleInvoiceRepo = _repositoriesFactory.GetRepository<SaleInvoice>();
            var saleInvoiceQuery = await saleInvoiceRepo.Filter(si => saleInvoiceIds.Contains(si.Id));
            var saleInvoices = saleInvoiceQuery.Data?.ToList() ?? new List<SaleInvoice>();
            saleInvoicesDict = saleInvoices.ToDictionary(x => x.Id, x => x);
        }

        var purchaseInvoicesDict = new Dictionary<int, PurchaseInvoice>();
        if (purchaseInvoiceIds.Count > 0)
        {
            var purchaseInvoiceRepo = _repositoriesFactory.GetRepository<PurchaseInvoice>();
            var purchaseInvoiceQuery = await purchaseInvoiceRepo.Filter(pi => purchaseInvoiceIds.Contains(pi.Id));
            var purchaseInvoices = purchaseInvoiceQuery.Data?.ToList() ?? new List<PurchaseInvoice>();
            purchaseInvoicesDict = purchaseInvoices.ToDictionary(x => x.Id, x => x);
        }

        var reservationDict = new Dictionary<Guid, SmartAndPro.ERP.Domains.Entities.Reservation.Reservation>();
        if (reservationIds.Count > 0)
        {
            var reservationRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Reservation.Reservation>();
            var reservationQuery = await reservationRepo.Filter(r => reservationIds.Contains(r.Id));
            var reservations = reservationQuery.Data?.ToList() ?? new List<SmartAndPro.ERP.Domains.Entities.Reservation.Reservation>();
            reservationDict = reservations.ToDictionary(x => x.Id, x => x);
        }

        foreach (var connection in receipt.ReceiptConnections)
        {
            if (connection.SaleInvoiceId.HasValue)
            {
                var saleInvoiceId = connection.SaleInvoiceId.Value;
                if (saleInvoicesDict.TryGetValue(saleInvoiceId, out var saleInvoice))
                {
                    connection.InvoiceReference = saleInvoice.InvoiceNo;
                    connection.OperationType = "فاتورة مبيعات";
                }
                connection.InvoiceTotalAmount = connection.LinkedAmount;
                connection.InvoicePaidAmount = connection.LinkedAmount;
                connection.InvoiceRemainingAmount = 0;
            }
            else if (connection.PurchaseInvoiceId.HasValue)
            {
                var purchaseInvoiceId = connection.PurchaseInvoiceId.Value;
                if (purchaseInvoicesDict.TryGetValue(purchaseInvoiceId, out var purchaseInvoice))
                {
                    connection.InvoiceReference = purchaseInvoice.InvoiceNo;
                    connection.OperationType = "فاتورة شراء";
                }
                connection.InvoiceTotalAmount = connection.LinkedAmount;
                connection.InvoicePaidAmount = connection.LinkedAmount;
                connection.InvoiceRemainingAmount = 0;
            }
            else if (connection.ReservationId.HasValue)
            {
                var reservationId = connection.ReservationId.Value;
                if (reservationDict.TryGetValue(reservationId, out var reservation))
                {
                    connection.InvoiceReference = reservation.ReferenceNumber ?? $"R-{reservationId}";
                    connection.OperationType = "حجز";
                }
                connection.InvoiceTotalAmount = connection.LinkedAmount;
                connection.InvoicePaidAmount = connection.LinkedAmount;
                connection.InvoiceRemainingAmount = 0;
            }
            else if (connection.ProjectId.HasValue)
            {
                var projectId = connection.ProjectId.Value;
                if (projectsDict.TryGetValue(projectId, out var project))
                {
                    connection.InvoiceReference = project.ProjectName;
                    connection.OperationType = "مشروع";
                }
                connection.InvoiceTotalAmount = connection.LinkedAmount;
                connection.InvoicePaidAmount = connection.LinkedAmount;
                connection.InvoiceRemainingAmount = 0;
            }

            if (string.IsNullOrWhiteSpace(connection.InvoiceReference))
                connection.InvoiceReference = connection.SaleInvoiceId.HasValue ? $"SI-{connection.SaleInvoiceId}" :
                    connection.PurchaseInvoiceId.HasValue ? $"PI-{connection.PurchaseInvoiceId}" :
                    connection.ReservationId.HasValue ? $"R-{connection.ReservationId}" :
                    connection.ProjectId.HasValue ? $"PRJ-{connection.ProjectId}" : "-";

            if (string.IsNullOrWhiteSpace(connection.OperationType))
                connection.OperationType = "عملية";
        }
    }

    public override async Task<OperationResponse<PaymentReceiptResponseDto>> DeleteAsync(Guid id)
    {
        var result = new OperationResponse<PaymentReceiptResponseDto>(ErrorCodes.GeneralFail, null);
        var paymentReceiptRepo = _repositoriesFactory.GetRepository<PaymentReceipt>(true);
        var dbContext = paymentReceiptRepo.GetDbContext();
        
        try
        {
            await paymentReceiptRepo.BeginTransactionAsync();

            var receipt = await dbContext.Set<PaymentReceipt>()
                .Include(x => x.ReceiptConnections)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (receipt == null)
            {
                await paymentReceiptRepo.RollbackTransactionAsync();
                return new OperationResponse<PaymentReceiptResponseDto>(ErrorCodes.NotFound, null);
            }

            if (!receipt.IsActive)
            {
                await paymentReceiptRepo.CommitTransactionAsync();
                return new OperationResponse<PaymentReceiptResponseDto>(ErrorCodes.NoContent, null);
            }

            // 1. Soft delete the receipt
            receipt.SoftDelete();
            dbContext.Entry(receipt).Property(r => r.IsActive).IsModified = true;

            // 2. Soft delete associated connections
            if (receipt.ReceiptConnections != null)
            {
                foreach (var connection in receipt.ReceiptConnections)
                {
                    connection.SoftDelete();
                    dbContext.Entry(connection).Property(c => c.IsActive).IsModified = true;
                }
            }

            // 3. Soft delete associated transaction
            if (receipt.TransactionId.HasValue)
            {
                var transaction = await dbContext.Set<Transaction>()
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(t => t.Id == receipt.TransactionId.Value);

                if (transaction != null)
                {
                    transaction.SoftDelete();
                    dbContext.Entry(transaction).Property(t => t.IsActive).IsModified = true;
                }
            }

            await dbContext.SaveChangesAsync();
            await paymentReceiptRepo.CommitTransactionAsync();

            var mappedReceipt = _mapper.Map<PaymentReceiptResponseDto>(receipt);
            return new OperationResponse<PaymentReceiptResponseDto>(ErrorCodes.NoContent, mappedReceipt);
        }
        catch (Exception ex)
        {
            await paymentReceiptRepo.RollbackTransactionAsync();
            LogService.LogError(ex);
            return result;
        }
    }

    public override async Task<OperationResponse<PaymentReceiptResponseDto>> DeleteRangeAsync(List<Guid> ids)
    {
        var result = new OperationResponse<PaymentReceiptResponseDto>(ErrorCodes.GeneralFail, null);
        var paymentReceiptRepo = _repositoriesFactory.GetRepository<PaymentReceipt>(true);
        var dbContext = paymentReceiptRepo.GetDbContext();
        
        try
        {
            await paymentReceiptRepo.BeginTransactionAsync();

            var receipts = await dbContext.Set<PaymentReceipt>()
                .Include(x => x.ReceiptConnections)
                .IgnoreQueryFilters()
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            if (receipts == null || !receipts.Any())
            {
                await paymentReceiptRepo.RollbackTransactionAsync();
                return new OperationResponse<PaymentReceiptResponseDto>(ErrorCodes.NotFound, null);
            }

            foreach (var receipt in receipts)
            {
                if (!receipt.IsActive) continue;

                // 1. Soft delete the receipt
                receipt.SoftDelete();
                dbContext.Entry(receipt).Property(r => r.IsActive).IsModified = true;

                // 2. Soft delete associated connections
                if (receipt.ReceiptConnections != null)
                {
                    foreach (var connection in receipt.ReceiptConnections)
                    {
                        connection.SoftDelete();
                        dbContext.Entry(connection).Property(c => c.IsActive).IsModified = true;
                    }
                }

                // 3. Soft delete associated transaction
                if (receipt.TransactionId.HasValue)
                {
                    var transaction = await dbContext.Set<Transaction>()
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(t => t.Id == receipt.TransactionId.Value);

                    if (transaction != null)
                    {
                        transaction.SoftDelete();
                        dbContext.Entry(transaction).Property(t => t.IsActive).IsModified = true;
                    }
                }
            }

            await dbContext.SaveChangesAsync();
            await paymentReceiptRepo.CommitTransactionAsync();

            return new OperationResponse<PaymentReceiptResponseDto>(ErrorCodes.NoContent, null);
        }
        catch (Exception ex)
        {
            await paymentReceiptRepo.RollbackTransactionAsync();
            LogService.LogError(ex);
            return result;
        }
    }
}

