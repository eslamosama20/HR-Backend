using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Services.HotelAllotmentInvoices;

namespace SmartAndPro.ERP.WebAPI.Controllers.AllotmentUsages
{
    [Route("api/[controller]")]
    [ApiController]
      public class AllotmentUsageController : ControllerBase
    {
        private readonly AllotmentUsageService _allotmentUsageService;

        public AllotmentUsageController(AllotmentUsageService allotmentUsageService)
        {
            _allotmentUsageService = allotmentUsageService;
        }


        // GET: api/HotelAllotment/Availability/{hotelId}?from=2025-12-23&to=2025-12-26
        [HttpGet("Availability/{hotelId}")]
        public async Task<IActionResult> GetHotelAvailability(
            Guid hotelId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            if (from > to)
                return BadRequest("From date cannot be after To date.");

            var availability = await _allotmentUsageService.GetHotelAvailability(hotelId, from, to);

            return Ok(availability);
        }

        // GET: api/HotelAllotment/Rooms/{hotelId}
        [HttpGet("Rooms/{hotelId}")]
        public async Task<IActionResult> GetAllHotelRooms(Guid hotelId)
        {
            var rooms = await _allotmentUsageService.GetAllHotelRooms(hotelId);
            return Ok(rooms);
        }
        //// GET: api/AllotmentUsage/daily-availability?roomId=1&from=2025-12-25&to=2025-12-30
        //[HttpGet("daily-availability")]
        //public async Task<IActionResult> GetDailyAvailability(
        //    [FromQuery] int roomId,
        //    [FromQuery] DateTime from,
        //    [FromQuery] DateTime to)
        //{
        //    try
        //    {
        //        var result = await _allotmentUsageService.GetDailyAvailability(roomId, from, to);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}

        //// GET: api/AllotmentUsage/check-booking?roomId=1&from=2025-12-25&to=2025-12-30&requestedRooms=2
        //[HttpGet("check-booking")]
        //public async Task<IActionResult> CheckBookingRequest(
        //    [FromQuery] int roomId,
        //    [FromQuery] DateTime from,
        //    [FromQuery] DateTime to,
        //    [FromQuery] int requestedRooms)
        //{
        //    try
        //    {
        //        var result = await _allotmentUsageService.CheckBookingRequest(roomId, from, to, requestedRooms);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}
    }
}
