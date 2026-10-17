using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainPurchaseClass;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.ClassesManagement;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;
using System.Security.Claims;

namespace SmartAndPro.ERP.WebAPI.Controllers.Accounting;
[Route("api/accounting/[controller]/")]
[ApiController]
[Authorize]
public class MainPurchaseClassController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly MainPurchaseClassService _service;
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly IDocumentShareService _documentShareService;

    public MainPurchaseClassController(ServiceBaseFactory serviceBaseFactory, RepositoriesFactory repositoriesFactory, IDocumentShareService documentShareService)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<MainPurchaseClassService>();
        _repositoriesFactory = repositoriesFactory;
        _documentShareService = documentShareService;
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
    
    
    // MainPurchaseClasses get all
    [PermissionAuthorize("MainPurchaseClass.Read")]
    [HttpGet("")]
    public async Task<IActionResult> GetMainPurchaseClasses([FromQuery] MainPurchaseClassFiltersDto filters )
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
    
    // MainPurchaseClasses get by id
    [PermissionAuthorize("MainPurchaseClass.Read")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMainPurchaseClassesById(Guid id)
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
    
    // MainPurchaseClasses create
    [PermissionAuthorize("MainPurchaseClass.Create")]
    [HttpPost("")]
    public async Task<IActionResult> CreateMainPurchaseClasses([FromBody] MainPurchaseClassPostDto mainPurchaseClass)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(mainPurchaseClass);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            // Return a proper error response instead of NoContent
            result = new ContentResult 
            {
                StatusCode = 500,
                Content = ex.Message ?? "An error occurred while processing your request.",
                ContentType = "text/plain"
            };
        }
        return result;
    }
    
    // MainPurchaseClasses update
    [PermissionAuthorize("MainPurchaseClass.Update")]
    [HttpPut]
    public async Task<IActionResult> UpdateMainPurchaseClasses([FromBody] MainPurchaseClassPutDto mainPurchaseClass)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(mainPurchaseClass);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            // Return a proper error response instead of NoContent
            result = new ContentResult 
            {
                StatusCode = 500,
                Content = ex.Message ?? "An error occurred while processing your request.",
                ContentType = "text/plain"
            };
        }
        return result;
    }
    
    // MainPurchaseClasses delete
    [PermissionAuthorize("MainPurchaseClass.Delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteMainPurchaseClasses(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<MainPurchaseClassService>();
            var responseDate = await service.DeleteAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    // ====================================
    // Purchase Classes Report Endpoints
    // ====================================

    /// <summary>
    /// Get purchase classes report as PDF document
    /// </summary>
    [HttpGet("Report/classes-document")]
    public async Task<IActionResult> GetPurchaseClassesDocument()
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var printedBy = await GetCurrentUserDisplayName();
            var document = await _service.GetPurchaseClassesReportDocument(printedBy);

            if (document == null || document.Length == 0)
            {
                LogService.LogError("Purchase classes report document is null or empty");
                return NotFound("Purchase classes report document not found or could not be generated.");
            }

            var fileName = $"purchase-classes-report-{DateTime.Now:yyyyMMddHHmmss}.pdf";
            return File(document, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "An error occurred while generating the purchase classes report PDF.", error = ex.Message });
        }
    }

    /// <summary>
    /// Share purchase classes report via email or WhatsApp
    /// </summary>
    [HttpPost("Report/share")]
    public async Task<IActionResult> SharePurchaseClassesReport([FromBody] ClassesReportShareRequest shareRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var printedBy = await GetCurrentUserDisplayName();
            var document = await _service.GetPurchaseClassesReportDocument(printedBy);

            if (document == null || document.Length == 0)
            {
                LogService.LogError("Purchase classes report document is null or empty");
                return NotFound("Purchase classes report document not found or could not be generated.");
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
            return StatusCode(500, new { message = "An error occurred while sharing the purchase classes report.", error = ex.Message });
        }
    }

}

[Authorize]
[PermissionAuthorize("MainPurchaseClass.Read")]
public class ClassesReportShareRequest
{
    public string? Method { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}