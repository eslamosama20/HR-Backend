using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.Extensions.Configuration;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Common.Dtos.BulkDocumentShare;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.Reservation;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.Invoice;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.Reservation;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.SharedKernel.Dtos.DocumentShare;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;
using SmartAndPro.ERP.WebAPI.Attributes;
using System.IO.Compression;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Reservation;

[Route("api/[controller]/")]
[ApiController]
[Authorize]
[PermissionAuthorize("Reservation.Read")]
public class ReservationController : Controller
{
    private const string OptionalCostsHeader = "X-Allow-Optional-Costs";

    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly IDocumentShareService _documentShareService;
    private readonly IConfiguration _configuration;
    private readonly ReservationService _service;
    private readonly SmartAndPro.ERP.Infrastructure.Repositories.RepositoriesFactory _repositoriesFactory;
    private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;

    public ReservationController(ServiceBaseFactory serviceBaseFactory,
        IDocumentShareService documentShareService,
        IConfiguration configuration,
        SmartAndPro.ERP.Infrastructure.Repositories.RepositoriesFactory repositoriesFactory,
        Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _documentShareService = documentShareService;
        _configuration = configuration;
        _repositoriesFactory = repositoriesFactory;
        _env = env;
        _service = _serviceBaseFactory.GetService<ReservationService>();
    }

    private bool IsOptionalCostsAllowed()
    {
        if (Request.Headers.TryGetValue(OptionalCostsHeader, out var headerValue) &&
            bool.TryParse(headerValue, out var headerFlag) &&
            headerFlag)
        {
            return true;
        }

        var configValue = _configuration["AppSettings:Reservation:AllowOptionalCosts"];
        return bool.TryParse(configValue, out var configFlag) && configFlag;
    }

    private string? CollectOptionalFieldsViolations(ReservationPostDto dto)
    {
        if (IsOptionalCostsAllowed()) return null;

        var violations = new List<string>();
        if (dto.ReservationRoomEntries != null)
        {
            foreach (var entry in dto.ReservationRoomEntries)
            {
                if (entry.RoomCost == null)
                {
                    violations.Add($"ReservationRoomEntries.RoomCost is required unless '{OptionalCostsHeader}' is enabled.");
                }
            }
        }

        if (dto.ReservationServiceEntries != null)
        {
            foreach (var entry in dto.ReservationServiceEntries)
            {
                if (entry.CostPrice == null)
                {
                    violations.Add($"ReservationServiceEntries.CostPrice is required unless '{OptionalCostsHeader}' is enabled.");
                }
                if (entry.ReservationSupplierId == null)
                {
                    violations.Add($"ReservationServiceEntries.ReservationSupplierId is required unless '{OptionalCostsHeader}' is enabled.");
                }
            }
        }

        return violations.Count == 0 ? null : string.Join(" | ", violations);
    }

    private string? CollectOptionalFieldsViolations(ReservationPutDto dto)
    {
        if (IsOptionalCostsAllowed()) return null;

        var violations = new List<string>();
        if (dto.ReservationRoomEntries != null)
        {
            foreach (var entry in dto.ReservationRoomEntries)
            {
                if (entry.RoomCost == null)
                {
                    violations.Add($"ReservationRoomEntries[{entry.Id}].RoomCost is required unless '{OptionalCostsHeader}' is enabled.");
                }
            }
        }

        if (dto.ReservationServiceEntries != null)
        {
            foreach (var entry in dto.ReservationServiceEntries)
            {
                if (entry.CostPrice == null)
                {
                    violations.Add($"ReservationServiceEntries[{entry.Id}].CostPrice is required unless '{OptionalCostsHeader}' is enabled.");
                }
                if (entry.ReservationSupplierId == null)
                {
                    violations.Add($"ReservationServiceEntries[{entry.Id}].ReservationSupplierId is required unless '{OptionalCostsHeader}' is enabled.");
                }
            }
        }

        return violations.Count == 0 ? null : string.Join(" | ", violations);
    }

