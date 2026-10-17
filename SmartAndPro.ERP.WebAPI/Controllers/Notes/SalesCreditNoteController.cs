using System.Net;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.SharedKernel.Constants;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Common;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Common.Dtos.Notes.SalesCreditNote;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.Accounting;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.Notes.CreditNotes;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Common.Dtos;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.SharedKernel.Dtos.DocumentShare;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Notes;
[Route("api/notes/[controller]/")]
[ApiController]
[PermissionAuthorize("SalesCreditNote.Read")]
public class SalesCreditNoteController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly SalesCreditNoteService _service;
    private readonly ILogger<SalesCreditNoteController> _logger;
    private readonly IDocumentShareService _documentShareService;
    
    public SalesCreditNoteController(
        ServiceBaseFactory serviceBaseFactory,
        ILogger<SalesCreditNoteController> logger,
        IDocumentShareService documentShareService)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<SalesCreditNoteService>();
        _logger = logger;
        _documentShareService = documentShareService;
    }
    
    [HttpPost("clearCreditNote")]
    public async Task<IActionResult> ClearCreditNote([FromBody] IdObject idObject)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = new ContentResult
        {
            StatusCode = (int)HttpStatusCode.InternalServerError,
            Content = "An error occurred while processing your request.",
            ContentType = "text/plain"
        };
        try
        {
            await _service.ZatcaClearSalesCreditNote(idObject.Id);
            result = new ContentResult
            {
                StatusCode = (int)HttpStatusCode.OK,
                Content = "Invoice cleared successfully.",
                ContentType = "text/plain"
            };
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    [HttpPost("createTransaction")]
    public async Task<IActionResult> CreateTransaction([FromQuery] Guid salesCreditNoteId)
    {

        try
        {
            await _service.CreateTransaction(salesCreditNoteId);
            return Ok();
        }
        catch (Exception e)
        {
            LogService.LogError(e);
            return BadRequest("Failed to create the transaction.");
        }
    }

    // CreditNotes get all
    
    [HttpGet("")]
    public async Task<IActionResult> GetSalesCreditNotes([FromQuery] SalesCreditNoteFiltersDto filters )
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllAsync(filters);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // CreditNotes get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSalesCreditNotesById(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // CreditNotes create
    [HttpPost("")]
    [PermissionAuthorize(Permissions.SalesCreditNote.Create)]
    public async Task<IActionResult> CreateSalesCreditNotes([FromBody] SalesCreditNotePostDto salesCreditNote)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(salesCreditNote);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // CreditNotes update
    [HttpPut]
    [PermissionAuthorize(Permissions.SalesCreditNote.Update)]
    public async Task<IActionResult> UpdateSalesCreditNotes([FromBody] SalesCreditNotePutDto salesCreditNote)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(salesCreditNote);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // CreditNotes delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.SalesCreditNote.Delete)]
    public async Task<IActionResult> DeleteSalesCreditNotes(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var creditNote =await _service.GetAsync(id);
            var transactionService =  _serviceBaseFactory.GetService<TransactionService>();
            if (creditNote.Data.TransactionId != null)
            {
                await transactionService.DeleteAsync((Guid)creditNote.Data.TransactionId);
            }
            var responseDate = await _service.DeleteAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    [HttpGet("VATReport")]
    public async Task<IActionResult> GetSalesCreditNoteVATReport([FromQuery] string startDate, [FromQuery] string endDate)
    {
        if (string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
        {
            _logger.LogWarning("Start date or end date is null or empty");
            return BadRequest("Start date and end date are required");
        }

        if (!DateOnly.TryParse(startDate, out DateOnly parsedStartDate))
        {
            _logger.LogWarning("Invalid start date format: {StartDate}", startDate);
            return BadRequest("Invalid start date format. Use YYYY-MM-DD");
        }

        if (!DateOnly.TryParse(endDate, out DateOnly parsedEndDate))
        {
            _logger.LogWarning("Invalid end date format: {EndDate}", endDate);
            return BadRequest("Invalid end date format. Use YYYY-MM-DD");
        }

        try
        {
            var result = await _service.GetSalesCreditNoteVATReport(parsedStartDate, parsedEndDate);
            if (result == null || !result.Any())
            {
                _logger.LogInformation("No sales credit note VAT data found for date range: {StartDate} to {EndDate}", startDate, endDate);
                return Ok(new { data = new List<object>(), message = "No data found for the specified date range" });
            }

            _logger.LogInformation("Successfully retrieved sales credit note VAT report with {Count} records", result.Count());
            return Ok(new { data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting sales credit note VAT report");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    [HttpPost("Report/VAT/share")]
    public async Task<IActionResult> ShareSalesCreditNoteVATReport([FromBody] SalesCreditNoteVATReportShareRequest shareRequest)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            LogService.LogError($"Model validation failed: {errors}");
            return BadRequest(new { message = "Invalid request data", errors = ModelState });
        }

        try
        {
            LogService.LogInformation($"Sales Credit Note VAT Share request received - StartDate: {shareRequest.StartDate}, EndDate: {shareRequest.EndDate}, Method: {shareRequest.Method}, Language: {shareRequest.Language ?? "ar"}");
            
            var document = await _service.GetSalesCreditNoteVATDocument(shareRequest.StartDate, shareRequest.EndDate, shareRequest.Language ?? "ar");
            
            if (document == null || document.Length == 0)
            {
                LogService.LogError("Sales Credit Note VAT report document is null or empty");
                return NotFound("Sales Credit Note VAT report document not found or could not be generated.");
            }

            LogService.LogInformation($"Sales Credit Note VAT Document generated successfully, size: {document.Length} bytes");

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

                var fileName = $"Sales_Credit_Note_VAT_Report_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.pdf";
                var filePath = Path.Combine(webRootPath, fileName);

                await System.IO.File.WriteAllBytesAsync(filePath, document);

                LogService.LogInformation($"[WhatsApp] PDF saved to: {filePath}");

                // Generate public download link
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var downloadUrl = $"{baseUrl}/downloads/{fileName}";
                
                var message = $"مرحباً،%0A%0Aتم إنشاء تقرير القيمة المضافة لمرتجعات المبيعات بنجاح.%0A%0Aلتحميل التقرير:%0A{downloadUrl}%0A%0Aحجم الملف: {document.Length / 1024} كيلوبايت";
                var whatsappLink = $"https://wa.me/{cleanReceiver}?text={message}";

                LogService.LogInformation($"[WhatsApp] Link: {whatsappLink}");
                
                var response = new DocumentShareResponseDto<string>
                {
                    Id = $"{shareRequest.StartDate}_{shareRequest.EndDate}",
                    Method = shareRequest.Method,
                    Receiver = cleanReceiver,
                    Message = "Sales Credit Note VAT report WhatsApp link generated successfully",
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
                    Message = "Sales Credit Note VAT report sent successfully",
                    IsSuccess = true
                };
                return Ok(response);
            }
        }
        catch (Exception ex)
        {
            LogService.LogError($"ShareSalesCreditNoteVATReport error: {ex.Message}");
            LogService.LogError($"StackTrace: {ex.StackTrace}");
            return StatusCode(500, new { message = "Error sharing sales credit note VAT report", details = ex.Message });
        }
    }

    [HttpGet("Report/VAT/download")]
    public async Task<IActionResult> DownloadSalesCreditNoteVATReport([FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] string language = "ar")
    {
        if (string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
        {
            _logger.LogWarning("Start date or end date is null or empty");
            return BadRequest("Start date and end date are required");
        }

        if (!DateOnly.TryParse(startDate, out DateOnly parsedStartDate))
        {
            _logger.LogWarning("Invalid start date format: {StartDate}", startDate);
            return BadRequest("Invalid start date format. Use YYYY-MM-DD");
        }

        if (!DateOnly.TryParse(endDate, out DateOnly parsedEndDate))
        {
            _logger.LogWarning("Invalid end date format: {EndDate}", endDate);
            return BadRequest("Invalid end date format. Use YYYY-MM-DD");
        }

        try
        {
            var document = await _service.GetSalesCreditNoteVATDocument(parsedStartDate, parsedEndDate, language);
            
            if (document == null || document.Length == 0)
            {
                LogService.LogError("Sales Credit Note VAT report document is null or empty");
                return NotFound("Sales Credit Note VAT report document not found or could not be generated.");
            }

            var fileName = $"Sales_Credit_Note_VAT_Report_{startDate}_{endDate}.pdf";
            return File(document, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while downloading sales credit note VAT report");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

}