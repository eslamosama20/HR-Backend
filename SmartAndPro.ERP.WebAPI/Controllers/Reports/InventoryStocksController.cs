using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Common.Dtos.Reports.InventoryStocks;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.Reports;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.Permits;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.SharedKernel.Dtos.DocumentShare;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.SharedKernel.Utilities.ShareHandlers;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using System.Linq;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Reports
{
    [Route("api/report/[controller]")]
    [ApiController]
    [PermissionAuthorize("InventoryReport.Read")]
    public class InventoryStocksController : ControllerBase
    {
        private readonly InventoryStocksService _service;
        private readonly ServiceBaseFactory _serviceBaseFactory;
        private readonly InventoryPermitService _permitService;
        private readonly IDocumentShareService _documentShareService;
        private readonly RepositoriesFactory _repositoriesFactory;

        public InventoryStocksController(ServiceBaseFactory serviceBaseFactory, IDocumentShareService documentShareService, RepositoriesFactory repositoriesFactory)
        {
            _serviceBaseFactory = serviceBaseFactory;
            _service = _serviceBaseFactory.GetService<InventoryStocksService>();
            _permitService = _serviceBaseFactory.GetService<InventoryPermitService>();
            _documentShareService = documentShareService;
            _repositoriesFactory = repositoriesFactory;
        }

        /// <summary>
        /// Get the current user's display name from the database
        /// </summary>
        private async Task<string> GetCurrentUserDisplayName()
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                    return "";
                
                var userQuery = await _repositoriesFactory.GetRepository<User>().Filter(u => u.Id == userId);
                var user = userQuery.Data?.FirstOrDefault();
                
                if (user == null) return "";
                
                // Return FirstName + LastName, or UserName, or Email
                if (!string.IsNullOrEmpty(user.FirstName))
                {
                    return $"{user.FirstName} {user.LastName}".Trim();
                }
                return user.UserName ?? user.Email ?? "";
            }
            catch
            {
                return "";
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetInventoryStocks([FromQuery] Guid? storeDetailsId = null)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var responseData = await _service.GetAllStocks(storeDetailsId);

                // Log the number of records found
                if (responseData == null || !responseData.Any())
                {
                    Console.WriteLine("No records found for the given store.");
                    return NotFound("No records found for the given store.");
                }

                return Ok(responseData);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get inventory stocks report as PDF document
        /// </summary>
        [HttpGet("pdf")]
        public async Task<IActionResult> GetInventoryStocksPDF(
            [FromQuery] Guid? storeId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string language = "ar")
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var printedBy = await GetCurrentUserDisplayName();
                var document = await _permitService.GetInventoryReportDocument(storeId, startDate, endDate, language, printedBy);

                if (document == null || document.Length == 0)
                {
                    LogService.LogError("Inventory stocks report document is null or empty");
                    return NotFound("Inventory stocks report document not found or could not be generated.");
                }

                var fileName = $"inventory-stocks-report-{DateTime.Now:yyyyMMddHHmmss}.pdf";
                return File(document, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, new { message = "An error occurred while generating the inventory stocks report PDF.", error = ex.Message });
            }
        }

        /// <summary>
        /// Export inventory stocks to Excel
        /// </summary>
        [HttpGet("excel")]
        public async Task<IActionResult> ExportInventoryStocksToExcel([FromQuery] Guid? storeDetailsId = null)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            try
            {
                var responseData = await _service.GetAllStocks(storeDetailsId);

                if (responseData == null || !responseData.Any())
                {
                    return NotFound("No records found for the given store.");
                }

                // Generate Excel file
                var excelData = GenerateExcelFile(responseData);
                var fileName = $"inventory-stocks-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Share inventory stocks report
        /// </summary>
        [HttpPost("share")]
        public async Task<IActionResult> ShareInventoryStocksReport([FromBody] InventoryReportShareRequest shareRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var printedBy = await GetCurrentUserDisplayName();
                var document = await _permitService.GetInventoryReportDocument(
                    shareRequest.StoreId,
                    shareRequest.StartDate,
                    shareRequest.EndDate,
                    shareRequest.Language ?? "ar",
                    printedBy);

                if (document == null || document.Length == 0)
                {
                    LogService.LogError("Inventory stocks report document is null or empty");
                    return NotFound("Inventory stocks report document not found or could not be generated.");
                }

                await _documentShareService.ShareDocumentAsync(document, shareRequest.Method, shareRequest.Receiver);

                var response = new DocumentShareResponseDto<string>
                {
                    Id = $"inventory-stocks-{DateTime.Now:yyyyMMdd}",
                    Method = shareRequest.Method,
                    Receiver = shareRequest.Receiver,
                    Message = "Inventory stocks report shared successfully",
                    IsSuccess = true
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, new { message = "An unexpected error occurred while sharing the inventory stocks report.", error = ex.Message });
            }
        }

        /// <summary>
        /// Share inventory stocks report via WhatsApp
        /// </summary>
        [HttpPost("whatsapp-share")]
        public async Task<IActionResult> ShareInventoryStocksReportOnWhatsApp([FromBody] InventoryReportShareRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var handler = new WhatsAppShareHandler<string>();
                var baseUrl = "https://erp.smartapro.com/inventory-stocks-report";
                var reportId = $"inventory-stocks-{DateTime.Now:yyyyMMdd}";
                var response = handler.BuildLinks(new[] { reportId }, dto.Receiver, baseUrl);
                return Ok(new { response, reportId });
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, "An unexpected error occurred while sharing the inventory stocks report on WhatsApp.");
            }
        }

        /// <summary>
        /// Get inventory stocks report for printing
        /// </summary>
        [HttpGet("print")]
        public async Task<IActionResult> GetInventoryStocksForPrint([FromQuery] Guid? storeDetailsId = null)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);
            
            try
            {
                var responseData = await _service.GetAllStocks(storeDetailsId);

                if (responseData == null || !responseData.Any())
                {
                    return NotFound("No records found for the given store.");
                }

                // Return formatted data for printing
                var printData = new
                {
                    title = "تقرير أرصدة المخزون",
                    date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    filters = new
                    {
                        storeId = storeDetailsId
                    },
                    stores = responseData.Select(store => new
                    {
                        storeName = store.StoreDetailsArabicName,
                        storeCode = store.StoreDetailsId.ToString(),
                        items = store.StoredItems.Select(item => new
                        {
                            productName = item.MainPurchaseClassArabicName,
                            productCode = item.MainPurchaseClassReferenceCode,
                            quantity = item.Quantity,
                            unitName = "-",
                            lastPrice = item.LastPrice,
                            totalValue = item.Quantity * item.LastPrice
                        }).ToList(),
                        totalQuantity = store.StoredItems.Sum(x => x.Quantity),
                        totalValue = store.StoredItems.Sum(x => x.Quantity * x.LastPrice)
                    }).ToList(),
                    grandTotalQuantity = responseData.SelectMany(x => x.StoredItems).Sum(x => x.Quantity),
                    grandTotalValue = responseData.SelectMany(x => x.StoredItems).Sum(x => x.Quantity * x.LastPrice)
                };

                return Ok(printData);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, new { message = "An error occurred while preparing inventory stocks data for printing.", error = ex.Message });
            }
        }

        private byte[] GenerateExcelFile(IEnumerable<dynamic> data)
        {
            // Simple Excel generation using CSV format
            // In production, you would use a proper Excel library like EPPlus or ClosedXML
            var csv = new System.Text.StringBuilder();
            
            // Headers
            csv.AppendLine("Store Name,Product Name,Quantity,Unit Name,Last Price");
            
            // Data rows
            foreach (var item in data)
            {
                foreach (var storedItem in item.StoredItems)
                {
                    csv.AppendLine($"{item.StoreDetailsArabicName},{storedItem.MainPurchaseClassArabicName},{storedItem.Quantity},{"-"},{storedItem.LastPrice}");
                }
            }
            
            return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        }

    }
}