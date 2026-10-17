using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.StagnantInventory;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.Inventory.StagnantInventoryManagement;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Domains.Entities.Inventory.StagnantInventoryManagement;
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

namespace SmartAndPro.ERP.WebAPI.Controllers.Inventory.StagnantInventoryManagement;

[Route("api/inventory/[controller]")]
[ApiController]
[PermissionAuthorize("InventoryManagement.Read")]
public class StagnantInventoryController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly StagnantInventoryService _service;
    private readonly IDocumentShareService _documentShareService;
    private readonly RepositoriesFactory _repositoriesFactory;

    public StagnantInventoryController(ServiceBaseFactory serviceBaseFactory, IDocumentShareService documentShareService, RepositoriesFactory repositoriesFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<StagnantInventoryService>();
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
    public async Task<IActionResult> GetAll([FromQuery] StagnantInventoryFiltersDto filters = null)
    {
        try
        {
            filters ??= new StagnantInventoryFiltersDto();
            var result = await _service.GetStagnantInventoryWithFiltersAsync(filters);
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

    /// <summary>
    /// إضافة منتج للرواكد يدوياً
    /// </summary>
    [HttpPost("manual")]
    public async Task<IActionResult> AddManual([FromBody] StagnantInventoryPostDto dto)
    {
        try
        {
            var addedBy = await GetCurrentUserDisplayName();
            var result = await _service.AddManualAsync(dto, addedBy);
            if (result.ErrorCode == 0)
                return Ok(result.Data);
            return BadRequest(new { message = "فشل في إضافة المنتج للرواكد" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// إضافة منتج للرواكد من إذن الدخول
    /// </summary>
    [HttpPost("from-entry-permit")]
    public async Task<IActionResult> AddFromEntryPermit([FromBody] StagnantFromEntryPermitDto dto)
    {
        try
        {
            var addedBy = await GetCurrentUserDisplayName();
            var result = await _service.AddFromEntryPermitAsync(
                dto.MainPurchaseClassId,
                dto.MainSalesClassId,
                dto.Quantity,
                dto.StoreDetailsId,
                dto.StagnantPeriodMonths,
                addedBy);
            
            if (result.ErrorCode == 0)
                return Ok(result.Data);
            return BadRequest(new { message = "فشل في إضافة المنتج للرواكد" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("")]
    public async Task<IActionResult> Update([FromBody] StagnantInventoryPutDto dto)
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

    /// <summary>
    /// التخلص من منتج راكد
    /// </summary>
    [HttpPost("{id}/dispose")]
    public async Task<IActionResult> Dispose(Guid id, [FromBody] DisposeStagnantDto dto)
    {
        try
        {
            var disposedBy = await GetCurrentUserDisplayName();
            var result = await _service.DisposeAsync(id, dto.DisposalMethod, disposedBy);
            if (result.ErrorCode == 0)
                return Ok(result.Data);
            return BadRequest(new { message = result.Message ?? "فشل في التخلص من المنتج" });
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
    // Stagnant Inventory Report Endpoints
    // ====================================

    /// <summary>
    /// Get stagnant inventory report as PDF document (POST method for complex filters)
    /// </summary>
    [HttpPost("report")]
    public async Task<IActionResult> GetStagnantInventoryReport([FromBody] StagnantInventoryFiltersDto filters)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var printedBy = await GetCurrentUserDisplayName();
            var document = await _service.GetStagnantInventoryReportDocument(
                filters.StoreDetailsId, 
                filters.FromDate, 
                filters.ToDate,
                printedBy);

            if (document == null || document.Length == 0)
            {
                LogService.LogError("Stagnant inventory report document is null or empty");
                return NotFound("Stagnant inventory report document not found or could not be generated.");
            }

            var fileName = $"stagnant-inventory-report-{DateTime.Now:yyyyMMddHHmmss}.pdf";
            return File(document, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "An error occurred while generating the stagnant inventory report PDF.", error = ex.Message });
        }
    }

    /// <summary>
    /// Get stagnant inventory report as PDF document
    /// </summary>
    [HttpGet("Report/stagnant-document")]
    public async Task<IActionResult> GetStagnantInventoryDocument(
        [FromQuery] Guid? storeId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var printedBy = await GetCurrentUserDisplayName();
            var document = await _service.GetStagnantInventoryReportDocument(storeId, startDate, endDate, printedBy);

            if (document == null || document.Length == 0)
            {
                LogService.LogError("Stagnant inventory report document is null or empty");
                return NotFound("Stagnant inventory report document not found or could not be generated.");
            }

            var fileName = $"stagnant-inventory-report-{DateTime.Now:yyyyMMddHHmmss}.pdf";
            return File(document, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "An error occurred while generating the stagnant inventory report PDF.", error = ex.Message });
        }
    }

    /// <summary>
    /// Share stagnant inventory report via email or other methods
    /// </summary>
    [HttpPost("Report/stagnant-share")]
    public async Task<IActionResult> ShareStagnantInventoryReport([FromBody] StagnantInventoryReportShareRequest shareRequest)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            LogService.LogError($"Model validation failed: {errors}");
            return BadRequest(new { message = "Invalid request data", errors = ModelState });
        }

        try
        {
            LogService.LogInformation($"Share stagnant inventory report request - Method: {shareRequest.Method}");

            var printedBy = await GetCurrentUserDisplayName();
            var document = await _service.GetStagnantInventoryReportDocument(
                shareRequest.StoreId,
                shareRequest.StartDate,
                shareRequest.EndDate,
                printedBy);

            if (document == null || document.Length == 0)
            {
                LogService.LogError("Stagnant inventory report document is null or empty");
                return NotFound("Stagnant inventory report document not found or could not be generated.");
            }

            LogService.LogInformation($"Document generated successfully, size: {document.Length} bytes");

            await _documentShareService.ShareDocumentAsync(document, shareRequest.Method, shareRequest.Receiver);

            LogService.LogInformation($"Document shared successfully via {shareRequest.Method} to {shareRequest.Receiver}");

            var response = new DocumentShareResponseDto<string>
            {
                Id = $"stagnant-{DateTime.Now:yyyyMMdd}",
                Method = shareRequest.Method,
                Receiver = shareRequest.Receiver,
                Message = "Stagnant inventory report shared successfully",
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
            return StatusCode(500, new { message = "An unexpected error occurred while sharing the stagnant inventory report.", error = ex.Message });
        }
    }

    /// <summary>
    /// Share stagnant inventory report via WhatsApp
    /// </summary>
    [HttpPost("Report/stagnant-whatsappShare")]
    public async Task<IActionResult> ShareStagnantInventoryReportOnWhatsApp([FromBody] StagnantInventoryReportShareRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var handler = new WhatsAppShareHandler<string>();
            var baseUrl = "https://erp.smartapro.com/stagnant-inventory-report";
            var reportId = $"stagnant-{DateTime.Now:yyyyMMdd}";
            var response = handler.BuildLinks(new[] { reportId }, dto.Receiver, baseUrl);
            return Ok(new { response, reportId });
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, "An unexpected error occurred while sharing the stagnant inventory report on WhatsApp.");
        }
    }

    /// <summary>
    /// Export stagnant inventory to Excel
    /// </summary>
    [HttpGet("excel")]
    public async Task<IActionResult> ExportStagnantInventoryToExcel(
        [FromQuery] Guid? storeId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var filters = new StagnantInventoryFiltersDto
            {
                StoreDetailsId = storeId,
                FromDate = startDate,
                ToDate = endDate
            };

            var stagnantItems = await _service.GetStagnantInventoryWithFiltersAsync(filters);

            if (stagnantItems == null || !stagnantItems.Any())
            {
                return NotFound("No stagnant inventory records found for the given criteria.");
            }

            // Generate Excel file
            var excelData = GenerateStagnantInventoryExcelFile(stagnantItems);
            var fileName = $"stagnant-inventory-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "An error occurred while exporting stagnant inventory to Excel.", error = ex.Message });
        }
    }

    /// <summary>
    /// Get stagnant inventory report for printing
    /// </summary>
    [HttpGet("Report/print")]
    public async Task<IActionResult> GetStagnantInventoryForPrint(
        [FromQuery] Guid? storeId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var filters = new StagnantInventoryFiltersDto
            {
                StoreDetailsId = storeId,
                FromDate = startDate,
                ToDate = endDate
            };

            var stagnantItems = await _service.GetStagnantInventoryWithFiltersAsync(filters);

            if (stagnantItems == null || !stagnantItems.Any())
            {
                return NotFound("No stagnant inventory records found for the given criteria.");
            }

            // Return formatted data for printing
            var printData = new
            {
                title = "تقرير الرواكد",
                date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                filters = new
                {
                    storeId,
                    startDate = startDate?.ToString("yyyy-MM-dd"),
                    endDate = endDate?.ToString("yyyy-MM-dd")
                },
                items = stagnantItems.Select(item => new
                {
                    id = item.Id,
                    productName = item.ProductName ?? "غير محدد",
                    productCode = item.ProductCode ?? "-",
                    quantity = item.Quantity,
                    lastMovementDate = item.LastMovementDate?.ToString("yyyy-MM-dd") ?? "-",
                    addedToStagnantDate = item.AddedToStagnantDate.ToString("yyyy-MM-dd"),
                    reason = GetReasonText(item.Reason),
                    storeName = item.StoreName ?? "غير محدد",
                    isDisposed = item.IsDisposed,
                    disposalMethod = item.DisposalMethod ?? "-"
                }).ToList(),
                totalQuantity = stagnantItems.Sum(x => x.Quantity),
                disposedCount = stagnantItems.Count(x => x.IsDisposed),
                pendingCount = stagnantItems.Count(x => !x.IsDisposed)
            };

            return Ok(printData);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "An error occurred while preparing stagnant inventory data for printing.", error = ex.Message });
        }
    }

    private byte[] GenerateStagnantInventoryExcelFile(IEnumerable<StagnantInventoryResponseDto> data)
    {
        var csv = new System.Text.StringBuilder();
        
        // Headers in Arabic
        csv.AppendLine("م,المخزن,الصنف,الكود,الكمية,آخر حركة,تاريخ الإضافة للرواكد,السبب,الحالة,طريقة التصرف");
        
        // Data rows
        int rowNum = 1;
        foreach (var item in data)
        {
            var storeName = item.StoreName ?? "غير محدد";
            var productName = item.ProductName ?? "غير محدد";
            var productCode = item.ProductCode ?? "-";
            var quantity = item.Quantity.ToString("N2");
            var lastMovementDate = item.LastMovementDate?.ToString("yyyy-MM-dd") ?? "-";
            var addedToStagnantDate = item.AddedToStagnantDate.ToString("yyyy-MM-dd");
            var reason = GetReasonText(item.Reason);
            var status = item.IsDisposed ? "تم التصرف" : "راكد";
            var disposalMethod = item.DisposalMethod ?? "-";
            
            csv.AppendLine($"{rowNum},{storeName},{productName},{productCode},{quantity},{lastMovementDate},{addedToStagnantDate},{reason},{status},{disposalMethod}");
            rowNum++;
        }
        
        // Summary row
        csv.AppendLine($"");
        csv.AppendLine($"الإجمالي,,,{data.Sum(x => x.Quantity):N2},,,,,,");
        csv.AppendLine($"إجمالي السجلات: {data.Count()}");
        csv.AppendLine($"تم التصرف فيها: {data.Count(x => x.IsDisposed)}");
        csv.AppendLine($"في انتظار التصرف: {data.Count(x => !x.IsDisposed)}");
        
        return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
    }

    private string GetReasonText(StagnantReason reason)
    {
        return reason switch
        {
            StagnantReason.Manual => "يدوي",
            StagnantReason.AutoByPeriod => "تلقائي بالفترة",
            StagnantReason.OnEntryPermit => "عند إذن الدخول",
            _ => "-"
        };
    }
}

// DTO للإضافة من إذن الدخول
public class StagnantFromEntryPermitDto
{
    public Guid? MainPurchaseClassId { get; set; }
    public Guid? MainSalesClassId { get; set; }
    public decimal Quantity { get; set; }
    public Guid StoreDetailsId { get; set; }
    public int StagnantPeriodMonths { get; set; }
}

// DTO للتخلص من المنتج
public class DisposeStagnantDto
{
    public string DisposalMethod { get; set; } = string.Empty;
}

// Request للمشاركة
public class StagnantInventoryReportShareRequest
{
    public Guid? StoreId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Receiver { get; set; } = string.Empty;
    public string Method { get; set; } = "email";
}
