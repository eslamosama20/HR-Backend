using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.DamagedInventory;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.Inventory.DamagedInventoryManagement;
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
using System.Linq;
using SmartAndPro.ERP.WebAPI.Attributes;
using System.Security.Claims;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Inventory.DamagedInventoryManagement;

[Route("api/inventory/[controller]")]
[ApiController]
[PermissionAuthorize("InventoryManagement.Read")]
public class DamagedInventoryController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly DamagedInventoryService _service;
    private readonly IDocumentShareService _documentShareService;
    private readonly RepositoriesFactory _repositoriesFactory;

    public DamagedInventoryController(ServiceBaseFactory serviceBaseFactory, IDocumentShareService documentShareService, RepositoriesFactory repositoriesFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<DamagedInventoryService>();
        _documentShareService = documentShareService;
        _repositoriesFactory = repositoriesFactory;
    }

    private async Task<string> GetCurrentUserDisplayName()
    {
        var userId = User.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return User.FindFirst(ClaimTypes.GivenName)?.Value ?? "المستخدم";
        }
        
        var userRepository = _repositoriesFactory.GetRepository<User>();
        var userResult = await userRepository.Filter(u => u.Id == userGuid);
        var user = userResult.Data.FirstOrDefault();
        
        if (user != null)
        {
            if (!string.IsNullOrEmpty(user.FirstName))
                return $"{user.FirstName} {user.LastName}".Trim();
            return user.UserName ?? "المستخدم";
        }
        
        return User.FindFirst(ClaimTypes.GivenName)?.Value ?? "المستخدم";
    }

    [HttpGet("")]
    public async Task<IActionResult> GetAll([FromQuery] DamagedInventoryFiltersDto filters = null)
    {
        try
        {
            filters ??= new DamagedInventoryFiltersDto();
            var result = await _service.GetDamagedInventoryWithFiltersAsync(filters);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _service.GetAsync(id);
            if (result.ErrorCode == 0)
                return Ok(result.Data);
            return BadRequest(new { message = "لم يتم العثور على السجل" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("")]
    public async Task<IActionResult> Create([FromBody] DamagedInventoryPostDto dto)
    {
        try
        {
            var result = await _service.AddAsync(dto);
            if (result.ErrorCode == 0)
                return Ok(result.Data);
            return BadRequest(new { message = "فشل في الإضافة" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("")]
    public async Task<IActionResult> Update([FromBody] DamagedInventoryPutDto dto)
    {
        try
        {
            var result = await _service.UpdateAsync(dto);
            if (result.ErrorCode == 0)
                return Ok(result.Data);
            return BadRequest(new { message = "فشل في التحديث" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            if (result.ErrorCode == 0)
                return Ok(new { message = "تم الحذف بنجاح" });
            return BadRequest(new { message = "فشل في الحذف" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ====================================
    // Damaged Inventory Report Endpoints
    // ====================================

    /// <summary>
    /// Get damaged inventory report as PDF document (POST method for complex filters)
    /// </summary>
    [HttpPost("report")]
    public async Task<IActionResult> GetDamagedInventoryReport([FromBody] DamagedInventoryFiltersDto filters)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var printedBy = await GetCurrentUserDisplayName();
            var document = await _service.GetDamagedInventoryReportDocument(
                filters.StoreDetailsId, 
                filters.FromDate, 
                filters.ToDate,
                printedBy);

            if (document == null || document.Length == 0)
            {
                LogService.LogError("Damaged inventory report document is null or empty");
                return NotFound("Damaged inventory report document not found or could not be generated.");
            }

            var fileName = $"damaged-inventory-report-{DateTime.Now:yyyyMMddHHmmss}.pdf";
            return File(document, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "An error occurred while generating the damaged inventory report PDF.", error = ex.Message });
        }
    }

    /// <summary>
    /// Get damaged inventory report as PDF document
    /// </summary>
    [HttpGet("Report/damaged-document")]
    public async Task<IActionResult> GetDamagedInventoryDocument(
        [FromQuery] Guid? storeId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var printedBy = await GetCurrentUserDisplayName();
            var document = await _service.GetDamagedInventoryReportDocument(storeId, startDate, endDate, printedBy);

            if (document == null || document.Length == 0)
            {
                LogService.LogError("Damaged inventory report document is null or empty");
                return NotFound("Damaged inventory report document not found or could not be generated.");
            }

            var fileName = $"damaged-inventory-report-{DateTime.Now:yyyyMMddHHmmss}.pdf";
            return File(document, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "An error occurred while generating the damaged inventory report PDF.", error = ex.Message });
        }
    }

    /// <summary>
    /// Share damaged inventory report via email or other methods
    /// </summary>
    [HttpPost("Report/damaged-share")]
    public async Task<IActionResult> ShareDamagedInventoryReport([FromBody] DamagedInventoryReportShareRequest shareRequest)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            LogService.LogError($"Model validation failed: {errors}");
            return BadRequest(new { message = "Invalid request data", errors = ModelState });
        }

        try
        {
            LogService.LogInformation($"Share damaged inventory report request - Method: {shareRequest.Method}");

            var printedBy = await GetCurrentUserDisplayName();
            var document = await _service.GetDamagedInventoryReportDocument(
                shareRequest.StoreId,
                shareRequest.StartDate,
                shareRequest.EndDate,
                printedBy);

            if (document == null || document.Length == 0)
            {
                LogService.LogError("Damaged inventory report document is null or empty");
                return NotFound("Damaged inventory report document not found or could not be generated.");
            }

            LogService.LogInformation($"Document generated successfully, size: {document.Length} bytes");

            await _documentShareService.ShareDocumentAsync(document, shareRequest.Method, shareRequest.Receiver);

            LogService.LogInformation($"Document shared successfully via {shareRequest.Method} to {shareRequest.Receiver}");

            var response = new DocumentShareResponseDto<string>
            {
                Id = $"damaged-{DateTime.Now:yyyyMMdd}",
                Method = shareRequest.Method,
                Receiver = shareRequest.Receiver,
                Message = "Damaged inventory report shared successfully",
                IsSuccess = true
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            LogService.LogError($"Error details: {ex.Message}\nStackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                LogService.LogError($"Inner exception: {ex.InnerException.Message}\nInner StackTrace: {ex.InnerException.StackTrace}");
            }
            return StatusCode(500, new { message = "An unexpected error occurred while sharing the damaged inventory report.", error = ex.Message });
        }
    }

    /// <summary>
    /// Share damaged inventory report via WhatsApp
    /// </summary>
    [HttpPost("Report/damaged-whatsappShare")]
    public async Task<IActionResult> ShareDamagedInventoryReportOnWhatsApp([FromBody] DamagedInventoryReportShareRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var handler = new WhatsAppShareHandler<string>();
            var baseUrl = "https://erp.smartapro.com/damaged-inventory-report";
            var reportId = $"damaged-{DateTime.Now:yyyyMMdd}";
            var response = handler.BuildLinks(new[] { reportId }, dto.Receiver, baseUrl);
            return Ok(new { response, reportId });
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, "An unexpected error occurred while sharing the damaged inventory report on WhatsApp.");
        }
    }

    /// <summary>
    /// Export damaged inventory to Excel
    /// </summary>
    [HttpGet("excel")]
    public async Task<IActionResult> ExportDamagedInventoryToExcel(
        [FromQuery] Guid? storeId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var filters = new DamagedInventoryFiltersDto
            {
                StoreDetailsId = storeId,
                FromDate = startDate,
                ToDate = endDate
            };

            var damagedItems = await _service.GetDamagedInventoryWithFiltersAsync(filters);

            if (damagedItems == null || !damagedItems.Any())
            {
                return NotFound("No damaged inventory records found for the given criteria.");
            }

            // Generate Excel file
            var excelData = GenerateDamagedInventoryExcelFile(damagedItems);
            var fileName = $"damaged-inventory-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "An error occurred while exporting damaged inventory to Excel.", error = ex.Message });
        }
    }

    /// <summary>
    /// Get damaged inventory report for printing
    /// </summary>
    [HttpGet("Report/print")]
    public async Task<IActionResult> GetDamagedInventoryForPrint(
        [FromQuery] Guid? storeId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var filters = new DamagedInventoryFiltersDto
            {
                StoreDetailsId = storeId,
                FromDate = startDate,
                ToDate = endDate
            };

            var damagedItems = await _service.GetDamagedInventoryWithFiltersAsync(filters);

            if (damagedItems == null || !damagedItems.Any())
            {
                return NotFound("No damaged inventory records found for the given criteria.");
            }

            // Return formatted data for printing
            var printData = new
            {
                title = "تقرير المخزون التالف",
                date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                filters = new
                {
                    storeId,
                    startDate = startDate?.ToString("yyyy-MM-dd"),
                    endDate = endDate?.ToString("yyyy-MM-dd")
                },
                items = damagedItems.Select(item => new
                {
                    id = item.Id,
                    productName = item.ProductName ?? "غير محدد",
                    quantity = item.Quantity,
                    damageDate = item.DamageDate.ToString("yyyy-MM-dd"),
                    reason = item.Reason,
                    reportedBy = item.ReportedBy,
                    storeName = item.StoreName ?? "غير محدد"
                }).ToList(),
                totalQuantity = damagedItems.Sum(x => x.Quantity)
            };

            return Ok(printData);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "An error occurred while preparing damaged inventory data for printing.", error = ex.Message });
        }
    }

    private byte[] GenerateDamagedInventoryExcelFile(IEnumerable<DamagedInventoryResponseDto> data)
    {
        // Simple Excel generation using CSV format
        // In production, you would use a proper Excel library like EPPlus or ClosedXML
        var csv = new System.Text.StringBuilder();
        
        // Headers in Arabic
        csv.AppendLine("المخزن,الصنف,الكمية,تاريخ التلف,السبب,المبلغ,تاريخ الإنشاء");
        
        // Data rows
        foreach (var item in data)
        {
            var storeName = item.StoreName ?? "غير محدد";
            var productName = item.ProductName ?? "غير محدد";
            var quantity = item.Quantity.ToString();
            var damageDate = item.DamageDate.ToString("yyyy-MM-dd");
            var reason = item.Reason ?? "غير محدد";
            var reportedBy = item.ReportedBy ?? "غير محدد";
            var createdAt = item.CreatedAt?.ToString("yyyy-MM-dd") ?? "";
            
            csv.AppendLine($"{storeName},{productName},{quantity},{damageDate},{reason},{reportedBy},{createdAt}");
        }
        
        return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
    }
}
