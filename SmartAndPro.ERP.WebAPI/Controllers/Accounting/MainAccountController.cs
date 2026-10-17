using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.MainAccount;
using SmartAndPro.ERP.Application.Services.Accounting;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.SharedKernel.Constants;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;

namespace SmartAndPro.ERP.WebAPI.Controllers.Accounting;
[Route("api/accounting/[controller]/")]
[ApiController]
[Authorize]
[PermissionAuthorize(Permissions.Account.Read)]
public class MainAccountController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly MainAccountService _service;
    public MainAccountController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<MainAccountService>();
    }
    
    // MainAccounts get all
    [HttpGet("")]
    public async Task<IActionResult> GetMainAccounts([FromQuery] MainAccountFiltersDto filters )
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

    // MainAccounts get lightweight lookup for dropdowns
    [HttpGet("lookup")]
    public async Task<IActionResult> GetLookup()
    {
        IActionResult result = NoContent();
        try
        {
            var responseData = await _service.GetLookupAsync();
            result = responseData.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // MainAccounts get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMainAccountsById(Guid id)
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
    
    // MainAccounts create
    [HttpPost("")]
    [PermissionAuthorize(Permissions.Account.Create)]
    public async Task<IActionResult> CreateMainAccounts([FromBody] MainAccountPostDto mainAccount)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(mainAccount);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // MainAccounts update
    [HttpPut]
    [PermissionAuthorize(Permissions.Account.Update)]
    public async Task<IActionResult> UpdateMainAccounts([FromBody] MainAccountPutDto mainAccount)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(mainAccount);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // MainAccounts delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.Account.Delete)]
    public async Task<IActionResult> DeleteMainAccounts(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<MainAccountService>();
            var responseDate = await service.DeleteAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }
}