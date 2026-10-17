using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Purchase;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Invoice.Purchase;
using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.SharedKernel.Constants;
using SmartAndPro.ERP.SharedKernel.Dtos.DocumentShare;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.SharedKernel.Utilities.ShareHandlers;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;

namespace SmartAndPro.ERP.WebAPI.Controllers.Invoice
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [PermissionAuthorize(Permissions.PurchaseInvoice.Read)]
    public class PurchaseInvoiceController : ControllerBase
    {
        #region Fields
        private readonly ILogger<PurchaseInvoiceController> _logger;
        private readonly ServiceBaseFactory _serviceBaseFactory;
        private readonly PurchaseInvoiceService _service;
        private readonly IPurchaseInvoiceService _purchaseInvoiceService;
        private readonly IMapper _mapper;
        private readonly IDocumentShareService _documentShareService;
        private const string inv = "Inv-";
        #endregion

        #region Constructors
        public PurchaseInvoiceController(
            ServiceBaseFactory serviceBaseFactory,
            IPurchaseInvoiceService purchaseInvoiceService,
            IMapper mapper,
            ILogger<PurchaseInvoiceController> logger,
            IDocumentShareService documentShareService)
        {
            _mapper = mapper;
            _serviceBaseFactory = serviceBaseFactory;
            _purchaseInvoiceService = purchaseInvoiceService;
            _service = serviceBaseFactory.GetService<PurchaseInvoiceService>();
            _logger = logger;
            _documentShareService = documentShareService;
        }
        #endregion

        #region APIs

        [HttpGet("frontal")]
        public async Task<IActionResult> GetAllFrontalInvoices([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = await _service.GetAllFrontalAsync(pageNumber, pageSize);

                return response.HandleResult();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        #region OldGetAll
        //[HttpGet("frontal")]
        //public async Task<IActionResult> GetAllFrontalInvoices([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
        //{
        //    var result = await _service.GetAllFrontalAsync(pageNumber, pageSize);

        //    if (result.ErrorCode != ErrorCodes.Success)
        //    {
        //        return StatusCode(500, new { message = "An error occurred while retrieving frontal purchase invoices." });
        //    }

        //    if (result.Data == null || result.Data.Data == null || !result.Data.Data.Any())
        //    {
        //        return NoContent();

        //    }
        //    return Ok(new
        //    {
        //        totalRecords = result.Data.TotalCount,
        //        invoices = result.Data.Data
        //    });
        //}

        //[HttpGet]
        //public async Task<IActionResult> GetAllInvoices([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
        //{
        //    var result = await _service.GetAllAsync(pageNumber, pageSize);

        //    if (result.ErrorCode != ErrorCodes.Success)
        //    {
        //        return StatusCode(500, new { message = "An error occurred while retrieving purchase invoices." });
        //    }

        //    if (result.Data == null || result.Data.Data == null || !result.Data.Data.Any())
        //    {
        //        return NoContent();

        //    }
        //    return Ok(new
        //    {
        //        totalRecords = result.Data.TotalCount,
        //        invoices = result.Data.Data
        //    });
        //}
        #endregion

        [HttpPost("frontal/{id:int}")]
        public async Task<IActionResult> MakeFrontalAsync(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            IActionResult result = NoContent();
            try
            {
                var responseDate = await _service.MakeFrontalAsync(id);
                if (responseDate != null)
                {
                    var invoiceDto = _mapper.Map<PurchaseInvoiceDto>(responseDate);
                    return Ok(invoiceDto);
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
            return result;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllInvoices([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = await _service.GetAllAsync(pageNumber, pageSize);

                return response.HandleResult();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoiceById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid invoice ID: {Id}", id);
                return BadRequest("Invalid invoice ID.");
            }

            var invoice = await _service.GetByIdAsync(id);
            if (invoice != null)
            {
                var invoiceDto = _mapper.Map<PurchaseInvoiceDto>(invoice);
                return Ok(invoiceDto);
            }

            _logger.LogWarning("Invoice with ID: {Id} not found", id);
            return NotFound();
        }

        [HttpGet("product/barcode/{barcode}")]
        public async Task<IActionResult> GetProductByBarcode(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                return BadRequest("Barcode is required.");
            }

            var product = await _purchaseInvoiceService.GetProductByBarcodeAsync(barcode);
            if (product == null)
            {
                return NotFound(new { message = "الباركود غير موجود في الأصناف" });
            }

            return Ok(product);
        }

        [HttpDelete("{id}")]
        [PermissionAuthorize(Permissions.PurchaseInvoice.Delete)]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            var invoice = await _service.GetByIdAsync(id);
            if (invoice == null)
            {
                _logger.LogWarning("Invoice with ID: {Id} not found", id);
                return NotFound();
            }

            var result = await _service.RemoveAsync(invoice);
            if (result == 1)
            {
                _logger.LogInformation("Invoice with ID: {Id} deleted successfully", id);
                return NoContent();
            }

            _logger.LogError("Failed to delete the invoice with ID: {Id}", id);
            return BadRequest("Failed to delete the invoice.");
        }

        [HttpPost]
        [PermissionAuthorize(Permissions.PurchaseInvoice.Create)]
        public async Task<IActionResult> CreateInvoice([FromBody] PurchaseInvoicePostDto invoiceDto)
        {
            if (invoiceDto == null)
            {
                _logger.LogWarning("InvoiceDto object sent from client is null.");
                return BadRequest("InvoiceDto object is null.");
            }
            if (invoiceDto.PurchaseItems == null || invoiceDto.PurchaseItems.Count < 1)
            {
                _logger.LogWarning("PurchaseItems object sent from client is null.");
                return BadRequest("PurchaseItems object is null.");
            }
            var invoice = _mapper.Map<PurchaseInvoice>(invoiceDto);

            var result = await _service.AddAsync(invoice);
            if (result == 1)
            {
                _logger.LogInformation("Invoice created successfully.");
                return CreatedAtAction(nameof(GetInvoiceById), new { id = invoice.Id }, invoiceDto);
            }

            _logger.LogError("Failed to create the invoice.");
            return BadRequest("Failed to create the invoice.");
        }

        [HttpPost("createTransaction")]
        [PermissionAuthorize(Permissions.PurchaseInvoice.Create)]
        public async Task<IActionResult> CreateTransactionAndAddProductUnits([FromQuery] int invoiceId)
        {
            if (invoiceId == null)
            {
                _logger.LogWarning("Invoice id sent from client is null.");
                return BadRequest("Invoice id  is null.");
            }

            var result = await _service.CreateTransactionAndAddProductUnits(invoiceId);
            if (result == 1)
            {
                _logger.LogInformation("transaction created successfully.");
                return Ok();
            }

            _logger.LogError("Failed to create the transaction.");
            return BadRequest("Failed to create the transaction.");
        }
        [HttpPut("{id}")]
        [PermissionAuthorize(Permissions.PurchaseInvoice.Update)]
        public async Task<IActionResult> UpdateInvoice(int id, [FromBody] PurchaseInvoicePutDto invoiceDto)
        {
            if (invoiceDto == null || invoiceDto.Id != id)
            {
                _logger.LogWarning("Invoice data is null or ID mismatch");
                return BadRequest("Invoice data is null or ID mismatch");
            }

            var existingInvoice = await _service.GetByIdAsync(id);
            if (existingInvoice == null)
            {
                _logger.LogWarning("Invoice with ID: {Id} not found", id);
                return NotFound();
            }
            var oldItems = existingInvoice.PurchaseItems.ToList();
            existingInvoice.PurchaseItems.Clear();
            foreach (var item in invoiceDto.PurchaseItems)
            {
                existingInvoice.PurchaseItems.Add(_mapper.Map<PurchaseItem>(item));
            }

            var invoice = _mapper.Map(invoiceDto, existingInvoice);

            var result = await _service.EditAsync(invoice, oldItems);

            if (result == 1)
            {
                _logger.LogInformation("Invoice updated successfully");
                return NoContent();
            }

            _logger.LogError("Failed to update invoice");
            return BadRequest("Failed to update invoice");
        }

        [HttpGet("InvoiceNo")]
        public async Task<IActionResult> GetInvoiceNo()
        {
            var newInvoiceNum = await GetInvoiceNumber();

            return Ok(new { InvoiceNo = newInvoiceNum });
        }

        [HttpPost("Report/Details")]
        public ActionResult GetPurchaseInvoicesDetailsReport(GetPurchaseInvoiceDetailsRequest invoiceDetailsRequest)
        {
            try
            {

                return Ok(_purchaseInvoiceService.GetPurchaseInvoiceDetails(invoiceDetailsRequest));
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return BadRequest();
            }
        }

        [HttpGet("Report/Details/document")]
        public async Task<IActionResult> GetDocument([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] string language = "ar")
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            IActionResult result = NoContent();
            try
            {
                var document = await _purchaseInvoiceService.GetDocument(startDate, endDate, language);
                
                if (document != null && document.Length > 0)
                {
                    var fileName = $"purchase-invoice-report-{startDate:yyyyMMdd}-{endDate:yyyyMMdd}.pdf";
                    result = File(document, "application/pdf", fileName);
                }
            }
            catch (Exception ex)
            {
                LogService.LogError($"GetDocument controller error: {ex.Message}");
                LogService.LogError($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    LogService.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                result = StatusCode(500, new { message = ex.Message, details = ex.StackTrace });
            }
            return result;
        }

        [HttpGet("Report/Details/summary-document")]
        public async Task<IActionResult> GetSummaryDocument([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] string language = "ar")
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            IActionResult result = NoContent();
            try
            {
                var document = await _purchaseInvoiceService.GetSummaryDocument(startDate, endDate, language);
                
                if (document != null && document.Length > 0)
                {
                    var fileName = $"purchase-invoice-summary-report-{startDate:yyyyMMdd}-{endDate:yyyyMMdd}.pdf";
                    result = File(document, "application/pdf", fileName);
                }
            }
            catch (Exception ex)
            {
                LogService.LogError($"GetSummaryDocument controller error: {ex.Message}");
                LogService.LogError($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    LogService.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                result = StatusCode(500, new { message = ex.Message, details = ex.StackTrace });
            }
            return result;
        }

        [HttpPost("Report/Details/share")]
        public async Task<IActionResult> SharePurchaseInvoiceReport([FromBody] PurchaseInvoiceReportShareRequest shareRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                LogService.LogError($"Model validation failed: {errors}");
                return BadRequest(new { message = "Invalid request data", errors = ModelState });
            }

            try
            {
                LogService.LogInformation($"Share request received - StartDate: {shareRequest.StartDate}, EndDate: {shareRequest.EndDate}, Method: {shareRequest.Method}, Language: {shareRequest.Language ?? "ar"}");
                
                var document = await _purchaseInvoiceService.GetDocument(shareRequest.StartDate, shareRequest.EndDate, shareRequest.Language ?? "ar");
                
                if (document == null || document.Length == 0)
                {
                    LogService.LogError("Purchase invoice report document is null or empty");
                    return NotFound("Purchase invoice report document not found or could not be generated.");
                }

                LogService.LogInformation($"Document generated successfully, size: {document.Length} bytes");

                await _documentShareService.ShareDocumentAsync(document, shareRequest.Method, shareRequest.Receiver);
                
                LogService.LogInformation($"Document shared successfully via {shareRequest.Method} to {shareRequest.Receiver}");
                
                var response = new DocumentShareResponseDto<string>
                {
                    Id = $"{shareRequest.StartDate}_{shareRequest.EndDate}",
                    Method = shareRequest.Method,
                    Receiver = shareRequest.Receiver,
                    Message = "Purchase invoice report shared successfully",
                    IsSuccess = true
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                LogService.LogError($"Error details: {ex.Message}\nStackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    LogService.LogError($"Inner exception: {ex.InnerException.Message}\nInner StackTrace: {ex.InnerException.StackTrace}");
                }
                return StatusCode(500, new { message = "An unexpected error occurred while sharing the purchase invoice report.", error = ex.Message });
            }
        }

        [HttpPost("Report/Details/whatsappShare")]
        public async Task<IActionResult> SharePurchaseInvoiceReportOnWhatsApp([FromBody] PurchaseInvoiceReportShareRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var handler = new WhatsAppShareHandler<string>();
                var baseUrl = "https://erp.smartapro.com/purchasable-invoice-report";
                var reportId = $"{dto.StartDate}_{dto.EndDate}";
                var response = handler.BuildLinks(new[] { reportId }, dto.Receiver, baseUrl);
                return Ok(new { response, reportId });
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, "An unexpected error occurred while sharing the purchase invoice report on WhatsApp.");
            }
        }

        [HttpPost("Report/VAT/Details")]
        public IActionResult GetPurchaseVATDetails([FromBody] GetPurchaseVATDetailsRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = _purchaseInvoiceService.GetPurchaseVATDetails(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, new { message = "An error occurred while retrieving purchase VAT details.", error = ex.Message });
            }
        }

        [HttpPost("Report/VAT/Export")]
        public IActionResult ExportPurchaseVATToExcel(GetPurchaseVATDetailsRequest getVATDetails)
        {
            try
            {
                var excelData = _purchaseInvoiceService.ExportPurchaseVATDetails(getVATDetails);

                if (excelData != null)
                {
                    return File(
                        excelData,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "PurchaseVATReport.xlsx"
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "An error occurred while exporting the purchase VAT report.");
            }

            return NotFound("No data found to export.");
        }

        [HttpGet("Report/VAT/document")]
        public async Task<IActionResult> GetVATDocument([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] string language = "ar")
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            IActionResult result = NoContent();
            try
            {
                var document = await _purchaseInvoiceService.GetVATDocument(startDate, endDate, language);
                
                if (document != null && document.Length > 0)
                {
                    var fileName = $"purchase-vat-report-{startDate:yyyyMMdd}-{endDate:yyyyMMdd}.pdf";
                    result = File(document, "application/pdf", fileName);
                }
            }
            catch (Exception ex)
            {
                LogService.LogError($"GetVATDocument controller error: {ex.Message}");
                LogService.LogError($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    LogService.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                result = StatusCode(500, new { message = ex.Message, details = ex.StackTrace });
            }
            return result;
        }

        [HttpPost("Report/VAT/share")]
        public async Task<IActionResult> SharePurchaseVATReport([FromBody] PurchaseVATReportShareRequest shareRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                LogService.LogError($"Model validation failed: {errors}");
                return BadRequest(new { message = "Invalid request data", errors = ModelState });
            }

            try
            {
                LogService.LogInformation($"VAT Share request received - StartDate: {shareRequest.StartDate}, EndDate: {shareRequest.EndDate}, Method: {shareRequest.Method}, Language: {shareRequest.Language ?? "ar"}");
                
                var document = await _purchaseInvoiceService.GetVATDocument(shareRequest.StartDate, shareRequest.EndDate, shareRequest.Language ?? "ar");
                
                if (document == null || document.Length == 0)
                {
                    LogService.LogError("Purchase VAT report document is null or empty");
                    return NotFound("Purchase VAT report document not found or could not be generated.");
                }

                LogService.LogInformation($"VAT Document generated successfully, size: {document.Length} bytes");

                // Handle WhatsApp sharing differently
                if (shareRequest.Method.Equals("WhatsApp", StringComparison.OrdinalIgnoreCase))
                {
                    var cleanReceiver = shareRequest.Receiver.Replace("+", "").Replace(" ", "").TrimStart('0');
                    
                    // Ensure phone number has country code
                    var hasCountryCode = cleanReceiver.StartsWith("1") || 
                                        cleanReceiver.StartsWith("20") || 
                                        cleanReceiver.StartsWith("44") || 
                                        cleanReceiver.StartsWith("91") ||
                                        cleanReceiver.StartsWith("966") || 
                                        cleanReceiver.StartsWith("971") ||
                                        cleanReceiver.StartsWith("974") ||
                                        cleanReceiver.StartsWith("965");
                    
                    if (!hasCountryCode && cleanReceiver.Length >= 9)
                    {
                        cleanReceiver = "966" + cleanReceiver;
                    }

                    // Save PDF to wwwroot/downloads folder for public access
                    var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "downloads");
                    Directory.CreateDirectory(webRootPath);

                    var fileName = $"VAT_Report_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.pdf";
                    var filePath = Path.Combine(webRootPath, fileName);

                    await System.IO.File.WriteAllBytesAsync(filePath, document);

                    LogService.LogInformation($"[WhatsApp] PDF saved to: {filePath}");

                    // Generate public download link
                    var baseUrl = $"{Request.Scheme}://{Request.Host}";
                    var downloadUrl = $"{baseUrl}/downloads/{fileName}";
                    
                    var message = $"مرحباً،%0A%0Aتم إنشاء تقرير القيمة المضافة للمشتريات بنجاح.%0A%0Aلتحميل التقرير:%0A{downloadUrl}%0A%0Aحجم الملف: {document.Length / 1024} كيلوبايت";
                    var whatsappLink = $"https://wa.me/{cleanReceiver}?text={message}";

                    LogService.LogInformation($"[WhatsApp] Link: {whatsappLink}");
                    
                    var response = new DocumentShareResponseDto<string>
                    {
                        Id = $"{shareRequest.StartDate}_{shareRequest.EndDate}",
                        Method = shareRequest.Method,
                        Receiver = cleanReceiver,
                        Message = "Purchase VAT report WhatsApp link generated successfully",
                        IsSuccess = true,
                        WhatsAppLink = whatsappLink,
                        DownloadLink = downloadUrl
                    };
                    return Ok(response);
                }
                else
                {
                    // Email sharing using DocumentShareService
                    LogService.LogInformation($"[Email] Sending to: {shareRequest.Receiver}");
                    
                    await _documentShareService.ShareDocumentAsync(document, shareRequest.Method, shareRequest.Receiver);

                    LogService.LogInformation($"[Email] Successfully sent to: {shareRequest.Receiver}");
                    
                    var response = new DocumentShareResponseDto<string>
                    {
                        Id = $"{shareRequest.StartDate}_{shareRequest.EndDate}",
                        Method = shareRequest.Method,
                        Receiver = shareRequest.Receiver,
                        Message = "Purchase VAT report sent successfully",
                        IsSuccess = true
                    };
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                LogService.LogError($"SharePurchaseVATReport error: {ex.Message}");
                LogService.LogError($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Error sharing purchase VAT report", details = ex.Message });
            }
        }

        [HttpGet("Report/Summary/document")]
        public async Task<IActionResult> GetPurchaseInvoiceSummaryDocument([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] string language = "ar")
        {
            try
            {
                var document = await _purchaseInvoiceService.GetPurchaseInvoiceSummaryDocument(startDate, endDate, language);
                var fileName = $"purchase-invoice-summary-report-{startDate:yyyy-MM-dd}-{endDate:yyyy-MM-dd}.pdf";
                return File(document, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, new { message = "An error occurred while generating the purchase invoice summary report.", error = ex.Message });
            }
        }

        [HttpPost("Report/Summary/share")]
        public async Task<IActionResult> SharePurchaseInvoiceSummaryReport([FromBody] PurchaseInvoiceReportShareRequest shareRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                LogService.LogError($"Model validation failed: {errors}");
                return BadRequest(new { message = "Invalid request data", errors = ModelState });
            }

            try
            {
                LogService.LogInformation($"Share request received - StartDate: {shareRequest.StartDate}, EndDate: {shareRequest.EndDate}, Method: {shareRequest.Method}, Language: {shareRequest.Language ?? "ar"}");
                
                var document = await _purchaseInvoiceService.GetPurchaseInvoiceSummaryDocument(shareRequest.StartDate, shareRequest.EndDate, shareRequest.Language ?? "ar");
                
                if (document == null || document.Length == 0)
                {
                    LogService.LogError("Purchase invoice summary report document is null or empty");
                    return NotFound("Purchase invoice summary report document not found or could not be generated.");
                }

                LogService.LogInformation($"Document generated successfully, size: {document.Length} bytes");

                // Handle WhatsApp sharing differently
                if (shareRequest.Method.Equals("WhatsApp", StringComparison.OrdinalIgnoreCase))
                {
                    var cleanReceiver = shareRequest.Receiver.Replace("+", "").Replace(" ", "").TrimStart('0');
                    
                    var fileName = $"purchase-invoice-summary-report-{shareRequest.StartDate:yyyy-MM-dd}-{shareRequest.EndDate:yyyy-MM-dd}.pdf";
                    var downloadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "downloads");
                    
                    if (!Directory.Exists(downloadsPath))
                        Directory.CreateDirectory(downloadsPath);
                    
                    var filePath = Path.Combine(downloadsPath, fileName);
                    await System.IO.File.WriteAllBytesAsync(filePath, document);
                    
                    var baseUrl = $"{Request.Scheme}://{Request.Host}";
                    var fileUrl = $"{baseUrl}/downloads/{fileName}";
                    var whatsappLink = $"https://wa.me/{cleanReceiver}?text=تقرير%20ملخص%20فواتير%20المشتريات%0A{Uri.EscapeDataString(fileUrl)}";
                    
                    LogService.LogInformation($"WhatsApp link created: {whatsappLink}");
                    return Ok(new { WhatsAppLink = whatsappLink });
                }
                else
                {
                    // For email or other methods, use document share service
                    await _documentShareService.ShareDocumentAsync(document, shareRequest.Method, shareRequest.Receiver);
                    
                    LogService.LogInformation($"Document shared successfully via {shareRequest.Method} to {shareRequest.Receiver}");
                    
                    var response = new DocumentShareResponseDto<string>
                    {
                        Id = $"{shareRequest.StartDate}_{shareRequest.EndDate}",
                        Method = shareRequest.Method,
                        Receiver = shareRequest.Receiver,
                        Message = "Purchase invoice summary report shared successfully",
                        IsSuccess = true
                    };
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                LogService.LogError($"Error details: {ex.Message}\nStackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    LogService.LogError($"Inner exception: {ex.InnerException.Message}\nInner StackTrace: {ex.InnerException.StackTrace}");
                }
                return StatusCode(500, new { message = "An unexpected error occurred while sharing the purchase invoice summary report.", error = ex.Message });
            }
        }

        [HttpGet("Report/document")]
        public async Task<IActionResult> GetPurchaseInvoiceReportDocument([FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] string language = "ar")
        {
            try
            {
                LogService.LogInformation($"GetPurchaseInvoiceReportDocument called - StartDate: {startDate}, EndDate: {endDate}, Language: {language}");
                
                if (!DateOnly.TryParse(startDate, out var parsedStartDate) || !DateOnly.TryParse(endDate, out var parsedEndDate))
                {
                    LogService.LogError("Invalid date format");
                    return BadRequest("Invalid date format. Use YYYY-MM-DD format.");
                }

                var document = await _purchaseInvoiceService.GetDocument(parsedStartDate, parsedEndDate, language);
                
                if (document == null || document.Length == 0)
                {
                    LogService.LogError("Purchase invoice report document is null or empty");
                    return NotFound("Purchase invoice report document not found or could not be generated.");
                }

                LogService.LogInformation($"Document generated successfully, size: {document.Length} bytes");
                
                var fileName = $"purchase-invoice-report-{startDate}-{endDate}.pdf";
                return File(document, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                LogService.LogError($"GetPurchaseInvoiceReportDocument error: {ex.Message}");
                LogService.LogError($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Error generating purchase invoice report document", details = ex.Message });
            }
        }

        [HttpPost("Report/share")]
        public async Task<IActionResult> SharePurchaseInvoiceReportDocument([FromBody] PurchaseInvoiceReportShareRequest shareRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                LogService.LogError($"Model validation failed: {errors}");
                return BadRequest(new { message = "Invalid request data", errors = ModelState });
            }

            try
            {
                LogService.LogInformation($"Share request received - StartDate: {shareRequest.StartDate}, EndDate: {shareRequest.EndDate}, Method: {shareRequest.Method}, Language: {shareRequest.Language ?? "ar"}");
                
                var document = await _purchaseInvoiceService.GetDocument(shareRequest.StartDate, shareRequest.EndDate, shareRequest.Language ?? "ar");
                
                if (document == null || document.Length == 0)
                {
                    LogService.LogError("Purchase invoice report document is null or empty");
                    return NotFound("Purchase invoice report document not found or could not be generated.");
                }

                LogService.LogInformation($"Document generated successfully, size: {document.Length} bytes");

                // Handle WhatsApp sharing differently
                if (shareRequest.Method.Equals("WhatsApp", StringComparison.OrdinalIgnoreCase))
                {
                    var cleanReceiver = shareRequest.Receiver.Replace("+", "").Replace(" ", "").TrimStart('0');
                    
                    // Ensure phone number has country code
                    var hasCountryCode = cleanReceiver.StartsWith("1") || 
                                        cleanReceiver.StartsWith("20") || 
                                        cleanReceiver.StartsWith("44") || 
                                        cleanReceiver.StartsWith("91") ||
                                        cleanReceiver.StartsWith("966") || 
                                        cleanReceiver.StartsWith("971") ||
                                        cleanReceiver.StartsWith("974") ||
                                        cleanReceiver.StartsWith("965");
                    
                    if (!hasCountryCode && cleanReceiver.Length >= 9)
                    {
                        cleanReceiver = "966" + cleanReceiver;
                    }

                    // Save PDF to wwwroot/downloads folder for public access
                    var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "downloads");
                    Directory.CreateDirectory(webRootPath);

                    var fileName = $"Purchase_Invoice_Report_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.pdf";
                    var filePath = Path.Combine(webRootPath, fileName);

                    await System.IO.File.WriteAllBytesAsync(filePath, document);

                    LogService.LogInformation($"[WhatsApp] PDF saved to: {filePath}");

                    // Generate public download link
                    var baseUrl = $"{Request.Scheme}://{Request.Host}";
                    var downloadUrl = $"{baseUrl}/downloads/{fileName}";
                    
                    var message = $"مرحباً،%0A%0Aتم إنشاء تقرير فواتير المشتريات بنجاح.%0A%0Aلتحميل التقرير:%0A{downloadUrl}%0A%0Aحجم الملف: {document.Length / 1024} كيلوبايت";
                    var whatsappLink = $"https://wa.me/{cleanReceiver}?text={message}";

                    LogService.LogInformation($"[WhatsApp] Link: {whatsappLink}");
                    
                    var response = new DocumentShareResponseDto<string>
                    {
                        Id = $"{shareRequest.StartDate}_{shareRequest.EndDate}",
                        Method = shareRequest.Method,
                        Receiver = cleanReceiver,
                        Message = "Purchase invoice report WhatsApp link generated successfully",
                        IsSuccess = true,
                        WhatsAppLink = whatsappLink,
                        DownloadLink = downloadUrl
                    };
                    return Ok(response);
                }
                else
                {
                    // Email sharing using DocumentShareService
                    LogService.LogInformation($"[Email] Sending to: {shareRequest.Receiver}");
                    
                    await _documentShareService.ShareDocumentAsync(document, shareRequest.Method, shareRequest.Receiver);

                    LogService.LogInformation($"[Email] Successfully sent to: {shareRequest.Receiver}");
                    
                    var response = new DocumentShareResponseDto<string>
                    {
                        Id = $"{shareRequest.StartDate}_{shareRequest.EndDate}",
                        Method = shareRequest.Method,
                        Receiver = shareRequest.Receiver,
                        Message = "Purchase invoice report sent successfully",
                        IsSuccess = true
                    };
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                LogService.LogError($"SharePurchaseInvoiceReportDocument error: {ex.Message}");
                LogService.LogError($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Error sharing purchase invoice report", details = ex.Message });
            }
        }

        [HttpGet("NetVATReport")]
        public async Task<IActionResult> GetNetPurchaseVATReport([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] string language = "ar")
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            try
            {
                var reportData = await _purchaseInvoiceService.GetNetPurchaseVATReport(startDate, endDate, language);
                return Ok(reportData);
            }
            catch (Exception ex)
            {
                LogService.LogError($"GetNetPurchaseVATReport controller error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while generating net purchase VAT report.", error = ex.Message });
            }
        }

        [HttpGet("NetVATReport/document")]
        public async Task<IActionResult> GetNetPurchaseVATDocument([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] string language = "ar")
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            try
            {
                var document = await _purchaseInvoiceService.GetNetPurchaseVATDocument(startDate, endDate, language);
                
                if (document != null && document.Length > 0)
                {
                    var fileName = $"net-purchase-vat-report-{startDate:yyyyMMdd}-{endDate:yyyyMMdd}.pdf";
                    return File(document, "application/pdf", fileName);
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                LogService.LogError($"GetNetPurchaseVATDocument controller error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while generating PDF.", error = ex.Message });
            }
        }

        [HttpPost("NetVATReport/share")]
        public async Task<IActionResult> ShareNetPurchaseVATReport([FromBody] NetPurchaseVATReportShareRequest shareRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                LogService.LogError($"Model validation failed: {errors}");
                return BadRequest(new { message = "Invalid request data", errors = ModelState });
            }

            try
            {
                LogService.LogInformation($"Net Purchase VAT Share request received - StartDate: {shareRequest.StartDate}, EndDate: {shareRequest.EndDate}, Method: {shareRequest.Method}, Language: {shareRequest.Language ?? "ar"}");
                
                var document = await _purchaseInvoiceService.GetNetPurchaseVATDocument(shareRequest.StartDate, shareRequest.EndDate, shareRequest.Language ?? "ar");
                
                if (document == null || document.Length == 0)
                {
                    LogService.LogError("Net Purchase VAT report document is null or empty");
                    return NotFound("Net Purchase VAT report document not found or could not be generated.");
                }

                LogService.LogInformation($"Net Purchase VAT Document generated successfully, size: {document.Length} bytes");

                // Handle WhatsApp sharing differently
                if (shareRequest.Method.Equals("WhatsApp", StringComparison.OrdinalIgnoreCase))
                {
                    var cleanReceiver = shareRequest.Receiver.Replace("+", "").Replace(" ", "").TrimStart('0');
                    
                    // Ensure phone number has country code
                    var hasCountryCode = cleanReceiver.StartsWith("1") || 
                                        cleanReceiver.StartsWith("20") || 
                                        cleanReceiver.StartsWith("44") || 
                                        cleanReceiver.StartsWith("91") ||
                                        cleanReceiver.StartsWith("966") || 
                                        cleanReceiver.StartsWith("971") ||
                                        cleanReceiver.StartsWith("974") ||
                                        cleanReceiver.StartsWith("965");
                    
                    if (!hasCountryCode && cleanReceiver.Length >= 9)
                    {
                        cleanReceiver = "966" + cleanReceiver;
                    }

                    // Save PDF to wwwroot/downloads folder for public access
                    var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "downloads");
                    Directory.CreateDirectory(webRootPath);

                    var fileName = $"Net_Purchase_VAT_Report_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.pdf";
                    var filePath = Path.Combine(webRootPath, fileName);

                    await System.IO.File.WriteAllBytesAsync(filePath, document);

                    LogService.LogInformation($"[WhatsApp] PDF saved to: {filePath}");

                    // Generate public download link
                    var baseUrl = $"{Request.Scheme}://{Request.Host}";
                    var downloadUrl = $"{baseUrl}/downloads/{fileName}";
                    
                    var message = $"مرحباً،%0A%0Aتم إنشاء تقرير صافي القيمة المضافة للمشتريات بنجاح.%0A%0Aلتحميل التقرير:%0A{downloadUrl}%0A%0Aحجم الملف: {document.Length / 1024} كيلوبايت";
                    var whatsappLink = $"https://wa.me/{cleanReceiver}?text={message}";

                    LogService.LogInformation($"[WhatsApp] Link: {whatsappLink}");
                    
                    var response = new DocumentShareResponseDto<string>
                    {
                        Id = $"{shareRequest.StartDate}_{shareRequest.EndDate}",
                        Method = shareRequest.Method,
                        Receiver = cleanReceiver,
                        Message = "Net Purchase VAT report WhatsApp link generated successfully",
                        IsSuccess = true,
                        WhatsAppLink = whatsappLink,
                        DownloadLink = downloadUrl
                    };
                    return Ok(response);
                }
                else
                {
                    // Email sharing using DocumentShareService
                    LogService.LogInformation($"[Email] Sending to: {shareRequest.Receiver}");
                    
                    await _documentShareService.ShareDocumentAsync(document, shareRequest.Method, shareRequest.Receiver);

                    LogService.LogInformation($"[Email] Successfully sent to: {shareRequest.Receiver}");
                    
                    var response = new DocumentShareResponseDto<string>
                    {
                        Id = $"{shareRequest.StartDate}_{shareRequest.EndDate}",
                        Method = shareRequest.Method,
                        Receiver = shareRequest.Receiver,
                        Message = "Net Purchase VAT report sent successfully",
                        IsSuccess = true
                    };
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                LogService.LogError($"ShareNetPurchaseVATReport error: {ex.Message}");
                LogService.LogError($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Error sharing net purchase VAT report", details = ex.Message });
            }
        }
        #endregion

        #region Methods
        private async Task<string> GetInvoiceNumber()
        {
            var lastInvoiceNo = await _service.GetLastInvoiceNo();
            return GenerateNewInvoiceNo(lastInvoiceNo);
        }

        private string GenerateNewInvoiceNo(string lastInvoiceNo)
        {
            if (string.IsNullOrEmpty(lastInvoiceNo)) return inv + 1;

            var digits = new string(lastInvoiceNo.Where(char.IsDigit).ToArray());
            int.TryParse(digits, out int oldNum);

            return inv + (oldNum + 1);
        }
        #endregion
    }
}
