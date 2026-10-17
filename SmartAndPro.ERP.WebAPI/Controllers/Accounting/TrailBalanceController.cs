using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.TrailBalance;
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
[PermissionAuthorize("ReviewBalance.Read")]
public class TrailBalanceController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly IDocumentShareService _documentShareService;
    
    public TrailBalanceController(ServiceBaseFactory serviceBaseFactory, IDocumentShareService documentShareService)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _documentShareService = documentShareService;
    }
    
    
    
    [HttpGet("")]
    
    public async Task<IActionResult> GetTrailBalanceData([FromQuery] TrailBalanceRequestDto trailBalanceRequest)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _serviceBaseFactory.GetService<TrailBalanceService>().GetTrailBalanceData(trailBalanceRequest.StartDate, trailBalanceRequest.EndDate);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    [HttpGet("document")]
    public async Task<IActionResult> GetDocument([FromQuery] TrailBalanceRequestDto trailBalanceRequest)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var document = await _serviceBaseFactory.GetService<TrailBalanceService>().GetDocument(
                trailBalanceRequest.StartDate, 
                trailBalanceRequest.EndDate, 
                trailBalanceRequest.Level ?? "All");
            
            if (document != null && document.Length > 0)
            {
                var fileName = $"trail-balance-{trailBalanceRequest.Level ?? "All"}-{trailBalanceRequest.StartDate:yyyyMMdd}-{trailBalanceRequest.EndDate:yyyyMMdd}.pdf";
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
    public async Task<IActionResult> ShareTrailBalance([FromBody] TrailBalanceShareRequest shareRequest)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            LogService.LogError($"Model validation failed: {errors}");
            return BadRequest(new { message = "Invalid request data", errors = ModelState });
        }

        try
        {
            LogService.LogInformation($"Share request received - StartDate: {shareRequest.StartDate}, EndDate: {shareRequest.EndDate}, Level: {shareRequest.Level}, Method: {shareRequest.Method}");
            
            var trailBalanceService = _serviceBaseFactory.GetService<TrailBalanceService>();
            var document = await trailBalanceService.GetDocument(
                shareRequest.StartDate, 
                shareRequest.EndDate, 
                shareRequest.Level);
            
            if (document == null || document.Length == 0)
            {
                LogService.LogError("Trail balance document is null or empty");
                return NotFound("Trail balance document not found or could not be generated.");
            }

            LogService.LogInformation($"Document generated successfully, size: {document.Length} bytes");

            await _documentShareService.ShareDocumentAsync(document, shareRequest.Method, shareRequest.Receiver);
            
            LogService.LogInformation($"Document shared successfully via {shareRequest.Method} to {shareRequest.Receiver}");
            
            var response = new DocumentShareResponseDto<string>
            {
                Id = $"{shareRequest.Level}_{shareRequest.StartDate}_{shareRequest.EndDate}",
                Method = shareRequest.Method,
                Receiver = shareRequest.Receiver,
                Message = "Trail balance shared successfully",
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
            return StatusCode(500, new { message = "An unexpected error occurred while sharing the trail balance.", error = ex.Message });
        }
    }

    [HttpPost("whatsappShare")]
    public async Task<IActionResult> ShareTrailBalanceOnWhatsApp([FromBody] TrailBalanceShareRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var handler = new WhatsAppShareHandler<string>();
            var baseUrl = "https://erp.smartapro.com/trail-balance";
            var balanceId = $"{dto.Level}_{dto.StartDate}_{dto.EndDate}";
            var response = handler.BuildLinks(new[] { balanceId }, dto.Receiver, baseUrl);
            return Ok(new { response, balanceId });
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, "An unexpected error occurred while sharing the trail balance on WhatsApp.");
        }
    }
    
}