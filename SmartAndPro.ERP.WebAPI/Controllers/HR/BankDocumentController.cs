using SmartAndPro.ERP.Application.Common.Dtos;
using SmartAndPro.ERP.Application.Common.Dtos.HR.BankDocument;
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
public class BankDocumentController : Controller
{
    private readonly BankDocumentService _service;

    public BankDocumentController(ServiceBaseFactory serviceBaseFactory)
    {
        _service = serviceBaseFactory.GetService<BankDocumentService>();
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

    [HttpPost("")]
    public async Task<IActionResult> Create([FromBody] BankDocumentPostDto dto)
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
    public async Task<IActionResult> Update([FromBody] BankDocumentPutDto dto)
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
