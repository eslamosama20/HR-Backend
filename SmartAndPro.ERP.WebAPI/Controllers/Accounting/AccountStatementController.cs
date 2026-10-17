using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.AccountStatement;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Application.Services.Accounting;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.SharedKernel.Dtos.DocumentShare;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.SharedKernel.Utilities.ShareHandlers;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;

namespace SmartAndPro.ERP.WebAPI.Controllers.Accounting;
[Route("api/accounting/[controller]/")]
[ApiController]
[Authorize]
[Authorize]
[PermissionAuthorize("AccountStatement.Read")]
public class AccountStatementController : ControllerBase
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly IDocumentShareService _documentShareService;
    
    public AccountStatementController(ServiceBaseFactory serviceBaseFactory, IDocumentShareService documentShareService)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _documentShareService = documentShareService;
    }
    
    
    
    [HttpGet("")]
    
    public async Task<IActionResult> GetAccountStatement([FromQuery] AccountStatementRequest accountStatementRequest)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _serviceBaseFactory.GetService<AccountStatementService>().GetAccountStatement(accountStatementRequest.StartDate, accountStatementRequest.EndDate, accountStatementRequest.AccountId, accountStatementRequest.ProjectId, accountStatementRequest.IsFrontal);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    [HttpGet("document")]
    public async Task<IActionResult> GetDocument([FromQuery] AccountStatementRequest accountStatementRequest)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var document = await _serviceBaseFactory.GetService<AccountStatementService>().GetDocument(
                accountStatementRequest.StartDate, 
                accountStatementRequest.EndDate, 
                accountStatementRequest.AccountId, 
                accountStatementRequest.ProjectId,
                accountStatementRequest.IsFrontal);
            
            if (document != null && document.Length > 0)
            {
                var fileName = $"account-statement-{accountStatementRequest.AccountId}-{accountStatementRequest.StartDate:yyyyMMdd}-{accountStatementRequest.EndDate:yyyyMMdd}.pdf";
                result = File(document, "application/pdf", fileName);
            }
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            result = StatusCode(500, ex.Message);
        }
        return result;
    }

    [HttpPost("share")]
    public async Task<IActionResult> ShareAccountStatement([FromBody] AccountStatementShareRequest shareRequest)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            LogService.LogError($"Model validation failed: {errors}");
            return BadRequest(new { message = "Invalid request data", errors = ModelState });
        }

        try
        {
            LogService.LogInformation($"Share request received - AccountId: {shareRequest.AccountId}, StartDate: {shareRequest.StartDate}, EndDate: {shareRequest.EndDate}, Method: {shareRequest.Method}");
            
            var accountStatementService = _serviceBaseFactory.GetService<AccountStatementService>();
            var document = await accountStatementService.GetDocument(
                shareRequest.StartDate, 
                shareRequest.EndDate, 
                shareRequest.AccountId, 
                shareRequest.ProjectId,
                shareRequest.IsFrontal);
            
            if (document == null || document.Length == 0)
            {
                LogService.LogError("Account statement document is null or empty");
                return NotFound("Account statement document not found or could not be generated.");
            }

            LogService.LogInformation($"Document generated successfully, size: {document.Length} bytes");

            await _documentShareService.ShareDocumentAsync(document, shareRequest.Method, shareRequest.Receiver);
            
            LogService.LogInformation($"Document shared successfully via {shareRequest.Method} to {shareRequest.Receiver}");
            
            var response = new DocumentShareResponseDto<string>
            {
                Id = $"{shareRequest.AccountId}_{shareRequest.StartDate}_{shareRequest.EndDate}",
                Method = shareRequest.Method,
                Receiver = shareRequest.Receiver,
                Message = "Account statement shared successfully",
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
            return StatusCode(500, new { message = "An unexpected error occurred while sharing the account statement.", error = ex.Message });
        }
    }

    [HttpPost("whatsappShare")]
    public async Task<IActionResult> ShareAccountStatementOnWhatsApp([FromBody] AccountStatementShareRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var handler = new WhatsAppShareHandler<string>();
            var baseUrl = "https://erp.smartapro.com/account-statement";
            var statementId = $"{dto.AccountId}_{dto.StartDate}_{dto.EndDate}";
            var response = handler.BuildLinks(new[] { statementId }, dto.Receiver, baseUrl);
            return Ok(new { response, statementId });
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, "An unexpected error occurred while sharing the account statement on WhatsApp.");
        }
    }

    // Summary Account Statement Endpoints
    [HttpGet("summary/document")]
    public async Task<IActionResult> GetSummaryDocument([FromQuery] AccountStatementRequest accountStatementRequest)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var document = await _serviceBaseFactory.GetService<AccountStatementService>().GetSummaryDocument(
                accountStatementRequest.StartDate, 
                accountStatementRequest.EndDate, 
                accountStatementRequest.AccountId, 
                accountStatementRequest.ProjectId,
                accountStatementRequest.IsFrontal);
            
            if (document == null || document.Length == 0)
            {
                LogService.LogError("Summary account statement document is null or empty after generation.");
                return NotFound("Summary account statement document not found or could not be generated.");
            }

            return File(document, "application/pdf", $"SummaryAccountStatement_{accountStatementRequest.AccountId}_{accountStatementRequest.StartDate:yyyyMMdd}_{accountStatementRequest.EndDate:yyyyMMdd}.pdf");
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "An error occurred while generating the summary account statement PDF.", error = ex.Message });
        }
    }

    [HttpPost("summary/share")]
    public async Task<IActionResult> ShareSummaryAccountStatement([FromBody] AccountStatementShareRequest shareRequest)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            LogService.LogError($"Model validation failed: {errors}");
            return BadRequest(new { message = "Invalid request data", errors = ModelState });
        }

        try
        {
            LogService.LogInformation($"Share summary request received - AccountId: {shareRequest.AccountId}, StartDate: {shareRequest.StartDate}, EndDate: {shareRequest.EndDate}, Method: {shareRequest.Method}");
            
            var accountStatementService = _serviceBaseFactory.GetService<AccountStatementService>();
            var document = await accountStatementService.GetSummaryDocument(
                shareRequest.StartDate, 
                shareRequest.EndDate, 
                shareRequest.AccountId, 
                shareRequest.ProjectId,
                shareRequest.IsFrontal);
            
            if (document == null || document.Length == 0)
            {
                LogService.LogError("Summary account statement document is null or empty");
                return NotFound("Summary account statement document not found or could not be generated.");
            }

            LogService.LogInformation($"Summary document generated successfully, size: {document.Length} bytes");

            await _documentShareService.ShareDocumentAsync(document, shareRequest.Method, shareRequest.Receiver);
            
            LogService.LogInformation($"Summary document shared successfully via {shareRequest.Method} to {shareRequest.Receiver}");
            
            var response = new DocumentShareResponseDto<string>
            {
                Id = $"{shareRequest.AccountId}_{shareRequest.StartDate}_{shareRequest.EndDate}",
                Method = shareRequest.Method,
                Receiver = shareRequest.Receiver,
                Message = "Summary account statement shared successfully",
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
            return StatusCode(500, new { message = "An unexpected error occurred while sharing the summary account statement.", error = ex.Message });
        }
    }

    [HttpPost("summary/whatsappShare")]
    public async Task<IActionResult> ShareSummaryAccountStatementOnWhatsApp([FromBody] AccountStatementShareRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var handler = new WhatsAppShareHandler<string>();
            var baseUrl = "https://erp.smartapro.com/summary-account-statement";
            var statementId = $"{dto.AccountId}_{dto.StartDate}_{dto.EndDate}";
            var response = handler.BuildLinks(new[] { statementId }, dto.Receiver, baseUrl);
            return Ok(new { response, statementId });
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, "An unexpected error occurred while sharing the summary account statement on WhatsApp.");
        }
    }
    
}