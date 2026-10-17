using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BranchDetails;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.CompanyManagement;
using SmartAndPro.ERP.SharedKernel.Constants;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;

namespace SmartAndPro.ERP.WebAPI.Controllers.Inventory.CompanyManagement;

[Route("api/inventory/[controller]/")]
[ApiController]
[Authorize]
public class BranchDetailsController : Controller
{
    private readonly BranchService _service;
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly SmartAndPro.ERP.Application.Services.Auth.Interfaces.IRoleQuotaService _quotaService;

    public BranchDetailsController(ServiceBaseFactory serviceBaseFactory, SmartAndPro.ERP.Application.Services.Auth.Interfaces.IRoleQuotaService quotaService)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<BranchService>();
        _quotaService = quotaService;
    }

    // BranchDetails get all
    [PermissionAuthorize(Permissions.BranchDetails.Read)]
    [HttpGet("")]
    public async Task<IActionResult> GetBranchDetails([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllAsync(new BranchDetailsFiltersDto(), pageNumber, pageSize);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    // BranchDetails get by id
    [PermissionAuthorize(Permissions.BranchDetails.Read)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBranchDetailsById(Guid id)
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

    // BranchDetails create
    [HttpPost("")]
    [PermissionAuthorize(Permissions.BranchDetails.Create)]
    public async Task<IActionResult> CreateBranchDetails([FromBody] BranchDetailsPostDto branchDetails)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            await _quotaService.ValidateBranchQuotaAsync(User);
            var responseDate = await _service.AddAsync(branchDetails);
            return responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return BadRequest(new { Message = ex.Message });
        }
    }

    // BranchDetails update
    [HttpPut]
    [PermissionAuthorize(Permissions.BranchDetails.Update)]
    public async Task<IActionResult> UpdateBranchDetails([FromBody] BranchDetailsPutDto branchDetails)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(branchDetails);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    // BranchDetails delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.BranchDetails.Delete)]
    public async Task<IActionResult> DeleteBranchDetails(Guid id)
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
}