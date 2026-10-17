using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationService;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Services.@base;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Services.Reservation;
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

namespace SmartAndPro.ERP.WebAPI.Controllers.Reservation;

[Route("api/[controller]/")]
[ApiController]
[Authorize]
[PermissionAuthorize("ReservationService.Read")]
public class ReservationServiceController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly ReservationServiceService _service;
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly ICloudinaryService _cloudinaryService;

    public ReservationServiceController(
        ServiceBaseFactory serviceBaseFactory,
        RepositoriesFactory repositoriesFactory,
        ICloudinaryService cloudinaryService)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<ReservationServiceService>();
        _repositoriesFactory = repositoriesFactory;
        _cloudinaryService = cloudinaryService;
    }


    // ReservationServices get all

    [HttpGet("")]
    public async Task<IActionResult> GetReservationServices([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? fields = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllAsync(new ReservationServiceFiltersDto(), pageNumber, pageSize);
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

    // ReservationServices get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetReservationServicesById(Guid id)
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

    // ReservationServices create
    [HttpPost("")]
    [PermissionAuthorize("ReservationService.Create")]
    public async Task<IActionResult> CreateReservationServices([FromBody] ReservationServicePostDto reservationServices)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(reservationServices);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // ReservationServices update
    [HttpPut]
    [PermissionAuthorize("ReservationService.Update")]
    public async Task<IActionResult> UpdateReservationServices([FromBody] ReservationServicePutDto reservationServices)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(reservationServices);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // ReservationServices delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize("ReservationService.Delete")]
    public async Task<IActionResult> DeleteReservationServices(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<ReservationServiceService>();
            var responseDate = await service.DeleteAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    // Get all frontal reservation services
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

    // Make reservation service frontal
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

    [HttpPost("/api/reservationService/upload-logo")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadLogo([FromForm] Guid reservationServiceId, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        var repo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Reservation.ReservationService>();
        var serviceQuery = await repo.Filter(x => x.Id == reservationServiceId);
        var reservationService = serviceQuery.Data?.FirstOrDefault();
        if (reservationService is null)
            return NotFound(new { message = "Reservation service not found." });

        try
        {
            var uploadResult = await _cloudinaryService.UploadImageAsync(file, "reservationService/logo", cancellationToken);
            reservationService.Logo = null;
            reservationService.LogoUrl = uploadResult.Url;
            reservationService.LogoPublicId = uploadResult.PublicId;
            await repo.Update(reservationService);

            return Ok(new { url = reservationService.LogoUrl, publicId = reservationService.LogoPublicId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("/api/reservationService/upload-logo-temp")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadLogoTemp([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        try
        {
            var uploadResult = await _cloudinaryService.UploadImageAsync(file, "reservationService/logo", cancellationToken);
            return Ok(new { url = uploadResult.Url, publicId = uploadResult.PublicId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

}
