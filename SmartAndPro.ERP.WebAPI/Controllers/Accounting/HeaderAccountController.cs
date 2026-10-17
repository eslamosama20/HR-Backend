using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.HeaderAccount;
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
public class HeaderAccountController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly HeaderAccountService _service;
    public HeaderAccountController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<HeaderAccountService>();
    }
    
    // HeaderAccounts get all
    [HttpGet("")]
    public async Task<IActionResult> GetHeaderAccounts([FromQuery] HeaderAccountFiltersDto filters )
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
    
    // HeaderAccounts get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetHeaderAccountsById(Guid id)
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
    
    // HeaderAccounts create
    [HttpPost("")]
    [PermissionAuthorize(Permissions.Account.Create)]
    public async Task<IActionResult> CreateHeaderAccounts([FromBody] HeaderAccountPostDto headerAccount)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(headerAccount);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // HeaderAccounts update
    [HttpPut]
    [PermissionAuthorize(Permissions.Account.Update)]
    public async Task<IActionResult> UpdateHeaderAccounts([FromBody] HeaderAccountPutDto headerAccount)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(headerAccount);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // HeaderAccounts delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.Account.Delete)]
    public async Task<IActionResult> DeleteHeaderAccounts(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<HeaderAccountService>();
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
