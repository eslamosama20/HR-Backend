using AutoMapper;
using Azure;
using Bee.ZatcaHelper;
using Bee.ZatcaHelper.Contracts;
using Bee.ZatcaHelper.Model;
using Bee.ZatcaHelper.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using QuestPDF.Fluent; // for GeneratePdf extension
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.CustomerProfile;
using SmartAndPro.ERP.Application.Common.Dtos.Attachments;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Purchase;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale;
using SmartAndPro.ERP.Application.Common.Dtos.Notifications;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.Reservation;
using SmartAndPro.ERP.Application.Common.Utility;
using SmartAndPro.ERP.Application.Enforcers;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Application.Services.Accounting;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Invoice.Sale.DocumentUtils;
using SmartAndPro.ERP.Application.Services.Invoice.DocumentUtils;
using SmartAndPro.ERP.Application.Services.Notifications;
using SmartAndPro.ERP.Application.Utility;
using SmartAndPro.ERP.Domains.Entities;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Assets;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Domains.Entities.Notes.SalesCreditNote;
using SmartAndPro.ERP.Domains.Models.InputModel;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.Infrastructure.External;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.Infrastructure.Repositories.Base;
using SmartAndPro.ERP.Infrastructure.Repositories.SalesInvoice;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.BusinessRule;
using SmartAndPro.ERP.SharedKernel.Exceptions;
using SmartAndPro.ERP.SharedKernel.Utilities;
using System.Diagnostics.Metrics;
using System.Runtime.InteropServices;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using TimeZoneConverter;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Domains.Entities.Receipts;

namespace SmartAndPro.ERP.Application.Services.Invoice.Sale
{
    public class SaleInvoiceService : ISalesInvoicesService
    {
        #region Failds
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly ISalesInvoicesRepository _salesInvoicesRepository;
        private readonly IMapper _mapper;
        private readonly ServiceBaseFactory _serviceBaseFactory;
        private readonly INotificationService _notificationService;
        private readonly AppSettings _appSettings;
        private readonly IPurchaseInvoiceService _purchaseInvoiceService;
        private static long _fallbackTaxSeed = DateTime.UtcNow.Ticks;
        #endregion

        #region Constructors
        public SaleInvoiceService(RepositoriesFactory repositoriesFactory,
            ISalesInvoicesRepository salesInvoicesRepository,
            AppSettings appSettings,
            IMapper mapper, 
            ServiceBaseFactory serviceBaseFactory,
            INotificationService notificationService,
            IPurchaseInvoiceService purchaseInvoiceService)
        {
            _repositoriesFactory = repositoriesFactory;
            _salesInvoicesRepository = salesInvoicesRepository;
            _mapper = mapper;
            _appSettings = appSettings;
            _serviceBaseFactory = serviceBaseFactory;
            _notificationService = notificationService;
            _purchaseInvoiceService = purchaseInvoiceService;
        }

        #endregion

        public async Task<SaleProductBarcodeResultDto?> GetProductByBarcodeAsync(string barcode)
        {
            var normalizedBarcode = barcode?.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(normalizedBarcode))
            {
                return null;
            }

            var mainSalesClassRepository = _repositoriesFactory.GetRepository<MainSalesClass>();
            var productQueryResult = await mainSalesClassRepository.Filter(x => x.ReferenceCode.ToLower() == normalizedBarcode);
            var product = productQueryResult.Data.FirstOrDefault();

            if (product == null)
            {
                return null;
            }

            var saleItemRepository = _repositoriesFactory.GetRepository<SaleItem>();
            var lastSaleItemQueryResult = await saleItemRepository.Filter(x => x.MainSalesClassId == product.Id);
            var salePrice = lastSaleItemQueryResult.Data
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.Price)
                .FirstOrDefault();

            decimal? availableQuantity = null;
            var systemSettingRepository = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.SystemSetting>();
            var systemSettingQueryResult = await systemSettingRepository.GetAll();
            var systemSetting = systemSettingQueryResult.Data.FirstOrDefault();

            // IsLinkedToInventory is added in a later phase; reflection keeps this phase compatible.
            var isLinkedToInventoryProperty = systemSetting?.GetType().GetProperty("IsLinkedToInventory");
            var isLinkedToInventory = isLinkedToInventoryProperty?.GetValue(systemSetting) as bool? ?? false;

            if (isLinkedToInventory)
            {
                var productUnitRepository = _repositoriesFactory.GetRepository<ProductUnit>();
                var productUnitsQueryResult = await productUnitRepository.Filter(x => x.MainSalesClassId == product.Id);
                availableQuantity = productUnitsQueryResult.Data.Sum(x => x.Quantity ?? 0);
            }

