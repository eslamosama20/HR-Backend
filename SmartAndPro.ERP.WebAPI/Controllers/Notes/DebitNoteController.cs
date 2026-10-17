using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Notes.DebitNotes;
using SmartAndPro.ERP.Application.Services.Accounting;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Notes.DebitNotes;
using SmartAndPro.ERP.SharedKernel.Constants;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;

namespace SmartAndPro.ERP.WebAPI.Controllers.Notes;
[Route("api/notes/[controller]/")]
[ApiController]
[Authorize]
[PermissionAuthorize(Permissions.DebitNote.Read)]
public class DebitNoteController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly DebitNoteService _service;
    public DebitNoteController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<DebitNoteService>();
    }

    // DebitNotes get all
    [HttpGet("")]
    public async Task<IActionResult> GetDebitNotes([FromQuery] DebitNoteFiltersDto filters )
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
    
    // DebitNotes get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDebitNotesById(Guid id)
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
    
    // DebitNotes create
    [HttpPost("")]
    [PermissionAuthorize(Permissions.DebitNote.Create)]
    public async Task<IActionResult> CreateDebitNotes([FromBody] DebitNotePostDto debitNote)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(debitNote);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // DebitNotes update
    [HttpPut]
    [PermissionAuthorize(Permissions.DebitNote.Update)]
    public async Task<IActionResult> UpdateDebitNotes([FromBody] DebitNotePutDto debitNote)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(debitNote);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // DebitNotes delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.DebitNote.Delete)]
    public async Task<IActionResult> DeleteDebitNotes(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var debitNote =await _service.GetAsync(id);
            var transactionService =  _serviceBaseFactory.GetService<TransactionService>();
            if (debitNote.Data.TransactionId != null)
            {
                await transactionService.DeleteAsync((Guid)debitNote.Data.TransactionId);
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