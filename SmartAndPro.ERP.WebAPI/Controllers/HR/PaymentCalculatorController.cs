using SmartAndPro.ERP.Application.Common.Dtos;
using SmartAndPro.ERP.Application.Common.Dtos.HR.Bonus;
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
public class PaymentCalculatorController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly PaymentCalculatorService _service;
    public PaymentCalculatorController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<PaymentCalculatorService>();
    }
    
    
    
    [HttpGet("")]
    public async Task<IActionResult> GetPaymentResults( [FromQuery] string? date = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var month = DateOnly.FromDateTime(DateTime.Now.Date);
            if (string.IsNullOrEmpty(date))
            {
                // get the current month date only
                month = DateOnly.FromDateTime(DateTime.Now.Date);
            }
            else
            {
                month = DateTime.TryParse(date, out var dateTime) ? DateOnly.FromDateTime(dateTime.Date) : DateOnly.FromDateTime(DateTime.Now.Date);

            }

            var responseDate = await _service.CalculatePaymentsAsync(month);
            if (responseDate == null)
            {
                result = new NoContentResult();
            }
            else
            {
                result = new OkObjectResult(responseDate);
            }
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    [HttpGet("overtime-payments")]
    public async Task<IActionResult> GetOvertimePayments([FromQuery] string? date = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var month = DateOnly.FromDateTime(DateTime.Now.Date);
            if (string.IsNullOrEmpty(date))
            {
                // get the current month date only
                month = DateOnly.FromDateTime(DateTime.Now.Date);
            }
            else
            {
                month = DateTime.TryParse(date, out var dateTime) ? DateOnly.FromDateTime(dateTime.Date) : DateOnly.FromDateTime(DateTime.Now.Date);

            }

            var responseDate = await _service.CalculateOverTimeHoursPaymentsAsync(month);
            if (responseDate == null)
            {
                result = new NoContentResult();
            }
            else
            {
                result = new OkObjectResult(responseDate);
            }
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
}