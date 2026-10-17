using SmartAndPro.ERP.Application.Common.Dtos;
using SmartAndPro.ERP.Application.Common.Dtos.HR.MonthlyLoan;
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
public class MonthlyLoanController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly MonthlyLoanService _service;
    public MonthlyLoanController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<MonthlyLoanService>();
    }
    
    
    
    [HttpGet("")]
    public async Task<IActionResult> GetMonthlyLoans( [FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllAsync(new ParamFilter());
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // MonthlyLoans get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMonthlyLoansById(Guid id)
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
    
    // MonthlyLoans create
    [HttpPost("")]
    public async Task<IActionResult> CreateMonthlyLoans([FromBody] MonthlyLoanPostDto monthlyLoan)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(monthlyLoan);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // MonthlyLoans update
    [HttpPut]
    public async Task<IActionResult> UpdateMonthlyLoans([FromBody] MonthlyLoanPutDto monthlyLoan)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(monthlyLoan);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // MonthlyLoans delete
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteMonthlyLoans(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<MonthlyLoanService>();
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