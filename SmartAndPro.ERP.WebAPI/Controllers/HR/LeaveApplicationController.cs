using SmartAndPro.ERP.Application.Common.Dtos;
using SmartAndPro.ERP.Application.Common.Dtos.HR.LeaveApplication;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.HR;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartAndPro.ERP.WebAPI.Controllers.HR;

[Route("api/hr/[controller]/")]
[ApiController]
[Authorize]
public class LeaveApplicationController : Controller
{
    private readonly LeaveApplicationService _service;

    public LeaveApplicationController(ServiceBaseFactory serviceBaseFactory)
    {
        _service = serviceBaseFactory.GetService<LeaveApplicationService>();
    }

    [HttpGet("")]
    public async Task<IActionResult> GetAll([FromQuery] ParamFilter filter)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var response = await _service.GetAllAsync(filter);
            result = response.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var response = await _service.GetAsync(id);
            result = response.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        IActionResult result = NoContent();
        try
        {
            var response = await _service.GetPendingApplicationsAsync();
            result = response.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    [HttpGet("employee/{employeeId:guid}")]
    public async Task<IActionResult> GetByEmployee(Guid employeeId)
    {
        IActionResult result = NoContent();
        try
        {
            var response = await _service.GetByEmployeeAsync(employeeId);
            result = response.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    [HttpPost("")]
    public async Task<IActionResult> Create([FromBody] LeaveApplicationPostDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var response = await _service.AddAsync(dto);
            result = response.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] LeaveApplicationPutDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var response = await _service.UpdateAsync(dto);
            result = response.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] LeaveApprovalDto dto)
    {
        IActionResult result = NoContent();
        try
        {
            var response = await _service.ApproveAsync(id, dto.ApproverId);
            result = response.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] LeaveApprovalDto dto)
    {
        IActionResult result = NoContent();
        try
        {
            var response = await _service.RejectAsync(id, dto.ApproverId, dto.RejectionReason);
            result = response.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var response = await _service.DeleteAsync(id);
            result = response.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
}
