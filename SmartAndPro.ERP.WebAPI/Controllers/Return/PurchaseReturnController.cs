using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Return.Purchase;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Return.Purchase;
using SmartAndPro.ERP.Domains.Entities.Return.Purchase;
using SmartAndPro.ERP.SharedKernel.Constants;
using SmartAndPro.ERP.SharedKernel.Dtos.DocumentShare;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Return
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [PermissionAuthorize(Permissions.PurchaseReturn.Read)]
    public class PurchaseReturnController : ControllerBase
    {
        #region Fields
        private readonly ILogger<PurchaseReturnController> _logger;
        private readonly ServiceBaseFactory _serviceBaseFactory;
        private readonly PurchaseReturnService _service;
        private readonly IMapper _mapper;
        private readonly IDocumentShareService _documentShareService;
        private const string inv = "PR-";
        #endregion

        #region Constructors
        public PurchaseReturnController(ServiceBaseFactory serviceBaseFactory, IMapper mapper, ILogger<PurchaseReturnController> logger, IDocumentShareService documentShareService)
        {
            _mapper = mapper;
            _serviceBaseFactory = serviceBaseFactory;
            _service = serviceBaseFactory.GetService<PurchaseReturnService>();
            _logger = logger;
            _documentShareService = documentShareService;
        }
        #endregion

        #region APIs
        [HttpGet]
        public async Task<IActionResult> GetAllReturns()
        {
            var result = await _service.GetAllAsync();
            if (result != null && result.Any())
            {
                var purchaseReturnDtos = _mapper.Map<IEnumerable<PurchaseReturnDto>>(result);
                return Ok(purchaseReturnDtos);
            }
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReturnById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid purchaseReturn ID: {Id}", id);
                return BadRequest("Invalid purchaseReturn ID.");
            }

            var purchaseReturn = await _service.GetByIdAsync(id);
            if (purchaseReturn != null)
            {
                var purchaseReturnDto = _mapper.Map<PurchaseReturnDto>(purchaseReturn);
                return Ok(purchaseReturnDto);
            }

            _logger.LogWarning("Return with ID: {Id} not found", id);
            return NotFound();
        }

        [HttpDelete("{id}")]
        [PermissionAuthorize(Permissions.PurchaseReturn.Delete)]
        public async Task<IActionResult> DeleteReturn(int id)
        {
            var purchaseReturn = await _service.GetByIdAsync(id);
            if (purchaseReturn == null)
            {
                _logger.LogWarning("Return with ID: {Id} not found", id);
                return NotFound();
            }

            var result = await _service.RemoveAsync(purchaseReturn);
            if (result == 1)
            {
                _logger.LogInformation("Return with ID: {Id} deleted successfully", id);
                return NoContent();
            }

            _logger.LogError("Failed to delete the purchaseReturn with ID: {Id}", id);
            return BadRequest("Failed to delete the purchaseReturn.");
        }

        [HttpPost]
        [PermissionAuthorize(Permissions.PurchaseReturn.Create)]
        public async Task<IActionResult> CreateReturn([FromBody] PurchaseReturnPostDto purchaseReturnDto)
        {
            if (purchaseReturnDto == null)
            {
                _logger.LogWarning("ReturnDto object sent from client is null.");
                return BadRequest("ReturnDto object is null.");
            }
            if (purchaseReturnDto.PurchaseReturnItems == null || purchaseReturnDto.PurchaseReturnItems.Count < 1)
            {
                _logger.LogWarning("PurchaseReturnItems object sent from client is null.");
                return BadRequest("PurchaseReturnItems object is null.");
            }
            var purchaseReturn = _mapper.Map<PurchaseReturn>(purchaseReturnDto);

            var result = await _service.AddAsync(purchaseReturn);
            if (result == 1)
            {
                _logger.LogInformation("Return created successfully.");
                return CreatedAtAction(nameof(GetReturnById), new { id = purchaseReturn.Id }, purchaseReturnDto);
            }

            _logger.LogError("Failed to create the purchaseReturn.");
            return BadRequest("Failed to create the purchaseReturn.");
        }

        [HttpPost("createTransaction")]
        [PermissionAuthorize(Permissions.PurchaseReturn.Create)]
        public async Task<IActionResult> CreateTransaction([FromQuery] int purchaseReturnId)
        {
            if (purchaseReturnId == null)
            {
                _logger.LogWarning("Return id sent from client is null.");
                return BadRequest("Return id  is null.");
            }

            var result = await _service.CreateTransaction(purchaseReturnId);
            if (result == 1)
            {
                _logger.LogInformation("transaction created successfully.");
                return Ok();
            }

            _logger.LogError("Failed to create the transaction.");
            return BadRequest("Failed to create the transaction.");   
        }
        [HttpPut("{id}")]
        [PermissionAuthorize(Permissions.PurchaseReturn.Update)]
        public async Task<IActionResult> UpdateReturn(int id, [FromBody] PurchaseReturnPutDto purchaseReturnDto)
        {
            if (purchaseReturnDto == null || purchaseReturnDto.Id != id)
            {
                _logger.LogWarning("Return data is null or ID mismatch");
                return BadRequest("Return data is null or ID mismatch");
            }

            var existingReturn = await _service.GetByIdAsync(id);
            if (existingReturn == null)
            {
                _logger.LogWarning("Return with ID: {Id} not found", id);
                return NotFound();
            }
            var oldItems = existingReturn.PurchaseReturnItems.ToList();
            existingReturn.PurchaseReturnItems.Clear();
            foreach (var item in purchaseReturnDto.PurchaseReturnItems)
            {
                existingReturn.PurchaseReturnItems.Add(_mapper.Map<PurchaseReturnItem>(item));
            }

            var purchaseReturn = _mapper.Map(purchaseReturnDto, existingReturn);

            var result = await _service.EditAsync(purchaseReturn, oldItems);

            if (result == 1)
            {
                _logger.LogInformation("Return updated successfully");
                return NoContent();
            }

            _logger.LogError("Failed to update purchaseReturn");
            return BadRequest("Failed to update purchaseReturn");
        }

        [HttpGet("ReturnNo")]
        public async Task<IActionResult> GetReturnNo()
        {
            var newReturnNum = await GetReturnNumber();

            return Ok(new { ReturnNo = newReturnNum });
        }

        [HttpGet("VATReport")]
        public async Task<IActionResult> GetPurchaseReturnVATReport([FromQuery] string startDate, [FromQuery] string endDate)
        {
            if (string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
            {
                _logger.LogWarning("Start date or end date is null or empty");
                return BadRequest("Start date and end date are required");
            }

            if (!DateOnly.TryParse(startDate, out DateOnly parsedStartDate))
            {
                _logger.LogWarning("Invalid start date format: {StartDate}", startDate);
                return BadRequest("Invalid start date format. Use YYYY-MM-DD");
            }

            if (!DateOnly.TryParse(endDate, out DateOnly parsedEndDate))
            {
                _logger.LogWarning("Invalid end date format: {EndDate}", endDate);
                return BadRequest("Invalid end date format. Use YYYY-MM-DD");
            }

            try
            {
                var result = await _service.GetPurchaseReturnVATReport(parsedStartDate, parsedEndDate);
                if (result == null || !result.Any())
                {
                    _logger.LogInformation("No purchase return VAT data found for date range: {StartDate} to {EndDate}", startDate, endDate);
                    return Ok(new { data = new List<object>(), message = "No data found for the specified date range" });
                }

                _logger.LogInformation("Successfully retrieved purchase return VAT report with {Count} records", result.Count());
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting purchase return VAT report");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpPost("Report/VAT/share")]
        public async Task<IActionResult> SharePurchaseReturnVATReport([FromBody] PurchaseReturnVATReportShareRequest shareRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                LogService.LogError($"Model validation failed: {errors}");
                return BadRequest(new { message = "Invalid request data", errors = ModelState });
            }

            try
            {
                LogService.LogInformation($"Purchase Return VAT Share request received - StartDate: {shareRequest.StartDate}, EndDate: {shareRequest.EndDate}, Method: {shareRequest.Method}, Language: {shareRequest.Language ?? "ar"}");
                
                var document = await _service.GetPurchaseReturnVATDocument(shareRequest.StartDate, shareRequest.EndDate, shareRequest.Language ?? "ar");
                
                if (document == null || document.Length == 0)
                {
                    LogService.LogError("Purchase Return VAT report document is null or empty");
                    return NotFound("Purchase Return VAT report document not found or could not be generated.");
                }

                LogService.LogInformation($"Purchase Return VAT Document generated successfully, size: {document.Length} bytes");

                // Handle WhatsApp sharing differently
                if (shareRequest.Method.Equals("WhatsApp", StringComparison.OrdinalIgnoreCase))
                {
                    var cleanReceiver = shareRequest.Receiver.Replace("+", "").Replace(" ", "").TrimStart('0');
                    
                    // Ensure phone number has country code
                    var hasCountryCode = cleanReceiver.StartsWith("1") || 
                                        cleanReceiver.StartsWith("20") || 
                                        cleanReceiver.StartsWith("44") || 
                                        cleanReceiver.StartsWith("91") ||
                                        cleanReceiver.StartsWith("966") || 
                                        cleanReceiver.StartsWith("971") ||
                                        cleanReceiver.StartsWith("974") ||
                                        cleanReceiver.StartsWith("965");
                    
                    if (!hasCountryCode && cleanReceiver.Length >= 9)
                    {
                        cleanReceiver = "966" + cleanReceiver;
                    }

                    // Save PDF to wwwroot/downloads folder for public access
                    var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "downloads");
                    Directory.CreateDirectory(webRootPath);

                    var fileName = $"Purchase_Return_VAT_Report_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.pdf";
                    var filePath = Path.Combine(webRootPath, fileName);

                    await System.IO.File.WriteAllBytesAsync(filePath, document);

                    LogService.LogInformation($"[WhatsApp] PDF saved to: {filePath}");

                    // Generate public download link
                    var baseUrl = $"{Request.Scheme}://{Request.Host}";
                    var downloadUrl = $"{baseUrl}/downloads/{fileName}";
                    
                    var message = $"مرحباً،%0A%0Aتم إنشاء تقرير القيمة المضافة لمرتجعات المشتريات بنجاح.%0A%0Aلتحميل التقرير:%0A{downloadUrl}%0A%0Aحجم الملف: {document.Length / 1024} كيلوبايت";
                    var whatsappLink = $"https://wa.me/{cleanReceiver}?text={message}";

                    LogService.LogInformation($"[WhatsApp] Link: {whatsappLink}");
                    
                    var response = new DocumentShareResponseDto<string>
                    {
                        Id = $"{shareRequest.StartDate}_{shareRequest.EndDate}",
                        Method = shareRequest.Method,
                        Receiver = cleanReceiver,
                        Message = "Purchase Return VAT report WhatsApp link generated successfully",
                        IsSuccess = true,
                        WhatsAppLink = whatsappLink,
                        DownloadLink = downloadUrl
                    };
                    return Ok(response);
                }
                else
                {
                    // Email sharing using DocumentShareService
                    LogService.LogInformation($"[Email] Sending to: {shareRequest.Receiver}");
                    
                    await _documentShareService.ShareDocumentAsync(document, shareRequest.Method, shareRequest.Receiver);

                    LogService.LogInformation($"[Email] Successfully sent to: {shareRequest.Receiver}");
                    
                    var response = new DocumentShareResponseDto<string>
                    {
                        Id = $"{shareRequest.StartDate}_{shareRequest.EndDate}",
                        Method = shareRequest.Method,
                        Receiver = shareRequest.Receiver,
                        Message = "Purchase Return VAT report sent successfully",
                        IsSuccess = true
                    };
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                LogService.LogError($"SharePurchaseReturnVATReport error: {ex.Message}");
                LogService.LogError($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Error sharing purchase return VAT report", details = ex.Message });
            }
        }

        [HttpGet("Report/VAT/download")]
        public async Task<IActionResult> DownloadPurchaseReturnVATReport([FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] string language = "ar")
        {
            if (string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
            {
                _logger.LogWarning("Start date or end date is null or empty");
                return BadRequest("Start date and end date are required");
            }

            if (!DateOnly.TryParse(startDate, out DateOnly parsedStartDate))
            {
                _logger.LogWarning("Invalid start date format: {StartDate}", startDate);
                return BadRequest("Invalid start date format. Use YYYY-MM-DD");
            }

            if (!DateOnly.TryParse(endDate, out DateOnly parsedEndDate))
            {
                _logger.LogWarning("Invalid end date format: {EndDate}", endDate);
                return BadRequest("Invalid end date format. Use YYYY-MM-DD");
            }

            try
            {
                var document = await _service.GetPurchaseReturnVATDocument(parsedStartDate, parsedEndDate, language);
                
                if (document == null || document.Length == 0)
                {
                    LogService.LogError("Purchase Return VAT report document is null or empty");
                    return NotFound("Purchase Return VAT report document not found or could not be generated.");
                }

                var fileName = $"Purchase_Return_VAT_Report_{startDate}_{endDate}.pdf";
                return File(document, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while downloading purchase return VAT report");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }
        #endregion

        #region Methods
        private async Task<string> GetReturnNumber()
        {
            var lastReturnNo = await _service.GetLastReturnNo();
            return GenerateNewReturnNo(lastReturnNo);
        }

        private string GenerateNewReturnNo(string lastReturnNo)
        {
            if (string.IsNullOrEmpty(lastReturnNo)) return inv + 1;

            var digits = new string(lastReturnNo.Where(char.IsDigit).ToArray());
            int.TryParse(digits, out int oldNum);

            return inv + (oldNum + 1);
        }
        #endregion
    }
}
