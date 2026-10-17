using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Services.PublicBooking;
using SmartAndPro.ERP.Domains.Models.PublicBooking;
using SmartAndPro.ERP.SharedKernel.Enums;
using System.Security.Claims;

namespace SmartAndPro.ERP.WebAPI.Controllers.PublicBooking;

[ApiController]
[Route("api/[controller]")]
public class HotelReviewController : ControllerBase
{
    private readonly IHotelReviewService _reviewService;

    public HotelReviewController(IHotelReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// Submit a new hotel review (Public)
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateReview([FromBody] CreateHotelReviewDto dto)
    {
        var result = await _reviewService.AddAsync(dto);
        
        if (result.ErrorCode != ErrorCodes.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result);
    }

    /// <summary>
    /// Get reviews for a specific hotel (Public)
    /// </summary>
    [HttpGet("hotel/{hotelId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetHotelReviews(Guid hotelId)
    {
        var result = await _reviewService.GetByHotelAsync(hotelId, includeNotApproved: false);
        
        if (result.ErrorCode != ErrorCodes.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result);
    }

    /// <summary>
    /// Get hotel rating statistics (Public)
    /// </summary>
    [HttpGet("hotel/{hotelId}/stats")]
    [AllowAnonymous]
    public async Task<IActionResult> GetHotelRatingStats(Guid hotelId)
    {
        var result = await _reviewService.GetHotelRatingStatsAsync(hotelId);
        
        if (result.ErrorCode != ErrorCodes.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result);
    }

    /// <summary>
    /// Get all reviews including unapproved (Admin only)
    /// </summary>
    [HttpGet("hotel/{hotelId}/all")]
    [Authorize]
    public async Task<IActionResult> GetAllHotelReviews(Guid hotelId)
    {
        var result = await _reviewService.GetByHotelAsync(hotelId, includeNotApproved: true);
        
        if (result.ErrorCode != ErrorCodes.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result);
    }

    /// <summary>
    /// Get single review by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetReview(Guid id)
    {
        var result = await _reviewService.GetAsync(id);
        
        if (result.ErrorCode != ErrorCodes.Success)
            return NotFound(new { message = result.Message });

        return Ok(result);
    }

    /// <summary>
    /// Add admin response to a review (Admin only)
    /// </summary>
    [HttpPost("response")]
    [Authorize]
    public async Task<IActionResult> AddResponse([FromBody] UpdateReviewResponseDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid user ID" });

        var result = await _reviewService.UpdateResponseAsync(dto, userId);
        
        if (result.ErrorCode != ErrorCodes.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result);
    }

    /// <summary>
    /// Approve or reject a review (Admin only)
    /// </summary>
    [HttpPost("approve")]
    [Authorize]
    public async Task<IActionResult> ApproveReview([FromBody] ApproveReviewDto dto)
    {
        var result = await _reviewService.ApproveReviewAsync(dto);
        
        if (result.ErrorCode != ErrorCodes.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result);
    }

    /// <summary>
    /// Delete a review (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteReview(Guid id)
    {
        var result = await _reviewService.DeleteAsync(id);
        
        if (result.ErrorCode != ErrorCodes.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result);
    }
}
