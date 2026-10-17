using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Reports.InventoryReport;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Common.Dtos.Reports.PurchasableCardReport;
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
[PermissionAuthorize("PurchasableCardReport.Read")]
    public class PurchasableCardReportController : ControllerBase
    {
        private readonly PurchasableCardReportService _service;
        private readonly ServiceBaseFactory _serviceBaseFactory;

        public PurchasableCardReportController(ServiceBaseFactory serviceBaseFactory)
        {
            _serviceBaseFactory = serviceBaseFactory;
            _service = _serviceBaseFactory.GetService<PurchasableCardReportService>();
        }

        [HttpPost]
        public async Task<IActionResult> GetPurchasableCardReport([FromBody] PurchasableCardReportRequestDto reportRequest)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var responseData = await _service.GetReportAsync(reportRequest);

                // Log the number of records found
                if (responseData == null || !responseData.Any())
                {
                    Console.WriteLine("No records found for the given MainPurchaseClass and date range.");
                    return NotFound("No records found for the given MainPurchaseClass and date range.");
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