using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SmartAndPro.ERP.Application.Common.Dtos.Permits.InventoryPermit;
using SmartAndPro.ERP.Application.Common.Dtos.Reports.InventoryStocks;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Application.Services.Accounting;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Permits;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Dtos.DocumentShare;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.SharedKernel.Utilities.ShareHandlers;
using SmartAndPro.ERP.WebAPI.Utils;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Permits;
[Route("api/[controller]")]
[ApiController]
[PermissionAuthorize(SmartAndPro.ERP.SharedKernel.Constants.Permissions.InventoryPermit.Read)]
public class InventoryPermitController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly InventoryPermitService _service;
    private readonly IDocumentShareService _documentShareService;
    private readonly RepositoriesFactory _repositoriesFactory;

    public InventoryPermitController(ServiceBaseFactory serviceBaseFactory, IDocumentShareService documentShareService, RepositoriesFactory repositoriesFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<InventoryPermitService>();
        _documentShareService = documentShareService;
        _repositoriesFactory = repositoriesFactory;
    }

    /// <summary>
    /// Get the current user's display name from the database
    /// </summary>
    private async Task<string> GetCurrentUserDisplayName()
    {
        try
        {
            var userIdString = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                return "";
            
            var userQuery = await _repositoriesFactory.GetRepository<User>().Filter(u => u.Id == userId);
            var user = userQuery.Data?.FirstOrDefault();
            
            if (user == null) return "";
            
            // Return FirstName + LastName, or UserName, or Email
            if (!string.IsNullOrEmpty(user.FirstName))
            {
                return $"{user.FirstName} {user.LastName}".Trim();
            }
            return user.UserName ?? user.Email ?? "";
        }
        catch
        {
            return "";
        }
    }

    // InventoryPermits get all
    
    [HttpGet("")]
    public async Task<IActionResult> GetInventoryPermits([FromQuery] InventoryPermitFiltersDto filters )
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
    
    // InventoryPermits get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetInventoryPermitsById(Guid id)
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

    [HttpGet("product/barcode/{barcode}")]
    public async Task<IActionResult> GetProductByBarcode(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return BadRequest("Barcode is required.");
        }

        var product = await _service.GetProductByBarcodeAsync(barcode);
        if (product == null)
        {
            return NotFound(new { message = "Product not found for the provided barcode." });
        }

        return Ok(product);
    }
    
    // InventoryPermits create
    [HttpPost("")]
    [PermissionAuthorize(SmartAndPro.ERP.SharedKernel.Constants.Permissions.InventoryPermit.Create)]
    public async Task<IActionResult> CreateInventoryPermits([FromBody] InventoryPermitPostDto inventoryPermit)
    {
        // Log incoming request for debugging
        LogService.LogInformation($"[InventoryPermitController] Received PermitType: {inventoryPermit.PermitType}");
        LogService.LogInformation($"[InventoryPermitController] PermitEntries count: {inventoryPermit.PermitEntries?.Count ?? 0}");
        if (inventoryPermit.PermitEntries != null)
        {
            foreach (var entry in inventoryPermit.PermitEntries)
            {
                LogService.LogInformation($"[InventoryPermitController] Entry: PurchaseClassId={entry.MainPurchaseClassId}, Qty={entry.Quantity}, ExpiryDate={entry.ExpiryDate}, ShelfNumber={entry.ShelfNumber}, RowNumber={entry.RowNumber}");
            }
        }
        
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(inventoryPermit);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // InventoryPermits update
    [HttpPut]
    [PermissionAuthorize(SmartAndPro.ERP.SharedKernel.Constants.Permissions.InventoryPermit.Update)]
    public async Task<IActionResult> UpdateInventoryPermits([FromBody] InventoryPermitPutDto inventoryPermit)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(inventoryPermit);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // InventoryPermits delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(SmartAndPro.ERP.SharedKernel.Constants.Permissions.InventoryPermit.Delete)]
    public async Task<IActionResult> DeleteInventoryPermits(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var inventoryPermit =await _service.GetAsync(id);
            var transactionService =  _serviceBaseFactory.GetService<TransactionService>();
            if (inventoryPermit.Data.TransactionId != null)
            {
                await transactionService.DeleteAsync((Guid)inventoryPermit.Data.TransactionId);
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
    // Permits Report Endpoints
    // ====================================

    /// <summary>
    /// Get inventory permits report as PDF document
    /// </summary>
    [HttpGet("Report/permits-document")]
    public async Task<IActionResult> GetPermitsReportDocument(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string language = "ar")
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var printedBy = await GetCurrentUserDisplayName();
            var document = await _service.GetPermitsReportDocument(startDate, endDate, language, printedBy);

            if (document == null || document.Length == 0)
            {
                LogService.LogError("Permits report document is null or empty");
                return NotFound("Permits report document not found or could not be generated.");
            }

            var fileName = $"inventory-permits-report-{DateTime.Now:yyyyMMddHHmmss}.pdf";
            return File(document, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "An error occurred while generating the permits report PDF.", error = ex.Message });
        }
    }

    /// <summary>
    /// Share inventory permits report via email or WhatsApp
    /// </summary>
    [HttpPost("Report/permits-share")]
    public async Task<IActionResult> SharePermitsReport([FromBody] PermitsReportShareRequest shareRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            LogService.LogInformation($"Share permits report request - Method: {shareRequest.Method}, Receiver: {shareRequest.Receiver}");

            var printedBy = await GetCurrentUserDisplayName();
            var document = await _service.GetPermitsReportDocument(
                shareRequest.StartDate,
                shareRequest.EndDate,
                shareRequest.Language ?? "ar",
                printedBy);

            if (document == null || document.Length == 0)
            {
                LogService.LogError("Permits report document is null or empty");
                return NotFound("Permits report document not found or could not be generated.");
            }

            LogService.LogInformation($"Document generated successfully, size: {document.Length} bytes");

            // Share the document using the document share service
            await _documentShareService.ShareDocumentAsync(document, shareRequest.Method, shareRequest.Receiver);

            LogService.LogInformation($"Document shared successfully via {shareRequest.Method} to {shareRequest.Receiver}");

            var response = new DocumentShareResponseDto<string>
            {
                Id = $"permits-{DateTime.Now:yyyyMMdd}",
                Method = shareRequest.Method,
                Receiver = shareRequest.Receiver,
                Message = "Permits report shared successfully",
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
            return StatusCode(500, new { message = "An unexpected error occurred while sharing the permits report.", error = ex.Message });
        }
    }

    // ====================================
    // Inventory Report Endpoints
    // ====================================

    /// <summary>
    /// Get inventory report as PDF document
    /// </summary>
    [HttpGet("Report/inventory-document")]
    public async Task<IActionResult> GetInventoryDocument(
        [FromQuery] Guid storeId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string language = "ar")
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var printedBy = await GetCurrentUserDisplayName();
            var document = await _service.GetInventoryReportDocument(storeId, startDate, endDate, language, printedBy);

            if (document == null || document.Length == 0)
            {
                LogService.LogError("Inventory report document is null or empty");
                return NotFound("Inventory report document not found or could not be generated.");
            }

            var fileName = $"inventory-report-{DateTime.Now:yyyyMMddHHmmss}.pdf";
            return File(document, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "An error occurred while generating the inventory report PDF.", error = ex.Message });
        }
    }

    /// <summary>
    /// Share inventory report via email or WhatsApp
    /// </summary>
    [HttpPost("Report/inventory-share")]
    public async Task<IActionResult> ShareInventoryReport([FromBody] InventoryReportShareRequest shareRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            LogService.LogInformation($"Share inventory report request - Method: {shareRequest.Method}, Receiver: {shareRequest.Receiver}");

            var printedBy = await GetCurrentUserDisplayName();
            var document = await _service.GetInventoryReportDocument(
                shareRequest.StoreId,
                shareRequest.StartDate,
                shareRequest.EndDate,
                shareRequest.Language ?? "ar",
                printedBy);

            if (document == null || document.Length == 0)
            {
                LogService.LogError("Inventory report document is null or empty");
                return NotFound("Inventory report document not found or could not be generated.");
            }

            LogService.LogInformation($"Document generated successfully, size: {document.Length} bytes");

            // Share the document using the document share service
            await _documentShareService.ShareDocumentAsync(document, shareRequest.Method, shareRequest.Receiver);

            LogService.LogInformation($"Document shared successfully via {shareRequest.Method} to {shareRequest.Receiver}");

            var response = new DocumentShareResponseDto<string>
            {
                Id = $"inventory-{DateTime.Now:yyyyMMdd}",
                Method = shareRequest.Method,
                Receiver = shareRequest.Receiver,
                Message = "Inventory report shared successfully",
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
            return StatusCode(500, new { message = "An unexpected error occurred while sharing the inventory report.", error = ex.Message });
        }
    }

    // ====================================
    // Store Transfer Report Endpoints
    // ====================================

    /// <summary>
    /// Get store transfer report as PDF document
    /// </summary>
    [HttpGet("Report/store-transfer-document")]
    public async Task<IActionResult> GetStoreTransferDocument(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string language = "ar")
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var printedBy = await GetCurrentUserDisplayName();
            var document = await _service.GetStoreTransferReportDocument(startDate, endDate, language, printedBy);

            if (document == null || document.Length == 0)
            {
                LogService.LogError("Store transfer report document is null or empty");
                return NotFound("Store transfer report document not found or could not be generated.");
            }

            var fileName = $"store-transfer-report-{DateTime.Now:yyyyMMddHHmmss}.pdf";
            return File(document, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "An error occurred while generating the store transfer report PDF.", error = ex.Message });
        }
    }

    /// <summary>
    /// Share store transfer report via email or WhatsApp
    /// </summary>
    [HttpPost("Report/store-transfer-share")]
    public async Task<IActionResult> ShareStoreTransferReport([FromBody] StoreTransferReportShareRequest shareRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            LogService.LogInformation($"Share store transfer report request - Method: {shareRequest.Method}, Receiver: {shareRequest.Receiver}");

            var printedBy = await GetCurrentUserDisplayName();
            var document = await _service.GetStoreTransferReportDocument(
                shareRequest.StartDate,
                shareRequest.EndDate,
                shareRequest.Language ?? "ar",
                printedBy);

            if (document == null || document.Length == 0)
            {
                LogService.LogError("Store transfer report document is null or empty");
                return NotFound("Store transfer report document not found or could not be generated.");
            }

            LogService.LogInformation($"Document generated successfully, size: {document.Length} bytes");

            // Share the document using the document share service
            await _documentShareService.ShareDocumentAsync(document, shareRequest.Method, shareRequest.Receiver);

            LogService.LogInformation($"Document shared successfully via {shareRequest.Method} to {shareRequest.Receiver}");

            var response = new DocumentShareResponseDto<string>
            {
                Id = $"store-transfer-{DateTime.Now:yyyyMMdd}",
                Method = shareRequest.Method,
                Receiver = shareRequest.Receiver,
                Message = "Store transfer report shared successfully",
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
            return StatusCode(500, new { message = "An unexpected error occurred while sharing the store transfer report.", error = ex.Message });
        }
    }

}

// Request DTO for Permits Report Share
public class PermitsReportShareRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Method { get; set; } = "email";
    public string Receiver { get; set; } = string.Empty;
    public string? Language { get; set; } = "ar";
}

// Request DTO for Store Transfer Share
public class StoreTransferReportShareRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Method { get; set; } = "email";
    public string Receiver { get; set; } = string.Empty;
    public string? Language { get; set; } = "ar";
}
