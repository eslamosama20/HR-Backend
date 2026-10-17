using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Common.Dtos.Receipts.PaycheckReceiptAccountResponse;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.Receipts.PaycheckReceipts;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Receipts;
[Route("api/receipts/[controller]/")]
[ApiController]
[PermissionAuthorize("PaycheckReceipt.Read")]
public class PaycheckReceiptAccountController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly PaycheckReceiptAccountService _service;
    public PaycheckReceiptAccountController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<PaycheckReceiptAccountService>();
    }

    // PaycheckReceiptAccounts get all
    
    [HttpGet("")]
    public async Task<IActionResult> GetPaycheckReceiptAccounts([FromQuery] PaycheckReceiptAccountFiltersDto filters )
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
    
    // PaycheckReceiptAccounts get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPaycheckReceiptAccountsById(Guid id)
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
    
    // PaycheckReceiptAccounts create
    [HttpPost("")]
    public async Task<IActionResult> CreatePaycheckReceiptAccounts([FromBody] PaycheckReceiptAccountPostDto paycheckReceiptAccount)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(paycheckReceiptAccount);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // PaycheckReceiptAccounts update
    [HttpPut]
    public async Task<IActionResult> UpdatePaycheckReceiptAccounts([FromBody] PaycheckReceiptAccountPutDto paycheckReceiptAccount)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(paycheckReceiptAccount);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // PaycheckReceiptAccounts delete
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePaycheckReceiptAccounts(Guid id)
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