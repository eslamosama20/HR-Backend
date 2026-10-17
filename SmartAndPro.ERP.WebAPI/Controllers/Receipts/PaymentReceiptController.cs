using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Receipts.PaymentReceipts;
using SmartAndPro.ERP.Application.Services.Accounting;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Receipts.PaymentReceipts;
using SmartAndPro.ERP.SharedKernel.Constants;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;

namespace SmartAndPro.ERP.WebAPI.Controllers.Receipts;
[Route("api/receipts/[controller]/")]
[ApiController]
[Authorize]
[PermissionAuthorize(Permissions.PaymentReceipt.Read)]
public class PaymentReceiptController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly PaymentReceiptService _service;
    public PaymentReceiptController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<PaymentReceiptService>();
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
    [PermissionAuthorize(Permissions.PaymentReceipt.Update)]
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

    // PaymentReceipts get all
    [HttpGet("")]
    public async Task<IActionResult> GetPaymentReceipts([FromQuery] PaymentReceiptFiltersDto filters )
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
    
    // PaymentReceipts get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPaymentReceiptsById(Guid id)
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
    
    // PaymentReceipts create
    [HttpPost("")]
    [PermissionAuthorize(Permissions.PaymentReceipt.Create)]
    public async Task<IActionResult> CreatePaymentReceipts([FromBody] PaymentReceiptPostDto paymentReceipt)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(paymentReceipt);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // PaymentReceipts update
    [HttpPut]
    [PermissionAuthorize(Permissions.PaymentReceipt.Update)]
    public async Task<IActionResult> UpdatePaymentReceipts([FromBody] PaymentReceiptPutDto paymentReceipt)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(paymentReceipt);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // PaymentReceipts delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.PaymentReceipt.Delete)]
    public async Task<IActionResult> DeletePaymentReceipts(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var paymentReceipt =await _service.GetAsync(id);
            var transactionService =  _serviceBaseFactory.GetService<TransactionService>();
            if (paymentReceipt.Data.TransactionId != null)
            {
                await transactionService.DeleteAsync((Guid)paymentReceipt.Data.TransactionId);
            }
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