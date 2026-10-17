using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Reports.InventoryReport;
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

namespace SmartAndPro.ERP.WebAPI.Controllers.Reports
{
    [Route("api/report/[controller]")]
    [ApiController]
[Authorize]
[PermissionAuthorize("InventoryReport.Read")]
    public class InventoryReportController : ControllerBase
    {
        private readonly InventoryReportService _service;
        private readonly ServiceBaseFactory _serviceBaseFactory;

        public InventoryReportController(ServiceBaseFactory serviceBaseFactory)
        {
            _serviceBaseFactory = serviceBaseFactory;
            _service = _serviceBaseFactory.GetService<InventoryReportService>();
        }

        [HttpPost]
        public async Task<IActionResult> GetInventoryReport([FromBody] InventoryReportRequestDto reportRequest)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Log the incoming request details
            Console.WriteLine($"Store ID: {reportRequest.Id}, Start Date: {reportRequest.StartDate}, End Date: {reportRequest.EndDate}");

            try
            {
                var responseData = await _service.GetInventoryReportAsync(reportRequest);

                // Log the number of records found
                if (responseData == null || !responseData.Any())
                {
                    Console.WriteLine("No records found for the given store and date range.");
                    return NotFound("No records found for the given store and date range.");
                }

                return Ok(responseData);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return BadRequest(ex.Message);
            }
        }

    }
}