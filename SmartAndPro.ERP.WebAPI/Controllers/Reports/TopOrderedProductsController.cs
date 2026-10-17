using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Common.Dtos.Reports.TopProducts;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.Reports;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Utility;
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
using System.Security.Claims;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Reports;

[Route("api/reports/[controller]")]
[ApiController]
[PermissionAuthorize("SalesInvoiceReport.Read")]
public class TopOrderedProductsController : ControllerBase
{
    private readonly TopOrderedProductsService _service;
    private readonly IDocumentShareService _documentShareService;
    private readonly RepositoriesFactory _repositoriesFactory;

    public TopOrderedProductsController(
        TopOrderedProductsService service, 
        IDocumentShareService documentShareService,
        RepositoriesFactory repositoriesFactory)
    {
        _service = service;
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

    /// <summary>
    /// الحصول على قائمة المنتجات الأكثر طلباً
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTopOrderedProducts([FromQuery] TopProductsFilterDto filters)
    {
        try
        {
            filters ??= new TopProductsFilterDto();
            var result = await _service.GetTopOrderedProductsAsync(filters);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// الحصول على إحصائيات المشتريات
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics([FromQuery] TopProductsFilterDto filters)
    {
        try
        {
            filters ??= new TopProductsFilterDto();
            var result = await _service.GetStatisticsAsync(filters);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// تصدير التقرير كـ PDF
    /// </summary>
    [HttpGet("pdf")]
    public async Task<IActionResult> ExportToPdf([FromQuery] TopProductsFilterDto filters)
    {
        try
        {
            filters ??= new TopProductsFilterDto();
            var printedBy = await GetCurrentUserDisplayName();
            var document = await _service.GetTopOrderedProductsReportDocument(filters, printedBy);

            if (document == null || document.Length == 0)
            {
                LogService.LogError("Top ordered products report document is null or empty");
                return NotFound("لم يتم العثور على بيانات لإنشاء التقرير.");
            }

            var fileName = $"top-ordered-products-{DateTime.Now:yyyyMMddHHmmss}.pdf";
            return File(document, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "حدث خطأ أثناء إنشاء التقرير.", error = ex.Message });
        }
    }

    /// <summary>
    /// تصدير التقرير كـ Excel
    /// </summary>
    [HttpGet("excel")]
    public async Task<IActionResult> ExportToExcel([FromQuery] TopProductsFilterDto filters)
    {
        try
        {
            filters ??= new TopProductsFilterDto();
            var products = await _service.GetTopOrderedProductsAsync(filters);
            
            var bytes = ExcelExporter.ExportTopProductsToExcel(
                products, 
                "تقرير المنتجات الأكثر طلباً",
                "عدد الطلبات",
                "آخر طلب"
            );
            
            var fileName = $"top-ordered-products-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "حدث خطأ أثناء تصدير البيانات.", error = ex.Message });
        }
    }
    
    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }

    /// <summary>
    /// مشاركة التقرير عبر البريد الإلكتروني
    /// </summary>
    [HttpPost("share/email")]
    public async Task<IActionResult> ShareViaEmail([FromBody] TopProductsShareRequestDto request)
    {
        try
        {
            var filters = new TopProductsFilterDto
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate
            };
            
            var printedBy = await GetCurrentUserDisplayName();
            var document = await _service.GetTopOrderedProductsReportDocument(filters, printedBy);

            if (document == null || document.Length == 0)
            {
                LogService.LogError("Top ordered products report document is null or empty");
                return NotFound("لم يتم العثور على بيانات لإنشاء التقرير.");
            }

            LogService.LogInformation($"Document generated successfully, size: {document.Length} bytes");

            await _documentShareService.ShareDocumentAsync(document, "email", request.Receiver);

            LogService.LogInformation($"Document shared successfully via email to {request.Receiver}");

            var response = new DocumentShareResponseDto<string>
            {
                Id = $"top-ordered-{DateTime.Now:yyyyMMdd}",
                Method = "email",
                Receiver = request.Receiver,
                Message = "تم إرسال التقرير بنجاح",
                IsSuccess = true
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "حدث خطأ أثناء مشاركة التقرير.", error = ex.Message });
        }
    }

    /// <summary>
    /// مشاركة التقرير عبر واتساب
    /// </summary>
    [HttpPost("share/whatsapp")]
    public async Task<IActionResult> ShareViaWhatsApp([FromBody] TopProductsShareRequestDto request)
    {
        try
        {
            var handler = new WhatsAppShareHandler<string>();
            var baseUrl = "https://erp.smartapro.com/top-ordered-products-report";
            var reportId = $"top-ordered-{DateTime.Now:yyyyMMdd}";
            var response = handler.BuildLinks(new[] { reportId }, request.Receiver, baseUrl);
            return Ok(new { response, reportId });
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "حدث خطأ أثناء مشاركة التقرير.", error = ex.Message });
        }
    }

    /// <summary>
    /// طباعة التقرير
    /// </summary>
    [HttpGet("print")]
    public async Task<IActionResult> PrintReport([FromQuery] TopProductsFilterDto filters)
    {
        try
        {
            filters ??= new TopProductsFilterDto();
            var printedBy = await GetCurrentUserDisplayName();
            var document = await _service.GetTopOrderedProductsReportDocument(filters, printedBy);

            if (document == null || document.Length == 0)
            {
                return NotFound("لم يتم العثور على بيانات للطباعة.");
            }

            // Return PDF for printing
            return File(document, "application/pdf");
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "حدث خطأ أثناء تحضير التقرير للطباعة.", error = ex.Message });
        }
    }
}
