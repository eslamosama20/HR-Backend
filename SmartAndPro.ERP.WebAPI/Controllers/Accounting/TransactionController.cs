using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.Transaction;
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
[PermissionAuthorize(Permissions.Transaction.Read)]
public class TransactionController : Controller
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly TransactionService _service;
    public TransactionController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<TransactionService>();
    }
    
    [HttpGet("frontal")]
    public async Task<IActionResult> GetAllFrontal([FromQuery] TransactionFiltersDto filters, [FromQuery] bool includeDeleted = false, [FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? sortField = null, [FromQuery] int? sortOrder = null)
    {
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllFrontalAsync(filters, pageNumber, pageSize, includeDeleted, sortField, sortOrder);
            result = responseDate.HandleResult(true);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    [HttpPost("frontal/{id:guid}")]
    [PermissionAuthorize(Permissions.Transaction.Update)]
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
   
    // Transactions get all
    
    [HttpGet("")]
    
    public async Task<IActionResult> GetTransactions([FromQuery] TransactionFiltersDto filters, [FromQuery] bool includeDeleted = false, [FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? sortField = null, [FromQuery] int? sortOrder = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllAsync(filters, pageNumber, pageSize, includeDeleted, sortField, sortOrder);
            result = responseDate.HandleResult(true);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // Transactions get by id
    [HttpGet("{id:guid}")]
    
    public async Task<IActionResult> GetTransactionsById(Guid id)
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
    
    // Transactions create
    [HttpPost("")]
    [PermissionAuthorize(Permissions.Transaction.Create)]
    public async Task<IActionResult> CreateTransactions([FromBody] TransactionPostDto transaction)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(transaction);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    [HttpPost("range")]
    [PermissionAuthorize(Permissions.Transaction.Create)]
    public async Task<IActionResult> CreateTransactionRange([FromBody] List<TransactionPostDto> transactions)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddRangeAsync(transactions);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // Transactions update
    [HttpPut]
    [PermissionAuthorize(Permissions.Transaction.Update)]
    public async Task<IActionResult> UpdateTransactions([FromBody] TransactionPutDto transaction)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(transaction);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // Transactions delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.Transaction.Delete)]
    public async Task<IActionResult> DeleteTransactions(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<TransactionService>();
            var responseDate = await service.DeleteAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    [HttpPost("bulk-delete")]
    [PermissionAuthorize(Permissions.Transaction.Delete)]
    public async Task<IActionResult> BulkDeleteTransactions([FromBody] TransactionBulkDeleteRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.BulkDeleteAsync(request);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    [HttpPost("{id:guid}/restore")]
    [PermissionAuthorize(Permissions.Transaction.Update)]
    public async Task<IActionResult> RestoreTransaction(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.RestoreAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    [HttpPost("import")]
    [PermissionAuthorize(Permissions.Transaction.Import)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportFromExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".xlsx" && ext != ".xls")
            return BadRequest(new { message = "Only .xlsx and .xls files are supported." });

        try
        {
            await using var stream = file.OpenReadStream();
            var importResult = await _service.ImportFromExcelAsync(stream);
            return Ok(importResult);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "Import failed unexpectedly.", error = ex.Message });
        }
    }

    [HttpGet("template")]
    [AllowAnonymous]
    public IActionResult DownloadTemplate()
    {
        var fileName = "transaction-import-template.xlsx";
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Assets", fileName);

        if (!System.IO.File.Exists(templatePath))
            return NotFound(new { message = "Template file not found." });

        return PhysicalFile(templatePath, ExcelContentType, fileName);
    }
}
