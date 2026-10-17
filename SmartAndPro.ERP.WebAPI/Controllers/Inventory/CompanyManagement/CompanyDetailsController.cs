using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyDetails;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Constants;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Services.Cloudinary;
using SmartAndPro.ERP.WebAPI.Utils;
namespace SmartAndPro.ERP.WebAPI.Controllers.Inventory.CompanyManagement;

[Route("api/inventory/[controller]/")]
[ApiController]
[Authorize]
public class CompanyDetailsController : Controller
{
    private readonly CompanyService _service;
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly SmartAndPro.ERP.Application.Services.Auth.Interfaces.IRoleQuotaService _quotaService;

    public CompanyDetailsController(
        ServiceBaseFactory serviceBaseFactory,
        ICloudinaryService cloudinaryService,
        RepositoriesFactory repositoriesFactory,
        SmartAndPro.ERP.Application.Services.Auth.Interfaces.IRoleQuotaService quotaService)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<CompanyService>();
        _cloudinaryService = cloudinaryService;
        _repositoriesFactory = repositoriesFactory;
        _quotaService = quotaService;
    }


    // CompanyDetails get all
    [PermissionAuthorize(Permissions.CompanyDetails.Read)]
    [HttpGet("")]
    public async Task<IActionResult> GetCompanyDetails([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? fields = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllAsync(new CompanyDetailsFiltersDto(), pageNumber, pageSize);
            if (!string.IsNullOrWhiteSpace(fields) && responseDate.ErrorCode == SmartAndPro.ERP.SharedKernel.Enums.ErrorCodes.Success && responseDate.Data is not null)
            {
                return Ok(responseDate.Data.ApplyFieldSelection(fields));
            }
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }
    
    [PermissionAuthorize(Permissions.CompanyDetails.Read)]
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? q = null,
        [FromQuery] int? pageNumber = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sortField = null,
        [FromQuery] int? sortOrder = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var filters = new CompanyDetailsFiltersDto
            {
                SearchKeyword = q,
                ArabicName = HttpContext.Request.Query["arabicName"],
                EnglishName = HttpContext.Request.Query["englishName"],
                Email = HttpContext.Request.Query["email"],
                TaxNumber = HttpContext.Request.Query["taxNumber"]
            };
            var responseDate = await _service.GetAllAsync(filters, pageNumber, pageSize, sortField, sortOrder);
            result = responseDate.HandleResult(true);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    [HttpPost("toggleZatcaIntegration")]
    [PermissionAuthorize(Permissions.CompanyDetails.Update)]
    public async Task<IActionResult> ToggleZatcaIntegration([FromBody] IdObject idObject)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.ToggleZatcaIntegrationAsync(idObject.Id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }
    
    [HttpPost("integrateZatca/{id:guid}")]
    [PermissionAuthorize(Permissions.CompanyDetails.Update)]
    public async Task<IActionResult> IntegrateZatca(Guid id, [FromQuery] string otp)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = new ContentResult
        {
            StatusCode = (int)HttpStatusCode.InternalServerError,
            Content = "An error occurred while processing your request.",
            ContentType = "text/plain"
        };
        try
        {
            if (string.IsNullOrEmpty(otp))
            {
                return BadRequest("OTP is required");
            }
            var responseDate = await _service.IntegrateZatcaAsync(id, otp);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    // CompanyDetails get by id
    [PermissionAuthorize(Permissions.CompanyDetails.Read)]
    [HttpGet("{id:guid}")]
    
    public async Task<IActionResult> GetCompanyDetailsById(Guid id)
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

    // CompanyDetails create
    [HttpPost("")]
    [PermissionAuthorize(Permissions.CompanyDetails.Create)]
    public async Task<IActionResult> CreateCompanyDetails([FromBody] CompanyDetailsPostDto companyDetails)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            await _quotaService.ValidateCompanyQuotaAsync(User);
            var responseDate = await _service.AddAsync(companyDetails);
            return responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return BadRequest(new { Message = ex.Message });
        }
    }

    // CompanyDetails update
    [HttpPut]
    [PermissionAuthorize(Permissions.CompanyDetails.Update)]
    public async Task<IActionResult> UpdateCompanyDetails([FromBody] CompanyDetailsPutDto companyDetails)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(companyDetails);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    // CompanyDetails delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.CompanyDetails.Delete)]
    public async Task<IActionResult> DeleteCompanyDetails(Guid id)
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

    [HttpPost("/api/company/upload-logo")]
    [Consumes("multipart/form-data")]
    [PermissionAuthorize(Permissions.CompanyDetails.Update)]
    public async Task<IActionResult> UploadCompanyLogo([FromForm] Guid companyDetailsId, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded." });
        }

        var companyRepo = _repositoriesFactory.GetRepository<CompanyDetails>();
        var companyQuery = await companyRepo.Filter(x => x.Id == companyDetailsId);
        var company = companyQuery.Data?.FirstOrDefault();
        if (company is null)
        {
            return NotFound(new { message = "Company details not found." });
        }

        try
        {
            var uploadResult = await _cloudinaryService.UploadImageAsync(file, "company/logo", cancellationToken);
            company.Logo = null;
            company.LogoUrl = uploadResult.Url;
            await companyRepo.Update(company);

            return Ok(new { url = company.LogoUrl });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "Failed to upload logo." });
        }
    }

    [HttpPost("/api/company/upload-logo-temp")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadCompanyLogoTemp([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded." });
        }

        try
        {
            var uploadResult = await _cloudinaryService.UploadImageAsync(file, "company/logo", cancellationToken);
            return Ok(new { url = uploadResult.Url, publicId = uploadResult.PublicId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "Failed to upload logo." });
        }
    }

    [HttpPost("/api/company/upload-stamp")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadCompanyStamp([FromForm] Guid companyDetailsId, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded." });
        }

        var companyRepo = _repositoriesFactory.GetRepository<CompanyDetails>();
        var companyQuery = await companyRepo.Filter(x => x.Id == companyDetailsId);
        var company = companyQuery.Data?.FirstOrDefault();
        if (company is null)
        {
            return NotFound(new { message = "Company details not found." });
        }

        try
        {
            var uploadResult = await _cloudinaryService.UploadImageAsync(file, "company/stamp", cancellationToken);
            company.Stamp = null;
            company.StampUrl = uploadResult.Url;
            await companyRepo.Update(company);

            return Ok(new { url = company.StampUrl });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "Failed to upload stamp." });
        }
    }

    [HttpPost("/api/company/upload-stamp-temp")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadCompanyStampTemp([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded." });
        }

        try
        {
            var uploadResult = await _cloudinaryService.UploadImageAsync(file, "company/stamp", cancellationToken);
            return Ok(new { url = uploadResult.Url, publicId = uploadResult.PublicId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "Failed to upload stamp." });
        }
    }

}
