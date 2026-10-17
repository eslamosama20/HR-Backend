using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainSalesClass;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.ClassesManagement;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Constants;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;
using System.Security.Claims;

namespace SmartAndPro.ERP.WebAPI.Controllers.Accounting;
[Route("api/accounting/[controller]/")]
[ApiController]
[Authorize]
[Authorize]
[PermissionAuthorize("MainSalesClass.Read")]
public class MainSalesClassController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly MainSalesClassService _service;
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly IDocumentShareService _documentShareService;

    public MainSalesClassController(ServiceBaseFactory serviceBaseFactory, RepositoriesFactory repositoriesFactory, IDocumentShareService documentShareService)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<MainSalesClassService>();
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
    
    
    // MainSalesClasses get all
    
    [HttpGet("")]
    
    public async Task<IActionResult> GetMainSalesClasses([FromQuery] MainSalesClassFiltersDto filters )
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
    
    // MainSalesClasses get by id
    [HttpGet("{id:guid}")]
    
    public async Task<IActionResult> GetMainSalesClassesById(Guid id)
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
    
    // MainSalesClasses create
    [HttpPost("")]
    [PermissionAuthorize(Permissions.MainSalesClass.Create)]
    public async Task<IActionResult> CreateMainSalesClasses([FromBody] MainSalesClassPostDto mainSalesClass)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(mainSalesClass);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // MainSalesClasses update
    [HttpPut]
    [PermissionAuthorize(Permissions.MainSalesClass.Update)]
    public async Task<IActionResult> UpdateMainSalesClasses([FromBody] MainSalesClassPutDto mainSalesClass)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(mainSalesClass);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // MainSalesClasses delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.MainSalesClass.Delete)]
    public async Task<IActionResult> DeleteMainSalesClasses(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<MainSalesClassService>();
            var responseDate = await service.DeleteAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }


    [HttpPost("ensure-items-exist")]
    public async Task<IActionResult> EnsureItemsExist([FromBody] string[] itemNames)
    {
        if (itemNames == null || itemNames.Length == 0)
            return BadRequest("No items provided.");

        try
        {
            var response = await _service.EnsureItemsExistAsync(itemNames);
            if (response.Data == null || !response.Data.Any())
                return Ok(new { message = response.Message ?? "All items already exist" });
            return Ok(response.Data); 
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    // ====================================
    // Sales Classes Report Endpoints
    // ====================================

    /// <summary>
    /// Get sales classes report as PDF document
    /// </summary>
    [HttpGet("Report/classes-document")]
    public async Task<IActionResult> GetSalesClassesDocument()
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var printedBy = await GetCurrentUserDisplayName();
            var document = await _service.GetSalesClassesReportDocument(printedBy);

            if (document == null || document.Length == 0)
            {
                LogService.LogError("Sales classes report document is null or empty");
                return NotFound("Sales classes report document not found or could not be generated.");
            }

            var fileName = $"sales-classes-report-{DateTime.Now:yyyyMMddHHmmss}.pdf";
            return File(document, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "An error occurred while generating the sales classes report PDF.", error = ex.Message });
        }
    }

    /// <summary>
    /// Share sales classes report via email or WhatsApp
    /// </summary>
    [HttpPost("Report/share")]
    public async Task<IActionResult> ShareSalesClassesReport([FromBody] ClassesReportShareRequest shareRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var printedBy = await GetCurrentUserDisplayName();
            var document = await _service.GetSalesClassesReportDocument(printedBy);

            if (document == null || document.Length == 0)
            {
                LogService.LogError("Sales classes report document is null or empty");
                return NotFound("Sales classes report document not found or could not be generated.");
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
            return StatusCode(500, new { message = "An error occurred while sharing the sales classes report.", error = ex.Message });
        }
    }

}