using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.RoomType;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Services.@base;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Services.Reservation;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.SharedKernel.Utilities;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Services.Cloudinary;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace SmartAndPro.ERP.WebAPI.Controllers.Reservation;

[Route("api/[controller]/")]
[ApiController]
[Authorize]
[PermissionAuthorize("RoomType.Read")]
public class RoomTypeController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly RoomTypeService _service;
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly ICloudinaryService _cloudinaryService;

    public RoomTypeController(
        ServiceBaseFactory serviceBaseFactory,
        RepositoriesFactory repositoriesFactory,
        ICloudinaryService cloudinaryService)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<RoomTypeService>();
        _repositoriesFactory = repositoriesFactory;
        _cloudinaryService = cloudinaryService;
    }


    // RoomTypes get all

    /// <summary>
    /// Deprecated endpoint. Use <c>GET /api/RoomType/paginated</c> for paginated lightweight lookup payloads.
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> GetRoomTypes([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? fields = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllAsync(new RoomTypeFiltersDto(), pageNumber, pageSize);
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

    [HttpGet("paginated")]
    [OutputCache(Duration = 900, VaryByQueryKeys = new[] { "pageNumber", "pageSize", "sortField" })]
    public async Task<IActionResult> GetPaginatedRoomTypes([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, [FromQuery] string? sortField = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseData = await _service.GetPaginatedLookupAsync(pageNumber, pageSize, sortField);
            result = responseData.HandleResult(isPaginated: true);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // RoomTypes get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRoomTypesById(Guid id)
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

    // RoomTypes create
    [HttpPost("")]
    [PermissionAuthorize("RoomType.Create")]
    public async Task<IActionResult> CreateRoomTypes([FromBody] RoomTypePostDto roomTypes)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(roomTypes);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // RoomTypes update
    [HttpPut]
    [PermissionAuthorize("RoomType.Update")]
    public async Task<IActionResult> UpdateRoomTypes([FromBody] RoomTypePutDto roomTypes)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(roomTypes);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // RoomTypes delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize("RoomType.Delete")]
    public async Task<IActionResult> DeleteRoomTypes(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<RoomTypeService>();
            var responseDate = await service.DeleteAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    // Get all frontal room types
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

    // Make room type frontal
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

    [HttpPost("/api/roomType/upload-logo")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadLogo([FromForm] Guid roomTypeId, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        var repo = _repositoriesFactory.GetRepository<RoomType>();
        var roomTypeQuery = await repo.Filter(x => x.Id == roomTypeId);
        var roomType = roomTypeQuery.Data?.FirstOrDefault();
        if (roomType is null)
            return NotFound(new { message = "Room type not found." });

        try
        {
            var uploadResult = await _cloudinaryService.UploadImageAsync(file, "roomType/logo", cancellationToken);
            roomType.Logo = null;
            roomType.LogoUrl = uploadResult.Url;
            roomType.LogoPublicId = uploadResult.PublicId;
            await repo.Update(roomType);

            return Ok(new { url = roomType.LogoUrl, publicId = roomType.LogoPublicId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("/api/roomType/upload-logo-temp")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadLogoTemp([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        try
        {
            var uploadResult = await _cloudinaryService.UploadImageAsync(file, "roomType/logo", cancellationToken);
            return Ok(new { url = uploadResult.Url, publicId = uploadResult.PublicId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("/api/roomType/upload-images")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImages([FromForm] Guid roomTypeId, [FromForm] List<IFormFile> files, CancellationToken cancellationToken)
    {
        if (files is null || files.Count == 0)
            return BadRequest(new { message = "No files uploaded." });

        var repo = _repositoriesFactory.GetRepository<RoomType>();
        var roomTypeQuery = await repo.Filter(x => x.Id == roomTypeId);
        var roomType = roomTypeQuery.Data?.FirstOrDefault();
        if (roomType is null)
            return NotFound(new { message = "Room type not found." });

        try
        {
            var imageUrls = new List<string>();

            foreach (var file in files)
            {
                var uploadResult = await _cloudinaryService.UploadImageAsync(file, "roomType/images", cancellationToken);
                imageUrls.Add(uploadResult.Url);
            }

            roomType.Images = null;
            roomType.ImageUrls = JsonSerializer.Serialize(imageUrls);
            await repo.Update(roomType);

            return Ok(new { imageUrls });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("/api/roomType/upload-images-temp")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImagesTemp([FromForm] List<IFormFile> files, CancellationToken cancellationToken)
    {
        if (files is null || files.Count == 0)
            return BadRequest(new { message = "No files uploaded." });

        try
        {
            var imageUrls = new List<string>();
            foreach (var file in files)
            {
                var uploadResult = await _cloudinaryService.UploadImageAsync(file, "roomType/images", cancellationToken);
                imageUrls.Add(uploadResult.Url);
            }

            return Ok(new { imageUrls });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

}
