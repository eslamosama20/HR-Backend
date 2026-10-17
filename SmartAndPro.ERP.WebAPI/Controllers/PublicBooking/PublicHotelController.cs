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
public class PublicHotelController : ControllerBase
{
    private readonly PublicHotelService _service;
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ILogger<PublicHotelController> _logger;

    public PublicHotelController(
        ServiceBaseFactory serviceBaseFactory,
        RepositoriesFactory repositoriesFactory,
        ICloudinaryService cloudinaryService,
        ILogger<PublicHotelController> logger)
    {
        _service = serviceBaseFactory.GetService<PublicHotelService>();
        _repositoriesFactory = repositoriesFactory;
        _cloudinaryService = cloudinaryService;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PublicHotelFiltersDto filters, [FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _service.GetAllAsync(filters, pageNumber, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all hotels");
            return StatusCode(500, "An error occurred");
        }
    }

    [AllowAnonymous]
    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured()
    {
        try
        {
            var result = await _service.GetFeaturedHotelsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting featured hotels");
            return StatusCode(500, "An error occurred");
        }
    }

    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? searchTerm,
        [FromQuery] DateTime? checkIn,
        [FromQuery] DateTime? checkOut,
        [FromQuery] int? guests)
    {
        try
        {
            var result = await _service.SearchHotelsAsync(searchTerm, checkIn, checkOut, guests);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching hotels");
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
            _logger.LogError(ex, "Error getting hotel by ID {HotelId}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}/with-rooms")]
    public async Task<IActionResult> GetWithRooms(Guid id)
    {
        try
        {
            var result = await _service.GetHotelWithRoomsAsync(id);
            if (result.ErrorCode == SmartAndPro.ERP.SharedKernel.Enums.ErrorCodes.NotFound)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hotel with rooms {HotelId}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PublicHotelPostDto hotel)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _service.AddAsync(hotel);
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating hotel");
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PublicHotelPutDto hotel)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (id != hotel.Id) return BadRequest("ID mismatch");

        try
        {
            var result = await _service.UpdateAsync(hotel);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating hotel {HotelId}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            // Clean up Cloudinary images before soft-deleting
            var repo = _repositoriesFactory.GetRepository<PublicHotel>();
            var query = await repo.Filter(h => h.Id == id);
            var hotel = query.Data?.FirstOrDefault();
            if (hotel is not null)
            {
                if (!string.IsNullOrWhiteSpace(hotel.MainImagePublicId))
                {
                    try { await _cloudinaryService.DeleteImageAsync(hotel.MainImagePublicId, cancellationToken); }
                    catch { _logger.LogWarning("Failed to delete main image for hotel {HotelId}", id); }
                }

                if (!string.IsNullOrWhiteSpace(hotel.AdditionalImageUrls))
                {
                    try
                    {
                        var urls = JsonSerializer.Deserialize<List<string>>(hotel.AdditionalImageUrls);
                        if (urls is not null)
                        {
                            foreach (var url in urls)
                            {
                                var publicId = _cloudinaryService.ExtractPublicIdFromUrl(url);
                                if (publicId is not null)
                                    try { await _cloudinaryService.DeleteImageAsync(publicId, cancellationToken); }
                                    catch { _logger.LogWarning("Failed to delete additional image for hotel {HotelId}", id); }
                            }
                        }
                    }
                    catch { _logger.LogWarning("Failed to parse additional image URLs for hotel {HotelId}", id); }
                }
            }

            var result = await _service.DeleteAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting hotel {HotelId}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpPost("{publicHotelId:guid}/link-erp/{erpHotelId:guid}")]
    public async Task<IActionResult> LinkToErpHotel(Guid publicHotelId, Guid erpHotelId)
    {
        try
        {
            var result = await _service.LinkToErpHotelAsync(publicHotelId, erpHotelId);
            if (result.ErrorCode == SmartAndPro.ERP.SharedKernel.Enums.ErrorCodes.NotFound)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking hotel {PublicHotelId} to ERP {ErpHotelId}", publicHotelId, erpHotelId);
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpPost("{publicHotelId:guid}/unlink-erp")]
    public async Task<IActionResult> UnlinkFromErpHotel(Guid publicHotelId)
    {
        try
        {
            var result = await _service.UnlinkFromErpHotelAsync(publicHotelId);
            if (result.ErrorCode == SmartAndPro.ERP.SharedKernel.Enums.ErrorCodes.NotFound)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlinking hotel {PublicHotelId}", publicHotelId);
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpPost("{id:guid}/upload-main-image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadMainImage(Guid id, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });
        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(new { message = "File size exceeds the maximum allowed size of 10 MB." });

        var repo = _repositoriesFactory.GetRepository<PublicHotel>();
        var query = await repo.Filter(h => h.Id == id);
        var hotel = query.Data?.FirstOrDefault();
        if (hotel is null)
            return NotFound(new { message = "Hotel not found." });

        try
        {
            // Delete previous main image if exists
            if (!string.IsNullOrWhiteSpace(hotel.MainImagePublicId))
            {
                try { await _cloudinaryService.DeleteImageAsync(hotel.MainImagePublicId, cancellationToken); }
                catch { _logger.LogWarning("Failed to delete previous main image for hotel {HotelId}", id); }
            }

            var uploadResult = await _cloudinaryService.UploadImageAsync(file, "publicHotel/main", cancellationToken, transformation: new CloudinaryTransformation(Width: 1200, Quality: 85));
            hotel.MainImageUrl = uploadResult.Url;
            hotel.MainImagePublicId = uploadResult.PublicId;
            await repo.Update(hotel);

            return Ok(new { url = uploadResult.Url, thumbnailUrl = uploadResult.ThumbnailUrl, publicId = uploadResult.PublicId });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid image format for hotel {HotelId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/upload-additional-images")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAdditionalImages(Guid id, [FromForm] List<IFormFile> files, CancellationToken cancellationToken)
    {
        if (files is null || files.Count == 0)
            return BadRequest(new { message = "No files uploaded." });
        if (files.Count > 10)
            return BadRequest(new { message = "Maximum 10 files per upload." });
        if (files.Any(f => f.Length > 10 * 1024 * 1024))
            return BadRequest(new { message = "One or more files exceed the maximum allowed size of 10 MB." });

        var repo = _repositoriesFactory.GetRepository<PublicHotel>();
        var query = await repo.Filter(h => h.Id == id);
        var hotel = query.Data?.FirstOrDefault();
        if (hotel is null)
            return NotFound(new { message = "Hotel not found." });

        try
        {
            var urls = new List<string>();

            // Parse existing additional image URLs
            if (!string.IsNullOrWhiteSpace(hotel.AdditionalImageUrls))
            {
                try
                {
                    var existing = JsonSerializer.Deserialize<List<string>>(hotel.AdditionalImageUrls) ?? [];
                    urls.AddRange(existing);
                }
                catch { _logger.LogWarning("Failed to parse existing additional image URLs for hotel {HotelId}", id); }
            }

            foreach (var file in files)
            {
                if (file is null || file.Length == 0) continue;
                var uploadResult = await _cloudinaryService.UploadImageAsync(file, "publicHotel/additional", cancellationToken, transformation: new CloudinaryTransformation(Width: 1200, Quality: 85));
                urls.Add(uploadResult.Url);
            }

            hotel.AdditionalImageUrls = JsonSerializer.Serialize(urls);
            await repo.Update(hotel);

            return Ok(new { urls });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid image format for hotel additional images {HotelId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("upload-main-image-temp")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadMainImageTemp([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });
        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(new { message = "File size exceeds the maximum allowed size of 10 MB." });

        try
        {
            var uploadResult = await _cloudinaryService.UploadImageAsync(file, "publicHotel/main/temp", cancellationToken, transformation: new CloudinaryTransformation(Width: 1200, Quality: 85));
            return Ok(new { url = uploadResult.Url, thumbnailUrl = uploadResult.ThumbnailUrl, publicId = uploadResult.PublicId });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid image format for temp main image upload");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("upload-additional-images-temp")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAdditionalImagesTemp([FromForm] List<IFormFile> files, CancellationToken cancellationToken)
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
                var uploadResult = await _cloudinaryService.UploadImageAsync(file, "publicHotel/additional/temp", cancellationToken, transformation: new CloudinaryTransformation(Width: 1200, Quality: 85));
                results.Add(new { url = uploadResult.Url, thumbnailUrl = uploadResult.ThumbnailUrl, publicId = uploadResult.PublicId });
            }

            return Ok(new { urls = results });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid image format for temp additional images upload");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}/images/{imageType}")]
    public async Task<IActionResult> DeleteImage(Guid id, string imageType, CancellationToken cancellationToken)
    {
        var repo = _repositoriesFactory.GetRepository<PublicHotel>();
        var query = await repo.Filter(h => h.Id == id);
        var hotel = query.Data?.FirstOrDefault();
        if (hotel is null)
            return NotFound(new { message = "Hotel not found." });

        try
        {
            if (string.Equals(imageType, "main", StringComparison.OrdinalIgnoreCase))
            {
                // Delete main image
                if (!string.IsNullOrWhiteSpace(hotel.MainImagePublicId))
                {
                    try { await _cloudinaryService.DeleteImageAsync(hotel.MainImagePublicId, cancellationToken); }
                    catch { _logger.LogWarning("Failed to delete main image for hotel {HotelId}", id); }
                }

                hotel.MainImageUrl = null;
                hotel.MainImagePublicId = null;
                await repo.Update(hotel);
                return Ok(new { message = "Main image deleted." });
            }
            else if (int.TryParse(imageType, out var index) && index >= 0)
            {
                // Delete additional image by index
                if (string.IsNullOrWhiteSpace(hotel.AdditionalImageUrls))
                    return BadRequest(new { message = "No additional images." });

                List<string>? urls;
                try
                {
                    urls = JsonSerializer.Deserialize<List<string>>(hotel.AdditionalImageUrls);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Invalid additional images JSON for hotel {HotelId}", id);
                    return BadRequest(new { message = "Invalid additional images data." });
                }

                if (urls is null || index >= urls.Count)
                    return BadRequest(new { message = "Invalid image index." });

                try
                {
                    var publicId = _cloudinaryService.ExtractPublicIdFromUrl(urls[index]);
                    if (publicId is not null)
                        await _cloudinaryService.DeleteImageAsync(publicId, cancellationToken);
                }
                catch { _logger.LogWarning("Failed to delete additional image at index {Index} for hotel {HotelId}", index, id); }

                urls.RemoveAt(index);
                hotel.AdditionalImageUrls = urls.Count > 0 ? JsonSerializer.Serialize(urls) : null;
                await repo.Update(hotel);
                return Ok(new { message = "Image deleted.", additionalImageUrls = hotel.AdditionalImageUrls });
            }
            else
            {
                return BadRequest(new { message = "Invalid imageType. Use 'main' or an index number." });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image for hotel {HotelId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }
}
