using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Common.Dtos.Receipts.PaymentReceiptAccountResponse;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.Receipts.PaymentReceipts;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Receipts;
[Route("api/receipts/[controller]/")]
[ApiController]
[PermissionAuthorize("PaymentReceipt.Read")]
public class PaymentReceiptAccountController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly PaymentReceiptAccountService _service;
    public PaymentReceiptAccountController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<PaymentReceiptAccountService>();
    }

    // PaymentReceiptAccounts get all
    
    [HttpGet("")]
    public async Task<IActionResult> GetPaymentReceiptAccounts([FromQuery] PaymentReceiptAccountFiltersDto filters )
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
    
    // PaymentReceiptAccounts get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPaymentReceiptAccountsById(Guid id)
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
    
    // PaymentReceiptAccounts create
    [HttpPost("")]
    public async Task<IActionResult> CreatePaymentReceiptAccounts([FromBody] PaymentReceiptAccountPostDto paymentReceiptAccount)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(paymentReceiptAccount);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // PaymentReceiptAccounts update
    [HttpPut]
    public async Task<IActionResult> UpdatePaymentReceiptAccounts([FromBody] PaymentReceiptAccountPutDto paymentReceiptAccount)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(paymentReceiptAccount);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // PaymentReceiptAccounts delete
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePaymentReceiptAccounts(Guid id)
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