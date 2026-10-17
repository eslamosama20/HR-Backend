using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.PublicBooking;
using SmartAndPro.ERP.Domains.Entities.PublicBooking;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.WebAPI.Services.Cloudinary;
using System.Text.Json;

namespace SmartAndPro.ERP.WebAPI.Controllers.PublicBooking;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PublicRoomTypeController : ControllerBase
{
    private readonly PublicRoomTypeService _service;
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ILogger<PublicRoomTypeController> _logger;

    public PublicRoomTypeController(
        ServiceBaseFactory serviceBaseFactory,
        RepositoriesFactory repositoriesFactory,
        ICloudinaryService cloudinaryService,
        ILogger<PublicRoomTypeController> logger)
    {
        _service = serviceBaseFactory.GetService<PublicRoomTypeService>();
        _repositoriesFactory = repositoriesFactory;
        _cloudinaryService = cloudinaryService;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PublicRoomTypeFiltersDto filters, [FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _service.GetAllAsync(filters, pageNumber, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all room types");
            return StatusCode(500, "An error occurred");
        }
    }

    [AllowAnonymous]
    [HttpGet("hotel/{hotelId:guid}")]
    public async Task<IActionResult> GetByHotel(Guid hotelId)
    {
        try
        {
            var result = await _service.GetByHotelAsync(hotelId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting room types by hotel {HotelId}", hotelId);
            return StatusCode(500, "An error occurred");
        }
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _service.GetAsync(id);
            if (result.ErrorCode == SmartAndPro.ERP.SharedKernel.Enums.ErrorCodes.NotFound)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting room type by ID {RoomTypeId}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}/price")]
    public async Task<IActionResult> GetPrice(Guid id, [FromQuery] DateTime date)
    {
        try
        {
            var result = await _service.GetRoomPriceAsync(id, date);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting room price for {RoomTypeId}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}/availability")]
    public async Task<IActionResult> GetAvailability(
        Guid id,
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut)
    {
        try
        {
            var result = await _service.GetAvailableRoomsAsync(id, checkIn, checkOut);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting availability for room type {RoomTypeId}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PublicRoomTypePostDto roomType)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _service.AddAsync(roomType);
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating room type");
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PublicRoomTypePutDto roomType)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (id != roomType.Id) return BadRequest("ID mismatch");

        try
        {
            var result = await _service.UpdateAsync(roomType);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating room type {RoomTypeId}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            // Clean up Cloudinary images before soft-deleting
            var repo = _repositoriesFactory.GetRepository<PublicRoomType>();
            var query = await repo.Filter(r => r.Id == id);
            var roomType = query.Data?.FirstOrDefault();
            if (roomType is not null)
            {
                if (!string.IsNullOrWhiteSpace(roomType.MainImagePublicId))
                {
                    try { await _cloudinaryService.DeleteImageAsync(roomType.MainImagePublicId, cancellationToken); }
                    catch { _logger.LogWarning("Failed to delete main image for room type {RoomTypeId}", id); }
                }

                if (!string.IsNullOrWhiteSpace(roomType.AdditionalImageUrls))
                {
                    try
                    {
                        var urls = JsonSerializer.Deserialize<List<string>>(roomType.AdditionalImageUrls);
                        if (urls is not null)
                        {
                            foreach (var url in urls)
                            {
                                var publicId = _cloudinaryService.ExtractPublicIdFromUrl(url);
                                if (publicId is not null)
                                    try { await _cloudinaryService.DeleteImageAsync(publicId, cancellationToken); }
                                    catch { _logger.LogWarning("Failed to delete additional image for room type {RoomTypeId}", id); }
                            }
                        }
                    }
                    catch { _logger.LogWarning("Failed to parse additional image URLs for room type {RoomTypeId}", id); }
                }
            }

            var result = await _service.DeleteAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting room type {RoomTypeId}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpPost("{publicRoomTypeId:guid}/link-erp/{erpRoomTypeId:guid}")]
    public async Task<IActionResult> LinkToErpRoomType(Guid publicRoomTypeId, Guid erpRoomTypeId)
    {
        try
        {
            var result = await _service.LinkToErpRoomTypeAsync(publicRoomTypeId, erpRoomTypeId);
            if (result.ErrorCode == SmartAndPro.ERP.SharedKernel.Enums.ErrorCodes.NotFound)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking room type {PublicRoomTypeId} to ERP {ErpRoomTypeId}", publicRoomTypeId, erpRoomTypeId);
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpPost("{publicRoomTypeId:guid}/unlink-erp")]
    public async Task<IActionResult> UnlinkFromErpRoomType(Guid publicRoomTypeId)
    {
        try
        {
            var result = await _service.UnlinkFromErpRoomTypeAsync(publicRoomTypeId);
            if (result.ErrorCode == SmartAndPro.ERP.SharedKernel.Enums.ErrorCodes.NotFound)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlinking room type {PublicRoomTypeId}", publicRoomTypeId);
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpPost("{id:guid}/upload-images")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImages(Guid id, [FromForm] List<IFormFile> files, CancellationToken cancellationToken)
    {
        if (files is null || files.Count == 0)
            return BadRequest(new { message = "No files uploaded." });
        if (files.Count > 10)
            return BadRequest(new { message = "Maximum 10 files per upload." });
        if (files.Any(f => f.Length > 10 * 1024 * 1024))
            return BadRequest(new { message = "One or more files exceed the maximum allowed size of 10 MB." });

        var repo = _repositoriesFactory.GetRepository<PublicRoomType>();
        var query = await repo.Filter(h => h.Id == id);
        var roomType = query.Data?.FirstOrDefault();
        if (roomType is null)
            return NotFound(new { message = "Room type not found." });

        try
        {
            var additionalUrls = new List<string>();

            // Parse existing additional image URLs
            if (!string.IsNullOrWhiteSpace(roomType.AdditionalImageUrls))
            {
                try
                {
                    var existing = JsonSerializer.Deserialize<List<string>>(roomType.AdditionalImageUrls) ?? [];
                    additionalUrls.AddRange(existing);
                }
                catch { _logger.LogWarning("Failed to parse existing additional image URLs for room type {RoomTypeId}", id); }
            }

            for (int i = 0; i < files.Count; i++)
            {
                if (files[i] is null || files[i].Length == 0) continue;

                var uploadResult = await _cloudinaryService.UploadImageAsync(files[i], "publicRoomType/images", cancellationToken, transformation: new CloudinaryTransformation(Width: 1200, Quality: 85));

                if (i == 0 && string.IsNullOrWhiteSpace(roomType.MainImageUrl))
                {
                    // First upload becomes main image if none exists
                    if (!string.IsNullOrWhiteSpace(roomType.MainImagePublicId))
                    {
                        try { await _cloudinaryService.DeleteImageAsync(roomType.MainImagePublicId, cancellationToken); }
                        catch { _logger.LogWarning("Failed to delete previous main image for room type {RoomTypeId}", id); }
                    }
                    roomType.MainImageUrl = uploadResult.Url;
                    roomType.MainImagePublicId = uploadResult.PublicId;
                }
                else
                {
                    additionalUrls.Add(uploadResult.Url);
                }
            }

            roomType.AdditionalImageUrls = additionalUrls.Count > 0 ? JsonSerializer.Serialize(additionalUrls) : null;
            await repo.Update(roomType);

            return Ok(new { mainImageUrl = roomType.MainImageUrl, additionalImageUrls = roomType.AdditionalImageUrls });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid image format for room type {RoomTypeId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("upload-images-temp")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImagesTemp([FromForm] List<IFormFile> files, CancellationToken cancellationToken)
    {
        if (files is null || files.Count == 0)
            return BadRequest(new { message = "No files uploaded." });
        if (files.Count > 10)
            return BadRequest(new { message = "Maximum 10 files per upload." });
        if (files.Any(f => f.Length > 10 * 1024 * 1024))
            return BadRequest(new { message = "One or more files exceed the maximum allowed size of 10 MB." });

        try
        {
            var results = new List<object>();
            foreach (var file in files)
            {
                if (file is null || file.Length == 0) continue;
                var uploadResult = await _cloudinaryService.UploadImageAsync(file, "publicRoomType/images/temp", cancellationToken, transformation: new CloudinaryTransformation(Width: 1200, Quality: 85));
                results.Add(new { url = uploadResult.Url, thumbnailUrl = uploadResult.ThumbnailUrl, publicId = uploadResult.PublicId });
            }

            return Ok(new { urls = results });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid image format for temp images upload");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}/images/{index}")]
    public async Task<IActionResult> DeleteImage(Guid id, int index, CancellationToken cancellationToken)
    {
        var repo = _repositoriesFactory.GetRepository<PublicRoomType>();
        var query = await repo.Filter(h => h.Id == id);
        var roomType = query.Data?.FirstOrDefault();
        if (roomType is null)
            return NotFound(new { message = "Room type not found." });

        if (index < 0)
            return BadRequest(new { message = "Invalid image index." });

        try
        {
            if (index == 0)
            {
                // Delete main image
                if (!string.IsNullOrWhiteSpace(roomType.MainImagePublicId))
                {
                    try { await _cloudinaryService.DeleteImageAsync(roomType.MainImagePublicId, cancellationToken); }
                    catch { _logger.LogWarning("Failed to delete main image for room type {RoomTypeId}", id); }
                }

                roomType.MainImageUrl = null;
                roomType.MainImagePublicId = null;

                // Promote first additional image to main if exists
                if (!string.IsNullOrWhiteSpace(roomType.AdditionalImageUrls))
                {
                    try
                    {
                        var urls = JsonSerializer.Deserialize<List<string>>(roomType.AdditionalImageUrls);
                        if (urls is not null && urls.Count > 0)
                        {
                            roomType.MainImageUrl = urls[0];
                            roomType.MainImagePublicId = _cloudinaryService.ExtractPublicIdFromUrl(urls[0]);
                            urls.RemoveAt(0);
                            roomType.AdditionalImageUrls = urls.Count > 0 ? JsonSerializer.Serialize(urls) : null;
                        }
                    }
                    catch { _logger.LogWarning("Failed to parse additional image URLs for promotion on room type {RoomTypeId}", id); }
                }

                await repo.Update(roomType);
                return Ok(new { message = "Main image deleted." });
            }
            else
            {
                // Delete additional image by index (index is 0-based, but index=0 is main, so adjust)
                var additionalIndex = index - 1;
                if (string.IsNullOrWhiteSpace(roomType.AdditionalImageUrls))
                    return BadRequest(new { message = "No additional images." });

                List<string>? urls;
                try
                {
                    urls = JsonSerializer.Deserialize<List<string>>(roomType.AdditionalImageUrls);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Invalid additional images JSON for room type {RoomTypeId}", id);
                    return BadRequest(new { message = "Invalid additional images data." });
                }

                if (urls is null || additionalIndex >= urls.Count)
                    return BadRequest(new { message = "Invalid image index." });

                try
                {
                    var publicId = _cloudinaryService.ExtractPublicIdFromUrl(urls[additionalIndex]);
                    if (publicId is not null)
                        await _cloudinaryService.DeleteImageAsync(publicId, cancellationToken);
                }
                catch { _logger.LogWarning("Failed to delete additional image at index {Index} for room type {RoomTypeId}", additionalIndex, id); }

                urls.RemoveAt(additionalIndex);
                roomType.AdditionalImageUrls = urls.Count > 0 ? JsonSerializer.Serialize(urls) : null;
                await repo.Update(roomType);
                return Ok(new { message = "Image deleted.", additionalImageUrls = roomType.AdditionalImageUrls });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image for room type {RoomTypeId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }
}
