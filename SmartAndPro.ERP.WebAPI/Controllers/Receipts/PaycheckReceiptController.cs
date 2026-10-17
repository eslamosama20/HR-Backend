using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Receipts.PaycheckReceipts;
using SmartAndPro.ERP.Application.Services.Accounting;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Receipts.PaycheckReceipts;
using SmartAndPro.ERP.SharedKernel.Constants;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;

namespace SmartAndPro.ERP.WebAPI.Controllers.Receipts;
[Route("api/receipts/[controller]/")]
[ApiController]
[Authorize]
[PermissionAuthorize(Permissions.PaycheckReceipt.Read)]
public class PaycheckReceiptController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly PaycheckReceiptService _service;
    public PaycheckReceiptController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<PaycheckReceiptService>();
    }
    
    [HttpGet("frontal")]
    public async Task<IActionResult> GetAllFrontal()
    {
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllFrontalAsync();
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    [HttpPost("frontal/{id:guid}")]
    [PermissionAuthorize(Permissions.PaycheckReceipt.Update)]
    public async Task<IActionResult> MakeFrontalAsync(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.MakeFrontalAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // PaycheckReceipts get all
    [HttpGet("")]
    public async Task<IActionResult> GetPaycheckReceipts([FromQuery] PaycheckReceiptFiltersDto filters )
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
    
    // PaycheckReceipts get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPaycheckReceiptsById(Guid id)
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
    
    // PaycheckReceipts create
    [HttpPost("")]
    [PermissionAuthorize(Permissions.PaycheckReceipt.Create)]
    public async Task<IActionResult> CreatePaycheckReceipts([FromBody] PaycheckReceiptPostDto paycheckReceipt)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(paycheckReceipt);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // PaycheckReceipts update
    [HttpPut]
    [PermissionAuthorize(Permissions.PaycheckReceipt.Update)]
    public async Task<IActionResult> UpdatePaycheckReceipts([FromBody] PaycheckReceiptPutDto paycheckReceipt)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(paycheckReceipt);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // PaycheckReceipts delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.PaycheckReceipt.Delete)]
    public async Task<IActionResult> DeletePaycheckReceipts(Guid id, [FromQuery] string reason)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var userId = Guid.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value, out var parsedId) ? parsedId : Guid.Empty;
            var responseDate = await _service.DeleteWithReasonAsync(id, reason ?? "إلغاء السند", userId);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }
    
    // Get invoice paid amount
    [HttpGet("invoice-paid-amount/{saleInvoiceId:int}")]
    public async Task<IActionResult> GetInvoicePaidAmount(int saleInvoiceId)
    {
        IActionResult result = NoContent();
        try
        {
            var paidAmount = await _service.GetInvoicePaidAmountAsync(saleInvoiceId);
            result = Ok(paidAmount);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // Get invoice remaining amount
    [HttpGet("invoice-remaining-amount/{saleInvoiceId:int}")]
    public async Task<IActionResult> GetInvoiceRemainingAmount(int saleInvoiceId)
    {
        IActionResult result = NoContent();
        try
        {
            var remainingAmount = await _service.GetInvoiceRemainingAmountAsync(saleInvoiceId);
            result = Ok(remainingAmount);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    // Get paycheck receipts linked to invoice
    [HttpGet("ByInvoice/{invoiceId:int}")]
    public async Task<IActionResult> GetPaycheckReceiptsByInvoice(int invoiceId)
    {
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetBySaleInvoiceIdAsync(invoiceId);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

}
