using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.StoreDetails;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.StoreManagement;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;
using System.Security.Claims;

namespace SmartAndPro.ERP.WebAPI.Controllers.Inventory.StoreManagement;
[Route("api/inventory/[controller]/")]
[ApiController]
[PermissionAuthorize(SmartAndPro.ERP.SharedKernel.Constants.Permissions.StoreDetails.Read)]
public class StoreDetailsController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly StoreService _service;
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly IDocumentShareService _documentShareService;
    private readonly SmartAndPro.ERP.Application.Services.Auth.Interfaces.IRoleQuotaService _quotaService;

    public StoreDetailsController(ServiceBaseFactory serviceBaseFactory, RepositoriesFactory repositoriesFactory, IDocumentShareService documentShareService, SmartAndPro.ERP.Application.Services.Auth.Interfaces.IRoleQuotaService quotaService)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<StoreService>();
        _repositoriesFactory = repositoriesFactory;
        _documentShareService = documentShareService;
        _quotaService = quotaService;
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
            {
                var fullName = $"{user.FirstName} {user.LastName}".Trim();
                return string.IsNullOrEmpty(fullName) ? user.UserName ?? "المستخدم" : fullName;
            }
            return user.UserName ?? "المستخدم";
        }

        return User.FindFirst(ClaimTypes.GivenName)?.Value ?? "المستخدم";
    }
    
    
    // StoreDetails get all
    [HttpGet("")]
    public async Task<IActionResult> GetStoreDetails([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllAsync(new StoreDetailsFiltersDto(), pageNumber, pageSize);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // StoreDetails get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetStoreDetailsById(Guid id)
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
    
    // StoreDetails create
    [HttpPost("")]
    [PermissionAuthorize(SmartAndPro.ERP.SharedKernel.Constants.Permissions.StoreDetails.Create)]
    public async Task<IActionResult> CreateStoreDetails([FromBody] StoreDetailsPostDto storeDetails)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            await _quotaService.ValidateStoreQuotaAsync(User);
            var responseDate = await _service.AddAsync(storeDetails);
            return responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return BadRequest(new { Message = ex.Message });
        }
    }
    
    // StoreDetails update
    [HttpPut]
    [PermissionAuthorize(SmartAndPro.ERP.SharedKernel.Constants.Permissions.StoreDetails.Update)]
    public async Task<IActionResult> UpdateStoreDetails([FromBody] StoreDetailsPutDto storeDetails)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(storeDetails);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // StoreDetails delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(SmartAndPro.ERP.SharedKernel.Constants.Permissions.StoreDetails.Delete)]
    public async Task<IActionResult> DeleteStoreDetails(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
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
    // Store Details Report Endpoints
    // ====================================

    /// <summary>
    /// Get store details report as PDF document
    /// </summary>
    [HttpGet("Report/stores-document")]
    public async Task<IActionResult> GetStoreDetailsDocument()
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var printedBy = await GetCurrentUserDisplayName();
            var document = await _service.GetStoreDetailsReportDocument(printedBy);

            if (document == null || document.Length == 0)
            {
                LogService.LogError("Store details report document is null or empty");
                return NotFound("Store details report document not found or could not be generated.");
            }

            var fileName = $"store-details-report-{DateTime.Now:yyyyMMddHHmmss}.pdf";
            return File(document, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "An error occurred while generating the store details report PDF.", error = ex.Message });
        }
    }

    /// <summary>
    /// Share store details report via email or WhatsApp
    /// </summary>
    [HttpPost("Report/share")]
    public async Task<IActionResult> ShareStoreDetailsReport([FromBody] StoreDetailsReportShareRequest shareRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var printedBy = await GetCurrentUserDisplayName();
            var document = await _service.GetStoreDetailsReportDocument(printedBy);

            if (document == null || document.Length == 0)
            {
                LogService.LogError("Store details report document is null or empty");
                return NotFound("Store details report document not found or could not be generated.");
            }

            var receiver = shareRequest.Method?.ToLower() == "email" ? shareRequest.Email : shareRequest.Phone;
            await _documentShareService.ShareDocumentAsync(document, shareRequest.Method, receiver);

            return Ok(new { 
                success = true, 
                message = shareRequest.Method?.ToLower() == "email" 
                    ? "تم إرسال التقرير بالبريد الإلكتروني بنجاح" 
                    : "تم إرسال التقرير بالواتساب بنجاح" 
            });
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "An error occurred while sharing the store details report.", error = ex.Message });
        }
    }

}

public class StoreDetailsReportShareRequest
{
    public string? Method { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}