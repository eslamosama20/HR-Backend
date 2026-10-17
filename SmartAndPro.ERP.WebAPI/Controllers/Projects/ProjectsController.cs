using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Common.Dtos.Projects;
using SmartAndPro.ERP.Application.IServices.Projects;
using SmartAndPro.ERP.Infrastructure.Contexts;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Security.Claims;

namespace SmartAndPro.ERP.WebAPI.Controllers.Projects;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProjectsController : ControllerBase
{
    private Guid CurrentUserId
    {
        get
        {
            if (Guid.TryParse(User.FindFirstValue(ClaimTypes.Name), out var g1))
                return g1;
            if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var g2))
                return g2;
            return Guid.Empty;
        }
    }

    private readonly IProjectService _projectService;
    private readonly IProjectReportService _projectReportService;
    private readonly ERPContext _context;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(
        IProjectService projectService, 
        IProjectReportService projectReportService,
        ERPContext context,
        ILogger<ProjectsController> logger)
    {
        _projectService = projectService;
        _projectReportService = projectReportService;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [PermissionAuthorize("Projects.Read")]
    public async Task<IActionResult> GetAllProjects([FromQuery] bool includeInactive = false)
    {
        try
        {
            var projects = await _projectService.GetAllProjectsAsync(includeInactive);
            return Ok(new { success = true, data = projects });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all projects");
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء جلب المشاريع", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [PermissionAuthorize("Projects.Read")]
    public async Task<IActionResult> GetProjectById(Guid id)
    {
        try
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
                return NotFound(new { success = false, message = "المشروع غير موجود" });

            return Ok(new { success = true, data = project });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project {ProjectId}", id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء جلب المشروع", error = ex.Message });
        }
    }

    [HttpGet("{id}/details")]
    [PermissionAuthorize("Projects.Read")]
    public async Task<IActionResult> GetProjectDetails(Guid id)
    {
        try
        {
            var projectDetails = await _projectService.GetProjectDetailsAsync(id);
            if (projectDetails == null)
                return NotFound(new { success = false, message = "المشروع غير موجود" });

            return Ok(new { success = true, data = projectDetails });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project details {ProjectId}", id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء جلب تفاصيل المشروع", error = ex.Message });
        }
    }

    [HttpGet("by-status/{status}")]
    [PermissionAuthorize("Projects.Read")]
    public async Task<IActionResult> GetProjectsByStatus(string status)
    {
        try
        {
            var projects = await _projectService.GetProjectsByStatusAsync(status);
            return Ok(new { success = true, data = projects });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects by status {Status}", status);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء جلب المشاريع", error = ex.Message });
        }
    }

    [HttpGet("statuses")]
    [PermissionAuthorize("Projects.Read")]
    public async Task<IActionResult> GetProjectStatuses()
    {
        try
        {
            var statuses = await _projectService.GetProjectStatusesAsync();
            return Ok(new { success = true, data = statuses });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project statuses");
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء جلب حالات المشاريع", error = ex.Message });
        }
    }

    public class AddStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    [HttpPost("statuses")]
    [PermissionAuthorize("Projects.Create")]
    public async Task<IActionResult> AddCustomProjectStatus([FromBody] AddStatusRequest request)
    {
        try
        {
            var statuses = await _projectService.AddCustomProjectStatusAsync(request.Status);
            return Ok(new { success = true, data = statuses });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding custom project status");
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء إضافة حالة المشروع", error = ex.Message });
        }
    }

    [HttpPost]
    [PermissionAuthorize("Projects.Create")]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "بيانات غير صحيحة", errors = ModelState });

            var currentUserId = CurrentUserId;
            var project = await _projectService.CreateProjectAsync(request, currentUserId);
            
            return CreatedAtAction(nameof(GetProjectById), new { id = project.Id }, 
                new { success = true, message = "تم إنشاء المشروع بنجاح", data = project });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            var innerMsg = ex.InnerException?.Message ?? ex.Message;
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء إنشاء المشروع", error = innerMsg, details = ex.ToString() });
        }
    }

    [HttpPut("{id}")]
    [PermissionAuthorize("Projects.Update")]
    public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request)
    {
        try
        {
            if (request.Id == Guid.Empty)
            {
                request.Id = id;
            }
            else if (id != request.Id)
            {
                return BadRequest(new { success = false, message = "معرف المشروع غير متطابق" });
            }

            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "بيانات غير صحيحة", errors = ModelState });

            var currentUserId = CurrentUserId;
            var project = await _projectService.UpdateProjectAsync(request, currentUserId);
            
            return Ok(new { success = true, message = "تم تحديث المشروع بنجاح", data = project });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project {ProjectId}", id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء تحديث المشروع", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [PermissionAuthorize("Projects.Delete")]
    public async Task<IActionResult> DeleteProject(Guid id)
    {
        try
        {
            var currentUserId = CurrentUserId;
            var result = await _projectService.DeleteProjectAsync(id, currentUserId);
            
            if (!result)
                return NotFound(new { success = false, message = "المشروع غير موجود" });

            return Ok(new { success = true, message = "تم حذف المشروع بنجاح" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project {ProjectId}", id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء حذف المشروع", error = ex.Message });
        }
    }

    [HttpPut("{id}/status")]
    [PermissionAuthorize("Projects.Update")]
    public async Task<IActionResult> ChangeProjectStatus(Guid id, [FromBody] ChangeStatusRequest request)
    {
        try
        {
            var currentUserId = CurrentUserId;
            var result = await _projectService.ChangeProjectStatusAsync(id, request.NewStatus, currentUserId);
            
            if (!result)
                return NotFound(new { success = false, message = "المشروع غير موجود" });

            return Ok(new { success = true, message = "تم تغيير حالة المشروع بنجاح" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing project status {ProjectId}", id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء تغيير حالة المشروع", error = ex.Message });
        }
    }

    // ========== Project Sub-Module Read Endpoints ==========

    [HttpGet("{id}/assignment-phase")]
    [PermissionAuthorize("Projects.Assignment.Read")]
    public async Task<IActionResult> GetAssignmentPhase(Guid id)
    {
        var p = await _projectService.GetProjectDetailsAsync(id);
        return p == null ? NotFound() : Ok(p.AssignmentPhase);
    }

    [HttpGet("{id}/execution-phase")]
    [PermissionAuthorize("Projects.Execution.Read")]
    public async Task<IActionResult> GetExecutionPhase(Guid id)
    {
        var p = await _projectService.GetProjectDetailsAsync(id);
        return p == null ? NotFound() : Ok(p.ExecutionPhase);
    }

    [HttpGet("{id}/documents")]
    [PermissionAuthorize("Projects.Documents.Read")]
    public async Task<IActionResult> GetDocuments(Guid id)
    {
        var p = await _projectService.GetProjectDetailsAsync(id);
        return p == null ? NotFound() : Ok(p.Documents);
    }

    [HttpGet("{id}/payments")]
    [PermissionAuthorize("Projects.Payments.Read")]
    public async Task<IActionResult> GetPayments(Guid id)
    {
        var p = await _projectService.GetProjectDetailsAsync(id);
        return p == null ? NotFound() : Ok(p.Payments);
    }

    [HttpGet("{id}/price-quotes")]
    [PermissionAuthorize("Projects.PriceQuotes.Read")]
    public async Task<IActionResult> GetPriceQuotes(Guid id)
    {
        var p = await _projectService.GetProjectDetailsAsync(id);
        return p == null ? NotFound() : Ok(p.PriceQuotes);
    }

    [HttpGet("{id}/assessments")]
    [PermissionAuthorize("Projects.Assessments.Read")]
    public async Task<IActionResult> GetAssessments(Guid id)
    {
        var p = await _projectService.GetProjectDetailsAsync(id);
        return p == null ? NotFound() : Ok(p.Assessments);
    }

    // ========== Assignment Phase Endpoints ==========

    [HttpPost("{id}/assignment-phase")]
    [PermissionAuthorize("Projects.Assignment.Update")]
    public async Task<IActionResult> AddAssignmentPhase(Guid id, [FromBody] CreateProjectAssignmentPhaseRequest request)
    {
        try
        {
            request.ProjectId = id;
            var currentUserId = CurrentUserId;
            var result = await _projectService.AddAssignmentPhaseAsync(request, currentUserId);
            return Ok(new { success = true, message = "تم إضافة مرحلة الإسناد بنجاح", data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding assignment phase to project {ProjectId}", id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء إضافة مرحلة الإسناد", error = ex.Message });
        }
    }

    [HttpPut("{id}/assignment-phase")]
    [PermissionAuthorize("Projects.Assignment.Update")]
    public async Task<IActionResult> UpdateAssignmentPhase(Guid id, [FromBody] CreateProjectAssignmentPhaseRequest request)
    {
        try
        {
            request.ProjectId = id;
            var currentUserId = CurrentUserId;
            var result = await _projectService.UpdateAssignmentPhaseAsync(request, currentUserId);
            return Ok(new { success = true, message = "تم تحديث مرحلة الإسناد بنجاح", data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating assignment phase for project {ProjectId}", id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء تحديث مرحلة الإسناد", error = ex.Message });
        }
    }

    // ========== Execution Phase Endpoints ==========

    [HttpPost("{id}/execution-phase")]
    [PermissionAuthorize("Projects.Execution.Update")]
    public async Task<IActionResult> AddExecutionPhase(Guid id, [FromBody] CreateProjectExecutionPhaseRequest request)
    {
        try
        {
            request.ProjectId = id;
            var currentUserId = CurrentUserId;
            var result = await _projectService.AddExecutionPhaseAsync(request, currentUserId);
            return Ok(new { success = true, message = "تم إضافة مرحلة التنفيذ بنجاح", data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding execution phase to project {ProjectId}", id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء إضافة مرحلة التنفيذ", error = ex.Message });
        }
    }

    [HttpPut("{id}/execution-phase")]
    [PermissionAuthorize("Projects.Execution.Update")]
    public async Task<IActionResult> UpdateExecutionPhase(Guid id, [FromBody] CreateProjectExecutionPhaseRequest request)
    {
        try
        {
            request.ProjectId = id;
            var currentUserId = CurrentUserId;
            var result = await _projectService.UpdateExecutionPhaseAsync(request, currentUserId);
            return Ok(new { success = true, message = "تم تحديث مرحلة التنفيذ بنجاح", data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating execution phase for project {ProjectId}", id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء تحديث مرحلة التنفيذ", error = ex.Message });
        }
    }

    // ========== Document Endpoints ==========

    [HttpPost("{id}/documents")]
    [PermissionAuthorize("Projects.Documents.Create")]
    public async Task<IActionResult> AddDocument(Guid id, [FromBody] CreateProjectDocumentRequest request)
    {
        try
        {
            request.ProjectId = id;
            var currentUserId = CurrentUserId;
            var result = await _projectService.AddDocumentAsync(request, currentUserId);
            return Ok(new { success = true, message = "تم إضافة المستند بنجاح", data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding document to project {ProjectId}", id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء إضافة المستند", error = ex.Message });
        }
    }

    [HttpDelete("{id}/documents/{documentId}")]
    [PermissionAuthorize("Projects.Documents.Delete")]
    public async Task<IActionResult> DeleteDocument(Guid id, Guid documentId)
    {
        try
        {
            var currentUserId = CurrentUserId;
            var result = await _projectService.DeleteDocumentAsync(documentId, currentUserId);
            if (!result)
                return NotFound(new { success = false, message = "المستند غير موجود" });

            return Ok(new { success = true, message = "تم حذف المستند بنجاح" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId} from project {ProjectId}", documentId, id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء حذف المستند", error = ex.Message });
        }
    }

    // ========== Payment Endpoints ==========

    [HttpPost("{id}/payments")]
    [PermissionAuthorize("Projects.Payments.Create")]
    public async Task<IActionResult> AddPayment(Guid id, [FromBody] CreateProjectPaymentRequest request)
    {
        try
        {
            request.ProjectId = id;
            var currentUserId = CurrentUserId;
            var result = await _projectService.AddPaymentAsync(request, currentUserId);
            return Ok(new { success = true, message = "تم إضافة الدفعة بنجاح", data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding payment to project {ProjectId}", id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء إضافة الدفعة", error = ex.Message });
        }
    }

    [HttpPut("{id}/payments/{paymentId}")]
    [PermissionAuthorize("Projects.Payments.Update")]
    public async Task<IActionResult> UpdatePayment(Guid id, Guid paymentId, [FromBody] UpdateProjectPaymentRequest request)
    {
        try
        {
            if (paymentId != request.Id)
                return BadRequest(new { success = false, message = "معرف الدفعة غير متطابق" });

            request.ProjectId = id;
            var currentUserId = CurrentUserId;
            var result = await _projectService.UpdatePaymentAsync(request, currentUserId);
            return Ok(new { success = true, message = "تم تحديث الدفعة بنجاح", data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment {PaymentId} for project {ProjectId}", paymentId, id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء تحديث الدفعة", error = ex.Message });
        }
    }

    [HttpDelete("{id}/payments/{paymentId}")]
    [PermissionAuthorize("Projects.Payments.Delete")]
    public async Task<IActionResult> DeletePayment(Guid id, Guid paymentId)
    {
        try
        {
            var currentUserId = CurrentUserId;
            var result = await _projectService.DeletePaymentAsync(paymentId, currentUserId);
            if (!result)
                return NotFound(new { success = false, message = "الدفعة غير موجودة" });

            return Ok(new { success = true, message = "تم حذف الدفعة بنجاح" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment {PaymentId} from project {ProjectId}", paymentId, id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء حذف الدفعة", error = ex.Message });
        }
    }

    [HttpPost("{id}/upload-document")]
    [PermissionAuthorize("Projects.Documents.Create")]
    public async Task<IActionResult> UploadDocument(Guid id, [FromForm] string documentType, [FromForm] string documentName, IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "لم يتم اختيار ملف لرفعه" });

            var currentUserId = CurrentUserId;
            
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            var result = await _projectService.UploadDocumentAsync(id, documentType, documentName, file.FileName, fileBytes, file.ContentType, currentUserId);
            return Ok(new { success = true, message = "تم رفع الملف بنجاح للأرشيف", data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document to project {ProjectId}", id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء رفع الملف", error = ex.Message });
        }
    }

    [HttpPut("{id}/documents/{documentId}/rename")]
    [PermissionAuthorize("Projects.Documents.Create")]
    public async Task<IActionResult> UpdateDocumentName(Guid id, Guid documentId, [FromBody] UpdateDocumentNameDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto?.NewName))
                return BadRequest(new { success = false, message = "اسم المستند مطلوب" });

            var currentUserId = CurrentUserId;
            var result = await _projectService.UpdateDocumentNameAsync(documentId, dto.NewName, currentUserId);
            if (!result) return NotFound(new { success = false, message = "المستند غير موجود" });

            return Ok(new { success = true, message = "تم تعديل اسم المستند بنجاح" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renaming document {DocumentId} in project {ProjectId}", documentId, id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء تعديل اسم المستند", error = ex.Message });
        }
    }

    [HttpGet("{id}/account-statement/pdf")]
    [PermissionAuthorize("Projects.Reports.Read")]
    public async Task<IActionResult> DownloadAccountStatementPdf(Guid id)
    {
        try
        {
            var fileBytes = await _projectReportService.GenerateAccountStatementPdfAsync(id);
            var fileName = $"Project_Statement_{id}.pdf";
            return File(fileBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading statement PDF for project {ProjectId}", id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء توليد ملف كشف الحساب", error = ex.Message });
        }
    }

    [HttpGet("{id}/archive/pdf")]
    [PermissionAuthorize("Projects.Reports.Read")]
    public async Task<IActionResult> DownloadProjectArchivePdf(Guid id)
    {
        try
        {
            var fileBytes = await _projectReportService.GenerateProjectArchivePdfAsync(id);
            var fileName = $"Project_Archive_{id}.pdf";
            return File(fileBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading archive PDF for project {ProjectId}", id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء توليد ملف الأرشيف", error = ex.Message });
        }
    }

    [HttpGet("share/{shareToken}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProjectByShareToken(string shareToken)
    {
        try
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ShareToken == shareToken && !p.IsDeleted && p.IsActive);
            
            if (project == null)
                return NotFound(new { success = false, message = "المشروع غير موجود أو تم إيقاف الرابط" });

            var projectDetails = await _projectService.GetProjectDetailsAsync(project.Id);
            return Ok(new { success = true, data = projectDetails });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project by share token {ShareToken}", shareToken);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء جلب بيانات المشاركة", error = ex.Message });
        }
    }

    // ========== Price Quotes Endpoints ==========

    [HttpPost("{id}/price-quotes")]
    [PermissionAuthorize("Projects.PriceQuotes.Create")]
    public async Task<IActionResult> AddPriceQuote(Guid id, [FromBody] CreateProjectPriceQuoteRequest request)
    {
        try
        {
            request.ProjectId = id;
            var currentUserId = CurrentUserId;
            var result = await _projectService.AddPriceQuoteAsync(request, currentUserId);
            return Ok(new { success = true, message = "تم إضافة عرض السعر بنجاح", data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding price quote to project {ProjectId}", id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء إضافة عرض السعر", error = ex.Message });
        }
    }

    [HttpPut("{id}/price-quotes/{quoteId}")]
    [PermissionAuthorize("Projects.PriceQuotes.Update")]
    public async Task<IActionResult> UpdatePriceQuote(Guid id, Guid quoteId, [FromBody] UpdateProjectPriceQuoteRequest request)
    {
        try
        {
            request.ProjectId = id;
            request.Id = quoteId;
            var currentUserId = CurrentUserId;
            var result = await _projectService.UpdatePriceQuoteAsync(request, currentUserId);
            return Ok(new { success = true, message = "تم تعديل عرض السعر بنجاح", data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating price quote {QuoteId} for project {ProjectId}", quoteId, id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء تعديل عرض السعر", error = ex.Message });
        }
    }

    [HttpDelete("{id}/price-quotes/{quoteId}")]
    [PermissionAuthorize("Projects.PriceQuotes.Delete")]
    public async Task<IActionResult> DeletePriceQuote(Guid id, Guid quoteId)
    {
        try
        {
            var currentUserId = CurrentUserId;
            var result = await _projectService.DeletePriceQuoteAsync(quoteId, currentUserId);
            if (!result)
                return NotFound(new { success = false, message = "عرض السعر غير موجود" });

            return Ok(new { success = true, message = "تم حذف عرض السعر بنجاح" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting price quote {QuoteId} from project {ProjectId}", quoteId, id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء حذف عرض السعر", error = ex.Message });
        }
    }

    [HttpPut("{id}/price-quotes/{quoteId}/accept")]
    [PermissionAuthorize("Projects.PriceQuotes.Accept")]
    public async Task<IActionResult> AcceptPriceQuote(Guid id, Guid quoteId)
    {
        try
        {
            var currentUserId = CurrentUserId;
            var result = await _projectService.AcceptPriceQuoteAsync(quoteId, currentUserId);
            if (!result)
                return NotFound(new { success = false, message = "عرض السعر غير موجود" });

            return Ok(new { success = true, message = "تم قبول عرض السعر بنجاح كعرض معتمد للمشروع" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting price quote {QuoteId} for project {ProjectId}", quoteId, id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء قبول عرض السعر", error = ex.Message });
        }
    }

    // ========== Project Assessment Endpoints ==========

    [HttpPost("{id}/assessments")]
    [PermissionAuthorize("Projects.Assessments.Create")]
    public async Task<IActionResult> AddAssessment(Guid id, [FromBody] CreateProjectAssessmentRequest request)
    {
        try
        {
            request.ProjectId = id;
            var currentUserId = CurrentUserId;
            var result = await _projectService.AddAssessmentAsync(request, currentUserId);
            return Ok(new { success = true, message = "تم إضافة التقييم بنجاح", data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding assessment to project {ProjectId}", id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء إضافة التقييم", error = ex.Message });
        }
    }

    [HttpPut("{id}/assessments/{assessmentId}")]
    [PermissionAuthorize("Projects.Assessments.Update")]
    public async Task<IActionResult> UpdateAssessment(Guid id, Guid assessmentId, [FromBody] UpdateProjectAssessmentRequest request)
    {
        try
        {
            request.ProjectId = id;
            request.Id = assessmentId;
            var currentUserId = CurrentUserId;
            var result = await _projectService.UpdateAssessmentAsync(request, currentUserId);
            return Ok(new { success = true, message = "تم تعديل التقييم بنجاح", data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating assessment {AssessmentId} for project {ProjectId}", assessmentId, id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء تعديل التقييم", error = ex.Message });
        }
    }

    [HttpDelete("{id}/assessments/{assessmentId}")]
    [PermissionAuthorize("Projects.Assessments.Delete")]
    public async Task<IActionResult> DeleteAssessment(Guid id, Guid assessmentId)
    {
        try
        {
            var currentUserId = CurrentUserId;
            var result = await _projectService.DeleteAssessmentAsync(assessmentId, currentUserId);
            if (!result)
                return NotFound(new { success = false, message = "التقييم غير موجود" });

            return Ok(new { success = true, message = "تم حذف التقييم بنجاح" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting assessment {AssessmentId} from project {ProjectId}", assessmentId, id);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء حذف التقييم", error = ex.Message });
        }
    }

    // ========== Milestone Endpoints ==========

    [HttpPost("milestones/{milestoneId}/toggle")]
    [PermissionAuthorize("Projects.Update")]
    public async Task<IActionResult> ToggleMilestone(Guid milestoneId, [FromBody] ToggleMilestoneRequest request)
    {
        try
        {
            var currentUserId = CurrentUserId;
            var result = await _projectService.ToggleMilestoneCompletionAsync(milestoneId, request.IsCompleted, currentUserId);
            return Ok(new { success = true, message = "تم تحديث حالة المرحلة بنجاح", data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling milestone completion {MilestoneId}", milestoneId);
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء تحديث حالة المرحلة", error = ex.Message });
        }
    }

    [HttpGet("/uploads/{fileName}")]
    [AllowAnonymous]
    public IActionResult GetUploadedFile(string fileName)
    {
        try
        {
            var decodedFileName = Uri.UnescapeDataString(fileName);
            var pathsToTry = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "uploads", decodedFileName),
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", decodedFileName),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "uploads", fileName),
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName)
            };

            foreach (var path in pathsToTry)
            {
                if (System.IO.File.Exists(path))
                {
                    var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
                    if (!provider.TryGetContentType(path, out var contentType))
                    {
                        contentType = "application/octet-stream";
                    }
                    return File(System.IO.File.OpenRead(path), contentType);
                }
            }

            return NotFound("File not found on server");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

public class ChangeStatusRequest
{
    public string NewStatus { get; set; } = string.Empty;
}

public class ToggleMilestoneRequest
{
    public bool IsCompleted { get; set; }
}

public class UpdateDocumentNameDto
{
    public string NewName { get; set; } = string.Empty;
}
