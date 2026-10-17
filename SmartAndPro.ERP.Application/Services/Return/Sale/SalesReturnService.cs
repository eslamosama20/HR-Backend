using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Enforcers;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Return.Sale;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Return.Sale
{
    public class SalesReturnService
    {
        #region Failds
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly IMapper _mapper;
        #endregion

        #region Constructors
        public SalesReturnService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        {
            _repositoriesFactory = repositoriesFactory;
            _mapper = mapper;
        }

        #endregion

        #region Methods
        public async Task<string> GetLastReturnNo()
        {
            var salesReturnsQuery = await _repositoriesFactory.GetRepository<SalesReturn>().GetAll();
            var salesReturns = await salesReturnsQuery.Data.ToListAsync();
            if (!salesReturns.Any()) return await Task.FromResult(string.Empty);

            var salesReturn = salesReturns.OrderBy(i => i.Id).Last();
            return salesReturn.ReturnNo;
        }
        public async Task<IEnumerable<SalesReturn>> GetAllAsync()
        {
            var salesReturnsQuery = await _repositoriesFactory.GetRepository<SalesReturn>().GetAll();
            return await salesReturnsQuery.Data.ToListAsync();
        }
        public async Task<SalesReturn> GetFirstAsync()
        {
            var salesReturnsQuery = await _repositoriesFactory.GetRepository<SalesReturn>().GetAll();
            var salesReturns = await salesReturnsQuery.Data.FirstOrDefaultAsync();
            return salesReturns;
        }
        public async Task<SalesReturn> GetByIdAsync(int id)
        {
            var salesReturnsQuery = await _repositoriesFactory
                .GetRepository<SalesReturn>().GetAll();
            return await salesReturnsQuery.Data.FirstOrDefaultAsync(i => i.Id == id);
        }
        public async Task<int> AddAsync(SalesReturn salesReturn)
        {
            var resultQuery = await _repositoriesFactory.GetRepository<SalesReturn>().Add(salesReturn);
            var result = resultQuery.Data;
            return result != null ? 1 : 0;
        }
        public async Task<int> UpdateTransaction(int? salesReturnId, Guid? transactionId)
        {
            try
            {
                var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();

                
                var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();
                var salesReturnRepository = _repositoriesFactory.GetRepository<SalesReturn>();
                var transactionEntryRepository = _repositoriesFactory.GetRepository<TransactionEntry>();

                var salesReturnQuery = await salesReturnRepository.Filter(x => x.Id == salesReturnId);
                var salesReturn = salesReturnQuery.Data.FirstOrDefault();
                if (salesReturn == null) return 0;
                
                var transactionQuery = await transactionRepository.Filter(x=>x.Id == transactionId);
                var transaction = transactionQuery.Data.FirstOrDefault();
                if (transaction == null) return 0;

                foreach (var entry in transaction.TransactionEntries)
                {
                    entry.Transaction = null;
                    entry.MainAccount = null;
                    var deletionResult = await transactionEntryRepository.Delete(entry);
                }

                var customer = salesReturn.CustomerProfile;
                
                // Retrieve the "Main Vault" MainAccount
                var mainVaultAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "الخزينة الرئيسية");
                var mainVaultAccount = mainVaultAccountQuery.Data.FirstOrDefault();
                if (mainVaultAccount == null)
                {
                    return 0; // Main Vault account not found
                }

                // Retrieve the "Sale Tax" MainAccount
                var saleTaxAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "ضريبة القيمة المضافة  المبيعات");
                var saleTaxAccount = saleTaxAccountQuery.Data.FirstOrDefault();
                if (saleTaxAccount == null)
                {
                    return 0; // Sale Tax account not found
                }

                // Retrieve Transaction Type for salesReturn transaction
                var transactionTypeQuery = await _repositoriesFactory.GetRepository<TransactionType>()
                    .Filter(tt => tt.ArabicName.Contains("قيد مرتجع"));
                var transactionTypeId = transactionTypeQuery.Data.FirstOrDefault()?.Id;
                if (transactionTypeId == null)
                {
                    return 0; // Transaction Type not found
                }

                // Create transaction entries based on salesReturn items
                var transactionEntries = new List<TransactionEntry>();
                var saleTotalAmount = salesReturn.SalesReturnItems.Sum(item => item.Price * item.Quantity);
                var saleTaxAmount = saleTotalAmount * (decimal)0.15;
                saleTotalAmount += saleTaxAmount;
                var taxEntry = new TransactionEntry
                {
                    TransactionId = transaction.Id,
                    MainAccountId = saleTaxAccount.Id,
                    CreditAmount = 0, 
                    DebitAmount = saleTaxAmount,
                    Description = $"Return #{salesReturn.ReturnNo} - Sale Tax",
                    CreatedAt = DateTime.UtcNow
                };
                transactionEntries.Add(taxEntry);

                // 2. Entry for supplier MainAccount (Debit for supplier)
                var supplierEntry = new TransactionEntry
                {
                    TransactionId = transaction.Id,

                    MainAccountId = customer.MainAccountId.Value,
                    DebitAmount = 0,
                    CreditAmount = saleTotalAmount,
                    Description = $"Return #{salesReturn.ReturnNo} (Customer)",
                    CreatedAt = DateTime.UtcNow
                };
                transactionEntries.Add(supplierEntry);

                var itemsTotal = salesReturn.SalesReturnItems.Sum(item => item.Price * item.Quantity);

                var itemsEntry = new TransactionEntry
                {
                    TransactionId = transaction.Id,

                    MainAccountId = mainVaultAccount.Id,
                    DebitAmount = itemsTotal,
                    CreditAmount = 0,
                    Description = $"Return #{salesReturn.ReturnNo}",
                    CreatedAt = DateTime.UtcNow
                };
                transactionEntries.Add(itemsEntry);

                
                var additionResult = await transactionEntryRepository.AddRange(transactionEntries);
                if (additionResult.Data.Count()== transactionEntries.Count)
                {
                    return 1;
                }


            }
            catch (Exception e)
            {
                LogService.LogError(e);
            }

            return 0;
        }

        
        
        public async Task<int> CreateTransaction(int id)
        {
            try
            {
                var salesReturnRepository = _repositoriesFactory.GetRepository<SalesReturn>();
                var customerRepository = _repositoriesFactory.GetRepository<CustomerProfile>();
                var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();
                var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();

                var salesReturnQuery = await salesReturnRepository.Filter(i => i.Id == id);
                var salesReturn = salesReturnQuery.Data.FirstOrDefault();
                if (salesReturn == null)
                {
                    return 0; // Return not found
                }

                var customerQuery = await customerRepository.Filter(s => s.Id == salesReturn.CustomerProfileId);
                var customer = customerQuery.Data.FirstOrDefault();
                if (customer == null || customer.MainAccountId == null)
                {
                    return 0; 
                }

                // Retrieve the "Main Vault" MainAccount
                var mainVaultAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "الخزينة الرئيسية");
                var mainVaultAccount = mainVaultAccountQuery.Data.FirstOrDefault();
                if (mainVaultAccount == null)
                {
                    return 0; // Main Vault account not found
                }

                // Retrieve the "Sale Tax" MainAccount
                var saleTaxAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "ضريبة القيمة المضافة  المبيعات");
                var saleTaxAccount = saleTaxAccountQuery.Data.FirstOrDefault();
                if (saleTaxAccount == null)
                {
                    return 0; // Sale Tax account not found
                }

                // Retrieve Transaction Type for salesReturn transaction
                var transactionTypeQuery = await _repositoriesFactory.GetRepository<TransactionType>()
                    .Filter(tt => tt.ArabicName.Contains("قيد مرتجع"));
                var transactionTypeId = transactionTypeQuery.Data.FirstOrDefault()?.Id;
                if (transactionTypeId == null)
                {
                    return 0; // Transaction Type not found
                }

                var transactionEntries = new List<TransactionEntry>();
                var saleTotalAmount = salesReturn.SalesReturnItems.Sum(item => item.Price * item.Quantity);
                var saleTaxAmount = saleTotalAmount * (decimal)0.15;
                saleTotalAmount += saleTaxAmount;
                var taxEntry = new TransactionEntry
                {
                    MainAccountId = saleTaxAccount.Id,
                    CreditAmount = 0, 
                    DebitAmount = saleTaxAmount,
                    Description = $"Return #{salesReturn.ReturnNo} - Sale Tax",
                    CreatedAt = DateTime.UtcNow
                };
                transactionEntries.Add(taxEntry);

                var supplierEntry = new TransactionEntry
                {
                    MainAccountId = customer.MainAccountId.Value,
                    DebitAmount = 0,
                    CreditAmount = saleTotalAmount,
                    Description = $"Return #{salesReturn.ReturnNo} (Customer)",
                    CreatedAt = DateTime.UtcNow
                };
                transactionEntries.Add(supplierEntry);

                var itemsTotal = salesReturn.SalesReturnItems.Sum(item => item.Price * item.Quantity);

                var itemsEntry = new TransactionEntry
                {
                    MainAccountId = mainVaultAccount.Id,
                    DebitAmount = itemsTotal,
                    CreditAmount = 0,
                    Description = $"Return #{salesReturn.ReturnNo}",
                    CreatedAt = DateTime.UtcNow
                };
                transactionEntries.Add(itemsEntry);

                // Create the transaction
                var transaction = new Transaction
                {
                    Description = $"قيد مرتجع مبيعات رقم {salesReturn.ReturnNo}",
                    TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    TransactionTypeId = transactionTypeId.Value,
                    TransactionEntries = transactionEntries,
                    SalesReturnId = salesReturn.Id
                };

                // Enforce rules and save transaction
                var transactionEnforcer = new TransactionEnforcer();
                var enforcerResult = transactionEnforcer.EnforceRules(transaction);
                if (enforcerResult.IsSuccess)
                {
                    var result = await transactionRepository.Add(transaction);
                    if (result.Data != null)
                    {
                        salesReturn.SalesReturnItems = null;
                        salesReturn.TransactionId = result.Data.Id;
                        var r = await salesReturnRepository.Update(salesReturn);
                        return r.ErrorCode == ErrorCodes.Success ? 1 : 0;
                    }
                }

                return 0; // If rules enforcement failed
            }
            catch (Exception e)
            {
                LogService.LogError(e);
                return 0; // Handle error
            }
        }

        public async Task<int> EditAsync(SalesReturn salesReturn, List<SalesReturnItem> oldsalesReturnItems)
        {
            try
            {
                var salesReturnRepository = _repositoriesFactory.GetRepository<SalesReturn>();
                var saleItemRepository = _repositoriesFactory.GetRepository<SalesReturnItem>();
                var salesReturnItems = salesReturn.SalesReturnItems;
                salesReturn.SalesReturnItems = null;

                // Delete items that exist in oldsalesReturnItems but not in new salesReturnItems
                var itemsToDelete = oldsalesReturnItems
                    .Where(oldItem => !salesReturnItems.Any(newItem => newItem.Id == oldItem.Id))
                    .ToList();

                foreach (var itemToDelete in itemsToDelete)
                {
                    itemToDelete.MainSalesClass = null;
                    await saleItemRepository.Delete(itemToDelete);
                }

                // Update or add new sale items
                foreach (var saleItem in salesReturnItems)
                {
                    if (saleItem.Id != null)
                    {
                        saleItem.MainSalesClass = null;
                        await saleItemRepository.Update(saleItem);
                    }
                    else
                    {
                        await saleItemRepository.Add(saleItem);
                    }
                }

                // Nullify unrelated properties for the salesReturn before updating
                salesReturn.CustomerProfile= null;
                var response = await salesReturnRepository.Update(salesReturn);

                // Update the transaction if present
                if (salesReturn.TransactionId != null)
                {
                    var transactionUpdateResult = await UpdateTransaction(salesReturn.Id, salesReturn.TransactionId);
                }

                var result = response.Data;
                return result != null ? 1 : 0;
            }
            catch (Exception e)
            {
                LogService.LogError(e);
            }

            return 0;
        }
        public async Task<int> RemoveAsync(SalesReturn salesReturn)
        {
            salesReturn.CustomerProfile = null;
            var salesReturnItems = salesReturn.SalesReturnItems;
            salesReturn.SalesReturnItems = null;
            foreach (var saleItem in salesReturnItems)
            {
                saleItem.MainSalesClass = null;
                saleItem.SalesReturn = null;
                await _repositoriesFactory.GetRepository<SalesReturnItem>().Delete(saleItem);
            }

            var resultQuery = await _repositoriesFactory.GetRepository<SalesReturn>().Delete(salesReturn);
            var result = resultQuery.Data;
            return result != null ? 1 : 0;
        }
        #endregion
    }
}
