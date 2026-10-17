using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Receipts.PaycheckReceipts;
using SmartAndPro.ERP.Application.Common.Dtos.Receipts.PaycheckReceiptAccountResponse;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.Reservation;
using SmartAndPro.ERP.Application.Common.Utility;
using SmartAndPro.ERP.Application.Enforcers;
using SmartAndPro.ERP.Application.Services.Accounting;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Domains.Entities.Receipts;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.Application.Common.Extensions;
using SmartAndPro.ERP.Domains.Entities.Projects;
using SmartAndPro.ERP.Domains.Entities.Treasuries;

namespace SmartAndPro.ERP.Application.Services.Receipts.PaycheckReceipts;


public class PaycheckReceiptService
    : ServiceBase
    <
        PaycheckReceipt,
        PaycheckReceiptPostDto,
        PaycheckReceiptPutDto,
        PaycheckReceiptResponseDto,
        PaycheckReceiptFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    private readonly ServiceBaseFactory _serviceBaseFactory;
    public PaycheckReceiptService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper, ServiceBaseFactory serviceBaseFactory)
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
        _serviceBaseFactory = serviceBaseFactory;
    }
    
    public async Task<OperationResponse<List<PaycheckReceiptResponseDto>>> GetAllFrontalAsync()
    {
        var receiptRepository = _repositoriesFactory.GetRepository<PaycheckReceipt>();
        var frontalReceipts = await receiptRepository.Filter(s => s.IsFrontal);
        
        if (frontalReceipts.Data == null || !frontalReceipts.Data.Any())
        {
            return new OperationResponse<List<PaycheckReceiptResponseDto>>(ErrorCodes.NoContent, null);
        }
        var frontalReceiptsMapped = _mapper.Map<List<PaycheckReceiptResponseDto>>(frontalReceipts.Data.OrderByDescending(x => x.CreatedAt));

        return new OperationResponse<List<PaycheckReceiptResponseDto>>(ErrorCodes.Success, frontalReceiptsMapped);
    }
    
    public async Task<OperationResponse<PaycheckReceiptResponseDto>> MakeFrontalAsync(Guid receiptId)
    {
        var receiptRepository = _repositoriesFactory.GetRepository<PaycheckReceipt>();
        var receiptQuery = await receiptRepository.Filter(x => x.Id == receiptId);
        var receipt = receiptQuery.Data?
            .IgnoreAutoIncludes()
            .FirstOrDefault();
        if (receipt == null)
        {
            return new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.NotFound, null);
        }
        if (receipt.TransactionId != null)
        {
            var transactionService = _serviceBaseFactory.GetService<TransactionService>();
            var updateTransactionResult = await transactionService.MakeFrontalAsync(receipt.TransactionId.Value);
            if (updateTransactionResult.ErrorCode != ErrorCodes.Success)
            {
                return new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.GeneralFail, null);
            }
        }
        

        receipt.IsFrontal = true;
        var lastFrontalReceiptQuery = await receiptRepository
            .Filter(r => r.IsFrontal && r.ReferenceNumber.StartsWith("FC-"));

        var lastFrontalReceipt = lastFrontalReceiptQuery.Data?
            .OrderByDescending(r => r.ReferenceNumber)
            .FirstOrDefault();

        int nextNumber = 1;
        if (lastFrontalReceipt != null)
        {
            var numberPart = lastFrontalReceipt.ReferenceNumber.Substring("FC-".Length);
            if (int.TryParse(numberPart, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        receipt.ReferenceNumber = $"FC-{nextNumber}";

        
        
        var updateResult = await receiptRepository.Update(receipt);
        
        if (updateResult.Data == null)
        {
            return new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.GeneralFail, null);
        }

        var mappedReceipt = _mapper.Map<PaycheckReceiptResponseDto>(updateResult.Data);
        return new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.Success, mappedReceipt);
    }

    public override async Task<OperationResponse<List<PaycheckReceiptResponseDto>>> GetAllAsync(PaycheckReceiptFiltersDto filters, int? pageNumber = null, int? pageSize = null, string? sortField = null, int? sortOrder = null)
    {
        var result = new OperationResponse<List<PaycheckReceiptResponseDto>>(ErrorCodes.GeneralFail, null); try
        {
            var paycheckReceiptRepository =  _repositoriesFactory.GetRepository<PaycheckReceipt>();

            // Filter to get only non-frontal receipts
            var operation = await paycheckReceiptRepository.Filter(s => !s.IsFrontal);

            if(operation.ErrorCode != ErrorCodes.Success)
            {
                return new OperationResponse<List<PaycheckReceiptResponseDto>>(operation.ErrorCode, null);
            }

            var dataQuery = operation.Data;
            if (dataQuery == null)
                return new OperationResponse<List<PaycheckReceiptResponseDto>>(ErrorCodes.NoContent, null);

            var totalRecords = await dataQuery.CountAsync();


            // Apply sort
            dataQuery = dataQuery.ApplySort(sortField, sortOrder);

            // Apply pagination directly in the query
            if (pageNumber.HasValue && pageSize.HasValue && pageNumber > 0 && pageSize > 0)
            {
                var skip = (pageNumber.Value - 1) * pageSize.Value;
                dataQuery = dataQuery.Skip(skip).Take(pageSize.Value);
            }

            dataQuery = dataQuery.ApplySearchKeyword(filters.SearchKeyword);

            var operationData =
                filters.AllPropertiesAreNull()
                    ? dataQuery.ToList()
                    : dataQuery.FilterQueryableData(filters).ToList();

            if (operationData.Count == 0)
                return new OperationResponse<List<PaycheckReceiptResponseDto>>(ErrorCodes.NoContent, null);

            var responseData = _mapper.Map<List<PaycheckReceiptResponseDto>>(operationData);
            if (pageNumber.HasValue && pageSize.HasValue)
            {
                return new OperationResponse<List<PaycheckReceiptResponseDto>>(
                    ErrorCodes.Success,
                    responseData,
                    totalCount: totalRecords,
                    isPaginated: true
                );
            }
            return new OperationResponse<List<PaycheckReceiptResponseDto>>(ErrorCodes.Success, responseData);
        }
        catch(Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    public override async Task<OperationResponse<PaycheckReceiptResponseDto>> AddAsync(PaycheckReceiptPostDto dto)
    {
        if (dto.ReceiptConnections != null && dto.ReceiptConnections.Any())
        {
            foreach (var connection in dto.ReceiptConnections)
            {
                if (connection.LinkedAmount <= 0)
                {
                    var response = new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.GeneralFail, null);
                    response.Message = "Linked amount must be greater than zero.";
                    return response;
                }
            }

            var receiptConnectionRepo = _repositoriesFactory.GetRepository<ReceiptConnection>();

            var saleInvoiceGroups = dto.ReceiptConnections
                .Where(x => x.SaleInvoiceId.HasValue)
                .GroupBy(x => x.SaleInvoiceId!.Value);

            foreach (var group in saleInvoiceGroups)
            {
                var saleInvoiceRepo = _repositoriesFactory.GetRepository<SaleInvoice>();
                var saleInvoiceQuery = await saleInvoiceRepo.Filter(si => si.Id == group.Key);
                var saleInvoice = saleInvoiceQuery.Data?.FirstOrDefault();
                if (saleInvoice == null)
                {
                    var response = new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.GeneralFail, null);
                    response.Message = $"SaleInvoice with ID {group.Key} not found in DB.";
                    return response;
                }

                var existingConnections = await receiptConnectionRepo.Filter(r => r.SaleInvoiceId == group.Key);
                var currentTotal = existingConnections.Data?.Sum(r => r.LinkedAmount) ?? 0;
                var requestedTotal = group.Sum(x => x.LinkedAmount);
                var invoiceTotal = CalculateSaleInvoiceTotal(saleInvoice);

                if (currentTotal + requestedTotal > invoiceTotal)
                {
                    var response = new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.GeneralFail, null);
                    response.Message = $"Cannot add receipt: requested amount ({requestedTotal:F2}) with existing total ({currentTotal:F2}) exceeds sale invoice total ({invoiceTotal:F2}).";
                    return response;
                }
            }

            var reservationGroups = dto.ReceiptConnections
                .Where(x => x.ReservationId.HasValue)
                .GroupBy(x => x.ReservationId!.Value);

            foreach (var group in reservationGroups)
            {
                var reservationRepo = _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Reservation>();
                var reservationQuery = await reservationRepo.Filter(r => r.Id == group.Key);
                var reservation = reservationQuery.Data?.FirstOrDefault();
                if (reservation == null)
                {
                    var response = new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.GeneralFail, null);
                    response.Message = $"Reservation with ID {group.Key} not found in DB.";
                    return response;
                }

                var reservationDto = _mapper.Map<ReservationResponseDto>(reservation);
                var totalPrice = SmartAndPro.ERP.Application.Services.Reservation.ReservationService.GetTotalAmount(reservationDto);
                var existingConnections = await receiptConnectionRepo.Filter(r => r.ReservationId == group.Key);
                var currentTotal = existingConnections.Data?.Sum(r => r.LinkedAmount) ?? 0;
                var requestedTotal = group.Sum(x => x.LinkedAmount);

                if (currentTotal + requestedTotal > totalPrice)
                {
                    var response = new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.GeneralFail, null);
                    response.Message = $"Cannot add receipt: requested amount ({requestedTotal:F2}) with existing total ({currentTotal:F2}) exceeds reservation total ({totalPrice:F2}).";
                    return response;
                }
            }

            var purchaseInvoiceGroups = dto.ReceiptConnections
                .Where(x => x.PurchaseInvoiceId.HasValue)
                .GroupBy(x => x.PurchaseInvoiceId!.Value);

            foreach (var group in purchaseInvoiceGroups)
            {
                var purchaseInvoiceRepo = _repositoriesFactory.GetRepository<PurchaseInvoice>();
                var purchaseInvoiceQuery = await purchaseInvoiceRepo.Filter(pi => pi.Id == group.Key);
                var purchaseInvoice = purchaseInvoiceQuery.Data?.FirstOrDefault();

                if (purchaseInvoice == null)
                {
                    var response = new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.GeneralFail, null);
                    response.Message = $"PurchaseInvoice with ID {group.Key} not found in DB.";
                    return response;
                }

                var existingConnections = await receiptConnectionRepo.Filter(r => r.PurchaseInvoiceId == group.Key);
                var currentTotal = existingConnections.Data?.Sum(r => r.LinkedAmount) ?? 0;
                var requestedTotal = group.Sum(x => x.LinkedAmount);
                var invoiceTotal = CalculatePurchaseInvoiceTotal(purchaseInvoice);

                if (currentTotal + requestedTotal > invoiceTotal)
                {
                    var response = new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.GeneralFail, null);
                    response.Message = $"Cannot add receipt: requested amount ({requestedTotal:F2}) with existing total ({currentTotal:F2}) exceeds purchase invoice total ({invoiceTotal:F2}).";
                    return response;
                }
            }
        } 
        var result = await base.AddAsync(dto);

        if (result.Data != null)
        {
            // Save ReceiptConnections after creating the receipt
            if (dto.ReceiptConnections != null && dto.ReceiptConnections.Any())
            {
                var receiptConnectionRepo = _repositoriesFactory.GetRepository<ReceiptConnection>();
                foreach (var connectionDto in dto.ReceiptConnections)
                {
                    var connectionEntity = _mapper.Map<ReceiptConnection>(connectionDto);
                    connectionEntity.PaycheckReceiptId = result.Data.Id;
                    connectionEntity.PaycheckReceipt = null!; // Avoid circular reference
                    connectionEntity.SaleInvoice = null!;
                    connectionEntity.PurchaseInvoice = null!;
                    connectionEntity.Reservation = null!;
                    
                    await receiptConnectionRepo.Add(connectionEntity);
                }
                
                // إعادة جلب البيانات مع الـ connections
                var paycheckReceiptRepo = _repositoriesFactory.GetRepository<PaycheckReceipt>();
                var receiptWithConnectionsQuery = await paycheckReceiptRepo.Filter(x => x.Id == result.Data.Id);
                
                if (receiptWithConnectionsQuery.ErrorCode == ErrorCodes.Success)
                {
                    var receiptWithConnections = receiptWithConnectionsQuery.Data?
                        .Include(x => x.ReceiptConnections)
                        .FirstOrDefault();
                    
                    if (receiptWithConnections != null)
                    {
                        result.Data = _mapper.Map<PaycheckReceiptResponseDto>(receiptWithConnections);
                    }
                }
            }
            
            // Call the new abstracted method to create and handle the transaction
            await CreateTransactionForPaycheckReceiptAsync(result.Data.Id);

            if (result.Data.IsFrontal == true)
            {
                var makerFrontalResult = await MakeFrontalAsync(result.Data.Id);
                if (makerFrontalResult.ErrorCode == ErrorCodes.Success && makerFrontalResult.Data != null)
                {
                    result.Data.ReferenceNumber = makerFrontalResult.Data.ReferenceNumber;
                }
                else
                {
                    // If making frontal fails, we can log or handle it as needed
                    LogService.LogError($"Failed to make frontal for PaycheckReceipt with ID: {result.Data.Id}");
                }
            }

            await PopulateReceiptConnectionMetadataAsync(result.Data);
        }

        return result;
    }

    public override async Task<OperationResponse<PaycheckReceiptResponseDto>> UpdateAsync(PaycheckReceiptPutDto paycheckReceiptPut)
    {
        if (paycheckReceiptPut.ReceiptConnections != null && paycheckReceiptPut.ReceiptConnections.Any())
        {
            var receiptConnectionRepo = _repositoriesFactory.GetRepository<ReceiptConnection>();

            var saleInvoiceGroups = paycheckReceiptPut.ReceiptConnections
                .Where(x => x.SaleInvoiceId.HasValue)
                .GroupBy(x => x.SaleInvoiceId!.Value);

            foreach (var group in saleInvoiceGroups)
            {
                var saleInvoiceRepo = _repositoriesFactory.GetRepository<SaleInvoice>();
                var saleInvoiceQuery = await saleInvoiceRepo.Filter(si => si.Id == group.Key);
                var saleInvoice = saleInvoiceQuery.Data?.FirstOrDefault();

                if (saleInvoice == null)
                {
                    var response = new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.GeneralFail, null);
                    response.Message = $"SaleInvoice with ID {group.Key} not found in DB.";
                    return response;
                }

                var invoiceTotal = CalculateSaleInvoiceTotal(saleInvoice);
                var existingConnections = await receiptConnectionRepo.Filter(r =>
                    r.SaleInvoiceId == group.Key &&
                    r.PaycheckReceiptId != paycheckReceiptPut.Id);
                var currentTotal = existingConnections.Data?.Sum(r => r.LinkedAmount) ?? 0;
                var requestedTotal = group.Sum(x => x.LinkedAmount);

                if (currentTotal + requestedTotal > invoiceTotal)
                {
                    var response = new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.GeneralFail, null);
                    response.Message = $"Cannot update receipt: requested amount ({requestedTotal:F2}) with existing total ({currentTotal:F2}) exceeds sale invoice total ({invoiceTotal:F2}).";
                    return response;
                }
            }

            var reservationGroups = paycheckReceiptPut.ReceiptConnections
                .Where(x => x.ReservationId.HasValue)
                .GroupBy(x => x.ReservationId!.Value);

            foreach (var group in reservationGroups)
            {
                var reservationRepo = _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Reservation>();
                var reservationQuery = await reservationRepo.Filter(r => r.Id == group.Key);
                var reservation = reservationQuery.Data?.FirstOrDefault();
                if (reservation == null)
                {
                    var response = new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.GeneralFail, null);
                    response.Message = $"Reservation with ID {group.Key} not found in DB.";
                    return response;
                }

                var reservationDto = _mapper.Map<ReservationResponseDto>(reservation);
                var totalPrice = SmartAndPro.ERP.Application.Services.Reservation.ReservationService.GetTotalAmount(reservationDto);
                var existingConnections = await receiptConnectionRepo.Filter(r =>
                    r.ReservationId == group.Key &&
                    r.PaycheckReceiptId != paycheckReceiptPut.Id);
                var currentTotal = existingConnections.Data?.Sum(r => r.LinkedAmount) ?? 0;
                var requestedTotal = group.Sum(x => x.LinkedAmount);

                if (currentTotal + requestedTotal > totalPrice)
                {
                    var response = new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.GeneralFail, null);
                    response.Message = $"Cannot update receipt: requested amount ({requestedTotal:F2}) with existing total ({currentTotal:F2}) exceeds reservation total ({totalPrice:F2}).";
                    return response;
                }
            }

            var purchaseInvoiceGroups = paycheckReceiptPut.ReceiptConnections
                .Where(x => x.PurchaseInvoiceId.HasValue)
                .GroupBy(x => x.PurchaseInvoiceId!.Value);

            foreach (var group in purchaseInvoiceGroups)
            {
                var purchaseInvoiceRepo = _repositoriesFactory.GetRepository<PurchaseInvoice>();
                var purchaseInvoiceQuery = await purchaseInvoiceRepo.Filter(pi => pi.Id == group.Key);
                var purchaseInvoice = purchaseInvoiceQuery.Data?.FirstOrDefault();

                if (purchaseInvoice == null)
                {
                    var response = new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.GeneralFail, null);
                    response.Message = $"PurchaseInvoice with ID {group.Key} not found in DB.";
                    return response;
                }

                var invoiceTotal = CalculatePurchaseInvoiceTotal(purchaseInvoice);
                var existingConnections = await receiptConnectionRepo.Filter(r =>
                    r.PurchaseInvoiceId == group.Key &&
                    r.PaycheckReceiptId != paycheckReceiptPut.Id);
                var currentTotal = existingConnections.Data?.Sum(r => r.LinkedAmount) ?? 0;
                var requestedTotal = group.Sum(x => x.LinkedAmount);

                if (currentTotal + requestedTotal > invoiceTotal)
                {
                    var response = new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.GeneralFail, null);
                    response.Message = $"Cannot update receipt: requested amount ({requestedTotal:F2}) with existing total ({currentTotal:F2}) exceeds purchase invoice total ({invoiceTotal:F2}).";
                    return response;
                }
            }
        }

        var paycheckReceipt = _mapper.Map<PaycheckReceipt>(paycheckReceiptPut);
        GuidHelper.NormalizeGuids<PaycheckReceipt>(paycheckReceipt);

        var result = new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.GeneralFail, null);

        try
        {
            var paycheckReceiptAccounts = paycheckReceiptPut.PaycheckReceiptAccounts ?? new List<PaycheckReceiptAccountPutDto>();
            var existingPaycheckReceiptAccountsQuery = await _repositoriesFactory.GetRepository<PaycheckReceiptAccount>()
                .Filter(x => x.PaycheckReceiptId == paycheckReceipt.Id);
            var existingPaycheckReceiptAccounts = existingPaycheckReceiptAccountsQuery.Data!.ToList();

            foreach (var paycheckReceiptAccount in paycheckReceiptAccounts)
            {
                var accountEntity = _mapper.Map<PaycheckReceiptAccount>(paycheckReceiptAccount);
                GuidHelper.NormalizeGuids<PaycheckReceiptAccount>(accountEntity);
                accountEntity.MainAccount = null!;
                accountEntity.PaycheckReceipt = null!;

                var existingAccount = existingPaycheckReceiptAccounts.FirstOrDefault(x => x.Id == paycheckReceiptAccount.Id);

                if (existingAccount != null)
                {
                    existingAccount.MainAccount = null!;
                    existingAccount.PaycheckReceipt = null!;

                    if (
                        existingAccount.Amount == accountEntity.Amount &&
                        existingAccount.Tax == accountEntity.Tax &&
                        existingAccount.Description == accountEntity.Description &&
                        existingAccount.MainAccountId == accountEntity.MainAccountId)
                        continue;

                    await _repositoriesFactory.GetRepository<PaycheckReceiptAccount>().Update(accountEntity);
                }
                else
                {
                    await _repositoriesFactory.GetRepository<PaycheckReceiptAccount>().Add(accountEntity);
                }
            }

            var accountsToDelete = existingPaycheckReceiptAccounts
                .Where(x => !paycheckReceiptAccounts.Any(pra => pra.Id == x.Id)).ToList();

            foreach (var accountToDelete in accountsToDelete)
            {
                accountToDelete.MainAccount = null!;
                accountToDelete.PaycheckReceipt = null!;
                await _repositoriesFactory.GetRepository<PaycheckReceiptAccount>().Delete(accountToDelete);
            }

            paycheckReceipt.PaycheckReceiptAccounts = null!;
            paycheckReceipt.ReceiptConnections = null!;
            var operation = await _repositoriesFactory.GetRepository<PaycheckReceipt>().Update(paycheckReceipt);

            if (paycheckReceiptPut.ReceiptConnections != null)
            {
                var receiptConnectionRepo = _repositoriesFactory.GetRepository<ReceiptConnection>();
                var existingConnectionsQuery = await receiptConnectionRepo.Filter(x => x.PaycheckReceiptId == paycheckReceipt.Id);
                var existingConnections = existingConnectionsQuery.Data!.ToList();

                foreach (var connectionDto in paycheckReceiptPut.ReceiptConnections)
                {
                    var connectionEntity = _mapper.Map<ReceiptConnection>(connectionDto);
                    connectionEntity.PaycheckReceiptId = paycheckReceipt.Id;
                    connectionEntity.PaycheckReceipt = null!;
                    connectionEntity.SaleInvoice = null!;
                    connectionEntity.PurchaseInvoice = null!;
                    connectionEntity.Reservation = null!;

                    if (connectionEntity.Id == Guid.Empty)
                    {
                        connectionEntity.Id = Guid.NewGuid();
                    }

                    var existingConnection = existingConnections.FirstOrDefault(x => x.Id == connectionDto.Id);

                    if (existingConnection != null)
                    {
                        await receiptConnectionRepo.Update(connectionEntity);
                    }
                    else
                    {
                        await receiptConnectionRepo.Add(connectionEntity);
                    }
                }

                var connectionsToDelete = existingConnections
                    .Where(x => !paycheckReceiptPut.ReceiptConnections.Any(dto => dto.Id == x.Id)).ToList();

                foreach (var connectionToDelete in connectionsToDelete)
                {
                    connectionToDelete.PaycheckReceipt = null!;
                    connectionToDelete.SaleInvoice = null!;
                    connectionToDelete.PurchaseInvoice = null!;
                    connectionToDelete.Reservation = null!;
                    await receiptConnectionRepo.Delete(connectionToDelete);
                }
            }

            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<PaycheckReceiptResponseDto>(operation.ErrorCode, null);

            var responseData = _mapper.Map<PaycheckReceiptResponseDto>(operation.Data);

            await PopulateReceiptConnectionMetadataAsync(responseData);
            await UpdateTransactionForPaycheckReceiptAsync(paycheckReceipt.Id);

            return new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.Success, responseData);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    private async Task CreateTransactionForPaycheckReceiptAsync(Guid paycheckReceiptId)
    {
        var paycheckReceiptRepository = _repositoriesFactory.GetRepository<PaycheckReceipt>();
        var paycheckReceiptQuery = await paycheckReceiptRepository
            .Filter(p => p.Id == paycheckReceiptId);

        var paycheckReceipt = paycheckReceiptQuery.Data?
            .Include(p => p.PaycheckReceiptAccounts)
            .Include(p => p.Treasury)
            .FirstOrDefault();

        if (paycheckReceipt == null)
            return;

        var transactionTypeQuery = await _repositoriesFactory.GetRepository<TransactionType>()
            .Filter(tt => tt.ArabicName.Contains("سند قبض"));
        var transactionTypeId = transactionTypeQuery.Data?.FirstOrDefault()?.Id;
        if (transactionTypeId == null)
            return;

        var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();
        var purchaseTaxAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "ضريبة القيمة المضافة  المشتريات");
        var purchaseTaxAccount = purchaseTaxAccountQuery.Data?.FirstOrDefault();
        if (purchaseTaxAccount == null)
            return;

        if (paycheckReceipt.Treasury == null)
            return;

        var transactionEntries = new List<TransactionEntry>();
        var paycheckReceiptAccounts = paycheckReceipt.PaycheckReceiptAccounts ?? new List<PaycheckReceiptAccount>();

        foreach (var account in paycheckReceiptAccounts)
        {
            var entry = new TransactionEntry
            {
                CreditAmount = account.Amount,
                DebitAmount = 0,
                MainAccountId = account.MainAccountId,
                Description = account.Description ?? paycheckReceipt.Description
            };
            transactionEntries.Add(entry);
        }

        if (paycheckReceipt.Tax > 0)
        {
            var taxEntry = new TransactionEntry
            {
                DebitAmount = 0,
                CreditAmount = paycheckReceipt.Tax * paycheckReceipt.Amount,
                MainAccountId = purchaseTaxAccount.Id,
                Description = "Tax"
            };
            transactionEntries.Add(taxEntry);
        }

        var totalAmount = paycheckReceipt.Amount + (paycheckReceipt.Tax * paycheckReceipt.Amount);
        var mainEntry = new TransactionEntry
        {
            CreditAmount = 0,
            DebitAmount = totalAmount,
            MainAccountId = paycheckReceipt.Treasury.MainAccountId,
            Description = paycheckReceipt.Description
        };
        transactionEntries.Add(mainEntry);

        var transaction = new Transaction
        {
            TransactionTypeId = transactionTypeId.Value,
            Description = paycheckReceipt.Description,
            TransactionDate = paycheckReceipt.ReceiptDate,
            TransactionEntries = transactionEntries,
            PaycheckReceiptId = paycheckReceipt.Id,
            IsFrontal = paycheckReceipt.IsFrontal
        };

        var transactionEnforcer = new TransactionEnforcer();
        var enforcerResult = transactionEnforcer.EnforceRules(transaction);
        if (!enforcerResult.IsSuccess)
            return;

        var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();
        var transactionResult = await transactionRepository.Add(transaction);
        if (transactionResult.ErrorCode != ErrorCodes.Success || transactionResult.Data == null)
            return;

        paycheckReceipt.TransactionId = transactionResult.Data.Id;
        paycheckReceipt.Treasury = null!;
        paycheckReceipt.PaycheckReceiptAccounts = null!;
        await paycheckReceiptRepository.Update(paycheckReceipt);
    }

    private async Task UpdateTransactionForPaycheckReceiptAsync(Guid paycheckReceiptId)
    {
        var paycheckReceiptRepository = _repositoriesFactory.GetRepository<PaycheckReceipt>();
        var paycheckReceiptQuery = await paycheckReceiptRepository
            .Filter(p => p.Id == paycheckReceiptId);

        var paycheckReceipt = paycheckReceiptQuery.Data?
            .Include(p => p.PaycheckReceiptAccounts)
            .Include(p => p.Treasury)
            .FirstOrDefault();

        if (paycheckReceipt == null || paycheckReceipt.TransactionId is null || paycheckReceipt.Treasury == null)
            return;

        var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();

        // Fetch the existing transaction associated with the PaycheckReceipt
        var transactionQuery = await transactionRepository.Filter(t => t.PaycheckReceiptId == paycheckReceipt.Id);
        var transaction = transactionQuery.Data?.FirstOrDefault();

        if (transaction == null)
            return; // Transaction not found

        var transactionEntryRepository = _repositoriesFactory.GetRepository<TransactionEntry>();
        var existingTransactionEntriesQuery = await transactionEntryRepository
            .Filter(te => te.TransactionId == paycheckReceipt.TransactionId);
        var existingTransactionEntries = existingTransactionEntriesQuery.Data?.ToList() ?? new List<TransactionEntry>();

        foreach (var existingEntry in existingTransactionEntries)
        {
            existingEntry.MainAccount = null!;
            existingEntry.Transaction = null!;
            await transactionEntryRepository.Delete(existingEntry);
        }

        // Recalculate and add new transaction entries
    var paycheckReceiptAccounts = paycheckReceipt.PaycheckReceiptAccounts ?? new List<PaycheckReceiptAccount>();
        var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();

        // Retrieve the "Purchase Tax" and "Main Vault" accounts
        var purchaseTaxAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "ضريبة القيمة المضافة  المشتريات");
    var purchaseTaxAccount = purchaseTaxAccountQuery.Data?.FirstOrDefault();

        var transactionEntries = new System.Collections.Generic.List<TransactionEntry>();
        
        // Add paycheck receipt accounts to the transaction
        foreach (var account in paycheckReceiptAccounts)
        {
            var entry = new TransactionEntry
            {
                CreditAmount = account.Amount,
                DebitAmount = 0,
                MainAccountId = account.MainAccountId,
                Description = account.Description,
                TransactionId = paycheckReceipt.TransactionId.Value
            };
            transactionEntries.Add(entry);
        }

        // Add tax entry if applicable
        if (paycheckReceipt.Tax > 0 && purchaseTaxAccount != null)
        {
            var taxEntry = new TransactionEntry
            {
                DebitAmount = 0, // Credit the tax amount
                CreditAmount = paycheckReceipt.Tax * paycheckReceipt.Amount,
                MainAccountId = purchaseTaxAccount.Id,
                Description = "Tax",
                TransactionId = paycheckReceipt.TransactionId.Value
            };
            transactionEntries.Add(taxEntry);
        }

        // Add the main entry for the vault account
        var totalAmount = paycheckReceipt.Amount + (paycheckReceipt.Tax  * paycheckReceipt.Amount);
        var mainEntry = new TransactionEntry
        {
            CreditAmount = 0, // Debit the total amount
            DebitAmount = totalAmount,
            MainAccountId = paycheckReceipt.Treasury.MainAccountId,
            Description = paycheckReceipt.Description,
            TransactionId = paycheckReceipt.TransactionId.Value
        };
        transactionEntries.Add(mainEntry);
        
        await transactionEntryRepository.AddRange(transactionEntries);
    }

    public async Task<OperationResponse<List<PaycheckReceiptResponseDto>>> GetBySaleInvoiceIdAsync(int saleInvoiceId)
    {
        try
        {
            var receiptRepo = _repositoriesFactory.GetRepository<PaycheckReceipt>();
            var receiptsQuery = await receiptRepo.Filter(r => r.ReceiptConnections.Any(c => c.SaleInvoiceId == saleInvoiceId));
            
            if (receiptsQuery.ErrorCode != ErrorCodes.Success || receiptsQuery.Data == null)
                return new OperationResponse<List<PaycheckReceiptResponseDto>>(receiptsQuery.ErrorCode, null);
                
            var responseData = _mapper.Map<List<PaycheckReceiptResponseDto>>(receiptsQuery.Data);
            return new OperationResponse<List<PaycheckReceiptResponseDto>>(ErrorCodes.Success, responseData);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<List<PaycheckReceiptResponseDto>>(ErrorCodes.GeneralFail, null);
        }
    }

    public async Task<OperationResponse<decimal>> GetInvoicePaidAmountAsync(int saleInvoiceId)
    {
        try
        {
            var receiptConnectionRepo = _repositoriesFactory.GetRepository<ReceiptConnection>();
            var connectionsQuery = await receiptConnectionRepo.Filter(rc => rc.SaleInvoiceId == saleInvoiceId);
            var totalPaid = connectionsQuery.Data?.Sum(rc => rc.LinkedAmount) ?? 0;
            
            return new OperationResponse<decimal>(ErrorCodes.Success, totalPaid);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<decimal>(ErrorCodes.GeneralFail, 0);
        }
    }

    public async Task<OperationResponse<decimal>> GetInvoiceRemainingAmountAsync(int saleInvoiceId)
    {
        try
        {
            var saleInvoiceRepo = _repositoriesFactory.GetRepository<SaleInvoice>();
            var invoiceQuery = await saleInvoiceRepo.Filter(si => si.Id == saleInvoiceId);
            var invoice = invoiceQuery.Data?.FirstOrDefault();
            
            if (invoice == null)
                return new OperationResponse<decimal>(ErrorCodes.NotFound, 0);

            var paidAmountResult = await GetInvoicePaidAmountAsync(saleInvoiceId);
            if (paidAmountResult.ErrorCode != ErrorCodes.Success)
                return paidAmountResult;

            var invoiceTotal = CalculateSaleInvoiceTotal(invoice);
            var remainingAmount = Math.Max(0, invoiceTotal - paidAmountResult.Data);
            return new OperationResponse<decimal>(ErrorCodes.Success, remainingAmount);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<decimal>(ErrorCodes.GeneralFail, 0);
        }
    }

    private static decimal CalculateSaleInvoiceTotal(SaleInvoice saleInvoice)
    {
        if (saleInvoice == null)
            return 0;

        var itemsTotal = saleInvoice.SaleItems?.Sum(item => item.TotalPrice) ?? 0;
        if (itemsTotal > 0)
            return Math.Round(itemsTotal, 2);

        var netItems = saleInvoice.SaleItems?.Sum(item => item.NetPrice) ?? 0;
        var taxItems = saleInvoice.SaleItems?.Sum(item => item.TaxAmount) ?? 0;
        var discountItems = saleInvoice.SaleItems?.Sum(item => item.DiscountAmount) ?? 0;
        var computed = netItems + taxItems - discountItems;
        if (computed > 0)
            return Math.Round(computed, 2);

        var invoiceTotals = saleInvoice.NetPrice + saleInvoice.TaxAmount;
        if (invoiceTotals > 0)
            return Math.Round(invoiceTotals, 2);

        if (saleInvoice.TotalPrice > 0)
            return Math.Round(saleInvoice.TotalPrice, 2);

        return 0;
    }

    private static decimal CalculatePurchaseInvoiceTotal(PurchaseInvoice purchaseInvoice)
    {
        if (purchaseInvoice == null)
            return 0;

        var itemsTotal = purchaseInvoice.PurchaseItems?.Sum(item => item.TotalPrice) ?? 0;
        if (itemsTotal > 0)
            return Math.Round(itemsTotal, 2);

        var netItems = purchaseInvoice.PurchaseItems?.Sum(item => item.NetPrice) ?? 0;
        var taxItems = purchaseInvoice.PurchaseItems?.Sum(item => item.TaxAmount) ?? 0;
        var discountItems = purchaseInvoice.PurchaseItems?.Sum(item => item.DiscountAmount) ?? 0;
        var computed = netItems + taxItems - discountItems;
        if (computed > 0)
            return Math.Round(computed, 2);

        var invoiceTotals = purchaseInvoice.NetPrice + purchaseInvoice.TaxAmount;
        if (invoiceTotals > 0)
            return Math.Round(invoiceTotals, 2);

        if (purchaseInvoice.TotalPrice > 0)
            return Math.Round(purchaseInvoice.TotalPrice, 2);

        return 0;
    }

    private async Task PopulateReceiptConnectionMetadataAsync(PaycheckReceiptResponseDto receipt)
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
        var saleInvoiceTotals = new Dictionary<int, decimal>();
        if (saleInvoiceIds.Count > 0)
        {
            var saleInvoiceRepo = _repositoriesFactory.GetRepository<SaleInvoice>();
            var saleInvoiceQuery = await saleInvoiceRepo.Filter(si => saleInvoiceIds.Contains(si.Id));
            var saleInvoices = saleInvoiceQuery.Data?.ToList() ?? new List<SaleInvoice>();
            saleInvoicesDict = saleInvoices.ToDictionary(x => x.Id, x => x);
            foreach (var saleInvoice in saleInvoices)
            {
                saleInvoiceTotals[saleInvoice.Id] = CalculateSaleInvoiceTotal(saleInvoice);
            }
        }

        var purchaseInvoicesDict = new Dictionary<int, PurchaseInvoice>();
        var purchaseInvoiceTotals = new Dictionary<int, decimal>();
        if (purchaseInvoiceIds.Count > 0)
        {
            var purchaseInvoiceRepo = _repositoriesFactory.GetRepository<PurchaseInvoice>();
            var purchaseInvoiceQuery = await purchaseInvoiceRepo.Filter(pi => purchaseInvoiceIds.Contains(pi.Id));
            var purchaseInvoices = purchaseInvoiceQuery.Data?.ToList() ?? new List<PurchaseInvoice>();
            purchaseInvoicesDict = purchaseInvoices.ToDictionary(x => x.Id, x => x);
            foreach (var purchaseInvoice in purchaseInvoices)
            {
                purchaseInvoiceTotals[purchaseInvoice.Id] = CalculatePurchaseInvoiceTotal(purchaseInvoice);
            }
        }

        var reservationDict = new Dictionary<Guid, SmartAndPro.ERP.Domains.Entities.Reservation.Reservation>();
        var reservationTotals = new Dictionary<Guid, decimal>();
        if (reservationIds.Count > 0)
        {
            var reservationRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Reservation.Reservation>();
            var reservationQuery = await reservationRepo.Filter(r => reservationIds.Contains(r.Id));
            var reservations = reservationQuery.Data?.ToList() ?? new List<SmartAndPro.ERP.Domains.Entities.Reservation.Reservation>();
            reservationDict = reservations.ToDictionary(x => x.Id, x => x);
            foreach (var reservation in reservations)
            {
                var reservationDto = _mapper.Map<ReservationResponseDto>(reservation);
                reservationTotals[reservation.Id] = SmartAndPro.ERP.Application.Services.Reservation.ReservationService.GetTotalAmount(reservationDto);
            }
        }

        var receiptConnectionRepo = _repositoriesFactory.GetRepository<ReceiptConnection>();

        var salePayments = new Dictionary<int, decimal>();
        if (saleInvoiceIds.Count > 0)
        {
            var saleConnectionsQuery = await receiptConnectionRepo.Filter(rc => rc.SaleInvoiceId.HasValue && saleInvoiceIds.Contains(rc.SaleInvoiceId!.Value));
            var saleConnections = saleConnectionsQuery.Data?.ToList() ?? new List<ReceiptConnection>();
            salePayments = saleConnections
                .Where(x => x.SaleInvoiceId.HasValue)
                .GroupBy(x => x.SaleInvoiceId!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.LinkedAmount));
        }

        var purchasePayments = new Dictionary<int, decimal>();
        if (purchaseInvoiceIds.Count > 0)
        {
            var purchaseConnectionsQuery = await receiptConnectionRepo.Filter(rc => rc.PurchaseInvoiceId.HasValue && purchaseInvoiceIds.Contains(rc.PurchaseInvoiceId!.Value));
            var purchaseConnections = purchaseConnectionsQuery.Data?.ToList() ?? new List<ReceiptConnection>();
            purchasePayments = purchaseConnections
                .Where(x => x.PurchaseInvoiceId.HasValue)
                .GroupBy(x => x.PurchaseInvoiceId!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.LinkedAmount));
        }

        var reservationPayments = new Dictionary<Guid, decimal>();
        if (reservationIds.Count > 0)
        {
            var reservationConnectionsQuery = await receiptConnectionRepo.Filter(rc => rc.ReservationId.HasValue && reservationIds.Contains(rc.ReservationId!.Value));
            var reservationConnections = reservationConnectionsQuery.Data?.ToList() ?? new List<ReceiptConnection>();
            reservationPayments = reservationConnections
                .Where(x => x.ReservationId.HasValue)
                .GroupBy(x => x.ReservationId!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.LinkedAmount));
        }

        var saleAggregationIds = new HashSet<int>();
        var purchaseAggregationIds = new HashSet<int>();
        var reservationAggregationIds = new HashSet<Guid>();
        decimal receiptLinkedAmountTotal = 0;

        foreach (var connection in receipt.ReceiptConnections)
        {
            receiptLinkedAmountTotal += connection.LinkedAmount;

            if (connection.SaleInvoiceId.HasValue)
            {
                var saleInvoiceId = connection.SaleInvoiceId.Value;
                saleAggregationIds.Add(saleInvoiceId);

                var invoiceTotal = saleInvoiceTotals.TryGetValue(saleInvoiceId, out var total) ? total : 0;
                var paidAmount = salePayments.TryGetValue(saleInvoiceId, out var paid) ? paid : 0;
                var effectiveTotal = invoiceTotal > 0 ? invoiceTotal : connection.InvoiceTotalAmount;
                if (effectiveTotal <= 0)
                    effectiveTotal = connection.LinkedAmount;

                connection.InvoiceTotalAmount = Math.Round(effectiveTotal, 2);
                connection.InvoicePaidAmount = Math.Round(paidAmount, 2);
                var remaining = Math.Max(0, connection.InvoiceTotalAmount - connection.InvoicePaidAmount);
                connection.InvoiceRemainingAmount = Math.Round(remaining, 2);

                if (saleInvoicesDict.TryGetValue(saleInvoiceId, out var saleInvoice))
                {
                    connection.InvoiceReference = saleInvoice.InvoiceNo;
                    connection.OperationType = "فاتورة مبيعات";
                }
                else if (string.IsNullOrWhiteSpace(connection.OperationType))
                {
                    connection.OperationType = "فاتورة مبيعات";
                }
            }
            else if (connection.PurchaseInvoiceId.HasValue)
            {
                var purchaseInvoiceId = connection.PurchaseInvoiceId.Value;
                purchaseAggregationIds.Add(purchaseInvoiceId);

                var invoiceTotal = purchaseInvoiceTotals.TryGetValue(purchaseInvoiceId, out var total) ? total : 0;
                var paidAmount = purchasePayments.TryGetValue(purchaseInvoiceId, out var paid) ? paid : 0;
                var effectiveTotal = invoiceTotal > 0 ? invoiceTotal : connection.InvoiceTotalAmount;
                if (effectiveTotal <= 0)
                    effectiveTotal = connection.LinkedAmount;

                connection.InvoiceTotalAmount = Math.Round(effectiveTotal, 2);
                connection.InvoicePaidAmount = Math.Round(paidAmount, 2);
                var remaining = Math.Max(0, connection.InvoiceTotalAmount - connection.InvoicePaidAmount);
                connection.InvoiceRemainingAmount = Math.Round(remaining, 2);

                if (purchaseInvoicesDict.TryGetValue(purchaseInvoiceId, out var purchaseInvoice))
                {
                    connection.InvoiceReference = purchaseInvoice.InvoiceNo;
                    connection.OperationType = "فاتورة شراء";
                }
                else if (string.IsNullOrWhiteSpace(connection.OperationType))
                {
                    connection.OperationType = "فاتورة شراء";
                }
            }
            else if (connection.ReservationId.HasValue)
            {
                var reservationId = connection.ReservationId.Value;
                reservationAggregationIds.Add(reservationId);

                var invoiceTotal = reservationTotals.TryGetValue(reservationId, out var total) ? total : 0;
                var paidAmount = reservationPayments.TryGetValue(reservationId, out var paid) ? paid : 0;
                var effectiveTotal = invoiceTotal > 0 ? invoiceTotal : connection.InvoiceTotalAmount;
                if (effectiveTotal <= 0)
                    effectiveTotal = connection.LinkedAmount;

                connection.InvoiceTotalAmount = Math.Round(effectiveTotal, 2);
                connection.InvoicePaidAmount = Math.Round(paidAmount, 2);
                var remaining = Math.Max(0, connection.InvoiceTotalAmount - connection.InvoicePaidAmount);
                connection.InvoiceRemainingAmount = Math.Round(remaining, 2);

                if (reservationDict.TryGetValue(reservationId, out var reservation))
                {
                    connection.InvoiceReference = reservation.ReferenceNumber ?? $"R-{reservationId}";
                }
                connection.OperationType = "حجز";
            }
            else if (connection.ProjectId.HasValue)
            {
                var projectId = connection.ProjectId.Value;
                if (projectsDict.TryGetValue(projectId, out var project))
                {
                    connection.InvoiceReference = project.ProjectName;
                    connection.OperationType = "مشروع";
                }
                else
                {
                    connection.InvoiceReference = $"PRJ-{projectId}";
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

        var aggregatedSaleTotal = saleAggregationIds.Sum(id => saleInvoiceTotals.TryGetValue(id, out var total) ? total : 0);
        var aggregatedPurchaseTotal = purchaseAggregationIds.Sum(id => purchaseInvoiceTotals.TryGetValue(id, out var total) ? total : 0);
        var aggregatedReservationTotal = reservationAggregationIds.Sum(id => reservationTotals.TryGetValue(id, out var total) ? total : 0);

        var aggregatedSaleRemaining = saleAggregationIds.Sum(id =>
        {
            var total = saleInvoiceTotals.TryGetValue(id, out var value) ? value : 0;
            var paid = salePayments.TryGetValue(id, out var paidValue) ? paidValue : 0;
            return Math.Max(0, Math.Round(total - paid, 2));
        });

        var aggregatedPurchaseRemaining = purchaseAggregationIds.Sum(id =>
        {
            var total = purchaseInvoiceTotals.TryGetValue(id, out var value) ? value : 0;
            var paid = purchasePayments.TryGetValue(id, out var paidValue) ? paidValue : 0;
            return Math.Max(0, Math.Round(total - paid, 2));
        });

        var aggregatedReservationRemaining = reservationAggregationIds.Sum(id =>
        {
            var total = reservationTotals.TryGetValue(id, out var value) ? value : 0;
            var paid = reservationPayments.TryGetValue(id, out var paidValue) ? paidValue : 0;
            return Math.Max(0, Math.Round(total - paid, 2));
        });

        var totalInvoiceAmount = aggregatedSaleTotal + aggregatedPurchaseTotal + aggregatedReservationTotal;
        var totalRemainingAmount = aggregatedSaleRemaining + aggregatedPurchaseRemaining + aggregatedReservationRemaining;

        receipt.TotalInvoiceAmount = Math.Round(totalInvoiceAmount, 2);
        receipt.PaidAmount = Math.Round(receiptLinkedAmountTotal, 2);
        receipt.RemainingAmount = Math.Round(totalRemainingAmount, 2);
    }

    public override async Task<OperationResponse<PaycheckReceiptResponseDto>> GetAsync(Guid id)
    {
        try
        {
            var paycheckReceiptRepo = _repositoriesFactory.GetRepository<PaycheckReceipt>();
            var receiptQuery = await paycheckReceiptRepo.Filter(x => x.Id == id);
            
            var receipt = receiptQuery.Data?
                .Include(x => x.ReceiptConnections)
                .Include(x => x.PaycheckReceiptAccounts)
                    .ThenInclude(pra => pra.MainAccount)
                .Include(x => x.Treasury)
                    .ThenInclude(t => t.MainAccount)
                .Include(x => x.Attachments)
                .FirstOrDefault();
            
            if (receipt == null)
                return new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.NotFound, null);
            
            var mappedReceipt = _mapper.Map<PaycheckReceiptResponseDto>(receipt);
            await PopulateReceiptConnectionMetadataAsync(mappedReceipt);
            
            return new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.Success, mappedReceipt);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.GeneralFail, null);
        }
    }

    public override async Task<OperationResponse<PaycheckReceiptResponseDto>> DeleteAsync(Guid id)
    {
        var result = new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.GeneralFail, null);
        var paycheckReceiptRepo = _repositoriesFactory.GetRepository<PaycheckReceipt>(true);
        var dbContext = paycheckReceiptRepo.GetDbContext();
        
        try
        {
            await paycheckReceiptRepo.BeginTransactionAsync();

            var receipt = await dbContext.Set<PaycheckReceipt>()
                .Include(x => x.ReceiptConnections)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (receipt == null)
            {
                await paycheckReceiptRepo.RollbackTransactionAsync();
                return new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.NotFound, null);
            }

            if (!receipt.IsActive)
            {
                await paycheckReceiptRepo.CommitTransactionAsync();
                return new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.NoContent, null);
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
            await paycheckReceiptRepo.CommitTransactionAsync();

            var mappedReceipt = _mapper.Map<PaycheckReceiptResponseDto>(receipt);
            return new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.NoContent, mappedReceipt);
        }
        catch (Exception ex)
        {
            await paycheckReceiptRepo.RollbackTransactionAsync();
            LogService.LogError(ex);
            return result;
        }
    }

    public override async Task<OperationResponse<PaycheckReceiptResponseDto>> DeleteRangeAsync(List<Guid> ids)
    {
        var result = new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.GeneralFail, null);
        var paycheckReceiptRepo = _repositoriesFactory.GetRepository<PaycheckReceipt>(true);
        var dbContext = paycheckReceiptRepo.GetDbContext();
        
        try
        {
            await paycheckReceiptRepo.BeginTransactionAsync();

            var receipts = await dbContext.Set<PaycheckReceipt>()
                .Include(x => x.ReceiptConnections)
                .IgnoreQueryFilters()
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            if (receipts == null || !receipts.Any())
            {
                await paycheckReceiptRepo.RollbackTransactionAsync();
                return new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.NotFound, null);
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
            await paycheckReceiptRepo.CommitTransactionAsync();

            return new OperationResponse<PaycheckReceiptResponseDto>(ErrorCodes.NoContent, null);
        }
        catch (Exception ex)
        {
            await paycheckReceiptRepo.RollbackTransactionAsync();
            LogService.LogError(ex);
            return result;
        }
    }

    public async Task<OperationResponse<string>> DeleteWithReasonAsync(Guid id, string reason, Guid currentUserId)
    {
        var paycheckReceiptRepo = _repositoriesFactory.GetRepository<PaycheckReceipt>();
        var dbContext = paycheckReceiptRepo.GetDbContext();

        try
        {
            await paycheckReceiptRepo.BeginTransactionAsync();

            var paycheckReceipt = await dbContext.Set<PaycheckReceipt>()
                .Include(x => x.PaycheckReceiptAccounts)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive == true);

            if (paycheckReceipt == null)
            {
                await paycheckReceiptRepo.RollbackTransactionAsync();
                return new OperationResponse<string>(ErrorCodes.NotFound, null);
            }

            // Deactivate the paycheck receipt (Soft delete)
            paycheckReceipt.IsActive = false;
            dbContext.Entry(paycheckReceipt).Property(r => r.IsActive).IsModified = true;

            // Delete original transaction if exists
            if (paycheckReceipt.TransactionId.HasValue)
            {
                var origTransaction = await dbContext.Set<Transaction>()
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(t => t.Id == paycheckReceipt.TransactionId.Value);
                if (origTransaction != null)
                {
                    origTransaction.IsActive = false;
                    dbContext.Entry(origTransaction).Property(t => t.IsActive).IsModified = true;

                    var entries = await dbContext.Set<TransactionEntry>()
                        .IgnoreQueryFilters()
                        .Where(te => te.TransactionId == origTransaction.Id)
                        .ToListAsync();
                    foreach (var entry in entries)
                    {
                        entry.IsActive = false;
                        dbContext.Entry(entry).Property(e => e.IsActive).IsModified = true;
                    }
                }
            }

            // Create reverse PaymentReceipt (سند صرف)
            var reverseRef = "REV-" + paycheckReceipt.ReferenceNumber;
            var paymentReceipt = new PaymentReceipt
            {
                Id = Guid.NewGuid(),
                ReferenceNumber = reverseRef,
                Description = $"عملية عكسية بسبب إلغاء السند {paycheckReceipt.ReferenceNumber}: {reason}",
                Amount = paycheckReceipt.Amount,
                Tax = paycheckReceipt.Tax,
                TreasuryId = paycheckReceipt.TreasuryId,
                ReceiptDate = DateOnly.FromDateTime(DateTime.UtcNow),
                PaymentMethodType = paycheckReceipt.PaymentMethodType,
                CheckNumber = paycheckReceipt.CheckNumber,
                BankName = paycheckReceipt.BankName,
                BankReferenceNumber = paycheckReceipt.BankReferenceNumber,
                VoucherNumber = paycheckReceipt.VoucherNumber,
                TransactionNumber = paycheckReceipt.TransactionNumber,
                IsFrontal = paycheckReceipt.IsFrontal,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            };

            var paymentAccounts = new List<PaymentReceiptAccount>();
            foreach (var payAccount in paycheckReceipt.PaycheckReceiptAccounts)
            {
                paymentAccounts.Add(new PaymentReceiptAccount
                {
                    Id = Guid.NewGuid(),
                    PaymentReceiptId = paymentReceipt.Id,
                    MainAccountId = payAccount.MainAccountId,
                    Amount = payAccount.Amount,
                    Tax = payAccount.Tax,
                    Description = payAccount.Description ?? paymentReceipt.Description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            paymentReceipt.PaymentReceiptAccounts = paymentAccounts;

            dbContext.Set<PaymentReceipt>().Add(paymentReceipt);
            await dbContext.SaveChangesAsync();

            // Create journal transaction for reverse PaymentReceipt
            var transactionTypeId = await dbContext.Set<TransactionType>()
                .IgnoreQueryFilters()
                .Where(t => t.ArabicName == "سند صرف" || t.EnglishName == "PaymentReceipt")
                .Select(t => t.Id)
                .FirstOrDefaultAsync();

            if (transactionTypeId != Guid.Empty)
            {
                var treasury = await dbContext.Set<Treasury>().FindAsync(paymentReceipt.TreasuryId);
                if (treasury != null && treasury.MainAccountId != Guid.Empty)
                {
                    var transaction = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        TransactionTypeId = transactionTypeId,
                        TransactionDate = paymentReceipt.ReceiptDate,
                        Description = paymentReceipt.Description,
                        ReferenceNumber = paymentReceipt.ReferenceNumber,
                        IsOpeningTransaction = false,
                        IsFrontal = paymentReceipt.IsFrontal,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var entries = new List<TransactionEntry>();

                    // Debit: Client account (MainAccountId in accounts list)
                    foreach (var acc in paymentReceipt.PaymentReceiptAccounts)
                    {
                        entries.Add(new TransactionEntry
                        {
                            Id = Guid.NewGuid(),
                            TransactionId = transaction.Id,
                            MainAccountId = acc.MainAccountId,
                            DebitAmount = acc.Amount,
                            CreditAmount = 0,
                            Description = acc.Description,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    // Credit: Treasury's Account
                    entries.Add(new TransactionEntry
                    {
                        Id = Guid.NewGuid(),
                        TransactionId = transaction.Id,
                        MainAccountId = treasury.MainAccountId,
                        DebitAmount = 0,
                        CreditAmount = paymentReceipt.Amount,
                        Description = paymentReceipt.Description,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });

                    transaction.TransactionEntries = entries;
                    dbContext.Set<Transaction>().Add(transaction);

                    paymentReceipt.TransactionId = transaction.Id;
                    await dbContext.SaveChangesAsync();
                }
            }

            // Deactivate and handle linked Project connections
            var receiptConnection = await dbContext.Set<ReceiptConnection>()
                .Where(x => x.PaycheckReceiptId == paycheckReceipt.Id && x.IsActive == true)
                .FirstOrDefaultAsync();

            if (receiptConnection != null)
            {
                receiptConnection.IsActive = false;
                dbContext.Entry(receiptConnection).Property(c => c.IsActive).IsModified = true;

                var deleteHistory = new ProjectHistory
                {
                    Id = Guid.NewGuid(),
                    ProjectId = receiptConnection.ProjectId.Value,
                    ActionType = "إلغاء دفعة",
                    ActionBy = await GetUserNameLocalAsync(currentUserId, dbContext),
                    ActionDate = DateTime.UtcNow,
                    NewValue = $"تم إلغاء دفعة رقم: {paycheckReceipt.ReferenceNumber} بمبلغ: {paycheckReceipt.Amount:N2} ريال. السبب: {reason}. وتم إنشاء سند صرف عكسي رقم: {paymentReceipt.ReferenceNumber}",
                    Notes = "إلغاء دفعات المشروع"
                };
                dbContext.Set<ProjectHistory>().Add(deleteHistory);

                // Create reverse connection for project
                var reverseConnection = new ReceiptConnection
                {
                    Id = Guid.NewGuid(),
                    ProjectId = receiptConnection.ProjectId,
                    PaymentReceiptId = paymentReceipt.Id,
                    LinkedAmount = paymentReceipt.Amount,
                    ReceiptConnectionType = receiptConnection.ReceiptConnectionType,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                dbContext.Set<ReceiptConnection>().Add(reverseConnection);
                await dbContext.SaveChangesAsync();
            }

            await paycheckReceiptRepo.CommitTransactionAsync();
            return new OperationResponse<string>(ErrorCodes.Success, reverseRef);
        }
        catch (Exception ex)
        {
            await paycheckReceiptRepo.RollbackTransactionAsync();
            LogService.LogError(ex);
            return new OperationResponse<string>(ErrorCodes.GeneralFail, null);
        }
    }

    private async Task<string> GetUserNameLocalAsync(Guid userId, DbContext dbContext)
    {
        if (userId == Guid.Empty) return "مستخدم النظام";
        var user = await dbContext.Set<SmartAndPro.ERP.Domains.Entities.Auth.Security.User>().FindAsync(userId);
        if (user != null)
        {
            if (!string.IsNullOrEmpty(user.FirstName) || !string.IsNullOrEmpty(user.LastName))
                return $"{user.FirstName} {user.LastName}".Trim();
            return user.UserName ?? "مستخدم النظام";
        }
        return "مستخدم النظام";
    }
}
