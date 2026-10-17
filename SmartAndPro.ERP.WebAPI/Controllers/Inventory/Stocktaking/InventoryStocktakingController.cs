using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.Stocktaking.InventoryStocktaking;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Application.Services.Accounting;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.StocktakingManagement;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Dtos.DocumentShare;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Utils;
using SmartAndPro.ERP.WebAPI.Attributes;
using System.Security.Claims;

namespace SmartAndPro.ERP.WebAPI.Controllers.Inventory.Stocktaking;
[Route("api/inventory/[controller]")]
[ApiController]
[PermissionAuthorize(SmartAndPro.ERP.SharedKernel.Constants.Permissions.InventoryStocktaking.Read)]
public class InventoryStocktakingController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly InventoryStocktakingService _service;
    private readonly IDocumentShareService _documentShareService;
    private readonly RepositoriesFactory _repositoriesFactory;

    public InventoryStocktakingController(ServiceBaseFactory serviceBaseFactory, IDocumentShareService documentShareService, RepositoriesFactory repositoriesFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<InventoryStocktakingService>();
        _documentShareService = documentShareService;
        _repositoriesFactory = repositoriesFactory;
    }

    private async Task<string> GetCurrentUserDisplayName()
    {
        var userId = User.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return User.FindFirst(ClaimTypes.GivenName)?.Value ?? "المستخدم";
        }
        
        var userRepository = _repositoriesFactory.GetRepository<User>();
        var userResult = await userRepository.Filter(u => u.Id == userGuid);
        var user = userResult.Data.FirstOrDefault();
        
        if (user != null)
        {
            if (!string.IsNullOrEmpty(user.FirstName))
                return $"{user.FirstName} {user.LastName}".Trim();
            return user.UserName ?? "المستخدم";
        }
        
        return User.FindFirst(ClaimTypes.GivenName)?.Value ?? "المستخدم";
    }

    // InventoryStocktakings get all
    
    [HttpGet("")]
    public async Task<IActionResult> GetInventoryStocktakings([FromQuery] InventoryStocktakingFiltersDto filters )
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
    
    // InventoryStocktakings get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetInventoryStocktakingsById(Guid id)
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
    
    // InventoryStocktakings create
    [HttpPost("")]
    [PermissionAuthorize(SmartAndPro.ERP.SharedKernel.Constants.Permissions.InventoryStocktaking.Create)]
    public async Task<IActionResult> CreateInventoryStocktakings([FromBody] InventoryStocktakingPostDto inventoryStocktaking)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(inventoryStocktaking);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // InventoryStocktakings update
    [HttpPut]
    [PermissionAuthorize(SmartAndPro.ERP.SharedKernel.Constants.Permissions.InventoryStocktaking.Update)]
    public async Task<IActionResult> UpdateInventoryStocktakings([FromBody] InventoryStocktakingPutDto inventoryStocktaking)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(inventoryStocktaking);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // InventoryStocktakings delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(SmartAndPro.ERP.SharedKernel.Constants.Permissions.InventoryStocktaking.Delete)]
    public async Task<IActionResult> DeleteInventoryStocktakings(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var inventoryStocktaking =await _service.GetAsync(id);
            var transactionService =  _serviceBaseFactory.GetService<TransactionService>();
            if (inventoryStocktaking.Data.TransactionId != null)
            {
                await transactionService.DeleteAsync((Guid)inventoryStocktaking.Data.TransactionId);
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

    // ====================================
    // Stocktaking Report Endpoints
    // ====================================

    /// <summary>
    /// Get stocktaking report as PDF document
    /// </summary>
    [HttpGet("report/document")]
    public async Task<IActionResult> GetStocktakingReportDocument(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var printedBy = await GetCurrentUserDisplayName();
            var document = await _service.GetStocktakingReportDocument(startDate, endDate, printedBy);

            if (document == null || document.Length == 0)
            {
                LogService.LogError("Stocktaking report document is null or empty");
                return NotFound("Stocktaking report document not found or could not be generated.");
            }

            var fileName = $"stocktaking-report-{DateTime.Now:yyyyMMddHHmmss}.pdf";
            return File(document, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "An error occurred while generating the stocktaking report PDF.", error = ex.Message });
        }
    }

    /// <summary>
    /// Share stocktaking report via email or WhatsApp
    /// </summary>
    [HttpPost("report/share")]
    public async Task<IActionResult> ShareStocktakingReport([FromBody] StocktakingReportShareRequest shareRequest)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            LogService.LogError($"Model validation failed: {errors}");
            return BadRequest(new { message = "Invalid request data", errors = ModelState });
        }

        try
        {
            LogService.LogInformation($"Share stocktaking report request - Method: {shareRequest.Method}");

            var printedBy = await GetCurrentUserDisplayName();
            var document = await _service.GetStocktakingReportDocument(
                shareRequest.StartDate,
                shareRequest.EndDate,
                printedBy);

            if (document == null || document.Length == 0)
            {
                LogService.LogError("Stocktaking report document is null or empty");
                return NotFound("Stocktaking report document not found or could not be generated.");
            }

            LogService.LogInformation($"Document generated successfully, size: {document.Length} bytes");

            var receiver = shareRequest.Method?.ToLower() == "email" ? shareRequest.Email : shareRequest.Phone;
            await _documentShareService.ShareDocumentAsync(document, shareRequest.Method, receiver);

            LogService.LogInformation($"Document shared successfully via {shareRequest.Method} to {receiver}");

            var response = new DocumentShareResponseDto<string>
            {
                Id = $"stocktaking-{DateTime.Now:yyyyMMdd}",
                Method = shareRequest.Method,
                Receiver = receiver,
                Message = "Stocktaking report shared successfully",
                IsSuccess = true
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "An unexpected error occurred while sharing the stocktaking report.", error = ex.Message });
        }
    }

}

public class StocktakingReportShareRequest
{
    public string? Method { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}