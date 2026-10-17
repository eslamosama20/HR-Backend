using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.Hotel;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Reservation;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Services.Cloudinary;
using SmartAndPro.ERP.WebAPI.Utils;

namespace SmartAndPro.ERP.WebAPI.Controllers.Reservation;

[Route("api/[controller]/")]
[ApiController]
[Authorize]
[PermissionAuthorize("Hotels.Read")]
public class HotelController : Controller
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly HotelService _service;
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly ICloudinaryService _cloudinaryService;

    public HotelController(
        ServiceBaseFactory serviceBaseFactory,
        RepositoriesFactory repositoriesFactory,
        ICloudinaryService cloudinaryService)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<HotelService>();
        _repositoriesFactory = repositoriesFactory;
        _cloudinaryService = cloudinaryService;
    }


    // Hotels get all

    [HttpGet("")]
    public async Task<IActionResult> GetHotels([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? fields = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllAsync(new HotelFiltersDto(), pageNumber, pageSize);
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

    // Search
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
            var responseDate = await _service.SearchAsync(q, pageNumber, pageSize, sortField, sortOrder);
            result = responseDate.HandleResult(true);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // Hotels get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetHotelsById(Guid id)
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

    // Hotels create
    [HttpPost("")]
    [PermissionAuthorize("Hotels.Create")]
    public async Task<IActionResult> CreateHotels([FromBody] HotelPostDto hotels)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(hotels);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // Hotels update
    [HttpPut]
    [PermissionAuthorize("Hotels.Update")]
    public async Task<IActionResult> UpdateHotels([FromBody] HotelPutDto hotels)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(hotels);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // Hotels delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize("Hotels.Delete")]
    public async Task<IActionResult> DeleteHotels(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<HotelService>();
            var responseDate = await service.DeleteAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    // Get all frontal hotels
    [HttpGet("frontal")]
    public async Task<IActionResult> GetAllFrontal()
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseData = await _service.GetAllFrontalAsync();
            result = responseData.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // Make hotel frontal
    [HttpPost("frontal/{id}")]
    public async Task<IActionResult> MakeFrontalAsync(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseData = await _service.MakeFrontalAsync(id);
            result = responseData.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // Import hotels from Excel
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportHotels([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".xlsx" && ext != ".xls")
            return BadRequest("Invalid file type. Only .xlsx and .xls files are supported.");

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _service.ImportFromExcelAsync(stream);
            return Ok(result);
        }
        catch (Exception ex)
        {
            try { LogService.LogError(ex); } catch { /* LogService may not have write permission */ }
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("/api/reservation/hotels/template")]
    public IActionResult DownloadTemplate()
    {
        var fileName = "hotels_import_template.xlsx";
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Assets", fileName);

        if (!System.IO.File.Exists(templatePath))
            return NotFound(new { message = "Template file not found." });

        return PhysicalFile(templatePath, ExcelContentType, fileName);
    }

    [HttpPost("/api/hotel/upload-logo")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadLogo([FromForm] Guid hotelId, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        var repo = _repositoriesFactory.GetRepository<Hotel>();
        var hotelQuery = await repo.Filter(x => x.Id == hotelId);
        var hotel = hotelQuery.Data?.FirstOrDefault();
        if (hotel is null)
            return NotFound(new { message = "Hotel not found." });

        try
        {
            var uploadResult = await _cloudinaryService.UploadImageAsync(file, "hotel/logo", cancellationToken);
            hotel.Logo = null;
            hotel.LogoUrl = uploadResult.Url;
            hotel.LogoPublicId = uploadResult.PublicId;
            await repo.Update(hotel);

            return Ok(new { url = hotel.LogoUrl, publicId = hotel.LogoPublicId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("/api/hotel/upload-logo-temp")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadLogoTemp([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        try
        {
            var uploadResult = await _cloudinaryService.UploadImageAsync(file, "hotel/logo", cancellationToken);
            return Ok(new { url = uploadResult.Url, publicId = uploadResult.PublicId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

}
