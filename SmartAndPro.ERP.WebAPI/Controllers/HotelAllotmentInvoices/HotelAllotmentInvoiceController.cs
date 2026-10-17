using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.HotelAllotmentInvoices;
using SmartAndPro.ERP.Application.Services.Accounting;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.HotelAllotmentInvoices;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using SmartAndPro.ERP.Domains.Entities.HotelAllotmentInvoices;
using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Controllers.Invoice;
using SmartAndPro.ERP.WebAPI.Utils;
using System.IO.Compression;
using System.Security.Claims;

namespace SmartAndPro.ERP.WebAPI.Controllers.HotelAllotmentInvoiceController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [PermissionAuthorize("ReservationPurchases.Read")]
    public class HotelAllotmentInvoiceController : ControllerBase
    {
        private readonly ILogger<HotelAllotmentInvoiceController> _logger;

        private readonly ServiceBaseFactory _serviceBaseFactory;
        private readonly HotelAllotmentInvoiceService _service;
        private readonly RepositoriesFactory _repositoriesFactory;
        public HotelAllotmentInvoiceController(
            ServiceBaseFactory serviceBaseFactory,
            RepositoriesFactory repositoriesFactory,
            ILogger<HotelAllotmentInvoiceController> logger)

        {
            _serviceBaseFactory = serviceBaseFactory;
            _repositoriesFactory = repositoriesFactory;
            _service = _serviceBaseFactory.GetService<HotelAllotmentInvoiceService>();
            _logger = logger;

        }

            [HttpPost]
            [PermissionAuthorize("ReservationPurchases.Create")]
            public async Task<IActionResult> CreateAllotmentInvoice([FromBody] HotelAllotmentInvoicePostDto dto)
            {

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                try
                {
                    var userId = GetCurrentUserId();
                    if (userId == Guid.Empty)
                    {
                        return Unauthorized("Invalid user ID");
                    }
                    var response = await _service.AddAsync(dto ,userId);

                    return response.HandleResult();
                }
                catch (Exception ex)
                {
                    LogService.LogError(ex);
                    return StatusCode(500, "An unexpected error occurred");
                }
            }
        [HttpGet]
        public async Task<IActionResult> GetAllInvoices([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = await _service.GetAllAsync(pageNumber, pageSize);

                return response.HandleResult();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, new { message = ex.Message });
            }
        }


        [HttpGet("{invoiceId}")]
        public async Task<IActionResult> GetInvoiceById(int invoiceId)
        {
            var invoiceDto = await _service.GetByIdAsync(invoiceId);

            if (invoiceDto == null)
                return NotFound();

            return Ok(invoiceDto);
        }


        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> CancelHotelAllotmentInvoice(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            IActionResult result = NoContent();
            try
            {
                var service = _serviceBaseFactory.GetService<HotelAllotmentInvoiceService>();
                var responseData = await service.CancelAsync(id);
                result = responseData.HandleResult();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
            return result;
        }

        [HttpPut]
        [PermissionAuthorize("ReservationPurchases.Update")]
        public async Task<IActionResult> UpdateAllotmentInvoices([FromBody] HotelAllotmentInvoicePutDto hotelAllotmentInvoicePutDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            IActionResult result = NoContent();
            try
            {
                var responseDate = await _service.UpdateAsync(hotelAllotmentInvoicePutDto);
                result = responseDate.HandleResult();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
            return result;
        }


        [HttpPost("CreateTransaction")]
        public async Task<IActionResult> CreateTransaction(int id)
        {
            try
            {
                var result = await _service.CreateTransactionAsync(id);
                if (result != Guid.Empty)
                    return Ok(new { transactionId = result.ToString(), message = "Transaction created successfully" });
                else
                    return BadRequest(new { message = "Failed to create transaction" });
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, "An unexpected error occurred");
            }
        }

  
        [PermissionAuthorize("ReservationPurchases.Read")]
        [HttpGet("document/{invoiceId}")]
        public async Task<IActionResult> GetDocument(
            int invoiceId,
            [FromQuery] InvoicePrintType printType = InvoicePrintType.A4Portrait)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var sellerName = await GetSellerNameAsync();

                var document = await _service.GetDocument(invoiceId, printType, sellerName);
                if (document == null) return NotFound();
                if (document.Length == 0) return StatusCode(500, "System configuration error");

                return File(document, "application/pdf", $"reservation-invoice-{invoiceId}.pdf");
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, ex.Message);
            }
        }

        [PermissionAuthorize("ReservationPurchases.Read")]
        [HttpGet("documents")]
        public async Task<IActionResult> GetDocuments([FromQuery] int[] ids)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (ids == null || ids.Length == 0)
                return BadRequest("No invoice ids supplied");
            try
            {
                var sellerName = await GetSellerNameAsync();
                var validIds = await _service.GetExistingInvoiceIds(ids);
                if (!validIds.Any()) return NoContent();
                var documents = await _service.GetDocuments(validIds, sellerName: sellerName);
                if (documents.Length == 0) return NoContent();
                using var zipStream = new MemoryStream();
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    for (int i = 0; i < documents.Length; i++)
                    {
                        var entry = archive.CreateEntry($"reservation-invoice-{validIds[i]}.pdf");
                        using var entryStream = entry.Open();
                        entryStream.Write(documents[i], 0, documents[i].Length);
                    }
                }
                zipStream.Position = 0;
                return File(zipStream.ToArray(), "application/zip", "reservation-invoices.zip");
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, ex.Message);
            }
        }

        [PermissionAuthorize("ReservationPurchases.Read")]
        [HttpPost("documents/merged")]
        public async Task<IActionResult> GetMerged([FromBody] MergeRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                if (request == null || request.Ids == null || request.Ids.Length == 0)
                    return BadRequest("No invoice ids supplied");
                var sellerName = await GetSellerNameAsync();
                var merged = await _service.GetMergedDocument(request.Ids, sellerName: sellerName);
                if (merged == null || merged.Length == 0) return NoContent();
                return File(merged, "application/pdf", "reservation-invoices-merged.pdf");
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, ex.Message);
            }
        }

        [PermissionAuthorize("ReservationPurchases.Read")]
        [HttpGet("documents/all")]
        public async Task<IActionResult> GetAllDocuments()
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var sellerName = await GetSellerNameAsync();
                var allIds = await _service.GetAllInvoiceIds();
                if (!allIds.Any()) return NoContent();
                var documents = await _service.GetDocuments(allIds, sellerName: sellerName);
                using var zipStream = new MemoryStream();
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    for (int i = 0; i < documents.Length; i++)
                    {
                        var entry = archive.CreateEntry($"reservation-invoice-{allIds[i]}.pdf");
                        using var entryStream = entry.Open();
                        entryStream.Write(documents[i], 0, documents[i].Length);
                    }
                }
                zipStream.Position = 0;
                return File(zipStream.ToArray(), "application/zip", "all-reservation-invoices.zip");
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, ex.Message);
            }
        }

        [PermissionAuthorize("ReservationPurchases.Read")]
        [HttpGet("documents/merged/all")]
        public async Task<IActionResult> GetMergedAll()
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var sellerName = await GetSellerNameAsync();
                var allInvoiceIds = await _service.GetAllInvoiceIds();

                if (!allInvoiceIds.Any())
                    return NoContent();

                var merged = await _service.GetMergedDocument(allInvoiceIds, sellerName: sellerName);

                if (merged == null || merged.Length == 0)
                    return NoContent();

                return File(merged, "application/pdf", "reservation-invoices-merged-all.pdf");
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, ex.Message);
            }
        }

        private async Task<string?> GetSellerNameAsync()
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.Name);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                return null;

            var userRepository = _repositoriesFactory.GetRepository<User>();
            var userQuery = await userRepository.Filter(u => u.Id == userId);
            var user = await userQuery.Data?.FirstOrDefaultAsync();
            if (user == null) return null;

            var firstName = user.FirstName ?? "";
            var lastName = user.LastName ?? "";
            var sellerName = $"{firstName} {lastName}".Trim();
            return string.IsNullOrWhiteSpace(sellerName) ? user.UserName : sellerName;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.Name)?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }

        public class MergeRequest
        {
            public int[] Ids { get; set; } = Array.Empty<int>();
        }
    }
}
