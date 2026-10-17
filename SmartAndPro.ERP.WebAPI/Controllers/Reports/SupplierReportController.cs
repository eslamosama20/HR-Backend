using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Common.Dtos.Reports.SupplierReport;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Services.@base;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Services.Reports;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.SharedKernel.Utilities;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;

namespace SmartAndPro.ERP.WebAPI.Controllers.Reports;

[Route("api/report/[controller]/")]
[ApiController]
[Authorize]
[PermissionAuthorize("SupplierReport.Read")]
public class SupplierReportController : Controller
{
    private readonly SupplierReportService _service;
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly ISupplierDetailsReportService _supplierDetailsReportService;

    public SupplierReportController(ISupplierDetailsReportService supplierDetailsReportService, ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _supplierDetailsReportService = supplierDetailsReportService;
        _service = _serviceBaseFactory.GetService<SupplierReportService>();
    }
    
    [HttpPost]
    public async Task<IActionResult> GetSupplierReport([FromBody] SupplierReportRequestDto reportRequest)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _supplierDetailsReportService.GetSupplierDetails(reportRequest);
            result = Ok(responseDate);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            result = BadRequest(ex.Message);
        }

        return result;
    }
}