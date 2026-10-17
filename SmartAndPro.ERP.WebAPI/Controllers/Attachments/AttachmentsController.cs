using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Common.Dtos.Attachments;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.Attachments;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;
using SmartAndPro.ERP.SharedKernel.Enums;

namespace SmartAndPro.ERP.WebAPI.Controllers.Attachments;
[Route("api/[controller]/")]
[ApiController]
[PermissionAuthorize("System.Read")]
public class AttachmentsController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly AttachmentsService _service;
    public AttachmentsController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<AttachmentsService>();
    }
    
    // Download Attachments
    [HttpGet("download/{id:guid}")]
    public async Task<IActionResult> DownloadAttachments(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.DownloadAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    [HttpGet("noneReferenced")]
    public async Task<IActionResult> GetAllNoneReferenced([FromQuery] AttachmentFiltersDto filters)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllNoneReferencedAsync(filters);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadAttachment([FromForm] AttachmentUploadDto attachment)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UploadAsync(attachment);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    [HttpGet("download-file/{id:guid}")]
    public async Task<IActionResult> DownloadAttachmentFile(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var responseDate = await _service.GetFileStreamAsync(id);
            if (responseDate.ErrorCode != ErrorCodes.Success || responseDate.Data == null)
                return responseDate.HandleResult();

            return File(responseDate.Data.Stream, responseDate.Data.FileType, responseDate.Data.FileName);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return NoContent();
    }

    // Attachments get all
    [HttpGet("")]
    public async Task<IActionResult> GetAttachments([FromQuery] AttachmentFiltersDto filters )
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
    
    // Attachments get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAttachmentsById(Guid id)
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
    
    // Attachments create
    [HttpPost("")]
    public async Task<IActionResult> CreateAttachments([FromBody] AttachmentPostDto attachment)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(attachment);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // Attachments update
    [HttpPut]
    public async Task<IActionResult> UpdateAttachments([FromBody] AttachmentPutDto attachment)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(attachment);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // Attachments delete
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAttachments(Guid id)
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