            return new SaleProductBarcodeResultDto
            {
                Id = product.Id,
                ArabicName = product.ArabicName,
                EnglishName = product.EnglishName,
                ReferenceCode = product.ReferenceCode,
                MeasurementUnitName = product.MeasurementUnit?.ArabicName ?? product.MeasurementUnit?.EnglishName ?? string.Empty,
                SalePrice = salePrice,
                AvailableQuantity = availableQuantity
            };
        }

        public async Task<ZatcaPreCheckResultDto> ValidateZatcaInvoiceAsync(int id)
        {
            var result = new ZatcaPreCheckResultDto { IsValid = true };
            
            var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
            var companyDetails = companyDetailsQuery.Data?.FirstOrDefault();
            if (companyDetails == null)
            {
                result.CompanyErrors.Add("Company Details not found");
                result.IsValid = false;
                return result;
            }

            var invoiceRepository = _repositoriesFactory.GetRepository<SaleInvoice>();
            var invoiceQuery = await invoiceRepository.Filter(i => i.Id == id);
            var invoice = invoiceQuery.Data?.FirstOrDefault();
            if (invoice == null)
            {
                result.CustomerErrors.Add("Invoice not found");
                result.IsValid = false;
                return result;
            }

            // Company Validation
            if (string.IsNullOrWhiteSpace(companyDetails.ArabicName))
                result.CompanyErrors.Add("اسم الشركة باللغة العربية مفقود.");
            
            if (string.IsNullOrWhiteSpace(companyDetails.TaxNumber) || companyDetails.TaxNumber.Length != 15)
                result.CompanyErrors.Add("الرقم الضريبي للشركة مفقود أو غير صحيح (يجب أن يتكون من 15 رقماً).");
            else
            {
                if (!companyDetails.TaxNumber.StartsWith("3")) result.Warnings.Add("⚠️ الرقم الضريبي للشركة لا يبدأ بـ 3");
                if (!companyDetails.TaxNumber.EndsWith("3")) result.Warnings.Add("⚠️ الرقم الضريبي للشركة لا ينتهي بـ 3");
            }

            if (companyDetails.ArabicAddress == null || string.IsNullOrWhiteSpace(companyDetails.ArabicAddress.Street))
                result.CompanyErrors.Add("اسم الشارع في عنوان الشركة مفقود.");
            
            if (companyDetails.ArabicAddress == null || string.IsNullOrWhiteSpace(companyDetails.ArabicAddress.City))
                result.CompanyErrors.Add("اسم المدينة في عنوان الشركة مفقود.");
            
            if (companyDetails.ArabicAddress == null || string.IsNullOrWhiteSpace(companyDetails.ArabicAddress.ZipCode) || companyDetails.ArabicAddress.ZipCode.Length != 5)
                result.CompanyErrors.Add("الرمز البريدي للشركة مفقود أو غير صحيح (يجب أن يتكون من 5 أرقام).");
            
            if (string.IsNullOrWhiteSpace(companyDetails.BuildingNumber) || companyDetails.BuildingNumber.Length != 4)
                result.CompanyErrors.Add("رقم المبنى للشركة مفقود أو غير صحيح (يجب أن يتكون من 4 أرقام).");
            
            if (string.IsNullOrWhiteSpace(companyDetails.SubNumber) || companyDetails.SubNumber.Length != 4)
                result.Warnings.Add("⚠️ الرقم الفرعي للشركة مفقود أو غير صحيح (يفضل أن يتكون من 4 أرقام).");
            
            if (string.IsNullOrWhiteSpace(companyDetails.District))
                result.CompanyErrors.Add("اسم الحي للشركة مفقود.");

            // Customer Validation
            if (invoice.CustomerProfile == null)
            {
                result.CustomerErrors.Add("بيانات العميل غير متوفرة.");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(invoice.CustomerProfile.ArabicName))
                    result.CustomerErrors.Add("اسم العميل باللغة العربية مفقود.");

                var hasVat = !string.IsNullOrWhiteSpace(invoice.CustomerProfile.TaxNumber);
                result.InvoiceType = hasVat ? "standard" : "simplified";

                if (hasVat)
                {
                    if (invoice.CustomerProfile.TaxNumber.Length != 15)
                        result.Warnings.Add("⚠️ الرقم الضريبي للعميل غير صحيح (يجب أن يتكون من 15 رقماً).");
                    else
                    {
                        if (!invoice.CustomerProfile.TaxNumber.StartsWith("3")) result.Warnings.Add("⚠️ الرقم الضريبي للعميل لا يبدأ بـ 3");
                        if (!invoice.CustomerProfile.TaxNumber.EndsWith("3")) result.Warnings.Add("⚠️ الرقم الضريبي للعميل لا ينتهي بـ 3");
                    }
                    
                    if (string.IsNullOrWhiteSpace(invoice.CustomerProfile.Street))
                        result.CustomerErrors.Add("اسم الشارع للعميل مطلوب للفاتورة الضريبية .");
                    
                    if (string.IsNullOrWhiteSpace(invoice.CustomerProfile.City))
                        result.CustomerErrors.Add("اسم المدينة للعميل مطلوب للفاتورة الضريبية .");
                    
                    if (string.IsNullOrWhiteSpace(invoice.CustomerProfile.PostalCode))
                        result.CustomerErrors.Add("الرمز البريدي للعميل مطلوب للفاتورة الضريبية .");
                    else if (invoice.CustomerProfile.PostalCode.Length != 5)
                        result.Warnings.Add("⚠️ الرمز البريدي للعميل غير صحيح (يفضل أن يتكون من 5 أرقام).");
                        
                    if (string.IsNullOrWhiteSpace(invoice.CustomerProfile.BuildingNumber))
                        result.CustomerErrors.Add("رقم المبنى للعميل مطلوب للفاتورة الضريبية .");
                    else if (invoice.CustomerProfile.BuildingNumber.Length != 4)
                        result.Warnings.Add("⚠️ رقم المبنى للعميل غير صحيح (يفضل أن يتكون من 4 أرقام).");
                        
                    if (string.IsNullOrWhiteSpace(invoice.CustomerProfile.District))
                        result.CustomerErrors.Add("اسم الحي للعميل مطلوب للفاتورة الضريبية .");
                }
            }

            result.IsValid = result.CompanyErrors.Count == 0 && result.CustomerErrors.Count == 0;
            return result;
        }

        public async Task ZatcaClearInvoice(int id)
        {
            try
            {
                var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
                var companyDetails = companyDetailsQuery.Data?.FirstOrDefault();
                if (companyDetails == null) throw new Exception("Company Details not found");
                
                var systemSettingQuery = await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().GetAll();
                var systemSetting = systemSettingQuery.Data?.FirstOrDefault();

                if (companyDetails.IsZatcaIntegrated == false)  throw new Exception("Company Details ZatcaIntegrated is disabled");
                if (companyDetails.IsOnBoarded == false) throw new Exception("Company Details not onboarded");
                if (companyDetails.ProdBinarySecurityToken == null || companyDetails.ProdSecret == null) throw new Exception("ProdBinarySecurityToken or Secret not found");

                var invoiceRepository = _repositoriesFactory.GetRepository<SaleInvoice>();
                var invoiceQuery = await invoiceRepository.Filter(i => i.Id == id);
                var invoice = invoiceQuery.Data?.FirstOrDefault();
                if (invoice == null) throw new Exception("Invoice not found");

                if (invoice.IsCleared == true)
                {
                    LogService.LogInformation($"Invoice {id} is already cleared with ZATCA.");
                    return;
                }

                var validationResult = await ValidateZatcaInvoiceAsync(id);
                if (!validationResult.IsValid)
                {
                    var allErrors = validationResult.CompanyErrors.Concat(validationResult.CustomerErrors).ToList();
                    var arMessage = "نقص في البيانات الأساسية للفاتورة:\n" + string.Join("\n", allErrors.Select(e => "• " + e));
                    throw new Exception($"{arMessage} | {arMessage}");
                }

                if (NeedsFrontalConversion(invoice.IsFrontal, invoice.InvoiceNo))
                {
                    await MakeFrontalAsync(invoice.Id);
                    invoiceQuery = await invoiceRepository.Filter(i => i.Id == id);
                    invoice = invoiceQuery.Data?.FirstOrDefault();
                    if (invoice == null || invoice.IsCleared == true)
                    {
                        return;
                    }
                }
                
                
                var roundedLineAmounts = invoice.SaleItems
                    .Select(item => Math.Round(item.Price * item.Quantity * item.NoOfNights, 2))
                    .ToList();

                decimal sumOfLines       = roundedLineAmounts.Sum();
                decimal roundedDiscount  = Math.Round(invoice.DiscountAmount, 2);
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
                var roundedLineTaxes = invoice.SaleItems
                    .Select((item, idx) =>
                    {
                        var isExempt = item.MainSalesClass.IsTaxExempted;
                        var netLine = roundedNetLineAmounts[idx];
                        // line-level tax = netLine * rate, then round
                        return Math.Round(netLine * (isExempt ? 0m : 0.15m), 2);
                    })
                    .ToList();
                decimal taxableBase     = Math.Round(
                    invoice.SaleItems
                        .Where((item, idx) => !item.MainSalesClass.IsTaxExempted)
                        .Select((_, idx) => roundedNetLineAmounts[idx])
                        .Sum()
                    , 2);
                decimal taxAmountValue  = roundedLineTaxes.Sum();       
                decimal netLinesTotal   = roundedNetLineAmounts.Sum();
                decimal inclusiveAmount = Math.Round(netLinesTotal + taxAmountValue, 2);
                
                var invoiceIssueDate = TimeZoneUtils.ApplyTimeZone(systemSetting?.TimeZone ?? "Asia/Riyadh", invoice.IssueDate);
                
                var standardInvoice = new StandardInvoice
                {
                    Id                   = invoice.InvoiceNo,
                    UUID                 = Guid.NewGuid().ToString(),
                    IssuedDate           = invoiceIssueDate.ToString("yyyy-MM-dd"),
                    IssuedTime           = invoiceIssueDate.ToString("HH:mm:ssZ"),
                    InvoiceTypeCode      = "388",
                    DocumentCurrencyCode = "SAR",
                    TaxCurrencyCode      = "SAR",
                    ActualDeliveryDate = invoiceIssueDate.ToString("yyyy-MM-dd"),
                    LatestDeliveryDate = invoiceIssueDate.ToString("yyyy-MM-dd"),
                    SupplierInfo = new SupplierInfo
                    {
                        RegistrationName   = companyDetails.ArabicName,
                        PartyPostalAddress = new PartyPostalAddress
                        {
                            StreetName          = companyDetails.ArabicAddress?.Street,
                            CityName            = companyDetails.ArabicAddress?.City,
                            Country             = "SA",
                            PostalZone          = companyDetails.ArabicAddress?.ZipCode,
                            BuildingNumber      = companyDetails.BuildingNumber,
                            CitySubdivisionName = companyDetails.District,
                            PlotIdentification  = companyDetails.SubNumber
                        },
                        CRN = companyDetails.TaxNumber,
                        VAT = _appSettings.ZatcaParams.IsSandbox
                              ? "399999999900003"
                              : companyDetails.TaxNumber
                    },

                    CustomerInfo = new CustomerInfo
                    {
                        RegistrationName = invoice.CustomerProfile.ArabicName,
                        VAT              = invoice.CustomerProfile.TaxNumber,
                        PartyPostalAddress = new PartyPostalAddress
                        {
                            StreetName          = invoice.CustomerProfile.Street,
                            CityName            = invoice.CustomerProfile.City,
                            Country             = "SA",
                            PostalZone          = invoice.CustomerProfile.PostalCode,
                            BuildingNumber      = invoice.CustomerProfile.BuildingNumber,
                            CitySubdivisionName = invoice.CustomerProfile.District
                        }
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

                    InvoiceLineItems = invoice.SaleItems.Select((item, idx) =>
                    {
                        decimal lineNet  = roundedLineAmounts[idx];
                        decimal lineTax  = roundedLineTaxes[idx];
                        decimal lineDisc = roundedLineDiscounts[idx];
                        return new InvoiceLineItem
                        {
                            ID                  = item.Id.ToString(),
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

                    // 3. Document-level discount (must match AllowanceTotalAmount exactly)
                    DocumentLevelDiscountAmount = roundedDiscount,
                    DocumentLevelDiscountReason  = "Invoice-level discount",
                };

                
                if (standardInvoice.CustomerInfo.VAT != null)
                {
                    var standardInvoiceXml = StandardInvoiceXmlGenerator.Generate(standardInvoice, companyDetails.PrivateKey, companyDetails.ProdBinarySecurityToken);
                    var clearanceResponse = new StandardInvoiceClearanceApiClient(
                        new GlobalVariables
                        {
                            BaseUrl = _appSettings.ZatcaParams.IsSandbox ?  _appSettings.ZatcaParams.SandboxUrls.BaseUrl : _appSettings.ZatcaParams.ProdUrls.BaseUrl,
                            InvoiceClearanceEndPoint = _appSettings.ZatcaParams.IsSandbox ?  _appSettings.ZatcaParams.SandboxUrls.InvoiceClearanceEndPoint : _appSettings.ZatcaParams.ProdUrls.InvoiceClearanceEndPoint
                        }).ClearInvoice(new InvoiceClearanceRequest(standardInvoiceXml, companyDetails.ProdBinarySecurityToken, companyDetails.ProdSecret));
                    if (clearanceResponse.ClearanceStatus != "CLEARED") throw new Exception("Clearance failed");
                    invoice.IsCleared = true;
                    invoice.InvoiceHash = clearanceResponse.Hash;
                    invoice.GeneratedQR = clearanceResponse.GeneratedQR;
                    invoice.Attachments?.Clear();
                    // preserve sale items for potential later use; do not nullify non-nullable collections
                    // invoice.SaleItems remains as-is
                    // detach heavy navigation properties safely
                    // invoice.CustomerProfile left intact for persistence layer if required
                    invoice.SalesCreditNotes?.Clear();
                    await invoiceRepository.Update(invoice);
                    
                    if (invoice.TransactionId == null)
                    {
                        var isSuccess = await CreateTransaction(id);
                        if (isSuccess != 1) throw new Exception("Transaction creation failed");
                    }
                }
                else
                {
                    var simplifiedInvoiceXml = StandardInvoiceXmlGenerator.GenerateSimplifiedInvoice(standardInvoice, companyDetails.PrivateKey, companyDetails.ProdBinarySecurityToken);
                    var reportingClient = new SimplifiedInvoiceReportingApiClient(new GlobalVariables
                    {
                        BaseUrl = _appSettings.ZatcaParams.IsSandbox ?  _appSettings.ZatcaParams.SandboxUrls.BaseUrl : _appSettings.ZatcaParams.ProdUrls.BaseUrl,
                        InvoiceReportingEndPoint = _appSettings.ZatcaParams.IsSandbox ?  _appSettings.ZatcaParams.SandboxUrls.InvoiceReportingEndPoint : _appSettings.ZatcaParams.ProdUrls.InvoiceReportingEndPoint
                    });
                    /*var invoiceValidator = new Zatca.EInvoice.SDK.EInvoiceValidator();
                    var pemCertificateBase64 = Convert.FromBase64String(companyDetails.ProdBinarySecurityToken);
                    var cert = Encoding.UTF8.GetString(pemCertificateBase64);
                    var result = invoiceValidator
                        .ValidateEInvoice(simplifiedInvoiceXml.invoiceXml, cert, companyDetails.Secret);*/
                    var reportingResponse = reportingClient.ReportSimplifiedInvoice(new InvoiceClearanceRequest(simplifiedInvoiceXml, companyDetails.ProdBinarySecurityToken, companyDetails.ProdSecret));
                    if (reportingResponse == null || reportingResponse.ReportingStatus != "REPORTED") throw new Exception("Reporting failed");
                    invoice.IsCleared = true;
                    invoice.InvoiceHash = simplifiedInvoiceXml.invoiceHash;
                    invoice.GeneratedQR = simplifiedInvoiceXml.GeneratedQR;
                    invoice.Attachments?.Clear();
                    invoice.SalesCreditNotes?.Clear();
                    
                    await invoiceRepository.Update(invoice);
                    
                    if (invoice.TransactionId == null)
                    {
                        var isSuccess = await CreateTransaction(id, true);
                        if (isSuccess != 1) throw new Exception("Transaction creation failed");
                    }
                }
                
                
            }
            catch (Exception e)
            {
                LogService.LogError(e);
                throw;
            }
        }

        public async Task<string> GetLastInvoiceNo()
        {
            var repo = _repositoriesFactory.GetRepository<SaleInvoice>();
            var dbContext = repo.GetDbContext();
            LogService.LogInformation("POS-INVOICE-NO-V2: using raw SQL scalar query for last invoice number.");
            // Query scalar values directly to avoid EF auto-includes and schema drift on SaleInvoice columns.
            var invoices = await dbContext.Database.SqlQueryRaw<string>(
                @"SELECT [InvoiceNo] AS [Value]
                  FROM [SaleInvoice]
                  WHERE ([InvoiceNo] LIKE N'Inv-%' OR [InvoiceNo] LIKE N'INV-%')")
                .ToListAsync();
             
            LogService.LogInformation($"GetLastInvoiceNo - Found {invoices.Count} invoices");
            
            if (!invoices.Any())
            {
                LogService.LogInformation("GetLastInvoiceNo - No invoices found, returning empty");
                return string.Empty;
            }
            
            // Parse all invoice numbers and get the max (support both INV- and Inv-)
            int maxNumber = 0;
            foreach (var invoiceNo in invoices)
            {
                if (string.IsNullOrWhiteSpace(invoiceNo)) continue;
                string numberPart = "";
                if (invoiceNo.StartsWith("Inv-", StringComparison.OrdinalIgnoreCase))
                {
                    numberPart = invoiceNo.Substring("Inv-".Length);
                }
                
                if (int.TryParse(numberPart, out int num))
                {
                    if (num > maxNumber)
                        maxNumber = num;
                }
            }
            
            var result = maxNumber > 0 ? $"Inv-{maxNumber}" : string.Empty;
            LogService.LogInformation($"GetLastInvoiceNo - Max number found: {maxNumber}, Returning: {result}");
            
            return result;
        }
        public async Task<string> GetLastQuotationNo()
        {
            var repo = _repositoriesFactory.GetRepository<SaleInvoice>();
            var dbContext = repo.GetDbContext();
            LogService.LogInformation("POS-QUOTATION-NO-V2: using raw SQL scalar query for last quotation number.");
            // Query scalar values directly to avoid EF auto-includes and schema drift on SaleInvoice columns.
            var quotations = await dbContext.Database.SqlQueryRaw<string>(
                @"SELECT [InvoiceNo] AS [Value]
                  FROM [SaleInvoice]
                  WHERE ([InvoiceNo] LIKE N'Q-%' OR [InvoiceNo] LIKE N'q-%')")
                .ToListAsync();
             
            LogService.LogInformation($"GetLastQuotationNo - Found {quotations.Count} quotations");
            
            if (!quotations.Any())
            {
                LogService.LogInformation("GetLastQuotationNo - No quotations found, returning empty");
                return string.Empty;
            }
            
            // Parse all quotation numbers and get the max
            int maxNumber = 0;
            foreach (var invoiceNo in quotations)
            {
                if (string.IsNullOrWhiteSpace(invoiceNo)) continue;
                if (invoiceNo.StartsWith("Q-", StringComparison.OrdinalIgnoreCase))
                {
                    string numberPart = invoiceNo.Substring("Q-".Length);
                    if (int.TryParse(numberPart, out int num))
                    {
                        if (num > maxNumber)
                            maxNumber = num;
                    }
                }
            }
            
            var result = maxNumber > 0 ? $"Q-{maxNumber}" : string.Empty;
            LogService.LogInformation($"GetLastQuotationNo - Max number found: {maxNumber}, Returning: {result}");
            
            return result;
        }
        public async Task<OperationResponse<object>> GetAllAsync(int? pageNumber = null, int? pageSize = null, bool? isSentToZatca = null, string? keyword = null, bool? hasTransaction = null)
        {
            try
            {
                var companyDetailsResponse = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
                var companyDetails = await companyDetailsResponse.Data.FirstOrDefaultAsync();
                if (companyDetails == null)
                    return new OperationResponse<object>(ErrorCodes.GeneralFail, null, message : "Company Details not found");

                var systemSettingResponse = await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().GetAll();
                var systemSetting = await systemSettingResponse.Data.FirstOrDefaultAsync();
                var timeZone = systemSetting?.TimeZone;

                var invoicesQueryResponse = await _repositoriesFactory.GetRepository<SaleInvoice>()
                .Filter(i => i.IsQuotation == false && 
                            (i.SaleInvoiceType == Domains.Enums.SaleInvoiceType.SaleInvoice || 
                             i.SaleInvoiceType == Domains.Enums.SaleInvoiceType.ManualReservationInvoice || 
                             i.IsFrontal == true));
                var invoicesQuery = invoicesQueryResponse.Data.IgnoreAutoIncludes().AsNoTracking();

                // Apply ZATCA filter if specified
                if (isSentToZatca.HasValue)
                {
                    if (isSentToZatca.Value)
                        invoicesQuery = invoicesQuery.Where(i => i.GeneratedQR != null && i.GeneratedQR != "");
                    else
                        invoicesQuery = invoicesQuery.Where(i => i.GeneratedQR == null || i.GeneratedQR == "");
                }

                // Apply hasTransaction filter (server-side, fixes pagination)
                if (hasTransaction.HasValue)
                {
                    if (hasTransaction.Value)
                        invoicesQuery = invoicesQuery.Where(i => i.TransactionId != null);
                    else
                        invoicesQuery = invoicesQuery.Where(i => i.TransactionId == null);
                }

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    keyword = keyword.ToLower();
                    invoicesQuery = invoicesQuery.Where(i => 
                        (i.InvoiceNo != null && i.InvoiceNo.ToLower().Contains(keyword)) ||
                        (i.CustomerProfile.ArabicName != null && i.CustomerProfile.ArabicName.ToLower().Contains(keyword)) ||
                        (i.CustomerProfile.EnglishName != null && i.CustomerProfile.EnglishName.ToLower().Contains(keyword)) ||
                        (i.CustomerProfile.ContactNumber != null && i.CustomerProfile.ContactNumber.ToLower().Contains(keyword))
                    );
                }

                var totalRecords = await invoicesQuery.CountAsync();

                if (!pageNumber.HasValue || !pageSize.HasValue)
                {
                    var invoicesEntities = await invoicesQuery
                        .Include(i => i.CustomerProfile)
                        .OrderByDescending(i => i.CreatedAt)
                        .ToListAsync();

                    var invoiceIdsNullable = invoicesEntities.Select(i => (int?)i.Id).ToList();

                    var saleItemsResponse = await _repositoriesFactory.GetRepository<SaleItem>()
                        .Filter(s => invoiceIdsNullable.Contains(s.SaleInvoiceId));
                    var saleItems = await saleItemsResponse.Data.IgnoreAutoIncludes().AsNoTracking().ToListAsync();

                    var attachmentsResponse = await _repositoriesFactory.GetRepository<Attachment>()
                        .Filter(a => invoiceIdsNullable.Contains(a.SaleInvoiceId));
                    var attachments = await attachmentsResponse.Data.IgnoreAutoIncludes().AsNoTracking().ToListAsync();

                    foreach (var invoice in invoicesEntities)
                    {
                        invoice.SaleItems = saleItems.Where(s => s.SaleInvoiceId == invoice.Id).ToList();
                        invoice.Attachments = attachments.Where(a => a.SaleInvoiceId == invoice.Id).ToList();
                    }

                    var invoicesDto = _mapper.Map<List<SaleInvoiceDto>>(invoicesEntities);

                    foreach (var invoice in invoicesDto)
                    {
                        if (invoice.GeneratedQR == null)
                        {
                            var entity = invoicesEntities.First(e => e.Id == invoice.Id);
                            invoice.GeneratedQR = StandardInvoiceXmlGenerator.GenerateZatcaQrCode(
                                companyDetails.ArabicName,
                                companyDetails.TaxNumber,
                                GetTotalAmount(entity),
                                GetTaxAmount(entity),
                                TimeZoneUtils.ApplyTimeZone(timeZone, entity.IssueDate)
                                    .ToString("yyyy-MM-dd'T'HH:mm:ss")
                            );
                        }
                    }

                    // Set original quotation numbers for invoices created from quotations
                    await SetOriginalQuotationNumbers(invoicesDto);

                    return new OperationResponse<object>(ErrorCodes.Success, invoicesDto);
                }

                pageNumber ??= 1;
                pageSize ??= 10;
                var skip = (pageNumber.Value - 1) * pageSize.Value;

                var invoiceIds = await invoicesQuery
                    .OrderByDescending(i => i.CreatedAt)
                    .Skip(skip)
                    .Take(pageSize.Value)
                    .Select(i => i.Id)
                    .ToListAsync();

                var invoiceIdsNullablePaged = invoiceIds.Select(id => (int?)id).ToList();

                var invoicesPagedResponse = await _repositoriesFactory.GetRepository<SaleInvoice>()
                    .Filter(i => invoiceIds.Contains(i.Id));
                var invoicesEntitiesPaged = await invoicesPagedResponse.Data
                    .IgnoreAutoIncludes()
                    .Include(i => i.CustomerProfile)
                    .IgnoreQueryFilters()
                    .OrderByDescending(i => i.CreatedAt)
                    .ThenByDescending(i => i.Id)
                    .AsNoTracking().ToListAsync();

                var saleItemsPagedResponse = await _repositoriesFactory.GetRepository<SaleItem>()
                    .Filter(s => invoiceIdsNullablePaged.Contains(s.SaleInvoiceId));
                var saleItemsPaged = await saleItemsPagedResponse.Data.IgnoreAutoIncludes().AsNoTracking().ToListAsync();

                var attachmentsPagedResponse = await _repositoriesFactory.GetRepository<Attachment>()
                    .Filter(a => invoiceIdsNullablePaged.Contains(a.SaleInvoiceId));
                var attachmentsPaged = await attachmentsPagedResponse.Data.IgnoreAutoIncludes().AsNoTracking().ToListAsync();

                foreach (var invoice in invoicesEntitiesPaged)
                {
                    invoice.SaleItems = saleItemsPaged.Where(s => s.SaleInvoiceId == invoice.Id).ToList();
                    invoice.Attachments = attachmentsPaged.Where(a => a.SaleInvoiceId == invoice.Id).ToList();
                }

                var invoicesDtoPaged = _mapper.Map<List<SaleInvoiceDto>>(invoicesEntitiesPaged);

                foreach (var invoice in invoicesDtoPaged)
                {
                    if (invoice.GeneratedQR == null)
                    {
                        var entity = invoicesEntitiesPaged.First(e => e.Id == invoice.Id);
                        invoice.GeneratedQR = StandardInvoiceXmlGenerator.GenerateZatcaQrCode(
                            companyDetails.ArabicName,
                            companyDetails.TaxNumber,
                            GetTotalAmount(entity),
                            GetTaxAmount(entity),
                            TimeZoneUtils.ApplyTimeZone(timeZone, entity.IssueDate)
                                .ToString("yyyy-MM-dd'T'HH:mm:ss")
                        );
                    }
                }

                // Set original quotation numbers for invoices created from quotations
                await SetOriginalQuotationNumbers(invoicesDtoPaged);

                var paginatedResult = new PaginatedResult<IEnumerable<SaleInvoiceDto>>(totalRecords, invoicesDtoPaged);

                return new OperationResponse<object>(ErrorCodes.Success, paginatedResult);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return new OperationResponse<object>(ErrorCodes.GeneralFail, null,message: ex.Message);
            }
        }

        public async Task<OperationResponse<object>> GetAllReservationInvoicesAsync(int? pageNumber = null, int? pageSize = null, int? type = null, string keyword = null, string sortField = null, int? sortOrder = null, bool? isSentToZatca = null, bool? hasTransaction = null)
        {
            try
            {
                var invoicesGetAllResponse = await _repositoriesFactory.GetRepository<SaleInvoice>().GetAll();
                var invoicesQuery = invoicesGetAllResponse.Data
                    .IgnoreAutoIncludes()
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(i => i.IsQuotation == false && 
                                (type.HasValue ? (int)i.SaleInvoiceType == type.Value : 
                                 i.SaleInvoiceType == SmartAndPro.ERP.Domains.Enums.SaleInvoiceType.ComeFromReservation));

                if (isSentToZatca.HasValue)
                {
                    if (isSentToZatca.Value)
                        invoicesQuery = invoicesQuery.Where(i => i.GeneratedQR != null && i.GeneratedQR != "");
                    else
                        invoicesQuery = invoicesQuery.Where(i => i.GeneratedQR == null || i.GeneratedQR == "");
                }

                if (hasTransaction.HasValue)
                {
                    if (hasTransaction.Value)
                        invoicesQuery = invoicesQuery.Where(i => i.TransactionId != null);
                    else
                        invoicesQuery = invoicesQuery.Where(i => i.TransactionId == null);
                }

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var lowerKeyword = keyword.ToLower();
                    invoicesQuery = invoicesQuery.Where(i => 
                        (i.InvoiceNo != null && i.InvoiceNo.ToLower().Contains(lowerKeyword)) ||
                        (i.CustomerProfile != null && (
                            (i.CustomerProfile.ArabicName != null && i.CustomerProfile.ArabicName.ToLower().Contains(lowerKeyword)) ||
                            (i.CustomerProfile.EnglishName != null && i.CustomerProfile.EnglishName.ToLower().Contains(lowerKeyword))
                        ))
                    );
                }

                if (!string.IsNullOrWhiteSpace(sortField))
                {
                    bool isDesc = sortOrder == -1;
                    switch (sortField.ToLower())
                    {
                        case "issueno":
                        case "invoiceno":
                            invoicesQuery = isDesc ? invoicesQuery.OrderByDescending(i => i.InvoiceNo).ThenByDescending(i => i.Id) : invoicesQuery.OrderBy(i => i.InvoiceNo).ThenBy(i => i.Id);
                            break;
                        case "issuedate":
                            invoicesQuery = isDesc ? invoicesQuery.OrderByDescending(i => i.IssueDate).ThenByDescending(i => i.Id) : invoicesQuery.OrderBy(i => i.IssueDate).ThenBy(i => i.Id);
                            break;
                        case "totalprice":
                            invoicesQuery = isDesc ? invoicesQuery.OrderByDescending(i => i.TotalPrice).ThenByDescending(i => i.Id) : invoicesQuery.OrderBy(i => i.TotalPrice).ThenBy(i => i.Id);
                            break;
                        default:
                            invoicesQuery = isDesc ? invoicesQuery.OrderByDescending(i => i.CreatedAt).ThenByDescending(i => i.Id) : invoicesQuery.OrderBy(i => i.CreatedAt).ThenBy(i => i.Id);
                            break;
                    }
                }
                else
                {
                    invoicesQuery = invoicesQuery.OrderByDescending(i => i.CreatedAt).ThenByDescending(i => i.Id);
                }

                var totalRecords = await invoicesQuery.CountAsync();

                if (!pageNumber.HasValue || !pageSize.HasValue)
                {
                    pageNumber = 1;
                    pageSize = totalRecords > 0 ? totalRecords : 10;
                }

                var skip = (pageNumber.Value - 1) * pageSize.Value;

                var pagedInvoices = await invoicesQuery
                    .Skip(skip)
                    .Take(pageSize.Value)
                    .Select(i => new SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale.ReservationInvoiceListDto
                    {
                        Id = i.Id,
                        InvoiceNo = i.InvoiceNo,
                        IssueDate = i.IssueDate,
                        DueDate = i.DueDate,
                        NetPrice = i.NetPrice,
                        TotalPrice = i.TotalPrice,
                        CustomerProfileName = i.CustomerProfile != null ? (i.CustomerProfile.ArabicName ?? i.CustomerProfile.EnglishName) : null,
                        TransactionId = i.TransactionId,
                        ReservationIds = i.Reservations.Select(r => r.Id).ToList()
                    })
                    .ToListAsync();

                var paginatedResult = new PaginatedResult<IEnumerable<SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale.ReservationInvoiceListDto>>(totalRecords, pagedInvoices);
                return new OperationResponse<object>(ErrorCodes.Success, paginatedResult);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return new OperationResponse<object>(ErrorCodes.GeneralFail, null, message: ex.Message);
            }
        }

        #region OldGetAllService
        //public async Task<PaginatedResult<IEnumerable<SaleInvoice>>> GetAllAsync(int? pageNumber = null, int? pageSize = null)
        //{
        //    var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
        //    var companyDetails = await companyDetailsQuery.Data.FirstOrDefaultAsync();
        //    if (companyDetails == null) throw new Exception("Company Details not found");

        //    var systemSettingQuery = await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().GetAll();
        //    var systemSetting = systemSettingQuery.Data.FirstOrDefault();
        //    var timeZone = systemSetting?.TimeZone;

        //    var invoicesQuery = await _repositoriesFactory.GetRepository<SaleInvoice>().Filter(x => x.IsQuotation == false);

        //    var query = invoicesQuery.Data;
        //    var totalRecords = await query.CountAsync();


        //    if (pageNumber.HasValue && pageSize.HasValue && pageNumber > 0 && pageSize > 0)
        //    {
        //        var skip = (pageNumber.Value - 1) * pageSize.Value;
        //        query = query.OrderBy(x => x.CreatedAt).Skip(skip).Take(pageSize.Value);
        //    }

        //    var invoices = query
        //        .Select(i => new SaleInvoice
        //        {
        //            Id = i.Id,
        //            InvoiceNo = i.InvoiceNo,
        //            Description = i.Description,
        //            IssueDate = i.IssueDate,
        //            DueDate = i.DueDate,
        //            Tax = i.Tax,
        //            TaxAmount = i.TaxAmount,
        //            Discount = i.Discount,
        //            DiscountAmount = i.DiscountAmount,
        //            TotalPrice = i.TotalPrice,
        //            NetPrice = i.NetPrice,
        //            IsFrontal = i.IsFrontal,
        //            SaleItems = i.SaleItems.Select(s => new SaleItem
        //            {
        //                Id = s.Id,
        //                MainSalesClassId = s.MainSalesClassId,
        //                MainSalesClass = new MainSalesClass
        //                {
        //                    Id = s.MainSalesClass.Id,
        //                    IsTaxExempted = s.MainSalesClass.IsTaxExempted,
        //                    EnglishName = s.MainSalesClass.EnglishName,
        //                    IsActive = s.MainSalesClass.IsActive,
        //                    CreatedAt = s.MainSalesClass.CreatedAt,
        //                    CreatedBy = s.MainSalesClass.CreatedBy,
        //                    MeasurementUnit = new MeasurementUnit
        //                    {
        //                        Id = s.MainSalesClass.MeasurementUnit.Id,
        //                        EnglishName = s.MainSalesClass.MeasurementUnit.EnglishName,
        //                        ArabicName = s.MainSalesClass.MeasurementUnit.ArabicName,
        //                        ArabicSymbol = s.MainSalesClass.MeasurementUnit.ArabicSymbol,
        //                        EnglishSymbol = s.MainSalesClass.MeasurementUnit.EnglishSymbol,
        //                    },
        //                    MeasurementUnitId = s.MainSalesClass.MeasurementUnitId,
        //                    ReferenceCode = s.MainSalesClass.ReferenceCode,
        //                },
        //                Price = s.Price,
        //                Quantity = s.Quantity,
        //                NoOfNights = s.NoOfNights,
        //                Discount = s.Discount,
        //                DiscountAmount = s.DiscountAmount,
        //                TotalPrice = s.TotalPrice,
        //                NetPrice = s.NetPrice,
        //                Name = s.Name,
        //                IsActive = s.IsActive,
        //                Tax = s.Tax,
        //                SaleInvoiceId = s.SaleInvoiceId,
        //                TaxAmount = s.TaxAmount,
        //                ItemNo = s.ItemNo,
        //                CreatedAt = s.CreatedAt,
        //            }).ToList(),
        //            CustomerProfileId = i.CustomerProfileId,
        //            CustomerProfile = new CustomerProfile
        //            {
        //                Id = i.CustomerProfile.Id,
        //                EnglishName = i.CustomerProfile.EnglishName,
        //                ArabicName = i.CustomerProfile.ArabicName,
        //                MainAccountId = i.CustomerProfile.MainAccountId,

        //            },
        //            TransactionId = i.TransactionId,
        //            IsCleared = i.IsCleared,
        //            IsQuotation = i.IsQuotation,
        //            CreatedAt = i.CreatedAt,
        //            Currency = i.Currency,
        //            GeneratedQR = i.GeneratedQR ?? StandardInvoiceXmlGenerator
        //                    .GenerateZatcaQrCode(
        //                        companyDetails.ArabicName,
        //                        companyDetails.TaxNumber,
        //                        GetTotalAmount(i),
        //                        GetTaxAmount(i),
        //                        TimeZoneUtils.ApplyTimeZone(timeZone, i.IssueDate).ToString("yyyy-MM-dd'T'HH:mm:ss")
        //                        ),

        //        });

        //    var finalInvoices = await invoices.ToListAsync();
        //    return new PaginatedResult<IEnumerable<SaleInvoice>>(totalRecords, finalInvoices);
        //}

        //public async Task<PaginatedResult<IEnumerable<SaleInvoiceDto>>> GetAllAsync(int? pageNumber = null, int? pageSize = null)
        //{
        //    var companyDetailsResponse = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
        //    var companyDetails = await companyDetailsResponse.Data.FirstOrDefaultAsync();
        //    if (companyDetails == null) throw new Exception("Company Details not found");

        //    var systemSettingResponse = await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().GetAll();
        //    var systemSetting = await systemSettingResponse.Data.FirstOrDefaultAsync();
        //    var timeZone = systemSetting?.TimeZone;

        //    var invoicesQuery = _repositoriesFactory.GetRepository<SaleInvoice>()
        //        .Filter(i => i.IsQuotation == false)
        //        .Result.Data.AsNoTracking();

        //    var totalRecords = await invoicesQuery.CountAsync();

        //    if (!pageNumber.HasValue || !pageSize.HasValue)
        //    {
        //        pageNumber = 1;
        //        pageSize = totalRecords;
        //    }

        //    var skip = (pageNumber.Value - 1) * pageSize.Value;

        //    var invoiceIds = await invoicesQuery
        //        .OrderBy(i => i.CreatedAt)
        //        .Skip(skip)
        //        .Take(pageSize.Value)
        //        .Select(i => i.Id)
        //        .ToListAsync();

        //    var invoiceIdsNullable = invoiceIds.Select(id => (int?)id).ToList();

        //    var invoicesEntities = await _repositoriesFactory.GetRepository<SaleInvoice>()
        //        .Filter(i => invoiceIds.Contains(i.Id))
        //        .Result.Data
        //        .AsNoTracking()
        //        .ToListAsync();

        //    var saleItems = await _repositoriesFactory.GetRepository<SaleItem>()
        //        .Filter(s => invoiceIdsNullable.Contains(s.SaleInvoiceId))
        //        .Result.Data
        //        .AsNoTracking()
        //        .ToListAsync();

        //    var attachments = await _repositoriesFactory.GetRepository<Attachment>()
        //        .Filter(a => invoiceIdsNullable.Contains(a.SaleInvoiceId))
        //        .Result.Data
        //        .AsNoTracking()
        //        .ToListAsync();

        //    foreach (var invoice in invoicesEntities)
        //    {
        //        invoice.SaleItems = saleItems.Where(s => s.SaleInvoiceId == invoice.Id).ToList();
        //        invoice.Attachments = attachments.Where(a => a.SaleInvoiceId == invoice.Id).ToList();
        //    }

        //    var invoicesDto = _mapper.Map<List<SaleInvoiceDto>>(invoicesEntities);

        //    foreach (var invoice in invoicesDto)
        //    {
        //        if (invoice.GeneratedQR == null)
        //        {
        //            var issueDate = invoicesEntities.First(e => e.Id == invoice.Id).IssueDate;
        //            invoice.GeneratedQR = StandardInvoiceXmlGenerator.GenerateZatcaQrCode(
        //                companyDetails.ArabicName,
        //                companyDetails.TaxNumber,
        //                GetTotalAmount(invoicesEntities.First(e => e.Id == invoice.Id)),
        //                GetTaxAmount(invoicesEntities.First(e => e.Id == invoice.Id)),
        //                TimeZoneUtils.ApplyTimeZone(timeZone, issueDate).ToString("yyyy-MM-dd'T'HH:mm:ss")
        //            );
        //        }
        //    }

        //    return new PaginatedResult<IEnumerable<SaleInvoiceDto>>(totalRecords, invoicesDto);
        //}

        #endregion

        public async Task<SaleInvoice> MakeFrontalAsync(int salesInvoiceId)
        {
            const int maxRetries = 10;
            int retryCount = 0;
            
            while (retryCount < maxRetries)
            {
                var salesInvoiceRepository = _repositoriesFactory.GetRepository<SaleInvoice>(true);
                var companyDetailsRepository = _repositoriesFactory.GetRepository<CompanyDetails>();

                try
                {
                    await salesInvoiceRepository.BeginTransactionAsync();
                    var salesInvoiceQuery = await salesInvoiceRepository.Filter(x => x.Id == salesInvoiceId);
                    var salesInvoice = salesInvoiceQuery.Data?
                        .IgnoreAutoIncludes()
                        .FirstOrDefault();

                    var companyDetailsQuery = await companyDetailsRepository.GetAll();
                    var companyDetails = await companyDetailsQuery.Data.FirstOrDefaultAsync();
                    if (companyDetails == null) throw new Exception("Company Details not found");

                    if (salesInvoice == null)
                    {
                        throw new Exception("Sales invoice not found");
                    }

                    // Check if invoice already has a valid FINV- number
                    if (NeedsFrontalConversion(salesInvoice.IsFrontal, salesInvoice.InvoiceNo))
                    {
                        salesInvoice.IsFrontal = true;

                        var lastFinvInvoiceQuery = await salesInvoiceRepository
                            .Filter(i => i.InvoiceNo.StartsWith("FINV-") && i.IsFrontal == true);

                        var lastInvoiceNo = await lastFinvInvoiceQuery.Data
                            .OrderByDescending(i => i.InvoiceNo.Length)
                            .ThenByDescending(i => i.InvoiceNo)
                            .Select(i => i.InvoiceNo)
                            .FirstOrDefaultAsync();

                        int nextNumber = CalculateNextFrontalNumberFromLast(lastInvoiceNo, retryCount);
                        salesInvoice.InvoiceNo = $"FINV-{nextNumber}";
                    }
                    else
                    {
                        salesInvoice.IsFrontal = true;
                    }

                    var updateResult = await salesInvoiceRepository.Update(salesInvoice);

                    if (updateResult.Data == null)
                    {
                        throw new InvalidOperationException("Failed to update invoice");
                    }

                    await salesInvoiceRepository.CommitTransactionAsync();

                    if (companyDetails.IsZatcaIntegrated && salesInvoice.IsCleared != true)
                    {
                        try
                        {
                            await ZatcaClearInvoice(salesInvoice.Id);
                        }
                        catch (Exception zatcaEx)
                        {
                            LogService.LogError(zatcaEx);
                            throw;
                        }
                    }

                    return updateResult.Data;
                }
                catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
                {
                    // Duplicate key error - retry with a new invoice number
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        await salesInvoiceRepository.RollbackTransactionAsync();
                        LogService.LogError(ex);
                        throw new Exception($"Failed to make invoice frontal after {maxRetries} attempts due to duplicate invoice number. Please try again.");
                    }
                    await salesInvoiceRepository.RollbackTransactionAsync();
                    // Wait a small random amount to reduce collision probability
                    await Task.Delay(new Random().Next(50, 200));
                    // Continue to retry
                }
                catch (Exception ex)
                {
                    await salesInvoiceRepository.RollbackTransactionAsync();
                    LogService.LogError(ex);
                    throw;
                }
            }
            
            throw new Exception("Failed to make invoice frontal after maximum retries.");
        }

        public static string? NormalizeReferenceNumber(string? referenceNumber)
        {
            return string.IsNullOrWhiteSpace(referenceNumber) ? null : referenceNumber.Trim();
        }

        public static int CalculateNextInvoiceNumber(IEnumerable<string?>? existingInvoiceNumbers, int retryCount = 0)
        {
            int maxNumber = 0;
            if (existingInvoiceNumbers != null)
            {
                foreach (var no in existingInvoiceNumbers)
                {
                    if (string.IsNullOrWhiteSpace(no)) continue;
                    string trimmed = no.Trim();
                    if (trimmed.StartsWith("Inv-", StringComparison.OrdinalIgnoreCase))
                    {
                        var numberPart = trimmed.Substring("Inv-".Length);
                        if (int.TryParse(numberPart, out int numVal) && numVal > maxNumber)
                        {
                            maxNumber = numVal;
                        }
                    }
                }
            }

            int nextNumber = maxNumber + 1;
            if (retryCount > 0)
            {
                nextNumber += retryCount;
            }

            return nextNumber;
        }

        public static int CalculateNextQuotationNumber(IEnumerable<string?>? existingQuotationNumbers, int retryCount = 0)
        {
            int maxNumber = 0;
            if (existingQuotationNumbers != null)
            {
                foreach (var no in existingQuotationNumbers)
                {
                    if (string.IsNullOrWhiteSpace(no)) continue;
                    string trimmed = no.Trim();
                    if (trimmed.StartsWith("Q-", StringComparison.OrdinalIgnoreCase))
                    {
                        var numberPart = trimmed.Substring("Q-".Length);
                        if (int.TryParse(numberPart, out int numVal) && numVal > maxNumber)
                        {
                            maxNumber = numVal;
                        }
                    }
                }
            }

            int nextNumber = maxNumber + 1;
            if (retryCount > 0)
            {
                nextNumber += retryCount;
            }

            return nextNumber;
        }

        public static bool NeedsFrontalConversion(bool? isFrontal, string? invoiceNo)
        {
            return isFrontal != true || string.IsNullOrEmpty(invoiceNo) || !invoiceNo.StartsWith("FINV-");
        }

        public static int CalculateNextFrontalNumber(IEnumerable<string?>? existingInvoiceNumbers, int retryCount = 0)
        {
            int maxNumber = 0;
            if (existingInvoiceNumbers != null)
            {
                foreach (var no in existingInvoiceNumbers)
                {
                    if (!string.IsNullOrEmpty(no) && no.StartsWith("FINV-"))
                    {
                        var numberPart = no.Substring("FINV-".Length);
                        if (int.TryParse(numberPart, out int numVal) && numVal > maxNumber)
                        {
                            maxNumber = numVal;
                        }
                    }
                }
            }

            int nextNumber = maxNumber + 1;
            if (retryCount > 0)
            {
                nextNumber += retryCount;
            }

            return nextNumber;
        }

        public static int CalculateNextFrontalNumberFromLast(string? lastInvoiceNo, int retryCount = 0)
        {
            int maxNumber = 0;
            if (!string.IsNullOrEmpty(lastInvoiceNo) && lastInvoiceNo.StartsWith("FINV-"))
            {
                var numberPart = lastInvoiceNo.Substring("FINV-".Length);
                if (int.TryParse(numberPart, out int numVal))
                {
                    maxNumber = numVal;
                }
            }

            int nextNumber = maxNumber + 1;
            if (retryCount > 0)
            {
                nextNumber += retryCount;
            }

            return nextNumber;
        }

        public async Task<OperationResponse<object>> GetAllFrontalAsync(int? pageNumber = null, int? pageSize = null, bool? isSentToZatca = null, string? keyword = null)
        {
            try
            {
                var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
                var companyDetails = await companyDetailsQuery.Data.FirstOrDefaultAsync();
                if (companyDetails == null)
                    return new OperationResponse<object>(ErrorCodes.GeneralFail, null, message: "Company Details not found");

                var systemSettingQuery = await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().GetAll();
                var systemSetting = systemSettingQuery.Data.FirstOrDefault();
                var timeZone = systemSetting?.TimeZone;

                var invoicesQueryResponse = await _repositoriesFactory.GetRepository<SaleInvoice>()
                    .Filter(i => i.IsQuotation == false && i.IsFrontal == true);
                var invoicesQuery = invoicesQueryResponse.Data.IgnoreAutoIncludes().AsNoTracking();

                // Apply ZATCA filter if specified
                if (isSentToZatca.HasValue)
                {
                    if (isSentToZatca.Value)
                        invoicesQuery = invoicesQuery.Where(i => i.GeneratedQR != null && i.GeneratedQR != "");
                    else
                        invoicesQuery = invoicesQuery.Where(i => i.GeneratedQR == null || i.GeneratedQR == "");
                }

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    keyword = keyword.ToLower();
                    invoicesQuery = invoicesQuery.Where(i => 
                        (i.InvoiceNo != null && i.InvoiceNo.ToLower().Contains(keyword)) ||
                        (i.CustomerProfile.ArabicName != null && i.CustomerProfile.ArabicName.ToLower().Contains(keyword)) ||
                        (i.CustomerProfile.EnglishName != null && i.CustomerProfile.EnglishName.ToLower().Contains(keyword)) ||
                        (i.CustomerProfile.ContactNumber != null && i.CustomerProfile.ContactNumber.ToLower().Contains(keyword))
                    );
                }

                var totalRecords = await invoicesQuery.CountAsync();

                if (!pageNumber.HasValue || !pageSize.HasValue)
                {
                    var invoicesEntities = await invoicesQuery
                        .OrderByDescending(i => i.CreatedAt)
                        .ToListAsync();

                    var invoiceIdsNullable = invoicesEntities.Select(i => (int?)i.Id).ToList();

                    var saleItemsResponse = await _repositoriesFactory.GetRepository<SaleItem>()
                        .Filter(s => invoiceIdsNullable.Contains(s.SaleInvoiceId));
                    var saleItems = await saleItemsResponse.Data.AsNoTracking().ToListAsync();

                    var attachmentsResponse = await _repositoriesFactory.GetRepository<Attachment>()
                        .Filter(a => invoiceIdsNullable.Contains(a.SaleInvoiceId));
                    var attachments = await attachmentsResponse.Data.AsNoTracking().ToListAsync();

                    foreach (var invoice in invoicesEntities)
                    {
                        invoice.SaleItems = saleItems.Where(s => s.SaleInvoiceId == invoice.Id).ToList();
                        invoice.Attachments = attachments.Where(a => a.SaleInvoiceId == invoice.Id).ToList();
                    }

                    var invoicesDto = _mapper.Map<List<SaleInvoiceDto>>(invoicesEntities);

                    foreach (var invoice in invoicesDto)
                    {
                        if (invoice.GeneratedQR == null)
                        {
                            var entity = invoicesEntities.First(e => e.Id == invoice.Id);
                            invoice.GeneratedQR = StandardInvoiceXmlGenerator.GenerateZatcaQrCode(
                                companyDetails.ArabicName,
                                companyDetails.TaxNumber,
                                GetTotalAmount(entity),
                                GetTaxAmount(entity),
                                TimeZoneUtils.ApplyTimeZone(timeZone, entity.IssueDate)
                                    .ToString("yyyy-MM-dd'T'HH:mm:ss")
                            );
                        }
                    }

                    return new OperationResponse<object>(ErrorCodes.Success, invoicesDto);
                }

                pageNumber ??= 1;
                pageSize ??= 10;
                var skip = (pageNumber.Value - 1) * pageSize.Value;

                var invoiceIds = await invoicesQuery
                    .OrderByDescending(i => i.CreatedAt)
                    .Skip(skip)
                    .Take(pageSize.Value)
                    .Select(i => i.Id)
                    .ToListAsync();

                var invoiceIdsNullablePaged = invoiceIds.Select(id => (int?)id).ToList();

                var invoicesPagedResponse = await _repositoriesFactory.GetRepository<SaleInvoice>()
                    .Filter(i => invoiceIds.Contains(i.Id));
                var invoicesEntitiesPaged = await invoicesPagedResponse.Data
                    .IgnoreAutoIncludes()
                    .Include(i => i.CustomerProfile)
                    .IgnoreQueryFilters()
                    .OrderByDescending(i => i.CreatedAt)
                    .ThenByDescending(i => i.Id)
                    .AsNoTracking().ToListAsync();

                var saleItemsPagedResponse = await _repositoriesFactory.GetRepository<SaleItem>()
                    .Filter(s => invoiceIdsNullablePaged.Contains(s.SaleInvoiceId));
                var saleItemsPaged = await saleItemsPagedResponse.Data.IgnoreAutoIncludes().AsNoTracking().ToListAsync();

                var attachmentsPagedResponse = await _repositoriesFactory.GetRepository<Attachment>()
                    .Filter(a => invoiceIdsNullablePaged.Contains(a.SaleInvoiceId));
                var attachmentsPaged = await attachmentsPagedResponse.Data.IgnoreAutoIncludes().AsNoTracking().ToListAsync();

                foreach (var invoice in invoicesEntitiesPaged)
                {
                    invoice.SaleItems = saleItemsPaged.Where(s => s.SaleInvoiceId == invoice.Id).ToList();
                    invoice.Attachments = attachmentsPaged.Where(a => a.SaleInvoiceId == invoice.Id).ToList();
                }

                var invoicesDtoPaged = _mapper.Map<List<SaleInvoiceDto>>(invoicesEntitiesPaged);

                foreach (var invoice in invoicesDtoPaged)
                {
                    if (invoice.GeneratedQR == null)
                    {
                        var entity = invoicesEntitiesPaged.First(e => e.Id == invoice.Id);
                        invoice.GeneratedQR = StandardInvoiceXmlGenerator.GenerateZatcaQrCode(
                            companyDetails.ArabicName,
                            companyDetails.TaxNumber,
                            GetTotalAmount(entity),
                            GetTaxAmount(entity),
                            TimeZoneUtils.ApplyTimeZone(timeZone, entity.IssueDate)
                                .ToString("yyyy-MM-dd'T'HH:mm:ss")
                        );
                    }
                }

                var paginatedResult = new PaginatedResult<IEnumerable<SaleInvoiceDto>>(totalRecords, invoicesDtoPaged);

                return new OperationResponse<object>(ErrorCodes.Success, paginatedResult);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return new OperationResponse<object>(ErrorCodes.GeneralFail, null, message: ex.Message);
            }
        }


        #region GetAllFrontalAsync
        //public async Task<PaginatedResult<IEnumerable<SaleInvoiceDto>>> GetAllFrontalAsync(int? pageNumber = null, int? pageSize = null)
        //{
        //    var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
        //    var companyDetails = await companyDetailsQuery.Data.FirstOrDefaultAsync();
        //    if (companyDetails == null) throw new Exception("Company Details not found");

        //    var systemSettingQuery = await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().GetAll();
        //    var systemSetting = systemSettingQuery.Data.FirstOrDefault();
        //    var timeZone = systemSetting?.TimeZone;



        //    var invoicesQuery = _repositoriesFactory.GetRepository<SaleInvoice>()
        //       .Filter(i => i.IsQuotation == false && i.IsFrontal == true)
        //       .Result.Data?.AsNoTracking();

        //    var totalRecords = await invoicesQuery.CountAsync();

        //    if (!pageNumber.HasValue || !pageSize.HasValue)
        //    {
        //        pageNumber = 1;
        //        pageSize = totalRecords;
        //    }

        //    var skip = (pageNumber.Value - 1) * pageSize.Value;

        //    var invoiceIds = await invoicesQuery
        //        .OrderBy(i => i.CreatedAt)
        //        .Skip(skip)
        //        .Take(pageSize.Value)
        //        .Select(i => i.Id)
        //        .ToListAsync();

        //    var invoiceIdsNullable = invoiceIds.Select(id => (int?)id).ToList();

        //    var invoicesEntities = await _repositoriesFactory.GetRepository<SaleInvoice>()
        //        .Filter(i => invoiceIds.Contains(i.Id))
        //        .Result.Data
        //        .AsNoTracking()
        //        .ToListAsync();

        //    var saleItems = await _repositoriesFactory.GetRepository<SaleItem>()
        //        .Filter(s => invoiceIdsNullable.Contains(s.SaleInvoiceId))
        //        .Result.Data
        //        .AsNoTracking()
        //        .ToListAsync();

        //    var attachments = await _repositoriesFactory.GetRepository<Attachment>()
        //        .Filter(a => invoiceIdsNullable.Contains(a.SaleInvoiceId))
        //        .Result.Data
        //        .AsNoTracking()
        //        .ToListAsync();

        //    foreach (var invoice in invoicesEntities)
        //    {
        //        invoice.SaleItems = saleItems.Where(s => s.SaleInvoiceId == invoice.Id).ToList();
        //        invoice.Attachments = attachments.Where(a => a.SaleInvoiceId == invoice.Id).ToList();
        //    }

        //    var invoicesDto = _mapper.Map<List<SaleInvoiceDto>>(invoicesEntities);

        //    foreach (var invoice in invoicesDto)
        //    {
        //        if (invoice.GeneratedQR == null)
        //        {
        //            var issueDate = invoicesEntities.First(e => e.Id == invoice.Id).IssueDate;
        //            invoice.GeneratedQR = StandardInvoiceXmlGenerator.GenerateZatcaQrCode(
        //                companyDetails.ArabicName,
        //                companyDetails.TaxNumber,
        //                GetTotalAmount(invoicesEntities.First(e => e.Id == invoice.Id)),
        //                GetTaxAmount(invoicesEntities.First(e => e.Id == invoice.Id)),
        //                TimeZoneUtils.ApplyTimeZone(timeZone, issueDate).ToString("yyyy-MM-dd'T'HH:mm:ss")
        //            );
        //        }

        //    }
        //        return new PaginatedResult<IEnumerable<SaleInvoiceDto>>(totalRecords, invoicesDto);
        //}

        #endregion

        private async Task SetOriginalQuotationNumbers(List<SaleInvoiceDto> invoicesDto)
        {
            if (invoicesDto == null || !invoicesDto.Any())
                return;

            var invoiceIds = invoicesDto.Select(i => i.Id).ToList();
            var quotationsQuery = await _repositoriesFactory.GetRepository<SaleInvoice>()
                .Filter(q => q.IsQuotation == true && q.ConvertedToInvoiceId != null && invoiceIds.Contains(q.ConvertedToInvoiceId.Value));
            var quotations = await quotationsQuery.Data.IgnoreAutoIncludes().Select(q => new { q.ConvertedToInvoiceId, q.InvoiceNo }).ToListAsync();
            var quotationDict = quotations.ToDictionary(q => q.ConvertedToInvoiceId.Value, q => q.InvoiceNo);

            foreach (var invoice in invoicesDto)
            {
                if (quotationDict.ContainsKey(invoice.Id))
                {
                    invoice.OriginalQuotationNo = quotationDict[invoice.Id];
                }
            }
        }

        private async Task SetOriginalQuotationNumber(SaleInvoiceDto invoiceDto)
        {
            if (invoiceDto == null)
                return;

            var quotationQuery = await _repositoriesFactory.GetRepository<SaleInvoice>()
                .Filter(q => q.IsQuotation == true && q.ConvertedToInvoiceId == invoiceDto.Id);
            var quotation = await quotationQuery.Data.FirstOrDefaultAsync();
            
            if (quotation != null)
            {
                invoiceDto.OriginalQuotationNo = quotation.InvoiceNo;
            }
        }

        public async Task<string?> GetOriginalQuotationNoAsync(int invoiceId)
        {
            var dbContext = _repositoriesFactory.GetRepository<SaleInvoice>().GetDbContext();
            var quotationNo = await dbContext.Set<SaleInvoice>()
                .AsNoTracking()
                .IgnoreAutoIncludes()
                .Where(q => q.IsQuotation == true && q.ConvertedToInvoiceId == invoiceId)
                .Select(q => q.InvoiceNo)
                .FirstOrDefaultAsync();
            
            return quotationNo;
        }

        public async Task<string?> GetQuotationNoByIdAsync(int quotationId)
        {
            var dbContext = _repositoriesFactory.GetRepository<SaleInvoice>().GetDbContext();
            var quotationNo = await dbContext.Set<SaleInvoice>()
                .AsNoTracking()
                .IgnoreAutoIncludes()
                .Where(q => q.Id == quotationId && q.IsQuotation == true)
                .Select(q => q.InvoiceNo)
                .FirstOrDefaultAsync();
            
            return quotationNo;
        }

        public static decimal GetNetAmount(SaleInvoice invoice)
        {
            var netTotal = invoice.SaleItems.Sum(item => item.Price * item.Quantity * item.NoOfNights);
            var discount = invoice.DiscountAmount;
            return netTotal - discount;
        }
        
        public static decimal GetTaxAmount(SaleInvoice invoice)
        {
            var subtotals = invoice.SaleItems
                .Select(item => item.Price * item.Quantity * item.NoOfNights)
                .ToList();
            var grossTotal = subtotals.Sum();
            var discount = invoice.DiscountAmount;
            decimal totalTax = 0;
            var saleItems = invoice.SaleItems.ToList();
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
        
        public static decimal GetTotalAmount(SaleInvoice invoice)
        {
            var netAmount = GetNetAmount(invoice);
            var taxAmount = GetTaxAmount(invoice);
            return netAmount + taxAmount;
        }

        private static void RecalculateInvoicePaymentStatus(SaleInvoice invoice)
        {
            if (invoice == null) return;
            
            var totalAmount = (invoice.NetPrice + invoice.TaxAmount) > 0 ? (invoice.NetPrice + invoice.TaxAmount) : invoice.TotalPrice;
            
            if (invoice.ReceiptConnections != null)
            {
                invoice.PaidAmount = Math.Round(invoice.ReceiptConnections.Sum(rc => rc.LinkedAmount), 2);
            }

            invoice.RemainingAmount = Math.Round(Math.Max(0, totalAmount - invoice.PaidAmount), 2);
            
            if (totalAmount <= 0)
            {
                invoice.PaymentStatus = invoice.PaidAmount > 0 ? SmartAndPro.ERP.Domains.Enums.InvoicePaymentStatus.Paid : SmartAndPro.ERP.Domains.Enums.InvoicePaymentStatus.Unpaid;
            }
            else if (invoice.PaidAmount <= 0)
            {
                invoice.PaymentStatus = SmartAndPro.ERP.Domains.Enums.InvoicePaymentStatus.Unpaid;
            }
            else if (invoice.PaidAmount < (totalAmount - 0.01m))
            {
                invoice.PaymentStatus = SmartAndPro.ERP.Domains.Enums.InvoicePaymentStatus.PartiallyPaid;
            }
            else
            {
                invoice.PaymentStatus = SmartAndPro.ERP.Domains.Enums.InvoicePaymentStatus.Paid;
            }
        }
        
        public async Task<IEnumerable<SaleInvoice>> GetAllQuotationsAsync()
        {
            var invoicesQuery = await _repositoriesFactory.GetRepository<SaleInvoice>().Filter(x => x.IsQuotation == true);
            var invoices = await invoicesQuery.Data.ToListAsync();
            return invoices;
        }
        public async Task<SaleInvoice> GetFirstAsync()
        {
            var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
            var companyDetails = await companyDetailsQuery.Data.FirstOrDefaultAsync();
            if (companyDetails == null) throw new Exception("Company Details not found");
            
            var systemSettingQuery = await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().GetAll();
            var systemSetting = systemSettingQuery.Data.FirstOrDefault();
            var timeZone = systemSetting?.TimeZone;
            var invoicesQuery = await _repositoriesFactory.GetRepository<SaleInvoice>().GetAll();
            var invoice = await invoicesQuery.Data.FirstOrDefaultAsync();
            invoice.GeneratedQR = invoice.GeneratedQR ?? StandardInvoiceXmlGenerator
                .GenerateZatcaQrCode(
                    companyDetails.ArabicName,
                    companyDetails.TaxNumber,
                    GetTotalAmount(invoice),
                    GetTaxAmount(invoice),
                    TimeZoneUtils.ApplyTimeZone(timeZone, invoice.IssueDate).ToString("yyyy-MM-dd'T'HH:mm:ss")
                    );
            return invoice;
        }
        public async Task<SaleInvoice> GetByIdAsync(int id)
        {
            LogService.LogInformation($"[SaleInvoiceService] 🔍 GetByIdAsync called with id={id}");
            
            var dbContext = _repositoriesFactory.GetRepository<SaleInvoice>().GetDbContext();

            var companyDetails = await dbContext.Set<CompanyDetails>()
                .AsNoTracking()
                .Select(c => new { c.ArabicName, c.TaxNumber })
                .FirstOrDefaultAsync();
            
            if (companyDetails == null) throw new Exception("Company Details not found");
            
            var systemSetting = await dbContext.Set<Domains.Entities.SystemSetting>()
                .AsNoTracking()
                .Select(s => new { s.TimeZone })
                .FirstOrDefaultAsync();

            var timeZone = systemSetting?.TimeZone;
            
            var invoice = await dbContext.Set<SaleInvoice>()
                .AsNoTracking()
                .IgnoreAutoIncludes()
                .Include(i => i.CustomerProfile)
                    .ThenInclude(cp => cp.Departments)
                .Include(i => i.SaleItems)
                    .ThenInclude(s => s.MainSalesClass)
                        .ThenInclude(m => m.MeasurementUnit)
                .Include(i => i.ReceiptConnections)
                .FirstOrDefaultAsync(i => i.Id == id);
            
            if (invoice == null)
            {
                LogService.LogInformation($"[SaleInvoiceService] ❌ Invoice with id={id} NOT FOUND in database");
                return null;
            }
            
            LogService.LogInformation($"[SaleInvoiceService] ✅ Invoice found - Id={invoice.Id}, InvoiceNo={invoice.InvoiceNo}, NetPrice={invoice.NetPrice}, CustomerId={invoice.CustomerProfileId}");
            
            // On-the-fly calculation for Payment status (to ensure old invoices show correctly without DB migration)
            RecalculateInvoicePaymentStatus(invoice);

            invoice.GeneratedQR = invoice.GeneratedQR ?? StandardInvoiceXmlGenerator
                .GenerateZatcaQrCode(
                    companyDetails.ArabicName,
                    companyDetails.TaxNumber,
                    GetTotalAmount(invoice),
                    GetTaxAmount(invoice),
                    TimeZoneUtils.ApplyTimeZone(timeZone, invoice.IssueDate).ToString("yyyy-MM-dd'T'HH:mm:ss")
                    );
            return invoice;
        }

        public async Task<SaleInvoiceDto?> GetCopyTemplateAsync(int id)
        {
            // جلب الفاتورة مباشرة بدون حساب QR أو CompanyDetails (مش محتاجينهم للنسخ)
            var invoicesQuery = await _repositoriesFactory
                .GetRepository<SaleInvoice>().Filter(i => i.Id == id);
            var invoice = await invoicesQuery.Data
                .Include(i => i.CustomerProfile)
                    .ThenInclude(cp => cp.Departments)
                .Include(i => i.SaleItems)
                    .ThenInclude(s => s.MainSalesClass)
                        .ThenInclude(m => m.MeasurementUnit)
                .FirstOrDefaultAsync();

            if (invoice == null) return null;

            var dto = _mapper.Map<SaleInvoiceDto>(invoice);

            dto.Id = 0;
            dto.InvoiceNo = null;
            dto.IssueDate = DateTime.UtcNow;
            dto.DueDate = DateTime.UtcNow;
            dto.TransactionId = null;
            dto.ConvertedToInvoiceId = null;
            dto.OriginalQuotationNo = null;
            dto.ReferenceNumber = null;
            dto.IsQuotation = false;
            dto.IsCleared = false;
            dto.InvoiceHash = null;
            dto.GeneratedQR = null;
            dto.RefundStatus = "none";
            dto.RefundedAmount = 0;
            dto.PaidAmount = 0;
            dto.RemainingAmount = dto.NetPrice;
            dto.PaymentStatus = InvoicePaymentStatus.Unpaid;
            dto.Attachments = new List<AttachmentResponseDto>();

            if (dto.SaleItems != null)
            {
                foreach (var item in dto.SaleItems)
                {
                    item.Id = null;
                    item.SaleInvoiceId = null;
                    item.ReturnedQuantity = 0;
                }
            }

            return dto;
        }

        public async Task<SaleInvoice?> GetByReservationIdAsync(Guid reservationId)
        {
            var reservationRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Reservation.Reservation>();
            var reservationQuery = await reservationRepo.Filter(r => r.Id == reservationId && r.SaleInvoiceId !=null);

            var saleInvoiceId = reservationQuery.Data.Select(r => r.SaleInvoiceId).FirstOrDefault();
         
            if (saleInvoiceId == null)
                return null;
            var invoiceRepo = _repositoriesFactory.GetRepository<SaleInvoice>();

            var invoiceQuery = await invoiceRepo.Filter(i => i.Id == saleInvoiceId.Value);

            return await invoiceQuery.Data.FirstOrDefaultAsync();
        }

        public async Task<List<SaleInvoice>> GetByReservationIdsAsync(List<Guid> reservationIds)
        {
            if(reservationIds == null || !reservationIds.Any())
            {
                return new List<SaleInvoice>();
            }
            var reservationRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Reservation.Reservation>();
            var reservationsQuery = await reservationRepo.Filter(r => reservationIds.Contains(r.Id));
            var reservations = reservationsQuery.Data?.ToList();

            if (reservations == null || !reservations.Any())
                return new List<SaleInvoice>();

            var invoiceIds = reservations.Where(r => r.SaleInvoiceId.HasValue)
                .Select(r => r.SaleInvoiceId.Value)
                .Distinct()
                .ToList();

            if (!invoiceIds.Any())
                return new List<SaleInvoice>();

            var invoiceRepo = _repositoriesFactory.GetRepository<SaleInvoice>();
            var invoiceQuery = await invoiceRepo.Filter(i => invoiceIds.Contains(i.Id));

            return invoiceQuery.Data?.ToList() ?? new List<SaleInvoice>();
        }
        public async Task<SaleInvoice> ConvertQuotationToInvoiceAsync(int quotationId)
        {
            const int maxRetries = 10;
            int retryCount = 0;
            
            // Get the original quotation
            var quotationRepo = _repositoriesFactory.GetRepository<SaleInvoice>();
            var quotationQuery = await quotationRepo.Filter(q => q.Id == quotationId && q.IsQuotation == true);
            var quotation = await quotationQuery.Data.FirstOrDefaultAsync();
            
            if (quotation == null)
            {
                throw new Exception("Quotation not found");
            }
            
            if (quotation.ConvertedToInvoiceId != null)
            {
                throw new Exception("This quotation has already been converted to an invoice");
            }
            
            LogService.LogInformation($"Converting quotation {quotation.InvoiceNo} (ID: {quotationId}) to invoice");
            
            while (retryCount < maxRetries)
            {
                try
                {
                    // Create a new invoice from the quotation
                    var newInvoice = new SaleInvoice
                    {
                        CustomerProfileId = quotation.CustomerProfileId,
                        Currency = quotation.Currency,
                        Description = quotation.Description,
                        IssueDate = quotation.IssueDate,
                        DueDate = quotation.DueDate,
                        Tax = quotation.Tax,
                        TaxAmount = quotation.TaxAmount,
                        Discount = quotation.Discount,
                        DiscountAmount = quotation.DiscountAmount,
                        TotalPrice = quotation.TotalPrice,
                        NetPrice = quotation.NetPrice,
                        IsQuotation = false, // This is now an invoice
                        IsFrontal = false,
                        IsCleared = false
                    };
                    
                    // Copy sale items
                    foreach (var item in quotation.SaleItems)
                    {
                        newInvoice.SaleItems.Add(new SaleItem
                        {
                            ItemNo = item.ItemNo,
                            Quantity = item.Quantity,
                            Price = item.Price,
                            NoOfNights = item.NoOfNights,
                            MainSalesClassId = item.MainSalesClassId,
                            Name = item.Name,
                            Tax = item.Tax,
                            TaxAmount = item.TaxAmount,
                            Discount = item.Discount,
                            DiscountAmount = item.DiscountAmount,
                            TotalPrice = item.TotalPrice,
                            NetPrice = item.NetPrice
                        });
                    }
                    
                    // Generate new invoice number
                    var lastInvoiceNo = await GetLastInvoiceNo();
                    LogService.LogInformation($"Last invoice number: {lastInvoiceNo}");
                    int nextNumber = 1;
                    
                    if (!string.IsNullOrEmpty(lastInvoiceNo))
                    {
                        var numberPart = lastInvoiceNo.Substring("Inv-".Length);
                        if (int.TryParse(numberPart, out int lastNumber))
                        {
                            nextNumber = lastNumber + 1;
                        }
                    }
                    
                    if (retryCount > 0)
                    {
                        nextNumber += retryCount;
                    }
                    
                    newInvoice.InvoiceNo = $"Inv-{nextNumber}";
                    
                    LogService.LogInformation($"Generated invoice number: {newInvoice.InvoiceNo}, Attempt: {retryCount + 1}");
                    
                    // Save the new invoice
                    var resultQuery = await _repositoriesFactory.GetRepository<SaleInvoice>().Add(newInvoice);
                    var result = resultQuery.Data;
                    
                    if (result == null)
                    {
                        throw new Exception($"Failed to create the invoice - {resultQuery.Message}");
                    }
                    
                    // Update the quotation to mark it as converted
                    quotation.ConvertedToInvoiceId = result.Id;
                    await quotationRepo.Update(quotation);

                    await ProcessInvoiceAfterCreation(result, new SaleInvoicePostDto());

                    LogService.LogInformation($"Quotation {quotation.InvoiceNo} successfully converted to invoice {result.InvoiceNo} (ID: {result.Id})");
                    
                    return result;
                }
                catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        LogService.LogError(ex);
                        throw new Exception($"Failed to convert quotation to invoice after {maxRetries} attempts due to duplicate invoice number. Please try again.");
                    }
                    await Task.Delay(new Random().Next(50, 200));
                }
                catch (Exception ex)
                {
                    LogService.LogError(ex);
                    throw;
                }
            }
            
            throw new Exception("Failed to convert quotation to invoice after maximum retries.");
        }

        public async Task<SaleInvoice> AddAsync(SaleInvoicePostDto invoiceDto)
        {
            const int maxRetries = 10;
            int retryCount = 0;
            
            LogService.LogInformation($"AddAsync called - IsQuotation: {invoiceDto.IsQuotation}, CustomerName: {invoiceDto.CustomerProfileName}");
            
            while (retryCount < maxRetries)
            {
                var salesInvoiceRepository = _repositoriesFactory.GetRepository<SaleInvoice>(true);
                try
                {
                    if (invoiceDto.CustomerProfileId == null)
                    {
                        bool isResInvoice = invoiceDto.SaleInvoiceType == SmartAndPro.ERP.Domains.Enums.SaleInvoiceType.ComeFromReservation;
                        var customerRepository = _repositoriesFactory.GetRepository<CustomerProfile>();
                        
                        if (isResInvoice)
                        {
                            var customerName = invoiceDto.CustomerProfileName?.Trim();
                            if (string.IsNullOrEmpty(customerName))
                            {
                                throw new Exception("عفواً، اسم شركة الحجز غير متوفر. يرجى التأكد من ربط الحجز بشركة صحيحة.");
                            }

                            // مسار فواتير الحجوزات (النوع 2)
                            var existingCustomerQuery = await customerRepository.Filter(c => 
                                (c.ArabicName == customerName || c.EnglishName == customerName) &&
                                c.IsReservationCompany == true);
                            
                            var existingCustomer = existingCustomerQuery.Data?.FirstOrDefault();
                            
                            if (existingCustomer != null)
                            {
                                invoiceDto.CustomerProfileId = existingCustomer.Id;
                            }
                            else
                            {
                                throw new Exception("عفواً، شركة الحجز غير موجودة، يرجى التأكد من ربط الحجز بشركة صحيحة.");
                            }
                        }
                        else
                        {
                            var customerName = string.IsNullOrWhiteSpace(invoiceDto.CustomerProfileName)
                                ? "Walk-in Customer"
                                : invoiceDto.CustomerProfileName.Trim();

                            // مسار الفواتير العادية (النوع 0 و 1)
                            var existingCustomerQuery = await customerRepository.Filter(c => 
                                c.ArabicName == customerName || c.EnglishName == customerName);
                            
                            var existingCustomer = existingCustomerQuery.Data?.FirstOrDefault();
                            
                            if (existingCustomer != null)
                            {
                                invoiceDto.CustomerProfileId = existingCustomer.Id;
                            }
                            else
                            {
                                // إنشاء عميل جديد للفواتير العادية
                                var newCustomerProfile = new CustomerProfilePostDto
                                    {
                                        EnglishName = customerName,
                                        ArabicName = customerName,
                                        ContactNumber = invoiceDto.CustomerPhone,
                                        TaxNumber = invoiceDto.CustomerTaxNumber,
                                    };
                                GuidHelper.NormalizeGuids(newCustomerProfile);
                                var addResult = await _serviceBaseFactory.GetService<CustomerProfileService>().AddAsync(newCustomerProfile);
                                if (addResult.ErrorCode != ErrorCodes.Success)
                                    throw new Exception($"Failed to create Customer Profile: {addResult.Message}");
                                invoiceDto.CustomerProfileId = addResult.Data.Id;
                            }
                        }
                    }
                    
                    var invoice = _mapper.Map<SaleInvoice>(invoiceDto);
                    
                    LogService.LogInformation($"Invoice mapped - IsQuotation: {invoice.IsQuotation}, IsFrontal: {invoice.IsFrontal}");

                    if (invoice.IsFrontal != true)
                    {
                        if (invoice.IsQuotation == true)
                        {
                            // Generate quotation number (Q-)
                            var lastQuotationNo = await GetLastQuotationNo();
                            LogService.LogInformation($"Last quotation number: {lastQuotationNo}");
                            int nextNumber = 1;

                            if (!string.IsNullOrEmpty(lastQuotationNo))
                            {
                                var numberPart = lastQuotationNo.Substring("Q-".Length);
                                if (int.TryParse(numberPart, out int lastNumber))
                                {
                                    nextNumber = lastNumber + 1;
                                }
                            }

                            // Add retry count to ensure uniqueness even if multiple requests get the same lastQuotationNo
                            if (retryCount > 0)
                            {
                                nextNumber += retryCount;
                            }

                            invoice.InvoiceNo = $"Q-{nextNumber}";
                        }
                        else
                        {
                            // Generate invoice number (Inv-)
                            // Always get the latest invoice number to avoid duplicates in concurrent scenarios
                            var lastInvoiceNo = await GetLastInvoiceNo();
                            LogService.LogInformation($"Last invoice number: {lastInvoiceNo}");
                            int nextNumber = 1;

                            if (!string.IsNullOrEmpty(lastInvoiceNo))
                            {
                                var numberPart = lastInvoiceNo.Substring("Inv-".Length);
                                if (int.TryParse(numberPart, out int lastNumber))
                                {
                                    nextNumber = lastNumber + 1;
                                }
                            }

                            // Add retry count to ensure uniqueness even if multiple requests get the same lastInvoiceNo
                            if (retryCount > 0)
                            {
                                nextNumber += retryCount;
                            }

                            invoice.InvoiceNo = $"Inv-{nextNumber}";
                        }
                    }
                    
                    invoice.ReferenceNumber = string.IsNullOrWhiteSpace(invoice.ReferenceNumber) ? null : invoice.ReferenceNumber.Trim();

                    if (!string.IsNullOrWhiteSpace(invoice.ReferenceNumber))
                    {
                        var referenceExists = await salesInvoiceRepository.Filter(i => i.ReferenceNumber == invoice.ReferenceNumber);
                        if (await referenceExists.Data.AnyAsync())
                        {
                            throw new BusinessRuleViolationException(new List<BusinessRuleError> 
                            { 
                                new BusinessRuleError($"رقم المرجع {invoice.ReferenceNumber} موجود بالفعل، الرجاء إدخال رقم مرجع غير مكرر.", "ReferenceNumber") 
                            });
                        }
                    }

                    LogService.LogInformation($"Generated invoice number: {invoice.InvoiceNo}, Attempt: {retryCount + 1}");

                    await salesInvoiceRepository.BeginTransactionAsync();
                    var dbContext = salesInvoiceRepository.GetDbContext();

                    if (!invoice.IsQuotation)
                    {
                        await ValidateAndDeductInventoryAsync(invoiceDto, dbContext);
                    }

                    var resultQuery = await salesInvoiceRepository.Add(invoice);
                    
                    var result = resultQuery.Data;
                    if (result == null) 
                    {
                        throw new Exception($"Failed to create the invoice - {resultQuery.Message}");
                    }

                    await salesInvoiceRepository.CommitTransactionAsync();

                    LogService.LogInformation($"[SaleInvoiceService] ✅ Transaction committed successfully - Invoice Id={result.Id}, InvoiceNo={result.InvoiceNo}, NetPrice={result.NetPrice}, CustomerId={result.CustomerProfileId}, IsQuotation={result.IsQuotation}");

                    if (result.IsQuotation)
                    {
                        LogService.LogInformation("Quotation created, skipping notifications.");
                        if (result.ProjectId.HasValue)
                        {
                            var projectPriceQuote = new SmartAndPro.ERP.Domains.Entities.Projects.ProjectPriceQuote
                            {
                                Id = Guid.NewGuid(),
                                ProjectId = result.ProjectId.Value,
                                QuoteDate = result.IssueDate,
                                QuoteNumber = result.InvoiceNo,
                                QuoteAmount = result.NetPrice,
                                IsAccepted = false,
                                SaleInvoiceId = result.Id
                            };
                            var quoteRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Projects.ProjectPriceQuote>();
                            await quoteRepo.Add(projectPriceQuote);
                        }
                        return result;
                    }

                    // Success - break out of retry loop
                    return await ProcessInvoiceAfterCreation(result, invoiceDto);
                }
                catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
                {
                    await salesInvoiceRepository.RollbackTransactionAsync();

                    if (sqlEx.Message.Contains("IX_SaleInvoice_ReferenceNumber") || sqlEx.Message.Contains("ReferenceNumber"))
                    {
                        throw new BusinessRuleViolationException(new List<BusinessRuleError>
                        {
                            new BusinessRuleError($"رقم المرجع {invoiceDto.ReferenceNumber} موجود بالفعل، الرجاء إدخال رقم مرجع غير مكرر.", "ReferenceNumber")
                        });
                    }

                    // Duplicate key error (2601) or unique constraint violation (2627) - retry with a new invoice number
                    retryCount++;
                    LogService.LogInformation($"[SaleInvoiceService] ⚠️  Duplicate key error ({sqlEx.Number}): {sqlEx.Message} (attempt {retryCount}/{maxRetries}), retrying with new number...");
                    if (retryCount >= maxRetries)
                    {
                        LogService.LogError(ex.ToString());
                        throw new Exception($"Failed to create invoice after {maxRetries} attempts due to duplicate invoice number (DB detail: {sqlEx.Message}). Please try again.");
                    }
                    // Wait a small random amount to reduce collision probability
                    await Task.Delay(new Random().Next(50, 200));
                    // Continue to retry - the invoice will be recreated with a new number in the next iteration
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    await salesInvoiceRepository.RollbackTransactionAsync();
                    LogService.LogError($"[SaleInvoiceService] ❌ DbUpdateConcurrencyException - Transaction rolled back: {ex.Message}");
                    throw BuildInsufficientInventoryException("حدث تعارض أثناء تحديث المخزون. الرجاء المحاولة مرة أخرى.");
                }
                catch (BusinessRuleViolationException ex)
                {
                    await salesInvoiceRepository.RollbackTransactionAsync();
                    LogService.LogError($"[SaleInvoiceService] ❌ BusinessRuleViolationException - Transaction rolled back: {ex.Message}");
                    throw;
                }
                catch (Exception ex)
                {
                    // Other exceptions - don't retry
                    await salesInvoiceRepository.RollbackTransactionAsync();
                    LogService.LogError($"[SaleInvoiceService] ❌ Unexpected exception in AddAsync - Transaction rolled back: {ex.Message}\n{ex.StackTrace}");
                    throw;
                }
            }
            
            throw new Exception("Failed to create invoice after maximum retries.");
        }

        private async Task ValidateAndDeductInventoryAsync(SaleInvoicePostDto invoiceDto, DbContext dbContext)
        {
            var systemSetting = await dbContext.Set<SmartAndPro.ERP.Domains.Entities.SystemSetting>()
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (systemSetting?.IsLinkedToInventory != true)
            {
                return;
            }

            if (!invoiceDto.StoreDetailsId.HasValue)
            {
                throw BuildInsufficientInventoryException("يجب تحديد المخزن عند تفعيل ربط فاتورة المبيعات بالمخزن.");
            }

            var requiredQuantities = invoiceDto.SaleItems
                .GroupBy(item => item.MainSalesClassId)
                .Select(group => new
                {
                    MainSalesClassId = group.Key,
                    Quantity = group.Sum(item => item.Quantity)
                })
                .ToList();

            foreach (var required in requiredQuantities)
            {
                var productUnit = await dbContext.Set<ProductUnit>()
                    .FirstOrDefaultAsync(x =>
                        x.MainSalesClassId == required.MainSalesClassId &&
                        x.StoreDetailsId == invoiceDto.StoreDetailsId);

                if (productUnit == null)
                {
                    throw BuildInsufficientInventoryException("الصنف غير موجود في المخزن المحدد.");
                }

                var availableQuantity = productUnit.Quantity ?? 0m;
                if (availableQuantity < required.Quantity)
                {
                    throw BuildInsufficientInventoryException("الكمية المتاحة في المخزن لا تكفي لإتمام العملية.");
                }

                productUnit.Quantity = availableQuantity - required.Quantity;
            }
        }

        private static BusinessRuleViolationException BuildInsufficientInventoryException(string message)
        {
            var errors = new List<BusinessRuleError>
            {
                new BusinessRuleError(
                    "Inventory.Insufficient",
                    message,
                    ErrorCodes.InsufficientInventory.ToString())
            };

            return new BusinessRuleViolationException(errors);
        }
        
        private async Task<SaleInvoice> ProcessInvoiceAfterCreation(SaleInvoice result, SaleInvoicePostDto invoiceDto)
        {
            var baseUrl = _appSettings.FrontBaseUrl;

            if (invoiceDto.ReservationIds != null && invoiceDto.ReservationIds.Any())
            {
                var reservationRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Reservation.Reservation>();
                var dbContext = reservationRepo.GetDbContext();

                try
                {
                    // Use Raw SQL for maximum performance and to avoid any EF tracking/timeout issues
                    var ids = string.Join(",", invoiceDto.ReservationIds.Select(id => $"'{id}'"));
                    if (!string.IsNullOrEmpty(ids))
                    {
                        await dbContext.Database.ExecuteSqlRawAsync(
                            $"UPDATE [Reservation] SET SaleInvoiceId = {result.Id} WHERE Id IN ({ids})");
                    }
                }
                catch (Exception ex)
                {
                    LogService.LogError($"Failed to update reservations with invoice ID: {ex}");
                }

                try
                {
                    var notificationDto = new NotificationPostDto
                    {
                        Title = "فاتورة حجز جديدة",
                        Content = $"تم إصدار فاتورة حجز للاطلاع عليها",
                        Link = $"{baseUrl}/invoices/reservation-details/{result.Id}"
                    };
                    var permissions = new List<string> { "Reservation.Read" };

                    var userIds = new List<Guid> { };

                    try
                    {
                        await _notificationService.SendInSystemNotificationAsync(notificationDto, userIds, permissions);
                    }
                    catch (Exception ex)
                    {
                        LogService.LogError(ex);
                    }

                    //try
                    //{
                    //    await _notificationService.SendEmailNotificationAsync(notificationDto, userIds, permissions);
                    //}
                    //catch (Exception ex)
                    //{
                    //    LogService.LogError(ex);
                    //}

                    return result;
                }
                catch (Exception ex)
                {
                    LogService.LogError(ex);
                }

            }

            if (result.IsFrontal == true)
            {
                var makerFrontalResult = await MakeFrontalAsync(result.Id);
                if (makerFrontalResult != null)
                {
                    result.InvoiceNo = makerFrontalResult.InvoiceNo;
                }
                else
                {
                    throw new Exception("Failed to make the invoice frontal");
                }
            }
       
            try
            {
                var notificationDto = new NotificationPostDto
                {
                    Title = "فاتورة مبيعات جديدة",
                    Content = $"تم إصدار فاتورة مبيعات للاطلاع عليها",
                    Link = $"{baseUrl}/invoices/{result.Id}"
                };
                var permissions = new List<string> { "SalesInvoice.Read" };

                var userIds = new List<Guid> { };

                try
                {
                    await _notificationService.SendInSystemNotificationAsync(notificationDto, userIds, permissions);
                }
                catch (Exception ex)
                {
                    LogService.LogError(ex);
                }

                //try
                //{
                //    await _notificationService.SendEmailNotificationAsync(notificationDto, userIds, permissions);
                //}
                //catch (Exception ex)
                //{
                //    LogService.LogError(ex);
                //}

                return result;
            }
            catch (Exception ex) 
                { 
                    LogService.LogError(ex); 
                }
            return result;

        }
        public async Task<int> UpdateTransaction(int? invoiceId, Guid? transactionId)
        {
            try
            {
                var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();
                var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();
                var invoiceRepository = _repositoriesFactory.GetRepository<SaleInvoice>();
                var transactionEntryRepository = _repositoriesFactory.GetRepository<TransactionEntry>();

                var invoiceQuery = await invoiceRepository.Filter(x => x.Id == invoiceId);
                var invoice = invoiceQuery.Data.FirstOrDefault();
                if (invoice == null) return 0;

                if (invoice.CustomerProfileId == null)
                {
                    var normalizedCustomerName = invoice.CustomerProfile?.ArabicName
                                                 ?? invoice.CustomerProfile?.EnglishName
                                                 ?? "Walk-in Customer";

                    var newCustomerProfile = new CustomerProfilePostDto
                    {
                        EnglishName = normalizedCustomerName,
                        ArabicName = normalizedCustomerName,
                        TaxNumber = invoice.CustomerProfile?.TaxNumber
                    };

                    GuidHelper.NormalizeGuids(newCustomerProfile);

                    var addResult = await _serviceBaseFactory
                        .GetService<CustomerProfileService>()
                        .AddAsync(newCustomerProfile);

                    if (addResult.ErrorCode != ErrorCodes.Success)
                        throw new Exception("Failed to create Customer Profile");

                    invoice.CustomerProfileId = addResult.Data.Id;
                    await invoiceRepository.Update(invoice);
                }

                var transactionQuery = await transactionRepository.Filter(x=>x.Id == transactionId);
                var transaction = transactionQuery.Data.FirstOrDefault();
                if (transaction == null) return 0;

                foreach (var entry in transaction.TransactionEntries)
                {
                    entry.Transaction = null;
                    entry.MainAccount = null;
                    var deletionResult = await transactionEntryRepository.Delete(entry);
                }

                var customer = invoice.CustomerProfile;
                
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

                // Retrieve Transaction Type for invoice transaction
                var transactionTypeQuery = await _repositoriesFactory.GetRepository<TransactionType>()
                    .Filter(tt => tt.ArabicName.Contains("قيد فاتورة"));
                var transactionTypeId = transactionTypeQuery.Data.FirstOrDefault()?.Id;
                if (transactionTypeId == null)
                {
                    return 0; // Transaction Type not found
                }

                // Create transaction entries based on invoice items
                var transactionEntries = new List<TransactionEntry>();
                
                var itemsTotal = invoice.SaleItems
                    .Sum(item => item.Price * item.Quantity * item.NoOfNights);
                var taxableTotal = invoice.SaleItems
                    .Where(item => !item.MainSalesClass.IsTaxExempted)
                    .Sum(item => item.Price * item.Quantity * item.NoOfNights);
                var discount = invoice.DiscountAmount;
                var netSaleTotal = itemsTotal - discount;
                decimal taxableDiscount = discount * (taxableTotal / itemsTotal);;
                var netTaxableAmount = Math.Max(taxableTotal - taxableDiscount, 0m);
                var saleTaxAmount = netTaxableAmount * 0.15m;
                var saleTotalAmount = netSaleTotal + saleTaxAmount;
                
                // 1. Entry for Sale Tax (Credit)
                var taxEntry = new TransactionEntry
                {
                    TransactionId = transaction.Id,
                    MainAccountId = saleTaxAccount.Id,
                    CreditAmount = saleTaxAmount, 
                    DebitAmount = 0,
                    Description = $"Invoice #{invoice.InvoiceNo} - Sale Tax",
                    CreatedAt = DateTime.UtcNow
                };
                transactionEntries.Add(taxEntry);

                // 2. Entry for supplier MainAccount (Debit for supplier)
                var supplierEntry = new TransactionEntry
                {
                    TransactionId = transaction.Id,

                    MainAccountId = customer.MainAccountId.Value,
                    DebitAmount = saleTotalAmount,
                    CreditAmount = 0,
                    Description = $"Invoice #{invoice.InvoiceNo} (Customer)",
                    CreatedAt = DateTime.UtcNow
                };
                transactionEntries.Add(supplierEntry);


                var itemsEntry = new TransactionEntry
                {
                    TransactionId = transaction.Id,

                    MainAccountId = mainVaultAccount.Id,
                    DebitAmount = 0,
                    CreditAmount = netSaleTotal,
                    Description = $"Invoice #{invoice.InvoiceNo}",
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

        
        
        public async Task<int> CreateTransaction(int id,bool? isFrontal = false)
        {
            try
            {
                var invoiceRepository = _repositoriesFactory.GetRepository<SaleInvoice>(true);
                var customerRepository = _repositoriesFactory.GetRepository<CustomerProfile>();
                var transactionRepository = _repositoriesFactory.GetRepository<Transaction>(true);
                var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();

                var invoiceQuery = await invoiceRepository.Filter(i => i.Id == id);
                var invoice = invoiceQuery.Data.FirstOrDefault();
                if (invoice == null)
                {
                    return 0; // Invoice not found
                }

                // If a transaction already exists for this invoice in dbo.Transaction, sync TransactionId & return 1
                var existingTxRepo = _repositoriesFactory.GetRepository<Transaction>();
                var existingTx = (await existingTxRepo.Filter(t => t.SaleInvoiceId == id)).Data.FirstOrDefault();
                if (existingTx != null)
                {
                    if (invoice.TransactionId != existingTx.Id)
                    {
                        invoice.TransactionId = existingTx.Id;
                        invoice.SaleItems = null;
                        invoice.SalesCreditNotes = null;
                        invoice.Attachments = null;
                        invoice.CustomerProfile = null;
                        await invoiceRepository.Update(invoice);
                    }
                    return 1;
                }

                var customerQuery = await customerRepository.Filter(s => s.Id == invoice.CustomerProfileId);
                var customer = customerQuery.Data.FirstOrDefault();
                if (customer == null || customer.MainAccountId == null)
                {
                    return 0; 
                }

                // Retrieve the "Main Vault" MainAccount
                var mainVaultAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "الخزينة الرئيسية" || ma.ArabicName.Contains("الخزينة"));
                var mainVaultAccount = mainVaultAccountQuery.Data.FirstOrDefault();
                if (mainVaultAccount == null)
                {
                    return 0; // Main Vault account not found
                }

                // Retrieve the "Sale Tax" MainAccount
                var saleTaxAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName.Contains("ضريبة القيمة المضافة") && ma.ArabicName.Contains("المبيعات"));
                var saleTaxAccount = saleTaxAccountQuery.Data.FirstOrDefault();
                if (saleTaxAccount == null)
                {
                    return 0; // Sale Tax account not found
                }

                // Retrieve Transaction Type for invoice transaction
                var transactionTypeQuery = await _repositoriesFactory.GetRepository<TransactionType>()
                    .Filter(tt => tt.ArabicName.Contains("قيد فاتورة") || tt.ArabicName.Contains("فاتورة"));
                var transactionTypeId = transactionTypeQuery.Data.FirstOrDefault()?.Id;
                if (transactionTypeId == null)
                {
                    return 0; // Transaction Type not found
                }

                // Create transaction entries based on invoice items
                var transactionEntries = new List<TransactionEntry>();

                var itemsTotal = invoice.SaleItems
                    .Sum(item => item.Price * item.Quantity * item.NoOfNights);
                var taxableTotal = invoice.SaleItems
                    .Where(item => !item.MainSalesClass.IsTaxExempted)
                    .Sum(item => item.Price * item.Quantity * item.NoOfNights);
                var discount = invoice.DiscountAmount;
                var netSaleTotal = itemsTotal - discount;
                decimal taxableDiscount = discount * (taxableTotal / itemsTotal);;
                var netTaxableAmount = Math.Max(taxableTotal - taxableDiscount, 0m);
                var saleTaxAmount = netTaxableAmount * 0.15m;
                var saleTotalAmount = netSaleTotal + saleTaxAmount;
                var taxEntry = new TransactionEntry
                {
                    MainAccountId = saleTaxAccount.Id,
                    CreditAmount = saleTaxAmount,
                    DebitAmount = 0,
                    Description = $"Invoice #{invoice.InvoiceNo} - Sale Tax",
                    CreatedAt = DateTime.UtcNow
                };
                transactionEntries.Add(taxEntry);

                // 2. Entry for supplier MainAccount (Debit for supplier)
                var supplierEntry = new TransactionEntry
                {
                    MainAccountId = customer.MainAccountId.Value,
                    DebitAmount = saleTotalAmount,
                    CreditAmount = 0,
                    Description = $"Invoice #{invoice.InvoiceNo} (Customer)",
                    CreatedAt = DateTime.UtcNow
                };
                transactionEntries.Add(supplierEntry);
                var itemsEntry = new TransactionEntry
                {
                    MainAccountId = mainVaultAccount.Id,
                    DebitAmount = 0,
                    CreditAmount = netSaleTotal,
                    Description = $"Invoice #{invoice.InvoiceNo}",
                    CreatedAt = DateTime.UtcNow
                };
                transactionEntries.Add(itemsEntry);

                // Create the transaction
                var transaction = new Transaction
                {
                    Description = $"قيد فاتورة مبيعات رقم {invoice.InvoiceNo}",
                    TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    TransactionTypeId = transactionTypeId.Value,
                    TransactionEntries = transactionEntries,
                    SaleInvoiceId = invoice.Id,
                    IsFrontal = isFrontal ?? false
                };

                // Enforce rules and save transaction
                var transactionEnforcer = new TransactionEnforcer();
                var enforcerResult = transactionEnforcer.EnforceRules(transaction);
                if (enforcerResult.IsSuccess)
                {
                    await transactionRepository.BeginTransactionAsync();
                    try
                    {
                        var result = await transactionRepository.Add(transaction);
                        if (result.Data == null)
                        {
                            await transactionRepository.RollbackTransactionAsync();
                            return 0;
                        }

                        invoice.SaleItems = null;
                        invoice.TransactionId = result.Data.Id;
                        invoice.SalesCreditNotes = null;
                        invoice.Attachments = null;
                        invoice.CustomerProfile = null;

                        var updateResult = await invoiceRepository.Update(invoice);
                        if (updateResult.ErrorCode != ErrorCodes.Success)
                        {
                            await transactionRepository.RollbackTransactionAsync();
                            return 0;
                        }

                        await transactionRepository.CommitTransactionAsync();
                        return 1;
                    }
                    catch
                    {
                        await transactionRepository.RollbackTransactionAsync();
                        throw;
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

        //public async Task<int> CreateBulkTransactions(List<int> ids)
        //{
        //    if (ids == null || !ids.Any()) return 0;

        //    int successCount = 0;
        //    foreach (var id in ids)
        //    {
        //        try
        //        {
        //            var result = await CreateTransaction(id);
        //            if (result == 1)
        //            {
        //                successCount++;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LogService.LogError(ex);
        //        }
        //    }

        //    return successCount > 0 ? 1 : 0;
        //}

        public async Task<int> CreateBulkTransactionsOptimized(List<int> ids)
        {
            if (ids == null || !ids.Any()) 
            {
                LogService.LogInformation("Bulk Transaction Optimized: No IDs provided.");
                return 1; // Return 1 even if empty to avoid frontend error message
            }

            try
            {
                var invoiceRepository = _repositoriesFactory.GetRepository<SaleInvoice>(true);
                var customerRepository = _repositoriesFactory.GetRepository<CustomerProfile>();
                var transactionRepository = _repositoriesFactory.GetRepository<Transaction>(true);
                var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();

                // 1. Pre-fetch common accounts and transaction type
                // Using Contains for safer matching in case of whitespace differences
                var mainVaultAccount = (await mainAccountRepository.Filter(ma => ma.ArabicName.Contains("الخزينة الرئيسية"))).Data.FirstOrDefault();
                var saleTaxAccount = (await mainAccountRepository.Filter(ma => ma.ArabicName.Contains("ضريبة القيمة المضافة"))).Data.FirstOrDefault();
                var transactionType = (await _repositoriesFactory.GetRepository<TransactionType>().Filter(tt => tt.ArabicName.Contains("قيد فاتورة"))).Data.FirstOrDefault();

                if (mainVaultAccount == null || saleTaxAccount == null || transactionType == null)
                {
                    string missing = $"{(mainVaultAccount == null ? "Main Vault," : "")}{(saleTaxAccount == null ? "Sale Tax," : "")}{(transactionType == null ? "Transaction Type," : "")}";
                    LogService.LogError(new Exception($"Bulk Transaction Optimized: Required data missing: {missing}"));
                    return 0;
                }

                // 2. Fetch all invoices with items
                var invoicesQuery = await invoiceRepository.Filter(i => ids.Contains(i.Id));
                var invoices = await invoicesQuery.Data
                    .Include(i => i.SaleItems)
                        .ThenInclude(si => si.MainSalesClass)
                    .ToListAsync();

                if (!invoices.Any())
                {
                    LogService.LogInformation("Bulk Transaction Optimized: No matching invoices found in database.");
                    return 1;
                }

                // 2.1 Calculate next Transaction ReferenceNumber manually to avoid duplicates in bulk
                // Optimized: Fetch only the TOP 1 latest reference number
                // Improved: Use MAX(TRY_CAST) to get the true numeric maximum, bypassing string sorting issues
                // Using both TRN- and trn- to handle case sensitivity
                var dbContext = invoiceRepository.GetDbContext();
                var lastRefValue = await dbContext.Database.SqlQueryRaw<string>(
                    @"SELECT CAST(MAX(TRY_CAST(SUBSTRING([ReferenceNumber], 5, 20) AS INT)) AS NVARCHAR) AS [Value] 
                      FROM [Transaction] 
                      WHERE [ReferenceNumber] LIKE 'TRN-%' OR [ReferenceNumber] LIKE 'trn-%'")
                    .FirstOrDefaultAsync();

                int maxTrnNumber = 0;
                if (!string.IsNullOrEmpty(lastRefValue))
                {
                    int.TryParse(lastRefValue, out maxTrnNumber);
                }

                LogService.LogInformation($"Bulk Transaction Optimized: Max ReferenceNumber numeric part found: {lastRefValue}. Starting from: {maxTrnNumber}");

                // 3. Start DB Transaction
                await transactionRepository.BeginTransactionAsync();
                int successCount = 0;
                int processedCount = 0;

                try
                {
                    foreach (var invoice in invoices)
                    {
                        processedCount++;
                        if (invoice.TransactionId != null) 
                        {
                            LogService.LogInformation($"Bulk Transaction Optimized: Skipping Invoice #{invoice.InvoiceNo} - Already has TransactionId.");
                            continue;
                        }

                        var customer = (await customerRepository.Filter(s => s.Id == invoice.CustomerProfileId)).Data.FirstOrDefault();
                        if (customer == null || customer.MainAccountId == null)
                        {
                            LogService.LogInformation($"Bulk Transaction Optimized: Skipping Invoice #{invoice.InvoiceNo} - Customer missing or not linked to MainAccount.");
                            continue;
                        }

                        // Prepare entries
                        var transactionEntries = new List<TransactionEntry>();
                        var itemsTotal = invoice.SaleItems.Sum(item => item.Price * item.Quantity * item.NoOfNights);
                        var taxableTotal = invoice.SaleItems.Where(item => !item.MainSalesClass.IsTaxExempted).Sum(item => item.Price * item.Quantity * item.NoOfNights);
                        var discount = invoice.DiscountAmount;
                        var netSaleTotal = itemsTotal - discount;
                        decimal taxableDiscount = itemsTotal > 0 ? discount * (taxableTotal / itemsTotal) : 0;
                        var netTaxableAmount = Math.Max(taxableTotal - taxableDiscount, 0m);
                        var saleTaxAmount = netTaxableAmount * 0.15m;
                        var saleTotalAmount = netSaleTotal + saleTaxAmount;

                        transactionEntries.Add(new TransactionEntry
                        {
                            MainAccountId = saleTaxAccount.Id,
                            CreditAmount = saleTaxAmount,
                            DebitAmount = 0,
                            Description = $"Invoice #{invoice.InvoiceNo} - Sale Tax",
                            CreatedAt = DateTime.UtcNow
                        });

                        transactionEntries.Add(new TransactionEntry
                        {
                            MainAccountId = customer.MainAccountId.Value,
                            DebitAmount = saleTotalAmount,
                            CreditAmount = 0,
                            Description = $"Invoice #{invoice.InvoiceNo} (Customer)",
                            CreatedAt = DateTime.UtcNow
                        });

                        transactionEntries.Add(new TransactionEntry
                        {
                            MainAccountId = mainVaultAccount.Id,
                            DebitAmount = 0,
                            CreditAmount = netSaleTotal,
                            Description = $"Invoice #{invoice.InvoiceNo}",
                            CreatedAt = DateTime.UtcNow
                        });

                        maxTrnNumber++;
                        var trnRefManual = $"TRN-{maxTrnNumber:D5}";
                        
                        var transaction = new Transaction
                        {
                            ReferenceNumber = trnRefManual,
                            Description = $"قيد فاتورة مبيعات رقم {invoice.InvoiceNo}",
                            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                            TransactionTypeId = transactionType.Id,
                            TransactionEntries = transactionEntries,
                            SaleInvoiceId = invoice.Id,
                            IsFrontal = invoice.IsFrontal ?? false
                        };

                        var transactionEnforcer = new TransactionEnforcer();
                        var enforcementResult = transactionEnforcer.EnforceRules(transaction);
                        if (enforcementResult.IsSuccess)
                        {
                            var result = await transactionRepository.Add(transaction);
                            if (result.Data != null)
                            {
                                invoice.SaleItems = null;
                                invoice.TransactionId = result.Data.Id;
                                invoice.SalesCreditNotes = null;
                                invoice.Attachments = null;
                                invoice.CustomerProfile = null;
                                await invoiceRepository.Update(invoice);
                                successCount++;
                            }
                        }
                        else
                        {
                            var firstError = enforcementResult.Errors.FirstOrDefault()?.ErrorMessage ?? "Unknown rule violation";
                            LogService.LogInformation($"Bulk Transaction Optimized: Skipping Invoice #{invoice.InvoiceNo} - Rule enforcement failed: {firstError}");
                        }
                    }

                    // Always success if we processed items, even if none were new/eligible
                    // But if we tried to add and failed, we return 0
                    await transactionRepository.CommitTransactionAsync();
                    LogService.LogInformation($"Bulk Transaction Optimized: Finished. Success: {successCount}/{processedCount}");
                    return 1;
                }
                catch (Exception ex)
                {
                    await transactionRepository.RollbackTransactionAsync();
                    LogService.LogError(ex);
                    return 0;
                }
            }
            catch (Exception e)
            {
                LogService.LogError(e);
                return 0;
            }
        }

        public async Task<int> EditAsync(SaleInvoice invoice, List<SaleItem> oldsaleItems)
        {
            if (invoice.IsCleared == true)
            {
                throw new BusinessRuleViolationException(new List<BusinessRuleError> 
                { 
                    new BusinessRuleError("لا يمكن تعديل الفاتورة؛ تم إرسالها واعتمادها لدى هيئة الزكاة والضريبة والجمارك.", "IsCleared") 
                });
            }

            try
            {
                var saleInvoiceRepository = _repositoriesFactory.GetRepository<SaleInvoice>();
                
                if (!string.IsNullOrWhiteSpace(invoice.ReferenceNumber))
                {
                    var referenceExists = await saleInvoiceRepository.Filter(i => i.ReferenceNumber == invoice.ReferenceNumber && i.Id != invoice.Id);
                    if (await referenceExists.Data.AnyAsync())
                    {
                        throw new BusinessRuleViolationException(new List<BusinessRuleError> 
                        { 
                            new BusinessRuleError($"رقم المرجع {invoice.ReferenceNumber} موجود بالفعل، الرجاء إدخال رقم مرجع غير مكرر.", "ReferenceNumber") 
                        });
                    }
                }

                var saleItemRepository = _repositoriesFactory.GetRepository<SaleItem>();
                var saleItems = invoice.SaleItems;
                invoice.SaleItems = null;

                // Delete items that exist in oldsaleItems but not in new saleItems
                var itemsToDelete = oldsaleItems
                    .Where(oldItem => !saleItems.Any(newItem => newItem.Id == oldItem.Id))
                    .ToList();

                foreach (var itemToDelete in itemsToDelete)
                {
                    itemToDelete.MainSalesClass = null;
                    await saleItemRepository.Delete(itemToDelete);
                }

                // Update or add new sale items
                foreach (var saleItem in saleItems)
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

                // Nullify unrelated properties for the invoice before updating
                invoice.CustomerProfile= null;
                invoice.SalesCreditNotes = null;
                invoice.Attachments = null;
                invoice.SaleItems = null;
                var response = await saleInvoiceRepository.Update(invoice);

                if (response.Data != null && response.Data.IsQuotation)
                {
                    var quoteRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Projects.ProjectPriceQuote>();
                    var existingQuoteQuery = await quoteRepo.Filter(q => q.SaleInvoiceId == response.Data.Id);
                    var existingQuote = existingQuoteQuery.Data.FirstOrDefault();

                    if (response.Data.ProjectId.HasValue)
                    {
                        if (existingQuote != null)
                        {
                            existingQuote.ProjectId = response.Data.ProjectId.Value;
                            existingQuote.QuoteDate = response.Data.IssueDate;
                            existingQuote.QuoteNumber = response.Data.InvoiceNo;
                            existingQuote.QuoteAmount = response.Data.NetPrice;
                            await quoteRepo.Update(existingQuote);
                        }
                        else
                        {
                            var projectPriceQuote = new SmartAndPro.ERP.Domains.Entities.Projects.ProjectPriceQuote
                            {
                                Id = Guid.NewGuid(),
                                ProjectId = response.Data.ProjectId.Value,
                                QuoteDate = response.Data.IssueDate,
                                QuoteNumber = response.Data.InvoiceNo,
                                QuoteAmount = response.Data.NetPrice,
                                IsAccepted = false,
                                SaleInvoiceId = response.Data.Id
                            };
                            await quoteRepo.Add(projectPriceQuote);
                        }
                    }
                    else if (existingQuote != null)
                    {
                        await quoteRepo.Delete(existingQuote);
                    }
                }

                // Update the transaction if present
                if (invoice.TransactionId != null)
                {
                    var transactionUpdateResult = await UpdateTransaction(invoice.Id, invoice.TransactionId);
                }

                var result = response.Data;
                return result != null ? 1 : 0;
            }
            catch (BusinessRuleViolationException)
            {
                throw;
            }
            catch (Exception e)
            {
                LogService.LogError(e);
            }

            return 0;
        }
        public async Task<int> RemoveAsync(SaleInvoice invoice)
        {
            if (invoice.IsCleared == true)
            {
                throw new BusinessRuleViolationException(new List<BusinessRuleError> 
                { 
                    new BusinessRuleError("لا يمكن حذف الفاتورة؛ تم إرسالها واعتمادها لدى هيئة الزكاة والضريبة والجمارك.", "IsCleared") 
                });
            }

            if (invoice.IsQuotation)
            {
                var quoteRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Projects.ProjectPriceQuote>();
                var existingQuoteQuery = await quoteRepo.Filter(q => q.SaleInvoiceId == invoice.Id);
                var existingQuote = existingQuoteQuery.Data.FirstOrDefault();
                if (existingQuote != null)
                {
                    var projectId = existingQuote.ProjectId;
                    await quoteRepo.Delete(existingQuote);

                    // Also clear Quote fields on the Project table if this was the active quote
                    var projectRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Projects.Project>();
                    var projectResult = await projectRepo.Filter(p => p.Id == projectId);
                    var project = await projectResult.Data.FirstOrDefaultAsync();
                    if (project != null && project.QuoteNumber == invoice.InvoiceNo)
                    {
                        project.QuoteNumber = null;
                        project.QuoteDate = null;
                        await projectRepo.Update(project);
                    }
                }
            }

            invoice.CustomerProfile = null;
            var saleItems = invoice.SaleItems;
            invoice.SaleItems = null;
            foreach (var saleItem in saleItems)
            {
                saleItem.MainSalesClass = null;
                saleItem.SaleInvoice = null;
                await _repositoriesFactory.GetRepository<SaleItem>().Delete(saleItem);
            }
            if (invoice.TransactionId != null)
            {
                // Delete the transaction
                var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();
                var transactionQuery = await transactionRepository.Filter(x => x.Id == invoice.TransactionId);
                var transaction = transactionQuery.Data.FirstOrDefault();
                if (transaction != null)
                {
                    var transactionEntryRepository = _repositoriesFactory.GetRepository<TransactionEntry>();
                    var transactionEntries = transaction.TransactionEntries;
                    transaction.TransactionEntries = null;
                    foreach (var entry in transactionEntries)
                    {
                        entry.Transaction = null;
                        entry.MainAccount = null;
                        var deletionResult = await transactionEntryRepository.Delete(entry);
                    }
                    await transactionRepository.Delete(transaction);
                }
            }
            var resultQuery = await _repositoriesFactory.GetRepository<SaleInvoice>().Delete(invoice);
            var result = resultQuery.Data;
            return result != null ? 1 : 0;
        }

        public OperationResponse<List<SalesInvoiceDetailsResponse>> GetSalesInvoiceDetails(GetSalesInvoiceDetailsRequest getSalesInvoiceDetails)
        {
            try
            {
                var salesInvoiceDetails =
                    _salesInvoicesRepository.GetSalesInvoiceDetails(_mapper.Map<GetSalesInvoiceDetailsInputModel>(getSalesInvoiceDetails));

                var purchaseInvoiceDetailsMapper = _mapper.Map<List<SalesInvoiceDetailsResponse>>(salesInvoiceDetails.ToList());

                return new OperationResponse<List<SalesInvoiceDetailsResponse>>(ErrorCodes.Success, data: purchaseInvoiceDetailsMapper, message: string.Empty);

            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return new OperationResponse<List<SalesInvoiceDetailsResponse>>(ErrorCodes.GeneralFail, null, message: ErrorCodes.GeneralFail.ToString());

            }
        }

        public byte[] ExportSalesInvoiceDetails(GetSalesInvoiceDetailsRequest getSalesInvoiceDetails)
        {
            try
            {
                var salesInvoiceDetails =
                    _salesInvoicesRepository.GetSalesInvoiceDetails(_mapper.Map<GetSalesInvoiceDetailsInputModel>(getSalesInvoiceDetails));

                var purchaseInvoiceDetailsMapper = _mapper.Map<List<SalesInvoiceDetailsResponse>>(salesInvoiceDetails.ToList());

                var excelData = ExcelExporter.ExportSalesInvoiceToExcel(purchaseInvoiceDetailsMapper);

                return excelData;
            }
            catch (Exception ex)
            {
                LogService.LogError(ex.Message);
                return null;
            }
        }

        public async Task<byte[]> GetDocument(int invoiceId, SmartAndPro.ERP.Domains.Enums.InvoicePrintType printType = SmartAndPro.ERP.Domains.Enums.InvoicePrintType.A4Portrait, string? sellerName = null, Print1ServerOptions? print1 = null, Print2ServerOptions? print2 = null)
        {
            var repository = _repositoriesFactory.GetRepository<SaleInvoice>();
            var invoiceQuery = await repository.Filter(i => i.Id == invoiceId);
            var queryable = invoiceQuery.Data;
            if (queryable == null) return Array.Empty<byte>();
            var invoice = await queryable
                .Include(i => i.SaleItems)
                    .ThenInclude(si => si.MainSalesClass)
                        .ThenInclude(msc => msc.MeasurementUnit)
                .Include(i => i.CustomerProfile)
                .Include(i => i.Project)
                    .ThenInclude(p => p.CompanyDetails)
                .FirstOrDefaultAsync();
            if (invoice == null) return Array.Empty<byte>();

            CompanyDetails companyDetails = invoice.Project?.CompanyDetails;
            if (companyDetails == null && invoice.Project?.CompanyDetailsId != null)
            {
                var companyRepo = _repositoriesFactory.GetRepository<CompanyDetails>();
                var compRes = await companyRepo.Filter(c => c.Id == invoice.Project.CompanyDetailsId.Value);
                companyDetails = compRes.Data?.FirstOrDefault();
            }

            if (companyDetails == null)
            {
                var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
                companyDetails = companyDetailsQuery.Data?.FirstOrDefault();
            }

            if (companyDetails == null) return Array.Empty<byte>();
            await FetchCompanyImagesAsync(companyDetails);

            var pdfTemplateConfigQuery = await _repositoriesFactory.GetRepository<Domains.Entities.PdfTemplateConfiguration>().GetAll();
            var pdfTemplateConfig = pdfTemplateConfigQuery.Data?.FirstOrDefault();

            var systemSettingQuery = await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().GetAll();
            var systemSetting = systemSettingQuery.Data?.FirstOrDefault();
            var timeZone = systemSetting?.TimeZone;
            var resolvedPrint1 = print1 != null && string.IsNullOrWhiteSpace(print1.TimeZone)
                ? print1 with { TimeZone = timeZone }
                : print1;
            var resolvedPrint2 = print2 != null && string.IsNullOrWhiteSpace(print2.TimeZone)
                ? print2 with { TimeZone = timeZone }
                : (print2 ?? (timeZone != null ? new Print2ServerOptions(TimeZone: timeZone) : null));

            if (!string.IsNullOrWhiteSpace(companyDetails.TaxNumber))
            {
                invoice.GeneratedQR = invoice.GeneratedQR ?? StandardInvoiceXmlGenerator
                    .GenerateZatcaQrCode(
                        companyDetails.ArabicName,
                        companyDetails.TaxNumber,
                        GetTotalAmount(invoice),
                        GetTaxAmount(invoice),
                        TimeZoneUtils.ApplyTimeZone(timeZone, invoice.IssueDate).ToString("yyyy-MM-dd'T'HH:mm:ss")
                    );
            }

            // Get original quotation number if this invoice was created from a quotation
            string originalQuotationNo = null;
            var quotationQuery = await _repositoriesFactory.GetRepository<SaleInvoice>()
                .Filter(q => q.IsQuotation == true && q.ConvertedToInvoiceId == invoice.Id);
            var quotation = await quotationQuery.Data.FirstOrDefaultAsync();
            if (quotation != null)
            {
                originalQuotationNo = quotation.InvoiceNo;
            }

            // Get cashier payment details if available
            SmartAndPro.ERP.Domains.Entities.Cashier.CashierPayment? cashierPayment = null;
            try
            {
                var paymentRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Cashier.CashierPayment>();
                var paymentQuery = await paymentRepo.Filter(p => p.InvoiceId == invoice.Id);
                cashierPayment = await paymentQuery.Data.OrderByDescending(p => p.Id).FirstOrDefaultAsync();
                if (cashierPayment != null)
                {
                    if (cashierPayment.PaidAmount > 0)
                    {
                        invoice.PaidAmount = cashierPayment.PaidAmount;
                    }
                    if (cashierPayment.ChangeAmount > 0)
                    {
                        invoice.RemainingAmount = cashierPayment.ChangeAmount;
                    }
                }
            }
            catch { }

            if (string.IsNullOrWhiteSpace(sellerName))
            {
                if (cashierPayment != null && cashierPayment.CreatedBy != Guid.Empty)
                {
                    sellerName = cashierPayment.CreatedBy.ToString();
                }
                else if (!string.IsNullOrWhiteSpace(invoice.CreatedBy))
                {
                    sellerName = invoice.CreatedBy;
                }
            }

            if (!string.IsNullOrWhiteSpace(sellerName) && Guid.TryParse(sellerName, out var userGuid))
            {
                try
                {
                    var userRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Auth.Security.User>();
                    var u = (await userRepo.Filter(x => x.Id == userGuid)).Data?.FirstOrDefault();
                    if (u != null)
                    {
                        var fullName = $"{u.FirstName} {u.LastName}".Trim();
                        sellerName = !string.IsNullOrWhiteSpace(fullName) ? fullName : u.UserName;
                    }
                }
                catch { }
            }

            if (resolvedPrint1 != null && string.IsNullOrWhiteSpace(resolvedPrint1.SellerName) && !string.IsNullOrWhiteSpace(sellerName))
            {
                resolvedPrint1 = resolvedPrint1 with { SellerName = sellerName };
            }

            // Select document format based on print type
            return BuildInvoiceDocumentPdf(invoice, companyDetails, pdfTemplateConfig, originalQuotationNo, printType, sellerName, resolvedPrint1, resolvedPrint2, cashierPayment);
        }

        /// <summary>
        /// Shared document-selection helper for single and batch (ZIP) generation so the
        /// print-type mapping used by <see cref="GetDocument(int, InvoicePrintType, string?)"/>
        /// and <see cref="GetDocuments(int[], InvoicePrintType)"/> cannot diverge.
        /// </summary>
        private byte[] BuildInvoiceDocumentPdf(
            SaleInvoice invoice,
            CompanyDetails companyDetails,
            Domains.Entities.PdfTemplateConfiguration? pdfTemplateConfig,
            string? originalQuotationNo,
            InvoicePrintType printType,
            string? sellerName = null,
            Print1ServerOptions? print1 = null,
            Print2ServerOptions? print2 = null,
            SmartAndPro.ERP.Domains.Entities.Cashier.CashierPayment? cashierPayment = null)
        {
            return printType switch
            {
                InvoicePrintType.Thermal80mm => new ThermalInvoiceDocument80mm(invoice, companyDetails, sellerName, cashierPayment).GeneratePdf(),
                InvoicePrintType.Thermal58mm => new ThermalInvoiceDocument58mm(invoice, companyDetails, sellerName).GeneratePdf(),
                InvoicePrintType.A4Landscape => new SalesInvoiceDocument(
                    invoice, companyDetails, pdfTemplateConfig, originalQuotationNo, true, sellerName,
                    showSignatures: true,
                    showTerms: print2?.ShowTerms ?? false,
                    showBanks: print2?.ShowBanks ?? false,
                    uncheckedPolicyIds: print2?.UncheckedPolicyIds,
                    uncheckedBankIds: print2?.UncheckedBankIds,
                    timeZone: print2?.TimeZone ?? print1?.TimeZone).GeneratePdf(),
                InvoicePrintType.A4Portrait => new SalesInvoiceDocument(
                    invoice, companyDetails, pdfTemplateConfig, originalQuotationNo, false, sellerName,
                    showSignatures: true,
                    showTerms: print2?.ShowTerms ?? false,
                    showBanks: print2?.ShowBanks ?? false,
                    uncheckedPolicyIds: print2?.UncheckedPolicyIds,
                    uncheckedBankIds: print2?.UncheckedBankIds,
                    timeZone: print2?.TimeZone ?? print1?.TimeZone).GeneratePdf(),
                InvoicePrintType.Format1Server => new SalesInvoicePrint1ServerDocument(
                    invoice,
                    companyDetails,
                    language: print1?.Language ?? "ar",
                    sellerName: print1?.SellerName ?? sellerName,
                    showStamp: print1?.ShowStamp ?? true,
                    showCompanySignature: print1?.ShowCompanySignature ?? true,
                    showCustomerSignature: print1?.ShowCustomerSignature ?? true,
                    uncheckedPolicyIds: print1?.UncheckedPolicyIds,
                    uncheckedBankIds: print1?.UncheckedBankIds,
                    timeZone: print1?.TimeZone ?? print2?.TimeZone,
                    orientation: print1?.Orientation).GeneratePdf(),
                _ => new SalesInvoiceDocument(
                    invoice, companyDetails, pdfTemplateConfig, originalQuotationNo, false, sellerName,
                    showSignatures: true,
                    showTerms: print2?.ShowTerms ?? false,
                    showBanks: print2?.ShowBanks ?? false,
                    uncheckedPolicyIds: print2?.UncheckedPolicyIds,
                    uncheckedBankIds: print2?.UncheckedBankIds,
                    timeZone: print2?.TimeZone ?? print1?.TimeZone).GeneratePdf()
            };
        }

        public async Task<byte[]> GetDocument(DateOnly startDate, DateOnly endDate, string language = "ar")
        {
            try
            {
                var request = new GetSalesInvoiceDetailsRequest
                {
                    FromDate = startDate.ToDateTime(TimeOnly.MinValue),
                    ToDate = endDate.ToDateTime(TimeOnly.MaxValue)
                };

                var salesInvoiceDetailsResponse = GetSalesInvoiceDetails(request);
                if (salesInvoiceDetailsResponse?.Data == null || !salesInvoiceDetailsResponse.Data.Any())
                {
                    LogService.LogError($"Sales invoice details data is null or empty for startDate: {startDate}, endDate: {endDate}");
                    throw new InvalidOperationException("Sales invoice details data not found");
                }

                var companyDetailsQuery = await _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement.CompanyDetails>().GetAll();
                var companyDetails = companyDetailsQuery.Data?.FirstOrDefault();
                if (companyDetails == null)
                {
                    LogService.LogError("CompanyDetails not found, using default values");
                    companyDetails = new SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement.CompanyDetails
                    {
                        ArabicName = "الشركة",
                        EnglishName = "Company",
                        TaxNumber = "000000000000000",
                        Email = "info@company.com"
                    };
                }

                var pdfTemplateConfigQuery = await _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.PdfTemplateConfiguration>().GetAll();
                var pdfTemplateConfig = pdfTemplateConfigQuery.Data?.FirstOrDefault();

                var document = new SalesInvoiceReportDocument(
                    salesInvoiceDetailsResponse.Data,
                    companyDetails,
                    pdfTemplateConfig,
                    startDate,
                    endDate,
                    language
                );

                var pdfBytes = document.GeneratePdf();
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    LogService.LogError("Generated PDF is null or empty");
                    throw new InvalidOperationException("Failed to generate PDF document");
                }

                return pdfBytes;
            }
            catch (Exception e)
            {
                LogService.LogError($"GetDocument error details: {e.Message}\nStackTrace: {e.StackTrace}");
                if (e.InnerException != null)
                {
                    LogService.LogError($"Inner exception: {e.InnerException.Message}\nInner StackTrace: {e.InnerException.StackTrace}");
                }
                throw;
            }
        }

        public async Task<byte[]> GetSummaryDocument(DateOnly startDate, DateOnly endDate, string language = "ar")
        {
            try
            {
                var request = new GetSalesInvoiceDetailsRequest
                {
                    FromDate = startDate.ToDateTime(TimeOnly.MinValue),
                    ToDate = endDate.ToDateTime(TimeOnly.MaxValue)
                };

                var salesInvoiceDetailsResponse = GetSalesInvoiceDetails(request);
                if (salesInvoiceDetailsResponse?.Data == null || !salesInvoiceDetailsResponse.Data.Any())
                {
                    LogService.LogError($"Sales invoice details data is null or empty for startDate: {startDate}, endDate: {endDate}");
                    throw new InvalidOperationException("Sales invoice details data not found");
                }

                var companyDetailsQuery = await _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement.CompanyDetails>().GetAll();
                var companyDetails = companyDetailsQuery.Data?.FirstOrDefault();
                if (companyDetails == null)
                {
                    LogService.LogError("CompanyDetails not found, using default values");
                    companyDetails = new SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement.CompanyDetails
                    {
                        ArabicName = "الشركة",
                        EnglishName = "Company",
                        TaxNumber = "000000000000000",
                        Email = "info@company.com"
                    };
                }

                var pdfTemplateConfigQuery = await _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.PdfTemplateConfiguration>().GetAll();
                var pdfTemplateConfig = pdfTemplateConfigQuery.Data?.FirstOrDefault();

                var document = new SalesInvoiceSummaryReportDocument(
                    salesInvoiceDetailsResponse.Data,
                    companyDetails,
                    pdfTemplateConfig,
                    startDate,
                    endDate,
                    language
                );

                var pdfBytes = document.GeneratePdf();
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    LogService.LogError("Generated summary PDF is null or empty");
                    throw new InvalidOperationException("Failed to generate summary PDF document");
                }

                return pdfBytes;
            }
            catch (Exception e)
            {
                LogService.LogError($"GetSummaryDocument error details: {e.Message}\nStackTrace: {e.StackTrace}");
                if (e.InnerException != null)
                {
                    LogService.LogError($"Inner exception: {e.InnerException.Message}\nInner StackTrace: {e.InnerException.StackTrace}");
                }
                throw;
            }
        }

        public async Task<byte[][]> GetDocuments(int[] invoiceIds, InvoicePrintType printType = InvoicePrintType.A4Portrait, Print1ServerOptions? print1 = null, Print2ServerOptions? print2 = null)
        {
            var repository = _repositoriesFactory.GetRepository<SaleInvoice>();
            var invoicesQuery = await repository.Filter(i => invoiceIds.Contains(i.Id));
            var data = invoicesQuery.Data;
            if (data == null) return Array.Empty<byte[]>();
            var invoices = await data
                .Include(i => i.SaleItems)
                    .ThenInclude(si => si.MainSalesClass)
                        .ThenInclude(msc => msc.MeasurementUnit)
                .Include(i => i.CustomerProfile)
                .ToListAsync();
            if (invoices == null || !invoices.Any()) return Array.Empty<byte[]>();

            var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
            var companyDetails = companyDetailsQuery.Data?.FirstOrDefault();
            if (companyDetails == null) return Array.Empty<byte[]>();
            await FetchCompanyImagesAsync(companyDetails);


            var systemSettingQuery = await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().GetAll();
            var systemSetting = systemSettingQuery.Data?.FirstOrDefault();
            var timeZone = systemSetting?.TimeZone ?? "Asia/Riyadh";
            var resolvedPrint1 = print1 != null && string.IsNullOrWhiteSpace(print1.TimeZone)
                ? print1 with { TimeZone = timeZone }
                : print1;

            var pdfTemplateConfigQuery = await _repositoriesFactory.GetRepository<Domains.Entities.PdfTemplateConfiguration>().GetAll();
            var pdfTemplateConfig = pdfTemplateConfigQuery.Data?.FirstOrDefault();

            // Get quotation numbers for all invoices
            var invoiceIdList = invoices.Select(i => i.Id).ToList();
            var quotationsQuery = await _repositoriesFactory.GetRepository<SaleInvoice>()
                .Filter(q => q.IsQuotation == true && q.ConvertedToInvoiceId != null && invoiceIdList.Contains(q.ConvertedToInvoiceId.Value));
            var quotations = await quotationsQuery.Data.Select(q => new { q.ConvertedToInvoiceId, q.InvoiceNo }).ToListAsync();
            var quotationDict = quotations.ToDictionary(q => q.ConvertedToInvoiceId.Value, q => q.InvoiceNo);

            var results = new List<byte[]>();
            foreach (var invoice in invoices)
            {
                if (!string.IsNullOrWhiteSpace(companyDetails.TaxNumber))
                {
                    invoice.GeneratedQR = invoice.GeneratedQR ?? StandardInvoiceXmlGenerator
                        .GenerateZatcaQrCode(
                            companyDetails.ArabicName,
                            companyDetails.TaxNumber,
                            GetTotalAmount(invoice),
                            GetTaxAmount(invoice),
                            TimeZoneUtils.ApplyTimeZone(timeZone, invoice.IssueDate).ToString("yyyy-MM-dd'T'HH:mm:ss")
                        );
                }
                
                string originalQuotationNo = quotationDict.ContainsKey(invoice.Id) ? quotationDict[invoice.Id] : null;
                results.Add(BuildInvoiceDocumentPdf(invoice, companyDetails, pdfTemplateConfig, originalQuotationNo, printType, resolvedPrint1?.SellerName, resolvedPrint1, print2));
            }
            return results.ToArray();
        }

        public async Task<byte[][]> GetAllDocuments()
        {
            var repository = _repositoriesFactory.GetRepository<SaleInvoice>();
            var allQuery = await repository.GetAll();
            var data = allQuery.Data;
            if (data == null) return Array.Empty<byte[]>();
            var invoices = await data
                .Where(i => i.IsQuotation == false)
                .Include(i => i.SaleItems)
                    .ThenInclude(si => si.MainSalesClass)
                        .ThenInclude(msc => msc.MeasurementUnit)
                .Include(i => i.CustomerProfile)
                .ToListAsync();
            if (!invoices.Any()) return Array.Empty<byte[]>();

            var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
            var companyDetails = companyDetailsQuery.Data?.FirstOrDefault();
            if (companyDetails == null) return Array.Empty<byte[]>();
            await FetchCompanyImagesAsync(companyDetails);

            var systemSettingQuery = await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().GetAll();
            var systemSetting = systemSettingQuery.Data?.FirstOrDefault();
            var timeZone = systemSetting?.TimeZone;

            var pdfTemplateConfigQuery = await _repositoriesFactory.GetRepository<Domains.Entities.PdfTemplateConfiguration>().GetAll();
            var pdfTemplateConfig = pdfTemplateConfigQuery.Data?.FirstOrDefault();

            // Get quotation numbers for all invoices
            var invoiceIdList = invoices.Select(i => i.Id).ToList();
            var quotationsQuery = await _repositoriesFactory.GetRepository<SaleInvoice>()
                .Filter(q => q.IsQuotation == true && q.ConvertedToInvoiceId != null && invoiceIdList.Contains(q.ConvertedToInvoiceId.Value));
            var quotations = await quotationsQuery.Data.Select(q => new { q.ConvertedToInvoiceId, q.InvoiceNo }).ToListAsync();
            var quotationDict = quotations.ToDictionary(q => q.ConvertedToInvoiceId.Value, q => q.InvoiceNo);

            var results = new List<byte[]>();
            foreach (var invoice in invoices)
            {
                if (!string.IsNullOrWhiteSpace(companyDetails.TaxNumber))
                {
                    invoice.GeneratedQR = invoice.GeneratedQR ?? StandardInvoiceXmlGenerator
                        .GenerateZatcaQrCode(
                            companyDetails.ArabicName,
                            companyDetails.TaxNumber,
                            GetTotalAmount(invoice),
                            GetTaxAmount(invoice),
                            TimeZoneUtils.ApplyTimeZone(timeZone, invoice.IssueDate).ToString("yyyy-MM-dd'T'HH:mm:ss")
                        );
                }
                
                string originalQuotationNo = quotationDict.ContainsKey(invoice.Id) ? quotationDict[invoice.Id] : null;
                results.Add(new SalesInvoiceDocument(invoice, companyDetails, pdfTemplateConfig, originalQuotationNo, timeZone: timeZone).GeneratePdf());
            }
            return results.ToArray();
        }

        public async Task<byte[]> GetMergedDocument(int[] invoiceIds, InvoicePrintType printType = InvoicePrintType.A4Portrait, Print1ServerOptions? print1 = null, Print2ServerOptions? print2 = null)
        {
            if (printType is InvoicePrintType.Thermal80mm or InvoicePrintType.Thermal58mm)
                throw new ArgumentException("Thermal print types are not supported for merged documents", nameof(printType));

            var repository = _repositoriesFactory.GetRepository<SaleInvoice>();
            var invoicesQuery = await repository.Filter(i => invoiceIds.Contains(i.Id));
            var data = invoicesQuery.Data;
            if (data == null) return Array.Empty<byte>();
            var invoices = await data
                .Include(i => i.SaleItems)
                    .ThenInclude(si => si.MainSalesClass)
                        .ThenInclude(msc => msc.MeasurementUnit)
                .Include(i => i.CustomerProfile)
                .ToListAsync();
            if (!invoices.Any()) return Array.Empty<byte>();

            var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
            var companyDetails = companyDetailsQuery.Data?.FirstOrDefault();
            if (companyDetails == null) return Array.Empty<byte>();
            await FetchCompanyImagesAsync(companyDetails);


            var systemSettingQuery = await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().GetAll();
            var systemSetting = systemSettingQuery.Data?.FirstOrDefault();
            var timeZone = systemSetting?.TimeZone;

            foreach (var invoice in invoices)
            {
                if (!string.IsNullOrWhiteSpace(companyDetails.TaxNumber))
                {
                    invoice.GeneratedQR = invoice.GeneratedQR ?? StandardInvoiceXmlGenerator
                        .GenerateZatcaQrCode(
                            companyDetails.ArabicName,
                            companyDetails.TaxNumber,
                            GetTotalAmount(invoice),
                            GetTaxAmount(invoice),
                            TimeZoneUtils.ApplyTimeZone(timeZone, invoice.IssueDate).ToString("yyyy-MM-dd'T'HH:mm:ss")
                        );
                }
            }

            var pdfTemplateConfigQuery = await _repositoriesFactory.GetRepository<Domains.Entities.PdfTemplateConfiguration>().GetAll();
            var pdfTemplateConfig = pdfTemplateConfigQuery.Data?.FirstOrDefault();
            
            // Get quotation numbers for all invoices
            var invoiceIdList = invoices.Select(i => i.Id).ToList();
            var quotationsQuery = await _repositoriesFactory.GetRepository<SaleInvoice>()
                .Filter(q => q.IsQuotation == true && q.ConvertedToInvoiceId != null && invoiceIdList.Contains(q.ConvertedToInvoiceId.Value));
            var quotations = await quotationsQuery.Data.Select(q => new { q.ConvertedToInvoiceId, q.InvoiceNo }).ToListAsync();
            var quotationDict = quotations.ToDictionary(q => q.ConvertedToInvoiceId.Value, q => q.InvoiceNo);

            if (printType == InvoicePrintType.Format1Server)
            {
                var resolvedPrint1 = print1 != null && string.IsNullOrWhiteSpace(print1.TimeZone)
                    ? print1 with { TimeZone = timeZone }
                    : print1;
                return new CombinedSalesInvoicesPrint1ServerDocument(
                    invoices,
                    companyDetails,
                    language: resolvedPrint1?.Language ?? "ar",
                    sellerName: resolvedPrint1?.SellerName,
                    showStamp: resolvedPrint1?.ShowStamp ?? true,
                    showCompanySignature: resolvedPrint1?.ShowCompanySignature ?? true,
                    showCustomerSignature: resolvedPrint1?.ShowCustomerSignature ?? true,
                    uncheckedPolicyIds: resolvedPrint1?.UncheckedPolicyIds,
                    uncheckedBankIds: resolvedPrint1?.UncheckedBankIds,
                    timeZone: resolvedPrint1?.TimeZone ?? "Asia/Riyadh",
                    orientation: resolvedPrint1?.Orientation).GeneratePdf();
            }

            var landscape = printType == InvoicePrintType.A4Landscape;
            var combined = new CombinedSalesInvoicesDocument(
                invoices,
                companyDetails,
                pdfTemplateConfig,
                quotationDict,
                showSignatures: true,
                landscape: landscape,
                showTerms: print2?.ShowTerms ?? false,
                showBanks: print2?.ShowBanks ?? false,
                uncheckedPolicyIds: print2?.UncheckedPolicyIds,
                uncheckedBankIds: print2?.UncheckedBankIds).GeneratePdf();
            return combined;
        }

        // VAT Report Implementation
        public OperationResponse<List<SalesVATDetailsResponse>> GetSalesVATDetails(GetSalesVATDetailsRequest getVATDetails)
        {
            try
            {
                var salesInvoiceDetails = _salesInvoicesRepository.GetSalesInvoiceDetails(_mapper.Map<GetSalesInvoiceDetailsInputModel>(getVATDetails));

                var vatDetailsMapper = salesInvoiceDetails
                    .SelectMany(invoice => invoice.ItemDetails.Select(item => new
                    {
                        invoice.InvoiceId,
                        invoice.InvoiceNo,
                        invoice.SupplierId,
                        invoice.SuppilerArabicName,
                        invoice.SuppilerEnglishName,
                        invoice.MainAccountId,
                        invoice.IssueDate,
                        item.ReferenceCode,
                        item.ItemArabicName,
                        item.ItemEnglishName,
                        item.Quantity,
                        item.Price,
                        item.IsTaxExempted,
                        item.TotalPrice
                    }))
                    .GroupBy(x => new
                    {
                        x.InvoiceId,
                        x.InvoiceNo,
                        x.SupplierId,
                        x.SuppilerArabicName,
                        x.SuppilerEnglishName,
                        x.MainAccountId,
                        x.IssueDate
                    })
                    .Select(g => new SalesVATDetailsResponse
                    {
                        InvoiceId = g.Key.InvoiceId,
                        InvoiceNo = g.Key.InvoiceNo,
                        SupplierId = g.Key.SupplierId,
                        SuppilerArabicName = g.Key.SuppilerArabicName,
                        SuppilerEnglishName = g.Key.SuppilerEnglishName,
                        MainAccountId = g.Key.MainAccountId,
                        IssueDate = g.Key.IssueDate,
                        ItemDetails = g.Select(item => new SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale.VATItemDetails
                        {
                            ReferenceCode = item.ReferenceCode,
                            ItemArabicName = item.ItemArabicName,
                            ItemEnglishName = item.ItemEnglishName,
                            Quantity = item.Quantity,
                            Price = item.Price,
                            IsTaxExempted = item.IsTaxExempted,
                            VATAmount = item.IsTaxExempted ? 0 : (item.TotalPrice * 0.15m) // 15% VAT
                        }).ToList()
                    }).ToList();

                return new OperationResponse<List<SalesVATDetailsResponse>>(ErrorCodes.Success, data: vatDetailsMapper, message: string.Empty);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return new OperationResponse<List<SalesVATDetailsResponse>>(ErrorCodes.GeneralFail, null, message: ErrorCodes.GeneralFail.ToString());
            }
        }

        public byte[] ExportSalesVATDetails(GetSalesVATDetailsRequest getVATDetails)
        {
            try
            {
                var vatDetailsResponse = GetSalesVATDetails(getVATDetails);
                if (vatDetailsResponse?.Data == null) return null;

                var excelData = ExcelExporter.ExportSalesVATToExcel(vatDetailsResponse.Data);
                return excelData;
            }
            catch (Exception ex)
            {
                LogService.LogError(ex.Message);
                return null;
            }
        }

        public async Task<byte[]> GetVATDocument(DateOnly startDate, DateOnly endDate, string language = "ar")
        {
            try
            {
                LogService.LogInformation($"GetVATDocument called with startDate: {startDate}, endDate: {endDate}, language: {language}");
                
                var request = new GetSalesVATDetailsRequest
                {
                    FromDate = startDate.ToDateTime(TimeOnly.MinValue),
                    ToDate = endDate.ToDateTime(TimeOnly.MaxValue)
                };

                LogService.LogInformation($"Calling GetSalesVATDetails with FromDate: {request.FromDate}, ToDate: {request.ToDate}");
                var vatDetailsResponse = GetSalesVATDetails(request);
                
                LogService.LogInformation($"GetSalesVATDetails returned. Success: {vatDetailsResponse?.ErrorCode == ErrorCodes.Success}, Data count: {vatDetailsResponse?.Data?.Count ?? 0}");
                
                if (vatDetailsResponse?.Data == null || !vatDetailsResponse.Data.Any())
                {
                    LogService.LogError($"Sales VAT details data is null or empty for startDate: {startDate}, endDate: {endDate}");
                    throw new InvalidOperationException("Sales VAT details data not found");
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

                LogService.LogInformation("Creating SalesVATReportDocument...");
                var document = new SalesVATReportDocument(
                    vatDetailsResponse.Data,
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
                    LogService.LogError("Generated VAT PDF is null or empty");
                    throw new InvalidOperationException("Failed to generate VAT PDF document");
                }

                LogService.LogInformation($"PDF generated successfully. Size: {pdfBytes.Length} bytes");
                return pdfBytes;
            }
            catch (Exception e)
            {
                LogService.LogError($"GetVATDocument error details: {e.Message}\nStackTrace: {e.StackTrace}");
                if (e.InnerException != null)
                {
                    LogService.LogError($"Inner exception: {e.InnerException.Message}\nInner StackTrace: {e.InnerException.StackTrace}");
                }
                throw;
            }
        }

        public async Task<object> GetNetSalesVATReport(DateOnly startDate, DateOnly endDate, string language = "ar")
        {
            try
            {
                // 1. Get sales invoice VAT totals using GetSalesVATDetails method
                var salesVATRequest = new GetSalesVATDetailsRequest
                {
                    FromDate = startDate.ToDateTime(TimeOnly.MinValue),
                    ToDate = endDate.ToDateTime(TimeOnly.MaxValue)
                };
                
                var salesVATResponse = GetSalesVATDetails(salesVATRequest);
                
                decimal totalQuantity = 0;
                decimal totalUnitPrice = 0;
                decimal totalInvoicePrice = 0;
                decimal totalInvoicePriceWithTax = 0;
                decimal totalSalesVAT = 0;
                
                if (salesVATResponse?.Data != null && salesVATResponse.Data.Any())
                {
                    foreach (var invoice in salesVATResponse.Data)
                    {
                        foreach (var item in invoice.ItemDetails)
                        {
                            totalQuantity += item.Quantity;
                            totalUnitPrice += item.Price;
                            totalInvoicePrice += (item.Quantity * item.Price);
                            decimal vatAmount = item.IsTaxExempted ? 0 : (item.Quantity * item.Price * 0.15m);
                            totalInvoicePriceWithTax += (item.Quantity * item.Price) + vatAmount;
                            totalSalesVAT += vatAmount;
                        }
                    }
                }
                
                // 2. Get sales credit notes VAT total and other totals
                decimal totalReturnsVAT = 0;
                decimal totalReturnsQuantity = 0;
                decimal totalReturnsUnitPrice = 0;
                decimal totalReturnsInvoicePrice = 0;
                decimal totalReturnsInvoicePriceWithTax = 0;
                
                try
                {
                    var creditNotesResponse = await _repositoriesFactory.GetRepository<SalesCreditNote>().GetAll();
                    
                    if (creditNotesResponse.ErrorCode == ErrorCodes.Success && creditNotesResponse.Data != null)
                    {
                        var creditNoteIds = creditNotesResponse.Data
                            .Where(cn => cn.IsActive && 
                                        cn.IssueDate >= startDate.ToDateTime(TimeOnly.MinValue) && 
                                        cn.IssueDate <= endDate.ToDateTime(TimeOnly.MaxValue))
                            .Select(cn => cn.Id)
                            .ToList();

                        if (creditNoteIds.Any())
                        {
                            var creditNotesItemsResponse = await _repositoriesFactory.GetRepository<SalesCreditNoteItem>().GetAll();
                            
                            if (creditNotesItemsResponse.ErrorCode == ErrorCodes.Success && creditNotesItemsResponse.Data != null)
                            {
                                var mainClassesResponse = await _repositoriesFactory.GetRepository<MainSalesClass>().GetAll();
                                
                                if (mainClassesResponse.ErrorCode == ErrorCodes.Success && mainClassesResponse.Data != null)
                                {
                                    var taxExemptedClassIds = mainClassesResponse.Data
                                        .Where(mc => mc.IsTaxExempted)
                                        .Select(mc => mc.Id)
                                        .ToHashSet();

                                    var returnItems = creditNotesItemsResponse.Data
                                        .Where(item => creditNoteIds.Contains(item.SalesCreditNoteId))
                                        .ToList();
                                    
                                    foreach (var item in returnItems)
                                    {
                                        totalReturnsQuantity += item.Quantity;
                                        totalReturnsUnitPrice += item.Price;
                                        totalReturnsInvoicePrice += (item.Quantity * item.Price);
                                        decimal vatAmount = taxExemptedClassIds.Contains(item.MainSalesClassId) ? 0 : (item.Quantity * item.Price * 0.15m);
                                        totalReturnsInvoicePriceWithTax += (item.Quantity * item.Price) + vatAmount;
                                        totalReturnsVAT += vatAmount;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogService.LogError($"Error calculating returns VAT: {ex.Message}");
                    // Continue with totalReturnsVAT = 0
                }

                // 3. Create summary object with separate sales, returns, and net VAT
                var reportData = new
                {
                    totalSalesVAT = totalSalesVAT,
                    totalReturnsVAT = totalReturnsVAT,
                    totalVAT = totalSalesVAT - totalReturnsVAT, // Net VAT = Sales VAT - Returns VAT
                    startDate = startDate.ToString("yyyy-MM-dd"),
                    endDate = endDate.ToString("yyyy-MM-dd")
                };

                LogService.LogInformation($"Net Sales VAT Report: SalesVAT={totalSalesVAT}, ReturnsVAT={totalReturnsVAT}, NetVAT={totalSalesVAT - totalReturnsVAT}");
                return reportData;
            }
            catch (Exception ex)
            {
                LogService.LogError($"Error in GetNetSalesVATReport: {ex.Message}");
                throw;
            }
        }

        public async Task<byte[]> GetNetSalesVATDocument(DateOnly startDate, DateOnly endDate, string language = "ar")
        {
            try
            {
                LogService.LogInformation($"GetNetSalesVATDocument called with startDate: {startDate}, endDate: {endDate}, language: {language}");

                // Get report data
                var reportData = await GetNetSalesVATReport(startDate, endDate);

                LogService.LogInformation("Fetching company details...");
                var companyQueryResponse = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
                
                if (companyQueryResponse.ErrorCode != ErrorCodes.Success || companyQueryResponse.Data == null || !companyQueryResponse.Data.Any())
                {
                    LogService.LogError("No active company found for Net Sales VAT document generation");
                    return Array.Empty<byte>();
                }

                var company = companyQueryResponse.Data.FirstOrDefault();

                LogService.LogInformation($"Company details found: {company.ArabicName ?? company.EnglishName}");
                LogService.LogInformation("Fetching PDF template config...");

                var pdfTemplateQueryResponse = await _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.PdfTemplateConfiguration>().GetAll();
                var pdfTemplateConfig = pdfTemplateQueryResponse.Data?.FirstOrDefault();

                LogService.LogInformation($"PDF template config found: {pdfTemplateConfig != null}");

                LogService.LogInformation($"Creating Net Sales VAT Report PDF Document...");
                var pdfDocument = new NetSalesVATReportDocument(
                    reportData,
                    company,
                    pdfTemplateConfig,
                    language
                );

                LogService.LogInformation($"Generating PDF bytes...");

                var pdfBytes = pdfDocument.GeneratePdf();
                
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    LogService.LogInformation("PDF generation returned null or empty array");
                    return Array.Empty<byte>();
                }

                LogService.LogInformation($"PDF generated successfully. Size: {pdfBytes.Length} bytes");
                return pdfBytes;
            }
            catch (Exception e)
            {
                LogService.LogError($"GetNetSalesVATDocument error details: {e.Message}\nStackTrace: {e.StackTrace}");
                if (e.InnerException != null)
                {
                    LogService.LogError($"Inner exception: {e.InnerException.Message}\nInner StackTrace: {e.InnerException.StackTrace}");
                }
                throw;
            }
        }

        public async Task<object> GetNetDueVATReport(DateOnly startDate, DateOnly endDate, string language = "ar")
        {
            try
            {
                LogService.LogInformation($"GetNetDueVATReport called with startDate: {startDate}, endDate: {endDate}, language: {language}");

                // Get Net Sales VAT
                var netSalesVATData = await GetNetSalesVATReport(startDate, endDate, language);
                var netSalesVATType = netSalesVATData.GetType();
                var netSalesVAT = Convert.ToDecimal(netSalesVATType.GetProperty("totalVAT")?.GetValue(netSalesVATData) ?? 0m);

                LogService.LogInformation($"Net Sales VAT: {netSalesVAT}");

                // Get Net Purchase VAT
                var netPurchaseVATData = await _purchaseInvoiceService.GetNetPurchaseVATReport(startDate, endDate, language);
                var netPurchaseVATType = netPurchaseVATData.GetType();
                var netPurchasesVAT = Convert.ToDecimal(netPurchaseVATType.GetProperty("totalVAT")?.GetValue(netPurchaseVATData) ?? 0m);

                LogService.LogInformation($"Net Purchase VAT: {netPurchasesVAT}");

                // Calculate Due VAT
                var dueVAT = netSalesVAT - netPurchasesVAT;

                LogService.LogInformation($"Due VAT: {dueVAT}");

                var result = new
                {
                    startDate = startDate.ToString("yyyy-MM-dd"),
                    endDate = endDate.ToString("yyyy-MM-dd"),
                    netSalesVAT = netSalesVAT,
                    netPurchasesVAT = netPurchasesVAT,
                    dueVAT = dueVAT
                };

                return result;
            }
            catch (Exception ex)
            {
                LogService.LogError($"Error in GetNetDueVATReport: {ex.Message}");
                throw;
            }
        }

        public async Task<byte[]> GetNetDueVATDocument(DateOnly startDate, DateOnly endDate, string language = "ar")
        {
            try
            {
                LogService.LogInformation($"GetNetDueVATDocument called with startDate: {startDate}, endDate: {endDate}, language: {language}");

                // Get report data
                var reportData = await GetNetDueVATReport(startDate, endDate);

                LogService.LogInformation("Fetching company details...");
                var companyQueryResponse = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
                
                if (companyQueryResponse.ErrorCode != ErrorCodes.Success || companyQueryResponse.Data == null || !companyQueryResponse.Data.Any())
                {
                    LogService.LogError("No active company found for Net Due VAT document generation");
                    return Array.Empty<byte>();
                }

                var company = companyQueryResponse.Data.FirstOrDefault();

                LogService.LogInformation($"Company details found: {company.ArabicName ?? company.EnglishName}");
                LogService.LogInformation("Fetching PDF template config...");

                var pdfTemplateQueryResponse = await _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.PdfTemplateConfiguration>().GetAll();
                var pdfTemplateConfig = pdfTemplateQueryResponse.Data?.FirstOrDefault();

                LogService.LogInformation($"PDF template config found: {pdfTemplateConfig != null}");

                LogService.LogInformation($"Creating Net Due VAT Report PDF Document...");
                var pdfDocument = new NetDueVATReportDocument(
                    reportData,
                    company,
                    pdfTemplateConfig,
                    language
                );

                LogService.LogInformation($"Generating PDF bytes...");

                var pdfBytes = pdfDocument.GeneratePdf();
                
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    LogService.LogInformation("PDF generation returned null or empty array");
                    return Array.Empty<byte>();
                }

                LogService.LogInformation($"PDF generated successfully. Size: {pdfBytes.Length} bytes");
                return pdfBytes;
            }
            catch (Exception e)
            {
                LogService.LogError($"GetNetDueVATDocument error details: {e.Message}\nStackTrace: {e.StackTrace}");
                if (e.InnerException != null)
                {
                    LogService.LogError($"Inner exception: {e.InnerException.Message}\nInner StackTrace: {e.InnerException.StackTrace}");
                }
                throw;
            }
        }

        private static string GenerateFallbackTaxNumber()
        {
            // Some deployed databases still enforce NOT NULL TaxNumber for CustomerProfile.
            // Use a process-unique 15-digit fallback when POS creates walk-in customers.
            var value = Interlocked.Increment(ref _fallbackTaxSeed).ToString();
            return value.Length >= 15 ? value[^15..] : value.PadLeft(15, '0');
        }



        private async Task FetchCompanyImagesAsync(SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement.CompanyDetails? companyDetails)
        {
            if (companyDetails == null) return;

            if ((companyDetails.Logo == null || companyDetails.Logo.Length == 0) && !string.IsNullOrWhiteSpace(companyDetails.LogoUrl))
            {
                companyDetails.Logo = await SmartAndPro.ERP.Application.Utility.ImageDownloader.DownloadImageBytesAsync(companyDetails.LogoUrl);
            }

            if ((companyDetails.Stamp == null || companyDetails.Stamp.Length == 0) && !string.IsNullOrWhiteSpace(companyDetails.StampUrl))
            {
                companyDetails.Stamp = await SmartAndPro.ERP.Application.Utility.ImageDownloader.DownloadImageBytesAsync(companyDetails.StampUrl);
            }
        }




        public static void PopulatePaymentStatus(IEnumerable<SaleInvoiceDto> dtos, IEnumerable<SaleInvoice> entities)
        {
            if (dtos == null || entities == null) return;

            var entityMap = entities.ToDictionary(e => e.Id);

            foreach (var dto in dtos)
            {
                if (!entityMap.TryGetValue(dto.Id, out var entity)) continue;

                // Always try to calculate from ReceiptConnections if available for maximum accuracy.
                // Otherwise, fallback to the persisted fields.
                if (entity.ReceiptConnections != null)
                {
                    var totalPaid = entity.ReceiptConnections.Sum(r => r.LinkedAmount);
                    var total = entity.TotalPrice > 0 ? entity.TotalPrice : entity.NetPrice;
                    
                    dto.PaidAmount = Math.Round(totalPaid, 2);
                    dto.RemainingAmount = Math.Round(Math.Max(0, total - totalPaid), 2);
                    
                    if (dto.PaidAmount <= 0) dto.PaymentStatus = InvoicePaymentStatus.Unpaid;
                    else if (dto.PaidAmount < total) dto.PaymentStatus = InvoicePaymentStatus.PartiallyPaid;
                    else dto.PaymentStatus = InvoicePaymentStatus.Paid;
                }
                else
                {
                    dto.PaymentStatus = entity.PaymentStatus;
                    dto.PaidAmount = entity.PaidAmount;
                    dto.RemainingAmount = entity.RemainingAmount;
                }
            }
        }

        public async Task UpdatePaymentStatusAsync(int invoiceId)
        {
            var repository = _repositoriesFactory.GetRepository<SaleInvoice>();
            var dbContext = repository.GetDbContext();
            
            var invoice = await dbContext.Set<SaleInvoice>()
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null) return;

            // Explicitly fetch connections to bypass any EF Core tracking/caching issues with collections
            var connections = await dbContext.Set<ReceiptConnection>()
                .Where(rc => rc.SaleInvoiceId == invoiceId)
                .ToListAsync();

            invoice.ReceiptConnections = connections;

            RecalculateInvoicePaymentStatus(invoice);

            await repository.Update(invoice);
        }

        public async Task<InvoicePaymentSummaryDto?> GetPaymentSummaryAsync(int invoiceId)
        {
            var repository = _repositoriesFactory.GetRepository<SaleInvoice>();
            var dbContext = repository.GetDbContext();

            var summary = await dbContext.Set<SaleInvoice>()
                .AsNoTracking()
                .Where(i => i.Id == invoiceId)
                .Select(i => new InvoicePaymentSummaryDto
                {
                    Id = i.Id,
                    InvoiceNo = i.InvoiceNo,
                    // قراءة مباشرة من الداتابيز: الصافي + الضريبة (أو الإجمالي)
                    InvoiceTotal = (i.NetPrice + i.TaxAmount) > 0 ? (i.NetPrice + i.TaxAmount) : i.TotalPrice,
                    PaidAmount = i.PaidAmount,
                    RemainingAmount = i.RemainingAmount,
                    PaymentStatus = i.PaymentStatus
                })
                .FirstOrDefaultAsync();

            return summary;
        }




        // تسوية الرصسد من المبلغ الموجود في حساب العميل 
        public async Task<OperationResponse<object>> ApplyCreditBalanceAsync(SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale.ApplyCreditBalanceDto dto)
        {
            try
            {
                var customerRepo = _repositoriesFactory.GetRepository<CustomerProfile>();
                var customerQuery = await customerRepo.Filter(c => c.MainAccountId == dto.MainAccountId);
                var customer = await customerQuery.Data.FirstOrDefaultAsync();

                if (customer == null)
                    return new OperationResponse<object>(ErrorCodes.NotFound, null, message: "Customer not found");

                decimal totalAllocation = dto.Allocations?.Sum(a => a.Amount) ?? 0;
                if (totalAllocation <= 0)
                    return new OperationResponse<object>(ErrorCodes.GeneralFail, null, message: "Invalid allocation amount");

                if (customer.CreditBalance < totalAllocation)
                    return new OperationResponse<object>(ErrorCodes.GeneralFail, null, message: "Not enough credit balance");

                // Fetch Transaction Type for "قيد يوميه" (Journal Entry)
                var transactionTypeQuery = await _repositoriesFactory.GetRepository<TransactionType>()
                    .Filter(tt => tt.ArabicName.Contains("قيد يوميه"));
                var transactionType = await transactionTypeQuery.Data.FirstOrDefaultAsync();
                if (transactionType == null)
                    return new OperationResponse<object>(ErrorCodes.GeneralFail, null, message: "Transaction type 'قيد يوميه' not found");

                // Prepare Transaction Entries
                var transactionEntries = new List<TransactionEntry>();
                var invoiceRepo = _repositoriesFactory.GetRepository<SaleInvoice>();
                var receiptConnectionRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Receipts.ReceiptConnection>();

                foreach (var alloc in dto.Allocations)
                {
                    if (alloc.Amount <= 0) continue;

                    var invoiceQuery = await invoiceRepo.Filter(i => i.Id == alloc.InvoiceId);
                    var invoice = await invoiceQuery.Data.FirstOrDefaultAsync();
                    var invoiceNo = invoice?.InvoiceNo ?? alloc.InvoiceId.ToString();

                    // 1. Debit Entry: Reduce the credit balance (Move from "Prepayment" liability)
                    transactionEntries.Add(new TransactionEntry
                    {
                        MainAccountId = dto.MainAccountId,
                        DebitAmount = alloc.Amount,
                        CreditAmount = 0,
                        Description = $"خصم من الرصيد الدائن لتسوية فاتورة رقم {invoiceNo}",
                        CreatedAt = DateTime.UtcNow
                    });

                    // 2. Credit Entry: Pay the invoice (Record the payment in the statement)
                    transactionEntries.Add(new TransactionEntry
                    {
                        MainAccountId = dto.MainAccountId,
                        DebitAmount = 0,
                        CreditAmount = alloc.Amount,
                        Description = $"تسوية فاتورة رقم {invoiceNo} من الرصيد الدائن",
                        CreatedAt = DateTime.UtcNow
                    });

                    var conn = new SmartAndPro.ERP.Domains.Entities.Receipts.ReceiptConnection
                    {
                        SaleInvoiceId = alloc.InvoiceId,
                        LinkedAmount = alloc.Amount,
                        ReceiptConnectionType = SmartAndPro.ERP.SharedKernel.Enums.ReceiptConnectionType.SaleInvoice
                    };
                    await receiptConnectionRepo.Add(conn);
                    
                    // Update the invoice payment status in the database
                    await UpdatePaymentStatusAsync(alloc.InvoiceId);
                }

                // Deduct the balance from the customer profile record
                customer.CreditBalance -= totalAllocation;
                await customerRepo.Update(customer);

                // Create and Enforce the Transaction
                var transaction = new Transaction
                {
                    TransactionTypeId = transactionType.Id,
                    Description = "تسوية الرصيد الدائن",
                    TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    TransactionEntries = transactionEntries
                };

                var transactionEnforcer = new TransactionEnforcer();
                var enforcerResult = transactionEnforcer.EnforceRules(transaction);
                if (!enforcerResult.IsSuccess)
                {
                    return new OperationResponse<object>(ErrorCodes.GeneralFail, null, message: "Accounting rules enforcement failed: " + enforcerResult.Errors.FirstOrDefault()?.ErrorCode);
                }

                var transactionRepo = _repositoriesFactory.GetRepository<Transaction>();
                await transactionRepo.Add(transaction);

                return new OperationResponse<object>(ErrorCodes.Success, null);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return new OperationResponse<object>(ErrorCodes.GeneralFail, null, message: ex.Message);
            }
        }

        public async Task<OperationResponse<object>> GetCustomerCreditBalanceAsync(Guid mainAccountId)
        {
            try
            {
                var customerRepo = _repositoriesFactory.GetRepository<CustomerProfile>();
                var customerQuery = await customerRepo.Filter(c => c.MainAccountId == mainAccountId);
                var customer = customerQuery.Data?.FirstOrDefault();
                if (customer == null)
                    return new OperationResponse<object>(ErrorCodes.NotFound, 0, message: "Customer not found");

                return new OperationResponse<object>(ErrorCodes.Success, customer.CreditBalance);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return new OperationResponse<object>(ErrorCodes.GeneralFail, 0, message: ex.Message);
            }
        }


    }
}