    [HttpGet("document/{reservationId:guid}")]
    public async Task<IActionResult> GetDocument(Guid reservationId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var document = await _service.GetDocument(reservationId);
            if (document != null)
            {
                var refNum = await _service.GetReferenceNumberAsync(reservationId);
                result = File(document, "application/pdf", $"{refNum}.pdf");
            }
        }
        catch (Exception ex)
        {
            result = StatusCode(500, ex.ToString());
        }
        return result;
    }

    // get multiple documents using reservationId array from query string
    [HttpGet("documents")]
    public async Task<IActionResult> GetDocuments([FromQuery] string[] ids)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var reservationGuids = ids.Select(Guid.Parse).ToArray();
            var documents = await _service.GetDocuments(reservationGuids); // List<byte[]>
            var refNums = await _service.GetReferenceNumbersAsync(reservationGuids);

            using var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                for (int i = 0; i < documents.Length; i++)
                {
                    var fileName = refNums.TryGetValue(reservationGuids[i], out var refNum) ? refNum : ids[i];
                    var entry = archive.CreateEntry($"{fileName}.pdf");
                    using var entryStream = entry.Open();
                    entryStream.Write(documents[i], 0, documents[i].Length);
                }
            }

            zipStream.Position = 0;
            return File(zipStream.ToArray(), "application/zip", "documents.zip");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }

    [HttpGet("costsheet/{reservationId:guid}")]
    public async Task<IActionResult> GetCostSheet(Guid reservationId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var document = await _service.GetCostSheet(reservationId);
            if (document != null)
            {
                var refNum = await _service.GetReferenceNumberAsync(reservationId);
                result = File(document, "application/pdf", $"Cost Sheet ({refNum}).pdf");
            }
        }
        catch (Exception ex)
        {
            result = StatusCode(500, ex.ToString());
        }
        return result;
    }

    [HttpGet("costsheets")]
    public async Task<IActionResult> GetCostsheets([FromQuery] string[] ids)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var reservationGuids = ids.Select(Guid.Parse).ToArray();
            var documents = await _service.GetCostSheets(reservationGuids); // List<byte[]>
            var refNums = await _service.GetReferenceNumbersAsync(reservationGuids);

            using var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                for (int i = 0; i < documents.Length; i++)
                {
                    var fileName = refNums.TryGetValue(reservationGuids[i], out var refNum) ? refNum : ids[i];
                    var entry = archive.CreateEntry($"Cost Sheet ({fileName}).pdf");
                    using var entryStream = entry.Open();
                    entryStream.Write(documents[i], 0, documents[i].Length);
                }
            }

            zipStream.Position = 0;
            return File(zipStream.ToArray(), "application/zip", "documents.zip");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }

    [HttpGet("hotelrequest/{reservationId:guid}")]
    public async Task<IActionResult> GetHotelRequest(Guid reservationId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var document = await _service.GetHotelRequest(reservationId);
            if (document != null)
            {
                var refNum = await _service.GetReferenceNumberAsync(reservationId);
                result = File(document, "application/pdf", $"Hotel Request ({refNum}).pdf");
            }
        }
        catch (Exception ex)
        {
            result = StatusCode(500, ex.ToString());
        }
        return result;
    }

    [HttpGet("hotelrequests")]
    public async Task<IActionResult> GetHotelRequests([FromQuery] string[] ids)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var reservationGuids = ids.Select(Guid.Parse).ToArray();
            var documents = await _service.GetHotelRequests(reservationGuids); // List<byte[]>
            var refNums = await _service.GetReferenceNumbersAsync(reservationGuids);

            using var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                for (int i = 0; i < documents.Length; i++)
                {
                    var fileName = refNums.TryGetValue(reservationGuids[i], out var refNum) ? refNum : ids[i];
                    var entry = archive.CreateEntry($"Hotel Request ({fileName}).pdf");
                    using var entryStream = entry.Open();
                    entryStream.Write(documents[i], 0, documents[i].Length);
                }
            }

            zipStream.Position = 0;
            return File(zipStream.ToArray(), "application/zip", "documents.zip");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }


    // Reservations get all

    /// <summary>
    /// Deprecated endpoint. Use <c>GET /api/Reservation/lookup</c> for paginated lightweight lookup payloads.
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> GetReservations
        ([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, 
        [FromQuery] string? searchField = null, [FromQuery] string? searchValue = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllPaginatedAsync
                (new ReservationFiltersDto(), pageNumber, pageSize, searchField, searchValue);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    [HttpGet("lookup")]
    [OutputCache(Duration = 900, VaryByQueryKeys = new[] { "pageNumber", "pageSize", "sortField" })]
    public async Task<IActionResult> GetReservationLookup([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, [FromQuery] string? sortField = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseData = await _service.GetLookupAsync(new ReservationFiltersDto(), pageNumber, pageSize, sortField);
            result = responseData.HandleResult(isPaginated: true);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // Reservations get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetReservationsById(Guid id)
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

    // جلب عدة حجوزات بـ IDs دفعة واحدة - يستخدم في قائمة فواتير الحجوزات
    [HttpGet("by-ids")]
    public async Task<IActionResult> GetReservationsByIds([FromQuery] string[] ids)
    {
        if (ids == null || ids.Length == 0)
            return Ok(new List<object>());

        try
        {
            var guids = ids.Select(id => Guid.Parse(id)).ToList();
            var result = await _service.GetByIdsAsync(guids);
            return Ok(result);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, ex.Message);
        }
    }


    [HttpPost("")]
    [PermissionAuthorize("Reservation.Create")]
    public async Task<IActionResult> CreateReservations([FromBody] ReservationPostDto reservations)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var optionalFieldsViolation = CollectOptionalFieldsViolations(reservations);
        if (optionalFieldsViolation != null)
        {
            return BadRequest(new
            {
                error = "OptionalFieldsNotEnabled",
                message = optionalFieldsViolation
            });
        }

        try
        {
            var response = await _service.AddAsync(reservations);

            if (response.ErrorCode == ErrorCodes.Overbooking)
            {
                // نرجع الـ overbooked days للفرونت
                return Ok(new
                {
                    status = -11,
                    message = "Overbooking detected. Do you want to force booking?",
                    overbookedDays = response.Data.OverbookedDays
                });
            }

            // العملية نجحت
            return Ok(response);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, "Something went wrong");
        }
    }
    #region CreateTransaction
    //[HttpPost("create-transaction/{reservationId:guid}")]
    //public async Task<IActionResult> CreateTransaction(Guid reservationId)
    //{
    //    if (!ModelState.IsValid) return BadRequest(ModelState);
    //    IActionResult result = NoContent();
    //    try
    //    {
    //        var responseDate = await _service.CreateTransactionForReservationAsync(reservationId);
    //        result = responseDate.HandleResult();
    //    }
    //    catch (Exception ex)
    //    {
    //        LogService.LogError(ex);
    //    }
    //    return result;
    //}
    #endregion

    #region SendToZatca
    //[HttpPost("send-to-zatca/{reservationId:guid}")]
    //public async Task<IActionResult> SendToZatca(Guid reservationId)
    //{
    //    if (!ModelState.IsValid) return BadRequest(ModelState);
    //    IActionResult result = BadRequest();
    //    try
    //    {
    //        await _service.SendReservationToZatca(reservationId);
    //        result = Ok();
    //    }
    //    catch (Exception ex)
    //    {
    //        LogService.LogError(ex);
    //    }
    //    return result;
    //}
    #endregion

    // Reservations update
    [HttpPut]
    [PermissionAuthorize("Reservation.Update")]
    public async Task<IActionResult> UpdateReservations([FromBody] ReservationPutDto reservations)
    {
        Console.WriteLine($"[DEBUG] PUT UpdateReservations endpoint hit for reservation ID: {reservations?.Id}");
        if (!ModelState.IsValid)
        {
            Console.WriteLine("[DEBUG] ModelState is INVALID!");
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    Console.WriteLine($"[DEBUG] ModelError in {state.Key}: {error.ErrorMessage}");
                }
            }
            return BadRequest(ModelState);
        }

        var optionalFieldsViolation = CollectOptionalFieldsViolations(reservations);
        if (optionalFieldsViolation != null)
        {
            Console.WriteLine($"[DEBUG] OptionalFieldsViolation: {optionalFieldsViolation}");
            return BadRequest(new
            {
                error = "OptionalFieldsNotEnabled",
                message = optionalFieldsViolation
            });
        }

        IActionResult result = NoContent();
        try
        {
            Console.WriteLine("[DEBUG] Calling _service.UpdateAsync...");
            var responseDate = await _service.UpdateAsync(reservations);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    [HttpDelete("{id:guid}")]
    [PermissionAuthorize("Reservation.Delete")]
    public async Task<IActionResult> DeleteReservations(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<ReservationService>();
            var responseDate = await service.DeleteAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelReservation(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<ReservationService>();
            var responseData = await service.CancelAsync(id);
            result = responseData.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    [HttpPost("{id:guid}/reactivate")]
    public async Task<IActionResult> ReactivateReservation(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<ReservationService>();
            var responseData = await service.ReactivateAsync(id);
            result = responseData.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> ConfirmReservation(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseData = await _service.ConfirmAsync(id);
            result = responseData.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    [HttpGet("{id:guid}/logs")]
    public async Task<IActionResult> GetReservationLogs(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var logs = await _service.GetReservationLogsAsync(id);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, ex.Message);
        }
    }


    [HttpPost("print-template")]
    public async Task<IActionResult> PrintTemplate([FromBody] ReservationPrintTemplateCommand command)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var document = await _service.GenerateTemplateDocument(command);
            if (document != null)
            {
                result = File(document, "application/pdf", $"reservations-template-{command.TemplateId}.pdf");
            }
        }
        catch (Exception ex)
        {
            result = StatusCode(500, ex.Message);
        }
        return result;
    }

    // Get all frontal reservations

    [HttpGet("frontal")]
    public async Task<IActionResult> GetAllFrontalReservations([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseData = await _service.GetAllFrontalAsync(new ReservationFiltersDto(), pageNumber, pageSize);
            result = responseData.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // Make reservation frontal
    [HttpPost("frontal/{reservationId:guid}")]
    public async Task<IActionResult> MakeFrontalAsync(Guid reservationId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseData = await _service.MakeFrontalAsync(reservationId);
            result = responseData.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    [HttpPost("download-template")]
    public async Task<IActionResult> DownloadTemplate([FromBody] ReservationPrintTemplateCommand command)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var document = await _service.GenerateTemplateDocument(command);
            if (document != null)
            {
                result = File(document, "application/pdf", $"reservations-template-{command.TemplateId}.pdf");
            }
        }
        catch (Exception ex)
        {
            result = StatusCode(500, ex.Message);
        }
        return result;
    }


    [HttpPost("{reservationId:guid}/share")]
    public async Task<IActionResult> ShareReservation(Guid reservationId, [FromBody] DocumentSharePostDto documentShareDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var document = await _service.GetDocument(reservationId);
            if (document == null)
                return NotFound("Reservation document not found.");

            var referenceNo = await _service.GetReferenceNumberAsync(reservationId);
            string emailSubject = $"تأكيد حجز {referenceNo}";
            string attachmentName = $"{referenceNo}.pdf";
            string emailBody = BuildReservationEmailBody(referenceNo);

            string? senderDisplayName = null;
            try
            {
                var companyRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement.CompanyDetails>();
                var compRes = await companyRepo.GetAll();
                var comp = compRes?.Data?.FirstOrDefault();
                if (comp != null)
                {
                    senderDisplayName = !string.IsNullOrWhiteSpace(comp.EnglishName)
                        ? comp.EnglishName
                        : (!string.IsNullOrWhiteSpace(comp.ArabicName) ? comp.ArabicName : null);
                }
            }
            catch { }

            await _documentShareService.ShareDocumentAsync(
                document, 
                documentShareDto.Method, 
                documentShareDto.Receiver,
                emailSubject,
                emailBody,
                attachmentName,
                senderDisplayName
            );

            var response = new DocumentShareResponseDto<Guid>
            {
                Id = reservationId,
                Method = documentShareDto.Method,
                Receiver = documentShareDto.Receiver,
                Message = "Reservation shared successfully"
            };
            return Ok(response);

        }

        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, "An unexpected error occurred while sharing the reservation.");
        }
    }

    [HttpPost("reservations/share")]
    public async Task<IActionResult> ShareReservations([FromBody] BulkDocumentSharePostDto<Guid> bulkDocumentSharePostDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState); 

        var responses = new List<DocumentShareResponseDto<Guid>>();

        foreach (var reservationId in bulkDocumentSharePostDto.Ids)
        {
            try
            {
                var document = await _service.GetDocument(reservationId);
                if (document == null)
                {
                    responses.Add(new DocumentShareResponseDto<Guid>
                    {
                        Id = reservationId,
                        Method = bulkDocumentSharePostDto.Method,
                        Receiver = bulkDocumentSharePostDto.Receiver,
                        Message = "Reservation document not found",
                        IsSuccess = false

                    });
                }

                else
                {

                    var referenceNo = await _service.GetReferenceNumberAsync(reservationId);
                    string emailSubject = $"تأكيد حجز {referenceNo}";
                    string attachmentName = $"{referenceNo}.pdf";
                    string emailBody = BuildReservationEmailBody(referenceNo);

                    string? senderDisplayName = null;
                    try
                    {
                        var companyRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement.CompanyDetails>();
                        var compRes = await companyRepo.GetAll();
                        var comp = compRes?.Data?.FirstOrDefault();
                        if (comp != null)
                        {
                            senderDisplayName = !string.IsNullOrWhiteSpace(comp.EnglishName)
                                ? comp.EnglishName
                                : (!string.IsNullOrWhiteSpace(comp.ArabicName) ? comp.ArabicName : null);
                        }
                    }
                    catch { }

                    await _documentShareService.ShareDocumentAsync(
                        document,
                        bulkDocumentSharePostDto.Method,
                        bulkDocumentSharePostDto.Receiver,
                        emailSubject,
                        emailBody,
                        attachmentName,
                        senderDisplayName
                    );

                    responses.Add(new DocumentShareResponseDto<Guid>
                    {
                        Id = reservationId,
                        Method = bulkDocumentSharePostDto.Method,
                        Receiver = bulkDocumentSharePostDto.Receiver,
                        Message = "Reservation shared successfully",
                        IsSuccess = true
                    });

                }
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                responses.Add(new DocumentShareResponseDto<Guid>
                {
                    Id = reservationId,
                    Method = bulkDocumentSharePostDto.Method,
                    Receiver = bulkDocumentSharePostDto.Receiver,
                    Message = "Failed to share reservation"
                });
            }
        }

        var result = new BulkDocumentShareResponseDto<Guid>
        {
            Total = responses.Count,
            SuccessCount = responses.Count(r => r.IsSuccess),
            FailedCount = responses.Count(r => !r.IsSuccess),
            Results = responses
        };
        return Ok(result);
    }


    [HttpPost("{reservationId:guid}/whatsappShare")]

    public async Task<IActionResult> ShareReservationOnWhatsApp(Guid reservationId, [FromBody] DocumentSharePostDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try { 

        var handler = new WhatsAppShareHandler<Guid>();

        var baseUrl = "https://erp.smartapro.com/reservation";

        var response = handler.BuildLinks(new[] { reservationId }, dto.Receiver, baseUrl);

        return Ok(new { response, reservationId });
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, "An unexpected error occurred while sharing the reservation on WhatsApp.");
        }

    }


    [HttpPost("reservations/whatsappShare")]
    public async Task<IActionResult> ShareReservationsOnWhatsApp([FromBody] BulkDocumentSharePostDto<Guid> dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            if (dto.Ids.Count > 20)
                return BadRequest("You cannot share more than 20 reservations at once.");

            var handler = new WhatsAppShareHandler<Guid>();

            var baseUrl = "https://erp.smartapro.com/reservation";

            var response = handler.BuildLinks(dto.Ids, dto.Receiver, baseUrl);

            return Ok(new { response, ReservationIds = dto.Ids.ToList() });

        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, "An unexpected error occurred while sharing the reservations on WhatsApp.");
        }
    }

    #region HTML Email Template Helper
    private string BuildReservationEmailBody(string referenceNo)
    {
        try
        {
            var basePath = _env?.WebRootPath ?? System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot");
            var templatePath = System.IO.Path.Combine(basePath, "templates", "email", "reservation_template.html");
            if (System.IO.File.Exists(templatePath))
            {
                var htmlContent = System.IO.File.ReadAllText(templatePath);
                return htmlContent
                    .Replace("{referenceNo}", referenceNo);
            }
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        // Fallback inline template in case file is missing or unreadable
        return $@"
<!DOCTYPE html>
<html dir=""rtl"" lang=""ar"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>تأكيد حجز</title>
</head>
<body style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f8fafc; margin: 0; padding: 12px; color: #334155;"">
    <div style=""width: 100%; max-width: 660px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; border: 1px solid #e2e8f0; box-shadow: 0 4px 14px rgba(0, 0, 0, 0.05);"">
        <!-- Header -->
        <div style=""background: linear-gradient(135deg, #0d9488 0%, #0f172a 100%); padding: 18px 24px; text-align: center; color: #ffffff;"">
            <h1 style=""margin: 0 0 4px 0; font-size: 20px; font-weight: 800; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; letter-spacing: 1px;"">
                SMART ERP SYSTEM
            </h1>
            <p style=""margin: 0; font-size: 13.5px; font-weight: 600; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; opacity: 0.92; line-height: 1.4;"">
                تأكيد حجز &nbsp;&bull;&nbsp; <span dir=""ltr"">{referenceNo}</span>
            </p>
        </div>

        <!-- Clean Body Content -->
        <div style=""padding: 20px 24px; direction: rtl; text-align: right; line-height: 1.6; font-size: 14px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;"">
            <p style=""margin: 0 0 8px 0; font-size: 15px; font-weight: 700; color: #0f172a;"">عزيزي العميل،</p>
            <p style=""margin: 0 0 10px 0; color: #475569;"">
                نشكركم لتعاملكم معنا. تجدون برفقة هذا البريد نسخة رسمية من <strong>مستند تأكيد الحجز رقم ({referenceNo})</strong> بصيغة PDF.
            </p>
            <p style=""margin: 0; color: #64748b; font-size: 13px;"">
                إذا كان لديكم أي استفسارات أو ملاحظات، يُرجى عدم التردد في التواصل معنا.
            </p>
        </div>

        <!-- Clean Footer -->
        <div style=""background-color: #f8fafc; border-top: 1px solid #f1f5f9; padding: 10px 20px; text-align: center; color: #94a3b8; font-size: 11px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;"">
            حقوق النشر &copy; 2026 Smart ERP System. جميع الحقوق محفوظة.
        </div>
    </div>
</body>
</html>";
    }
    #endregion
}
