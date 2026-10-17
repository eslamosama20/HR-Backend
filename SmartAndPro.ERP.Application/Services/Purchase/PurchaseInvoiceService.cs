using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent; // for GeneratePdf extension
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Purchase;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Application.Services.Invoice.Purchase.DocumentUtils;
using SmartAndPro.ERP.Application.Utility;
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;
using SmartAndPro.ERP.Domains.Models.InputModel;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.Infrastructure.Repositories.PurchaseInvoice;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Purchase
{
    public class PurchaseInvoiceService : IPurchaseInvoiceService
    {
        private readonly IPurchaseInvoiceRepository _purchaseInvoiceRepository;
        private readonly IMapper _mapper;
        private readonly RepositoriesFactory _repositoriesFactory;

        public PurchaseInvoiceService(
            IPurchaseInvoiceRepository purchaseInvoiceRepository,
            IMapper mapper,
            RepositoriesFactory repositoriesFactory)
        {
            _purchaseInvoiceRepository = purchaseInvoiceRepository;
            _mapper = mapper;
            _repositoriesFactory = repositoriesFactory;
        }

        public async Task<ProductBarcodeResultDto?> GetProductByBarcodeAsync(string barcode)
        {
            var normalizedBarcode = barcode?.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(normalizedBarcode))
            {
                return null;
            }

            var mainPurchaseClassRepository = _repositoriesFactory.GetRepository<MainPurchaseClass>();
            var queryResult = await mainPurchaseClassRepository.Filter(x => x.ReferenceCode.ToLower() == normalizedBarcode);
            var product = queryResult.Data.FirstOrDefault();

            if (product == null)
            {
                return null;
            }

            return new ProductBarcodeResultDto
            {
                Id = product.Id,
                ArabicName = product.ArabicName,
                EnglishName = product.EnglishName,
                ReferenceCode = product.ReferenceCode,
                MeasurementUnitName = product.MeasurementUnit?.ArabicName ?? product.MeasurementUnit?.EnglishName ?? string.Empty
            };
        }

        public OperationResponse<List<PurchaseInvoiceDetailsResponse>> GetPurchaseInvoiceDetails(GetPurchaseInvoiceDetailsRequest getPurchaseInvoiceDetails)
        {
			try
			{

                var purchaseInvoiceDetails = 
                    _purchaseInvoiceRepository.GetPurchaseInvoiceDetails(_mapper.Map<GetPurchaseInvoiceDetailsInputModel>(getPurchaseInvoiceDetails));

                var purchaseInvoiceDetailsMapper = _mapper.Map<List<PurchaseInvoiceDetailsResponse>>(purchaseInvoiceDetails.ToList());

                return new OperationResponse<List<PurchaseInvoiceDetailsResponse>>(ErrorCodes.Success, data: purchaseInvoiceDetailsMapper, message: string.Empty);

            }
            catch (Exception ex)
			{
                LogService.LogError(ex);
                return new OperationResponse<List<PurchaseInvoiceDetailsResponse>>(ErrorCodes.GeneralFail, null, message: ErrorCodes.GeneralFail.ToString());

            }
        }

        public OperationResponse<List<PurchaseVATDetailsResponse>> GetPurchaseVATDetails(GetPurchaseVATDetailsRequest request)
        {
            try
            {
                // Create input model for repository
                var inputModel = new GetPurchaseInvoiceDetailsInputModel
                {
                    FromDate = request.FromDate,
                    ToDate = request.ToDate,
                    StoreId = request.StoreID
                };

                // Use repository method like Sales does
                var purchaseInvoiceDetails = _purchaseInvoiceRepository.GetPurchaseInvoiceDetails(inputModel);

                // Map to VAT details response similar to Sales implementation
                var vatDetailsMapper = purchaseInvoiceDetails
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
                    .Select(g => new PurchaseVATDetailsResponse
                    {
                        InvoiceId = g.Key.InvoiceId,
                        InvoiceNo = g.Key.InvoiceNo,
                        SupplierId = g.Key.SupplierId,
                        SuppilerArabicName = g.Key.SuppilerArabicName,
                        SuppilerEnglishName = g.Key.SuppilerEnglishName,
                        MainAccountId = g.Key.MainAccountId,
                        IssueDate = g.Key.IssueDate,
                        Items = g.Select(item => new PurchaseVATDetailsResponse.VATItemDetails
                        {
                            ReferenceCode = item.ReferenceCode,
                            ItemArabicName = item.ItemArabicName,
                            ItemEnglishName = item.ItemEnglishName,
                            Quantity = item.Quantity,
                            Price = item.Price,
                            TotalPrice = item.TotalPrice,
                            IsTaxExempted = item.IsTaxExempted,
                            VATAmount = item.IsTaxExempted ? 0 : (item.TotalPrice * 0.15m) // 15% VAT
                        }).ToList()
                    }).ToList();

                return new OperationResponse<List<PurchaseVATDetailsResponse>>(ErrorCodes.Success, data: vatDetailsMapper);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return new OperationResponse<List<PurchaseVATDetailsResponse>>(ErrorCodes.GeneralFail, null);
            }
        }

        public async Task<byte[]> GetDocument(DateOnly startDate, DateOnly endDate, string language = "ar")
        {
            try
            {
                var request = new GetPurchaseInvoiceDetailsRequest
                {
                    FromDate = startDate.ToDateTime(TimeOnly.MinValue),
                    ToDate = endDate.ToDateTime(TimeOnly.MaxValue),
                    StoreID = 0 // All stores
                };

                var purchaseInvoiceDetailsResponse = GetPurchaseInvoiceDetails(request);
                if (purchaseInvoiceDetailsResponse?.Data == null || !purchaseInvoiceDetailsResponse.Data.Any())
                {
                    LogService.LogError($"Purchase invoice details data is null or empty for startDate: {startDate}, endDate: {endDate}");
                    throw new InvalidOperationException("Purchase invoice details data not found");
                }

                var companyDetailsQuery = await _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement.CompanyDetails>().GetAll();
                var companyDetails = companyDetailsQuery.Data?.FirstOrDefault();
                
                // If no company details found, create default values
                if (companyDetails == null)
                {
                    LogService.LogInformation("CompanyDetails not found in GetDocument, using default values");
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

                var document = new PurchaseInvoiceReportDocument(
                    purchaseInvoiceDetailsResponse.Data,
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
                var request = new GetPurchaseInvoiceDetailsRequest
                {
                    FromDate = startDate.ToDateTime(TimeOnly.MinValue),
                    ToDate = endDate.ToDateTime(TimeOnly.MaxValue),
                    StoreID = 0 // All stores
                };

                var purchaseInvoiceDetailsResponse = GetPurchaseInvoiceDetails(request);
                if (purchaseInvoiceDetailsResponse?.Data == null || !purchaseInvoiceDetailsResponse.Data.Any())
                {
                    LogService.LogError($"Purchase invoice details data is null or empty for startDate: {startDate}, endDate: {endDate}");
                    throw new InvalidOperationException("Purchase invoice details data not found");
                }

                var companyDetailsQuery = await _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement.CompanyDetails>().GetAll();
                var companyDetails = companyDetailsQuery.Data?.FirstOrDefault();
                if (companyDetails == null)
                {
                    LogService.LogError("CompanyDetails not found");
                    throw new InvalidOperationException("Company details not found");
                }

                var pdfTemplateConfigQuery = await _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.PdfTemplateConfiguration>().GetAll();
                var pdfTemplateConfig = pdfTemplateConfigQuery.Data?.FirstOrDefault();

                var document = new PurchaseInvoiceSummaryReportDocument(
                    purchaseInvoiceDetailsResponse.Data,
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

        public byte[] ExportPurchaseVATDetails(GetPurchaseVATDetailsRequest getVATDetails)
        {
            try
            {
                var vatDetailsResponse = GetPurchaseVATDetails(getVATDetails);
                if (vatDetailsResponse?.Data == null) return null;

                var excelData = ExcelExporter.ExportPurchaseVATToExcel(vatDetailsResponse.Data);
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
                
                var request = new GetPurchaseVATDetailsRequest
                {
                    FromDate = startDate.ToDateTime(TimeOnly.MinValue),
                    ToDate = endDate.ToDateTime(TimeOnly.MaxValue),
                    StoreID = 0
                };

                LogService.LogInformation($"Calling GetPurchaseVATDetails with FromDate: {request.FromDate}, ToDate: {request.ToDate}");
                var vatDetailsResponse = GetPurchaseVATDetails(request);
                
                LogService.LogInformation($"GetPurchaseVATDetails returned. Success: {vatDetailsResponse?.ErrorCode == ErrorCodes.Success}, Data count: {vatDetailsResponse?.Data?.Count ?? 0}");
                
                if (vatDetailsResponse?.Data == null || !vatDetailsResponse.Data.Any())
                {
                    LogService.LogError($"Purchase VAT details data is null or empty for startDate: {startDate}, endDate: {endDate}");
                    throw new InvalidOperationException("Purchase VAT details data not found");
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

                LogService.LogInformation("Creating PurchaseVATReportDocument...");
                var document = new PurchaseVATReportDocument(
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

        public async Task<byte[]> GetPurchaseInvoiceSummaryDocument(DateOnly startDate, DateOnly endDate, string language = "ar")
        {
            try
            {
                LogService.LogInformation($"GetPurchaseInvoiceSummaryDocument called with startDate: {startDate}, endDate: {endDate}, language: {language}");

                var companyRepo = _repositoriesFactory.GetRepository<Domains.Entities.Inventory.CompanyManagement.CompanyDetails>();
                var companyResult = await companyRepo.GetAll();
                var companyDetails = companyResult.Data?.FirstOrDefault();

                if (companyDetails == null)
                {
                    LogService.LogInformation("CompanyDetails not found, using default values");
                    companyDetails = new Domains.Entities.Inventory.CompanyManagement.CompanyDetails
                    {
                        ArabicName = "الشركة",
                        EnglishName = "Company",
                        TaxNumber = "000000000000000",
                        Email = "info@company.com"
                    };
                }

                // Get purchase invoice details for the date range
                var request = new GetPurchaseInvoiceDetailsRequest
                {
                    FromDate = startDate.ToDateTime(TimeOnly.MinValue),
                    ToDate = endDate.ToDateTime(TimeOnly.MaxValue)
                };

                var purchaseInvoiceDetailsResult = GetPurchaseInvoiceDetails(request);
                var purchaseInvoiceDetails = purchaseInvoiceDetailsResult?.Data ?? new List<PurchaseInvoiceDetailsResponse>();

                if (purchaseInvoiceDetails == null || !purchaseInvoiceDetails.Any())
                {
                    LogService.LogInformation("No purchase invoice details found for the specified date range");
                    // Return empty report instead of throwing exception
                    purchaseInvoiceDetails = new List<PurchaseInvoiceDetailsResponse>();
                }

                var document = new PurchaseInvoiceSummaryReportDocument(
                    purchaseInvoiceDetails,
                    companyDetails,
                    null,
                    startDate,
                    endDate,
                    language
                );

                var pdfBytes = document.GeneratePdf();

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    LogService.LogError("Generated Purchase Invoice Summary PDF is null or empty");
                    throw new InvalidOperationException("Failed to generate Purchase Invoice Summary PDF document");
                }

                LogService.LogInformation($"Purchase Invoice Summary PDF generated successfully. Size: {pdfBytes.Length} bytes");
                return pdfBytes;
            }
            catch (Exception e)
            {
                LogService.LogError($"GetPurchaseInvoiceSummaryDocument error details: {e.Message}\nStackTrace: {e.StackTrace}");
                if (e.InnerException != null)
                {
                    LogService.LogError($"Inner exception: {e.InnerException.Message}\nInner StackTrace: {e.InnerException.StackTrace}");
                }
                throw;
            }
        }

        public async Task<object> GetNetPurchaseVATReport(DateOnly startDate, DateOnly endDate, string language = "ar")
        {
            try
            {
                // 1. Get purchase invoice VAT totals using GetPurchaseVATDetails method
                var purchaseVATRequest = new GetPurchaseVATDetailsRequest
                {
                    FromDate = startDate.ToDateTime(TimeOnly.MinValue),
                    ToDate = endDate.ToDateTime(TimeOnly.MaxValue),
                    StoreID = 0 // All stores
                };
                
                var purchaseVATResponse = GetPurchaseVATDetails(purchaseVATRequest);
                
                decimal totalPurchasesVAT = 0;
                
                if (purchaseVATResponse?.Data != null && purchaseVATResponse.Data.Any())
                {
                    foreach (var invoice in purchaseVATResponse.Data)
                    {
                        foreach (var item in invoice.Items)
                        {
                            totalPurchasesVAT += item.VATAmount;
                        }
                    }
                }
                
                // 2. Get purchase returns VAT total
                decimal totalReturnsVAT = 0;
                
                try
                {
                    var returnsResponse = await _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Return.Purchase.PurchaseReturn>().GetAll();
                    
                    if (returnsResponse.ErrorCode == ErrorCodes.Success && returnsResponse.Data != null)
                    {
                        var returnIds = returnsResponse.Data
                            .Where(r => r.IsActive && 
                                        r.IssueDate >= startDate.ToDateTime(TimeOnly.MinValue) && 
                                        r.IssueDate <= endDate.ToDateTime(TimeOnly.MaxValue))
                            .Select(r => r.Id)
                            .ToList();

                        if (returnIds.Any())
                        {
                            var returnItemsResponse = await _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Return.Purchase.PurchaseReturnItem>().GetAll();
                            
                            if (returnItemsResponse.ErrorCode == ErrorCodes.Success && returnItemsResponse.Data != null)
                            {
                                var returnItems = returnItemsResponse.Data
                                    .Where(item => returnIds.Contains(item.PurchaseReturnId))
                                    .ToList();
                                
                                foreach (var item in returnItems)
                                {
                                    // Calculate VAT at 15% for all return items
                                    // Note: Tax exemption check is not available for PurchaseReturnItem
                                    decimal vatAmount = item.Quantity * item.Price * 0.15m;
                                    totalReturnsVAT += vatAmount;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogService.LogError($"Error calculating purchase returns VAT: {ex.Message}");
                    // Continue with totalReturnsVAT = 0
                }

                // 3. Create summary object with separate purchases, returns, and net VAT
                var reportData = new
                {
                    totalPurchasesVAT = totalPurchasesVAT,
                    totalReturnsVAT = totalReturnsVAT,
                    totalVAT = totalPurchasesVAT - totalReturnsVAT, // Net VAT = Purchases VAT - Returns VAT
                    startDate = startDate.ToString("yyyy-MM-dd"),
                    endDate = endDate.ToString("yyyy-MM-dd")
                };

                LogService.LogInformation($"Net Purchase VAT Report: PurchasesVAT={totalPurchasesVAT}, ReturnsVAT={totalReturnsVAT}, NetVAT={totalPurchasesVAT - totalReturnsVAT}");

                return reportData;
            }
            catch (Exception ex)
            {
                LogService.LogError($"GetNetPurchaseVATReport error: {ex.Message}");
                throw;
            }
        }

        public async Task<byte[]> GetNetPurchaseVATDocument(DateOnly startDate, DateOnly endDate, string language = "ar")
        {
            try
            {
                LogService.LogInformation($"GetNetPurchaseVATDocument called with startDate: {startDate}, endDate: {endDate}, language: {language}");

                // Get report data
                var reportData = await GetNetPurchaseVATReport(startDate, endDate, language);

                LogService.LogInformation("Fetching company details...");
                var companyQueryResponse = await _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement.CompanyDetails>().GetAll();
                
                if (companyQueryResponse.ErrorCode != ErrorCodes.Success || companyQueryResponse.Data == null || !companyQueryResponse.Data.Any())
                {
                    LogService.LogError("No active company found for Net Purchase VAT document generation");
                    return Array.Empty<byte>();
                }

                var company = companyQueryResponse.Data.FirstOrDefault();

                LogService.LogInformation($"Company details found: {company.ArabicName ?? company.EnglishName}");
                LogService.LogInformation("Fetching PDF template config...");

                var pdfTemplateQueryResponse = await _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.PdfTemplateConfiguration>().GetAll();
                var pdfTemplateConfig = pdfTemplateQueryResponse.Data?.FirstOrDefault();

                LogService.LogInformation($"PDF template config found: {pdfTemplateConfig != null}");

                LogService.LogInformation($"Creating Net Purchase VAT Report PDF Document...");
                var pdfDocument = new DocumentUtils.NetPurchaseVATReportDocument(
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
            catch (Exception ex)
            {
                LogService.LogError($"GetNetPurchaseVATDocument error details: {ex.Message}\nStackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    LogService.LogError($"Inner exception: {ex.InnerException.Message}\nInner StackTrace: {ex.InnerException.StackTrace}");
                }
                throw;
            }
        }
    }
}
