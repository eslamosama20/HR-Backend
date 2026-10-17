using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.PublicBooking;
using SmartAndPro.ERP.Application.Services;
using SmartAndPro.ERP.SharedKernel.Enums;

namespace SmartAndPro.ERP.WebAPI.Controllers.PublicBooking;

[Route("api/[controller]")]
[ApiController]
public class PublicBookingController : ControllerBase
{
    private readonly PublicBookingService _service;
    private readonly PaymobService _paymobService;

    public PublicBookingController(ServiceBaseFactory serviceBaseFactory, PaymobService paymobService)
    {
        _service = serviceBaseFactory.GetService<PublicBookingService>();
        _paymobService = paymobService;
    }

    /// <summary>
    /// Get all bookings with filters (Admin)
    /// </summary>
    [HttpGet]
    [Authorize]
    [PermissionAuthorize("SmartBooking.Read")]
    public async Task<IActionResult> GetAll([FromQuery] PublicBookingFiltersDto filters, [FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _service.GetAllAsync(filters, pageNumber, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Get booking by ID
    /// </summary>
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
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Get booking by reference number
    /// </summary>
    [HttpGet("reference/{reference}")]
    public async Task<IActionResult> GetByReference(string reference)
    {
        try
        {
            var filters = new PublicBookingFiltersDto { BookingReference = reference };
            var result = await _service.GetAllAsync(filters);
            
            if (result.Data == null || !result.Data.Any())
            {
                return NotFound(new { message = "Booking not found" });
            }

            return Ok(result.Data.First());
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Create new booking (Public)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PublicBookingPostDto booking)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _service.AddAsync(booking);
            
            if (result.ErrorCode != SmartAndPro.ERP.SharedKernel.Enums.ErrorCodes.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Update booking (Admin)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] PublicBookingPutDto booking)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (id != booking.Id) return BadRequest("ID mismatch");

        try
        {
            var result = await _service.UpdateAsync(booking);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Confirm booking (Admin)
    /// </summary>
    [HttpPost("{id:guid}/confirm")]
    [Authorize]
    public async Task<IActionResult> Confirm(Guid id)
    {
        try
        {
            var result = await _service.ConfirmBookingAsync(id);
            
            if (result.ErrorCode == SmartAndPro.ERP.SharedKernel.Enums.ErrorCodes.NotFound)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Cancel booking
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelBookingRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _service.CancelBookingAsync(id, request.Reason);
            
            if (result.ErrorCode == SmartAndPro.ERP.SharedKernel.Enums.ErrorCodes.NotFound)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Update payment status (Payment Gateway Callback)
    /// </summary>
    [HttpPost("{id:guid}/payment")]
    public async Task<IActionResult> UpdatePayment(
        Guid id,
        [FromBody] UpdatePaymentRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _service.UpdatePaymentStatusAsync(
                id,
                request.PaymentStatus,
                request.TransactionId,
                request.PaymobOrderId);
            
            if (result.ErrorCode == SmartAndPro.ERP.SharedKernel.Enums.ErrorCodes.NotFound)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Cancel a booking with optional refund
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelBooking(Guid id, [FromBody] CancelBookingRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _service.CancelBookingAsync(id, request.Reason, request.ProcessRefund);

            if (result.ErrorCode == SmartAndPro.ERP.SharedKernel.Enums.ErrorCodes.NotFound)
            {
                return NotFound(result);
            }

            if (result.ErrorCode != SmartAndPro.ERP.SharedKernel.Enums.ErrorCodes.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Delete booking (Admin)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Initiate Paymob payment
    /// </summary>
    [HttpPost("initiate-payment")]
    public async Task<IActionResult> InitiatePaymobPayment([FromBody] InitiatePaymentRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var billingData = new PaymobBillingData
            {
                FirstName = request.FirstName ?? "Guest",
                LastName = request.LastName ?? "",
                Email = request.Email ?? "guest@example.com",
                PhoneNumber = request.PhoneNumber ?? "01000000000",
                Apartment = "NA",
                Floor = "NA",
                Street = "NA",
                Building = "NA",
                ShippingMethod = "NA",
                PostalCode = "00000",
                City = request.City ?? "Cairo",
                Country = request.Country ?? "EG",
                State = "NA"
            };

            var result = await _paymobService.InitiatePaymentAsync(
                request.Amount,
                request.MerchantOrderId,
                billingData
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message, error = ex.ToString() });
        }
    }

    /// <summary>
    /// TEMPORARY: Reset room availability (for testing only)
    /// </summary>
    [HttpPost("reset-room-availability/{roomTypeId:guid}")]
    public async Task<IActionResult> ResetRoomAvailability(Guid roomTypeId, [FromQuery] int quantity = 20)
    {
        try
        {
            await _service.ResetRoomAvailabilityAsync(roomTypeId, quantity);
            return Ok(new { message = $"Room availability reset to {quantity}" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}

// Request models
public class InitiatePaymentRequest
{
    public decimal Amount { get; set; }
    public string MerchantOrderId { get; set; } = "";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
}

public class CancelBookingRequest
{
    public string Reason { get; set; } = "";
    public bool ProcessRefund { get; set; } = true;
}

public class UpdatePaymentRequest
{
    public PaymentStatus PaymentStatus { get; set; }
    public string? TransactionId { get; set; }
    public int? PaymobOrderId { get; set; }
}
