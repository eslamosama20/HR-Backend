using AutoMapper;
using Bee.ZatcaHelper;
using Bee.ZatcaHelper.Contracts;
using Bee.ZatcaHelper.Model;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.CustomerProfile;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainSalesClass;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.MeasurementUnits;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale;
using SmartAndPro.ERP.Application.Common.Dtos.Notes.SalesCreditNote;
using SmartAndPro.ERP.Application.Common.Dtos.Notes.SalesCreditNoteItem;
using SmartAndPro.ERP.Application.Common.Utility;
using SmartAndPro.ERP.Application.Enforcers;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Domains.Entities.Notes;
using SmartAndPro.ERP.Domains.Entities.Notes.SalesCreditNote;
using SmartAndPro.ERP.Domains.Entities.Receipts;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.Application.Common.Extensions;

namespace SmartAndPro.ERP.Application.Services.Notes.CreditNotes;


public class SalesCreditNoteService
    : ServiceBase
    <
        SalesCreditNote,
        SalesCreditNotePostDto,
        SalesCreditNotePutDto,
        SalesCreditNoteResponseDto,
        SalesCreditNoteFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public SalesCreditNoteService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper)
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }

    public async Task ZatcaClearSalesCreditNote(Guid id)
    {
        try
        {
            var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
            var companyDetails = companyDetailsQuery.Data.FirstOrDefault();
            if (companyDetails == null) throw new Exception("Company Details not found");

            if (companyDetails.IsZatcaIntegrated == false) throw new Exception("Company Details ZatcaIntegrated is disabled");
            if (companyDetails.IsOnBoarded == false) throw new Exception("Company Details not onboarded");
            if (companyDetails.ProdBinarySecurityToken == null || companyDetails.ProdSecret == null) throw new Exception("ProdBinarySecurityToken or Secret not found");

            var salesCreditNoteRepository = _repositoriesFactory.GetRepository<SalesCreditNote>();
            var salesCreditNoteQuery = await salesCreditNoteRepository.Filter(i => i.Id == id);
            var salesCreditNote = salesCreditNoteQuery.Data
                .Include(sc => sc.SaleInvoice)
                .FirstOrDefault();
            if (salesCreditNote == null) throw new Exception("salesCreditNote not found");

            if (salesCreditNote.TransactionId == null)
            {
                try
                {
                    await CreateTransaction(id);
                }
                catch (Exception e)
                {
                    LogService.LogError(e);
                    throw;
                }
            }
            
            var roundedLineAmounts = salesCreditNote.SalesCreditNoteItems
                .Select(item => Math.Round(item.Price * item.Quantity * item.NoOfNights, 2))
                .ToList();

            decimal sumOfLines       = roundedLineAmounts.Sum();
            decimal roundedDiscount  = Math.Round(salesCreditNote.DiscountAmount, 2);
            var roundedLineDiscounts = roundedLineAmounts
                .Select(lineNet =>
                    sumOfLines > 0
                        ? Math.Round(lineNet / sumOfLines * roundedDiscount, 2)
                        : 0m
                )
                .ToList();
            var roundedNetLineAmounts = roundedLineAmounts
                .Select((lineNet, idx) => Math.Round(lineNet - roundedLineDiscounts[idx], 2))
                .ToList();
            var roundedLineTaxes = salesCreditNote.SalesCreditNoteItems
                .Select((item, idx) =>
                {
                    var isExempt = item.MainSalesClass.IsTaxExempted;
                    var netLine = roundedNetLineAmounts[idx];
                    // line‐level tax = netLine * rate, then round
                    return Math.Round(netLine * (isExempt ? 0m : 0.15m), 2);
                })
                .ToList();
            decimal taxableBase     = Math.Round(
                salesCreditNote.SalesCreditNoteItems
                    .Where((item, idx) => !item.MainSalesClass.IsTaxExempted)
                    .Select((_, idx) => roundedNetLineAmounts[idx])
                    .Sum()
                , 2);
            decimal taxAmountValue  = roundedLineTaxes.Sum();       
            decimal netLinesTotal   = roundedNetLineAmounts.Sum();
            decimal inclusiveAmount = Math.Round(netLinesTotal + taxAmountValue, 2);
            var standardSalesCreditNote = new StandardInvoiceAdjustment
            {
                AdjustmentReason = salesCreditNote.AdjustmentReason,
                AdjsutmentInvoiceNumber = salesCreditNote.SaleInvoice.InvoiceNo,
                
                Id                   = salesCreditNote.SalesCreditNoteNo,
                UUID                 = Guid.NewGuid().ToString(),
                IssuedDate           = salesCreditNote.IssueDate.ToString("yyyy-MM-dd"),
                IssuedTime           = salesCreditNote.IssueDate.ToString("HH:mm:ssZ"),
                DocumentCurrencyCode = "SAR",
                TaxCurrencyCode      = "SAR",
                ActualDeliveryDate = salesCreditNote.IssueDate.ToString("yyyy-MM-dd"),
                LatestDeliveryDate = salesCreditNote.IssueDate.ToString("yyyy-MM-dd"),
                SupplierInfo = new SupplierInfo
                {
                    RegistrationName   = companyDetails.ArabicName,
                    PartyPostalAddress = new PartyPostalAddress
                    {
                        StreetName = companyDetails.ArabicAddress?.Street,
                        CityName   = companyDetails.ArabicAddress?.City,
                        Country    = "SA",
                        PostalZone = companyDetails.ArabicAddress?.ZipCode
                    },
                    CRN = companyDetails.TaxNumber,
                    VAT = _appSettings.ZatcaParams.IsSandbox
                          ? "399999999900003"
                          : companyDetails.TaxNumber
                },

                CustomerInfo = new CustomerInfo
                {
                    RegistrationName = salesCreditNote.CustomerProfile.ArabicName,
                    VAT              = salesCreditNote.CustomerProfile.TaxNumber,
                },

                TaxTotal = new TaxTotal
                {
                    TaxableAmount = new Money("SAR", (double)taxableBase),
                    TaxAmount     = new Money("SAR", (double)taxAmountValue),
                    Percent       = "15.00",
                    TaxCategory   = "S",
                },

                LegalMonetaryTotal = new LegalMonetaryTotal
                {
                    LineExtensionAmount  = new Money("SAR", (double)sumOfLines),
                    TaxExclusiveAmount   = new Money("SAR", (double)netLinesTotal),
                    TaxInclusiveAmount   = new Money("SAR", (double)inclusiveAmount),
                    PayableAmount        = new Money("SAR", (double)inclusiveAmount),
                    AllowanceTotalAmount = new Money("SAR", (double)roundedDiscount),
                    PrepaidAmount        = new Money("SAR", 0),
                },

                InvoiceLineItems = salesCreditNote.SalesCreditNoteItems.Select((item, idx) =>
                {
                    decimal lineNet  = roundedLineAmounts[idx];
                    decimal lineTax  = roundedLineTaxes[idx];
                    decimal lineDisc = roundedLineDiscounts[idx];
                    return new InvoiceLineItem
                    {
                        ID                  = item.ItemNo ?? idx.ToString(),
                        InvoicedQuantity    = Math.Round(item.Quantity * item.NoOfNights, 2).ToString(),
                        LineExtensionAmount = new Money("SAR", (double)lineNet),
                        TaxAmount           = new Money("SAR", (double)lineTax),
                        RoundingAmount      = new Money("SAR", 0),
                        TaxSchemeId         = item.MainSalesClass.IsTaxExempted ? "E" : "S",
                        TaxScheme           = "VAT",
                        TaxPercent          = item.MainSalesClass.IsTaxExempted ? "0.00" : "15.00",
                        ItemName            = item.MainSalesClass.EnglishName,
                        PriceAmount         = new Money("SAR", (double)Math.Round(item.Price, 2)),
                    };
                }).ToList(),

                // 3. Document‐level discount (must match AllowanceTotalAmount exactly)
                DocumentLevelDiscountAmount = roundedDiscount,
                DocumentLevelDiscountReason  = "salesCreditNote-level discount",
            };
            if (standardSalesCreditNote.CustomerInfo.VAT != null)
            {
                var standardSalesCreditNoteXml = StandardInvoiceXmlGenerator.GenerateForCreditNote(standardSalesCreditNote, companyDetails.PrivateKey, companyDetails.ProdBinarySecurityToken);
                var clearanceResponse = new StandardCreditNoteClearanceApiClient(
                    new GlobalVariables
                    {
                        BaseUrl = _appSettings.ZatcaParams.IsSandbox ?  _appSettings.ZatcaParams.SandboxUrls.BaseUrl : _appSettings.ZatcaParams.ProdUrls.BaseUrl,
                        InvoiceClearanceEndPoint = _appSettings.ZatcaParams.IsSandbox ?  _appSettings.ZatcaParams.SandboxUrls.InvoiceClearanceEndPoint : _appSettings.ZatcaParams.ProdUrls.InvoiceClearanceEndPoint
                    }).ClearCreditNote(new InvoiceClearanceRequest(standardSalesCreditNoteXml, companyDetails.ProdBinarySecurityToken, companyDetails.ProdSecret));
                if (clearanceResponse.ClearanceStatus != "CLEARED") throw new Exception("Clearance failed");
                salesCreditNote.IsCleared = true;
                salesCreditNote.InvoiceHash = clearanceResponse.Hash;
                salesCreditNote.GeneratedQR = clearanceResponse.GeneratedQR;
                salesCreditNote.Attachments = null;
                salesCreditNote.SalesCreditNoteItems = null;
                salesCreditNote.CustomerProfile = null;
                salesCreditNote.SaleInvoice = null;
                await salesCreditNoteRepository.Update(salesCreditNote);
            }
            else
            {
                var simplifiedSalesCreditNoteXml = StandardInvoiceXmlGenerator.GenerateSimplifiedCreditNote(standardSalesCreditNote, companyDetails.PrivateKey, companyDetails.ProdBinarySecurityToken);
                var reportingClient = new SimplifiedCreditNoteReportingApiClient(new GlobalVariables
                {
                    BaseUrl = _appSettings.ZatcaParams.IsSandbox ?  _appSettings.ZatcaParams.SandboxUrls.BaseUrl : _appSettings.ZatcaParams.ProdUrls.BaseUrl,
                    InvoiceReportingEndPoint = _appSettings.ZatcaParams.IsSandbox ?  _appSettings.ZatcaParams.SandboxUrls.InvoiceReportingEndPoint : _appSettings.ZatcaParams.ProdUrls.InvoiceReportingEndPoint
                });
                /*var salesCreditNoteValidator = new Zatca.EsalesCreditNote.SDK.EsalesCreditNoteValidator();
                var pemCertificateBase64 = Convert.FromBase64String(companyDetails.ProdBinarySecurityToken);
                var cert = Encoding.UTF8.GetString(pemCertificateBase64);
                var result = salesCreditNoteValidator
                    .ValidateEsalesCreditNote(simplifiedSalesCreditNoteXml.salesCreditNoteXml, cert, companyDetails.Secret);*/
                var reportingResponse = reportingClient.ReportSimplifiedCreditNote(new InvoiceClearanceRequest(simplifiedSalesCreditNoteXml, companyDetails.ProdBinarySecurityToken, companyDetails.ProdSecret));
                if (reportingResponse.ReportingStatus != "REPORTED") throw new Exception("Reporting failed");
                salesCreditNote.IsCleared = true;
                salesCreditNote.InvoiceHash = simplifiedSalesCreditNoteXml.invoiceHash;
                salesCreditNote.GeneratedQR = simplifiedSalesCreditNoteXml.GeneratedQR;
                salesCreditNote.Attachments = null;
                salesCreditNote.SalesCreditNoteItems = null;
                salesCreditNote.CustomerProfile = null;
                salesCreditNote.SaleInvoice = null;

                await salesCreditNoteRepository.Update(salesCreditNote);
            }
            
        }
        catch (Exception e)
        {
            LogService.LogError(e);
            throw;
        }
    }
    public static decimal GetNetAmount(SalesCreditNote salesCreditNote)
    {
        if (salesCreditNote?.SalesCreditNoteItems == null || !salesCreditNote.SalesCreditNoteItems.Any())
            return 0 - (salesCreditNote?.DiscountAmount ?? 0);
        
        var netTotal = salesCreditNote.SalesCreditNoteItems.Sum(item => item.Price * item.Quantity * item.NoOfNights);
        var discount = salesCreditNote.DiscountAmount;
        return netTotal - discount;
    }
        
    public static decimal GetTaxAmount(SalesCreditNote salesCreditNote)
    {
        if (salesCreditNote?.SalesCreditNoteItems == null || !salesCreditNote.SalesCreditNoteItems.Any())
            return 0;
        
        var subtotals = salesCreditNote.SalesCreditNoteItems
            .Select(item => item.Price * item.Quantity * item.NoOfNights)
            .ToList();
        var grossTotal = subtotals.Sum();
        var discount = salesCreditNote.DiscountAmount;
        decimal totalTax = 0;
        var saleItems = salesCreditNote.SalesCreditNoteItems.ToList();
        for (int i = 0; i < saleItems.Count; i++)
        {
            var item = saleItems[i];
            var itemSubtotal = subtotals[i];

            // Determine tax rate based on mainSalesClass
            bool isExempt = item.MainSalesClass?.IsTaxExempted ?? false;
            decimal taxRate = isExempt ? 0 : 0.15m;

            // Allocate discount proportionally
            decimal itemDiscount = grossTotal > 0
                ? (itemSubtotal / grossTotal) * discount
                : 0;

            // Taxable base = remaining after discount
            decimal taxableBase = Math.Max(itemSubtotal - itemDiscount, 0);

            totalTax += taxableBase * taxRate;
        }

        // 4) Round and return to 4 decimal places
        return Math.Round(totalTax, 2);
    }
        
    public static decimal GetTotalAmount(SalesCreditNote salesCreditNote)
    {
        var netAmount = GetNetAmount(salesCreditNote);
        var taxAmount = GetTaxAmount(salesCreditNote);
        return netAmount + taxAmount;
    }
    
    public override async Task<OperationResponse<List<SalesCreditNoteResponseDto>>> GetAllAsync(SalesCreditNoteFiltersDto filters, int? pageNumber = null, int? pageSize = null, string? sortField = null, int? sortOrder = null)
    {
        var result = new OperationResponse<List<SalesCreditNoteResponseDto>>(ErrorCodes.GeneralFail, null);
        try
        {
            var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>()
                .GetAll();
            var companyDetails = await companyDetailsQuery.Data.FirstOrDefaultAsync();
            if (companyDetails == null)
            {
                return new OperationResponse<List<SalesCreditNoteResponseDto>>(ErrorCodes.Success, new List<SalesCreditNoteResponseDto>());
            }

            var salesCreditNotesQuery = await _repositoriesFactory.GetRepository<SalesCreditNote>().GetAll();
            IQueryable<SalesCreditNote> query = salesCreditNotesQuery.Data
                .Where(i => i.SaleInvoice != null && i.CustomerProfile != null);

            // Apply SearchKeyword
            query = query.ApplySearchKeyword(filters.SearchKeyword);

            var totalRecords = await query.CountAsync();

            pageNumber ??= 1;
            pageSize ??= 50;
            if (pageSize > 100) pageSize = 100;

            if (pageNumber > 0 && pageSize > 0)
            {
                var skip = (pageNumber.Value - 1) * pageSize.Value;
                query = query.ApplySort(sortField, sortOrder).Skip(skip).Take(pageSize.Value);
            }

            var salesCreditNotes = await query
                .Select(i => new SalesCreditNoteResponseDto
                {
                    Id = i.Id,
                    SalesCreditNoteNo = i.SalesCreditNoteNo,
                    AdjustmentReason = i.AdjustmentReason,
                    SaleInvoiceId = i.SaleInvoiceId,
                    SaleInvoice = new SaleInvoiceDto
                    {
                        Id = i.SaleInvoice.Id,
                        InvoiceNo = i.SaleInvoice.InvoiceNo,
                        Description = i.SaleInvoice.Description,
                        IssueDate = i.SaleInvoice.IssueDate,
                        DueDate = i.SaleInvoice.DueDate,
                    },
                    Description = i.Description,
                    IssueDate = i.IssueDate,
                    DueDate = i.DueDate,
                    DiscountAmount = i.DiscountAmount,
                    SalesCreditNoteItems = i.SalesCreditNoteItems != null ? i.SalesCreditNoteItems
                        .Where(s => s.MainSalesClass != null && s.MainSalesClass.MeasurementUnit != null)
                        .Select(s => new SalesCreditNoteItemResponseDto
                        {
                            Id = s.Id,
                            MainSalesClassId = s.MainSalesClassId,
                            MainSalesClass = new MainSalesClassResponseDto
                            {
                                Id = s.MainSalesClass.Id,
                                IsTaxExempted = s.MainSalesClass.IsTaxExempted,
                                EnglishName = s.MainSalesClass.EnglishName,
                                IsActive = s.MainSalesClass.IsActive,
                                CreatedAt = s.MainSalesClass.CreatedAt,
                                MeasurementUnit = new MeasurementUnitResponseDto
                                {
                                    Id = s.MainSalesClass.MeasurementUnit.Id,
                                    EnglishName = s.MainSalesClass.MeasurementUnit.EnglishName,
                                    ArabicName = s.MainSalesClass.MeasurementUnit.ArabicName,
                                    ArabicSymbol = s.MainSalesClass.MeasurementUnit.ArabicSymbol,
                                    EnglishSymbol = s.MainSalesClass.MeasurementUnit.EnglishSymbol,
                                },
                                MeasurementUnitId = s.MainSalesClass.MeasurementUnitId,
                                ReferenceCode = s.MainSalesClass.ReferenceCode,
                            },
                            Price = s.Price,
                            Quantity = s.Quantity,
                            NoOfNights = s.NoOfNights,
                            IsActive = s.IsActive,
                            CreatedAt = s.CreatedAt,
                            SalesCreditNoteId = s.SalesCreditNoteId,
                        }).ToList() : new List<SalesCreditNoteItemResponseDto>(),
                    CustomerProfileId = i.CustomerProfileId,
                    CustomerProfile = new CustomerProfileResponseDto
                    {
                        Id = i.CustomerProfile.Id,
                        EnglishName = i.CustomerProfile.EnglishName,
                        ArabicName = i.CustomerProfile.ArabicName,
                        MainAccountId = i.CustomerProfile.MainAccountId,
                    },
                    TransactionId = i.TransactionId,
                    IsCleared = i.IsCleared,
                    CreatedAt = i.CreatedAt,
                    GeneratedQR = i.GeneratedQR,
                    InvoiceHash = i.InvoiceHash,
                })
                .ToListAsync();
            
            // Calculate GeneratedQR for items that don't have it, after materialization
            var creditNoteIds = salesCreditNotes.Where(dto => string.IsNullOrEmpty(dto.GeneratedQR)).Select(dto => dto.Id).ToList();
            if (creditNoteIds.Any())
            {
                var entitiesQuery = await _repositoriesFactory.GetRepository<SalesCreditNote>().GetAll();
                var entitiesToUpdate = await entitiesQuery.Data
                    .Where(i => creditNoteIds.Contains(i.Id))
                    .IgnoreAutoIncludes() // Ignore AutoInclude to avoid HotelAllotmentInvoiceId column error
                    .Include(i => i.SalesCreditNoteItems)
                        .ThenInclude(s => s.MainSalesClass)
                            .ThenInclude(m => m.MeasurementUnit)
                    .ToListAsync();
                
                foreach (var entity in entitiesToUpdate)
                {
                    var dto = salesCreditNotes.FirstOrDefault(d => d.Id == entity.Id);
                    if (dto != null && string.IsNullOrEmpty(dto.GeneratedQR))
                    {
                        dto.GeneratedQR = StandardInvoiceXmlGenerator.GenerateZatcaQrCode(
                            companyDetails.ArabicName,
                            companyDetails.TaxNumber,
                            GetTotalAmount(entity),
                            GetTaxAmount(entity),
                            entity.IssueDate.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")
                        );
                    }
                }
            }
            
            result.Data = salesCreditNotes;
            result.ErrorCode = ErrorCodes.Success;

            if (pageNumber.HasValue && pageSize.HasValue)
            {
                return new OperationResponse<List<SalesCreditNoteResponseDto>>(
                    ErrorCodes.Success,
                    salesCreditNotes,
                    totalCount: totalRecords,
                    isPaginated: true
                );
            }
            return new OperationResponse<List<SalesCreditNoteResponseDto>>(ErrorCodes.Success, salesCreditNotes);
        }
        catch (Exception e)
        {
            LogService.LogError(e);
        }
        return result;
    }

    public override async Task<OperationResponse<SalesCreditNoteResponseDto>> GetAsync(Guid id)
    {
        var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
        var companyDetails = await companyDetailsQuery.Data.FirstOrDefaultAsync();
        if (companyDetails == null) throw new Exception("Company Details not found");
        var result = new OperationResponse<SalesCreditNoteResponseDto>(ErrorCodes.GeneralFail,
            null);
        try
        {
            var operation = await _repositoriesFactory.GetRepository<SalesCreditNote>().Filter(x => x.Id == id);
            
            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<SalesCreditNoteResponseDto>(ErrorCodes.GeneralFail, null);
            
            var entity = operation.Data!.FirstOrDefault();
            
            if (entity is null)
                return new OperationResponse<SalesCreditNoteResponseDto>(ErrorCodes.NotFound, null);
            entity.GeneratedQR = entity.GeneratedQR ?? StandardInvoiceXmlGenerator
                .GenerateZatcaQrCode(
                    companyDetails.ArabicName,
                    companyDetails.TaxNumber,
                    GetTotalAmount(entity),
                    GetTaxAmount(entity),
                    entity.IssueDate.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")
                );
            var responseData = _mapper.Map<SalesCreditNoteResponseDto>(entity);
            return new OperationResponse<SalesCreditNoteResponseDto>(ErrorCodes.Success, responseData);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    public override async Task<OperationResponse<SalesCreditNoteResponseDto>> UpdateAsync(SalesCreditNotePutDto salesCreditNotePut)
    {
    var salesCreditNote = _mapper.Map<SalesCreditNote>(salesCreditNotePut);
    GuidHelper.NormalizeGuids<SalesCreditNote>(salesCreditNote);

    var result = new OperationResponse<SalesCreditNoteResponseDto>(ErrorCodes.GeneralFail, null);

        try
        {
            var salesCreditNoteItems = salesCreditNotePut.SalesCreditNoteItems;
            var existingSalesCreditNoteItemsQuery = await _repositoriesFactory.GetRepository<SalesCreditNoteItem>()
                .Filter(x => x.SalesCreditNoteId == salesCreditNote.Id);
            var existingSalesCreditNoteItems = existingSalesCreditNoteItemsQuery.Data!.ToList();
            
            foreach (var salesCreditNoteItem in salesCreditNoteItems)
            {
                var itemEntity = _mapper.Map<SalesCreditNoteItem>(salesCreditNoteItem);
                GuidHelper.NormalizeGuids<SalesCreditNoteItem>(itemEntity);
                itemEntity.MainSalesClass = null;
                itemEntity.SalesCreditNote = null; 
                
                var existingItem = existingSalesCreditNoteItems.FirstOrDefault(x => x.Id == itemEntity.Id);
                
                if (existingItem != null)
                {
                    existingItem.MainSalesClass = null;
                    existingItem.SalesCreditNote = null;
                    
                    if (
                        existingItem.Price == itemEntity.Price &&
                        existingItem.Quantity == itemEntity.Quantity &&
                        existingItem.MainSalesClassId == itemEntity.MainSalesClassId 
                    )
                        continue;
                    
                    await _repositoriesFactory.GetRepository<SalesCreditNoteItem>().Update(itemEntity);
                }
                else
                {
                    await _repositoriesFactory.GetRepository<SalesCreditNoteItem>().Add(itemEntity);
                }
            }
            
            var itemsToDelete = existingSalesCreditNoteItems
                .Where(x => !salesCreditNoteItems.Any(pra => pra.Id == x.Id)).ToList();
            
            foreach (var itemToDelete in itemsToDelete)
            {
                itemToDelete.MainSalesClass = null;
                itemToDelete.SalesCreditNote = null;
                await _repositoriesFactory.GetRepository<SalesCreditNoteItem>().Delete(itemToDelete);
            }

            salesCreditNote.SalesCreditNoteItems = null;
            var operation = await _repositoriesFactory.GetRepository<SalesCreditNote>().Update(salesCreditNote);
            
            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<SalesCreditNoteResponseDto>(operation.ErrorCode, null);
            
            var responseData = _mapper.Map<SalesCreditNoteResponseDto>(operation.Data);

            if (salesCreditNote.TransactionId != null)
            {
                await UpdateTransaction(salesCreditNote.Id);
            }

            return new OperationResponse<SalesCreditNoteResponseDto>(ErrorCodes.Success, responseData);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
     public async Task CreateTransaction(Guid id)
    {
        try
        {
            var salesCreditNoteRepository = _repositoriesFactory.GetRepository<SalesCreditNote>();
            var customerRepository = _repositoriesFactory.GetRepository<CustomerProfile>();
            var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();
            var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();

            var salesCreditNoteQuery = await salesCreditNoteRepository.Filter(i => i.Id == id);
            var salesCreditNote = salesCreditNoteQuery.Data.FirstOrDefault();
            if (salesCreditNote == null)
            {
                throw new Exception("Sales Credit Note not found");
            }

            var customerQuery = await customerRepository.Filter(s => s.Id == salesCreditNote.CustomerProfileId);
            var customer = customerQuery.Data.FirstOrDefault();
            if (customer == null || customer.MainAccountId == null)
            {
                throw new Exception("Customer Profile Main Account not found");
            }

            // Retrieve the "Main Vault" MainAccount
            var mainVaultAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "الخزينة الرئيسية");
            var mainVaultAccount = mainVaultAccountQuery.Data.FirstOrDefault();
            if (mainVaultAccount == null)
            {
                throw new Exception("Main Vault account not found");
            }

            // Retrieve the "Sale Tax" MainAccount
            var saleTaxAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "ضريبة القيمة المضافة  المبيعات");
            var saleTaxAccount = saleTaxAccountQuery.Data.FirstOrDefault();
            if (saleTaxAccount == null)
            {
                throw new Exception("Sale Tax account not found");
            }

            // Retrieve Transaction Type for salesCreditNote transaction
            var transactionTypeQuery = await _repositoriesFactory.GetRepository<TransactionType>()
                .Filter(tt => tt.ArabicName.Contains("قيد مرتجع"));
            var transactionTypeId = transactionTypeQuery.Data.FirstOrDefault()?.Id;
            if (transactionTypeId == null)
            {
                throw new Exception("Transaction Type not found");
            }

            // Create transaction entries based on salesCreditNote items
            var transactionEntries = new List<TransactionEntry>();

            var itemsTotal = salesCreditNote.SalesCreditNoteItems
                .Sum(item => item.Price * item.Quantity * item.NoOfNights);
            var taxableTotal = salesCreditNote.SalesCreditNoteItems
                .Where(item => !item.MainSalesClass.IsTaxExempted)
                .Sum(item => item.Price * item.Quantity * item.NoOfNights);
            var discount = salesCreditNote.DiscountAmount;
            var netSaleTotal = itemsTotal - discount;
            decimal taxableDiscount = discount * (taxableTotal / itemsTotal);;
            var netTaxableAmount = Math.Max(taxableTotal - taxableDiscount, 0m);
            var saleTaxAmount = netTaxableAmount * 0.15m;
            var saleTotalAmount = netSaleTotal + saleTaxAmount;
            var taxEntry = new TransactionEntry
            {
                MainAccountId = saleTaxAccount.Id,
                CreditAmount = 0,
                DebitAmount = saleTaxAmount,
                Description = $"Credit Note #{salesCreditNote.SalesCreditNoteNo} - Sale Tax",
                CreatedAt = DateTime.UtcNow
            };
            transactionEntries.Add(taxEntry);

            // 2. Entry for supplier MainAccount (Debit for supplier)
            var supplierEntry = new TransactionEntry
            {
                MainAccountId = customer.MainAccountId.Value,
                DebitAmount = 0,
                CreditAmount = saleTotalAmount,
                Description = $"Credit Note #{salesCreditNote.SalesCreditNoteNo} (Customer)",
                CreatedAt = DateTime.UtcNow
            };
            transactionEntries.Add(supplierEntry);
            var itemsEntry = new TransactionEntry
            {
                MainAccountId = mainVaultAccount.Id,
                DebitAmount = netSaleTotal,
                CreditAmount = 0,
                Description = $"Credit Note #{salesCreditNote.SalesCreditNoteNo}",
                CreatedAt = DateTime.UtcNow
            };
            transactionEntries.Add(itemsEntry);

            // Create the transaction
            var transaction = new Transaction
            {
                Description = $"قيد إشعار دائن رقم {salesCreditNote.SalesCreditNoteNo}",
                TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                TransactionTypeId = transactionTypeId.Value,
                TransactionEntries = transactionEntries,
                SalesCreditNoteId = salesCreditNote.Id
            };

            // Enforce rules and save transaction
            var transactionEnforcer = new TransactionEnforcer();
            var enforcerResult = transactionEnforcer.EnforceRules(transaction);
            if (enforcerResult.IsSuccess)
            {
                var result = await transactionRepository.Add(transaction);
                if (result.Data != null)
                {
                    salesCreditNote.SalesCreditNoteItems = null;
                    salesCreditNote.Attachments = null;
                    salesCreditNote.CustomerProfile = null;
                    salesCreditNote.TransactionId = result.Data.Id;
                    var r = await salesCreditNoteRepository.Update(salesCreditNote);
                    if (r.ErrorCode != ErrorCodes.Success)
                        throw new Exception("Failed to update the sales credit note");
                }
            }
        }
        catch (Exception e)
        {
            LogService.LogError(e);
            throw;
        }
    }
    
    public async Task UpdateTransaction(Guid id)
    {
        try
        {
            var salesCreditNoteRepository = _repositoriesFactory.GetRepository<SalesCreditNote>();
            var customerRepository = _repositoriesFactory.GetRepository<CustomerProfile>();
            var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();
            var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();

            var salesCreditNoteQuery = await salesCreditNoteRepository.Filter(i => i.Id == id);
            var salesCreditNote = salesCreditNoteQuery.Data.FirstOrDefault();
            if (salesCreditNote == null)
            {
                throw new Exception("Sales Credit Note not found");
            }
            
            var transactionId = salesCreditNote.TransactionId;
            var transactionQuery = await transactionRepository.Filter(x=>x.Id == transactionId);
            var transaction = transactionQuery.Data.FirstOrDefault();
            if (transaction == null) throw new Exception("Transaction not found");

            var customerQuery = await customerRepository.Filter(s => s.Id == salesCreditNote.CustomerProfileId);
            var customer = customerQuery.Data.FirstOrDefault();
            if (customer == null || customer.MainAccountId == null)
            {
                throw new Exception("Customer Profile Main Account not found");
            }

            // Retrieve the "Main Vault" MainAccount
            var mainVaultAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "الخزينة الرئيسية");
            var mainVaultAccount = mainVaultAccountQuery.Data.FirstOrDefault();
            if (mainVaultAccount == null)
            {
                throw new Exception("Main Vault account not found");
            }

            // Retrieve the "Sale Tax" MainAccount
            var saleTaxAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "ضريبة القيمة المضافة  المبيعات");
            var saleTaxAccount = saleTaxAccountQuery.Data.FirstOrDefault();
            if (saleTaxAccount == null)
            {
                throw new Exception("Sale Tax account not found");
            }

            // Retrieve Transaction Type for salesCreditNote transaction
            var transactionTypeQuery = await _repositoriesFactory.GetRepository<TransactionType>()
                .Filter(tt => tt.ArabicName.Contains("قيد مرتجع"));
            var transactionTypeId = transactionTypeQuery.Data.FirstOrDefault()?.Id;
            if (transactionTypeId == null)
            {
                throw new Exception("Transaction Type not found");
            }

            // Create transaction entries based on salesCreditNote items
            var transactionEntries = new List<TransactionEntry>();

            var itemsTotal = salesCreditNote.SalesCreditNoteItems
                .Sum(item => item.Price * item.Quantity * item.NoOfNights);
            var taxableTotal = salesCreditNote.SalesCreditNoteItems
                .Where(item => !item.MainSalesClass.IsTaxExempted)
                .Sum(item => item.Price * item.Quantity * item.NoOfNights);
            var discount = salesCreditNote.DiscountAmount;
            var netSaleTotal = itemsTotal - discount;
            decimal taxableDiscount = discount * (taxableTotal / itemsTotal);;
            var netTaxableAmount = Math.Max(taxableTotal - taxableDiscount, 0m);
            var saleTaxAmount = netTaxableAmount * 0.15m;
            var saleTotalAmount = netSaleTotal + saleTaxAmount;
            var taxEntry = new TransactionEntry
            {
                TransactionId = transaction.Id,
                MainAccountId = saleTaxAccount.Id,
                CreditAmount = 0,
                DebitAmount = saleTaxAmount,
                Description = $"Credit Note #{salesCreditNote.SalesCreditNoteNo} - Sale Tax",
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
                Description = $"Credit Note #{salesCreditNote.SalesCreditNoteNo} (Customer)",
                CreatedAt = DateTime.UtcNow
            };
            transactionEntries.Add(supplierEntry);
            var itemsEntry = new TransactionEntry
            {
                TransactionId = transaction.Id,
                MainAccountId = mainVaultAccount.Id,
                DebitAmount = netSaleTotal,
                CreditAmount = 0,
                Description = $"Credit Note #{salesCreditNote.SalesCreditNoteNo}",
                CreatedAt = DateTime.UtcNow
            };
            transactionEntries.Add(itemsEntry);

            var additionResult = await _repositoriesFactory.GetRepository<TransactionEntry>().AddRange(transactionEntries);
            if (additionResult.Data.Count()!= transactionEntries.Count)
                throw new Exception("Failed to add transaction entries");
        }
        catch (Exception e)
        {
            LogService.LogError(e);
            throw;
        }
    }

    public async Task<IEnumerable<object>> GetSalesCreditNoteVATReport(DateOnly startDate, DateOnly endDate)
    {
        try
        {
            var creditNotesQueryResponse = await _repositoriesFactory.GetRepository<SalesCreditNote>().GetAll();
            
            if (creditNotesQueryResponse.ErrorCode != ErrorCodes.Success || creditNotesQueryResponse.Data == null)
                return new List<object>();

            var creditNotes = await creditNotesQueryResponse.Data
                .Where(cn => cn.IssueDate >= startDate.ToDateTime(TimeOnly.MinValue) 
                          && cn.IssueDate <= endDate.ToDateTime(TimeOnly.MaxValue)
                          && cn.IsActive)
                .Select(cn => new
                {
                    cn.Id,
                    cn.SalesCreditNoteNo,
                    cn.IssueDate,
                    Items = cn.SalesCreditNoteItems
                        .Where(item => item.IsActive)
                        .Select(item => new
                        {
                            ItemNo = item.MainSalesClass != null ? (item.MainSalesClass.ReferenceCode ?? "N/A") : "N/A",
                            ItemName = item.MainSalesClass != null 
                                ? (item.MainSalesClass.ArabicName ?? item.MainSalesClass.EnglishName ?? "N/A")
                                : "N/A",
                            item.Quantity,
                            item.Price,
                            TaxRate = item.MainSalesClass != null && !item.MainSalesClass.IsTaxExempted ? 15m : 0m,
                            TaxAmount = item.MainSalesClass != null && !item.MainSalesClass.IsTaxExempted 
                                ? (item.Quantity * item.Price * 0.15m) 
                                : 0m,
                            TotalBeforeTax = item.Quantity * item.Price,
                            TotalAfterTax = item.MainSalesClass != null && !item.MainSalesClass.IsTaxExempted
                                ? (item.Quantity * item.Price * 1.15m)
                                : (item.Quantity * item.Price),
                            Discount = 0m,
                            DiscountAmount = 0m,
                            NetPrice = item.MainSalesClass != null && !item.MainSalesClass.IsTaxExempted
                                ? (item.Quantity * item.Price * 1.15m)
                                : (item.Quantity * item.Price)
                        }).ToList(),
                    CustomerName = cn.CustomerProfile != null 
                        ? (cn.CustomerProfile.ArabicName ?? cn.CustomerProfile.EnglishName ?? "N/A")
                        : "N/A"
                })
                .AsNoTracking()
                .ToListAsync();

            if (!creditNotes.Any())
                return new List<object>();

            var reportData = creditNotes
                .SelectMany(cn => cn.Items.Select(item => new
                {
                    creditNoteId = cn.Id,
                    creditNoteNo = cn.SalesCreditNoteNo,
                    customerName = cn.CustomerName,
                    issueDate = cn.IssueDate,
                    itemName = item.ItemName,
                    itemNo = item.ItemNo,
                    quantity = item.Quantity,
                    price = item.Price,
                    totalBeforeTax = item.TotalBeforeTax,
                    taxRate = item.TaxRate,
                    taxAmount = item.TaxAmount,
                    totalAfterTax = item.TotalAfterTax,
                    discount = item.Discount,
                    discountAmount = item.DiscountAmount,
                    netPrice = item.NetPrice
                }))
                .ToList<object>();

            LogService.LogInformation($"Sales Credit Note VAT Report generated with {reportData.Count} items");
            return reportData;
        }
        catch (Exception ex)
        {
            LogService.LogError($"Error in GetSalesCreditNoteVATReport: {ex.Message}");
            throw;
        }
    }

    public async Task<byte[]> GetSalesCreditNoteVATDocument(DateOnly startDate, DateOnly endDate, string language = "ar")
    {
        try
        {
            var reportData = await GetSalesCreditNoteVATReport(startDate, endDate);
            
            if (!reportData.Any())
            {
                LogService.LogInformation("No data found for Sales Credit Note VAT report");
                return Array.Empty<byte>();
            }

            LogService.LogInformation("Fetching company details...");
            var companyDetailsQuery = await _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement.CompanyDetails>().GetAll();
            var companyDetails = companyDetailsQuery.Data?.FirstOrDefault();
            if (companyDetails == null)
            {
                LogService.LogError("CompanyDetails not found, using default values");
                companyDetails = new SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement.CompanyDetails
                {
                    ArabicName = "الشركة",
                    EnglishName = "Company",
                    TaxNumber = "",
                    Email = "",
                    Website = ""
                };
            }
            LogService.LogInformation($"Company details found: {companyDetails.ArabicName}");

            LogService.LogInformation("Fetching PDF template config...");
            var pdfTemplateConfigQuery = await _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.PdfTemplateConfiguration>().GetAll();
            var pdfTemplateConfig = pdfTemplateConfigQuery.Data?.FirstOrDefault();
            LogService.LogInformation($"PDF template config found: {pdfTemplateConfig != null}");

            LogService.LogInformation("Creating Sales Credit Note VAT Report PDF Document...");
            var document = new DocumentUtils.SalesCreditNoteVATReportDocument(
                reportData.ToList(),
                companyDetails,
                pdfTemplateConfig,
                startDate,
                endDate,
                language
            );

            LogService.LogInformation("Generating PDF bytes...");
            var pdfBytes = document.GeneratePdf();
            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                LogService.LogError("Generated Sales Credit Note VAT PDF is null or empty");
                throw new InvalidOperationException("Failed to generate Sales Credit Note VAT PDF document");
            }

            LogService.LogInformation($"PDF generated successfully. Size: {pdfBytes.Length} bytes");
            return pdfBytes;
        }
        catch (Exception ex)
        {
            LogService.LogError($"Error in GetSalesCreditNoteVATDocument: {ex.Message}");
            throw;
        }
    }

        
}
