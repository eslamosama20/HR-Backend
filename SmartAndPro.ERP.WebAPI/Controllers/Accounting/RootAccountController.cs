using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.RootAccount;
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
public class RootAccountController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly RootAccountService _service;
    public RootAccountController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<RootAccountService>();
    }
    
    // RootAccounts get all
    [HttpGet("")]
    public async Task<IActionResult> GetRootAccounts([FromQuery] RootAccountFiltersDto filters )
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
    
    // RootAccounts get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRootAccountsById(Guid id)
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
    
    // RootAccounts create
    [HttpPost("")]
    [PermissionAuthorize(Permissions.Account.Create)]
    public async Task<IActionResult> CreateRootAccounts([FromBody] RootAccountPostDto rootAccount)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(rootAccount);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // RootAccounts update
    [HttpPut]
    [PermissionAuthorize(Permissions.Account.Update)]
    public async Task<IActionResult> UpdateRootAccounts([FromBody] RootAccountPutDto rootAccount)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(rootAccount);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // RootAccounts delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.Account.Delete)]
    public async Task<IActionResult> DeleteRootAccounts(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<RootAccountService>();
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