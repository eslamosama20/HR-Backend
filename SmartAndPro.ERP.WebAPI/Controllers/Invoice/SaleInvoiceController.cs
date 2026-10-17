using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.SharedKernel.Constants;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using SmartAndPro.ERP.Application.Common;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Common.Dtos.BulkDocumentShare;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.Reservation;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Services.@base;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Services.Invoice;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Services.Invoice.Sale;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Domains.Models.OutputModel;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.SharedKernel.Dtos.DocumentShare;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.SharedKernel.Enums;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.SharedKernel.Exceptions;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.SharedKernel.Utilities;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.SharedKernel.Utilities.ShareHandlers;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using System.IO.Compression;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;

namespace SmartAndPro.ERP.WebAPI.Controllers.Invoice
{
    [Route("api/[controller]")]
    [ApiController]
[Authorize]
[PermissionAuthorize("SalesInvoice.Read")]
    public class SaleInvoiceController : ControllerBase
    {
        #region Fields
        private readonly ILogger<SaleInvoiceController> _logger;
        private readonly ServiceBaseFactory _serviceBaseFactory;
        // private readonly SaleInvoiceService _service;
        private readonly ISalesInvoicesService _salesService;
        private readonly ISalesInvoiceImportService _salesInvoiceImportService;
        private readonly IDocumentShareService _documentShareService;
        private readonly IMapper _mapper;
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly IWebHostEnvironment _env;
        private const string inv = "Inv-";
        private const string quotation = "Q-";
        private static readonly TimeSpan ReservationInvoicesCacheDuration = TimeSpan.FromMinutes(15);
        #endregion

        #region Constructors
        public SaleInvoiceController(ServiceBaseFactory serviceBaseFactory,
            ISalesInvoicesService salesInvoicesService,
            ISalesInvoiceImportService salesInvoiceImportService,
            IDocumentShareService documentShareService,
            IMapper mapper, ILogger<SaleInvoiceController> logger, RepositoriesFactory repositoriesFactory, IMemoryCache memoryCache,
            IWebHostEnvironment env)
        {
            _mapper = mapper;
            _repositoriesFactory = repositoriesFactory;
            _serviceBaseFactory = serviceBaseFactory;
            // _service = serviceBaseFactory.GetService<SaleInvoiceService>();
            _salesService = salesInvoicesService;
            _salesInvoiceImportService = salesInvoiceImportService;
            _documentShareService = documentShareService;
            _logger = logger;
            _memoryCache = memoryCache;
            _env = env;
        }
        #endregion

        #region Print-1 server helpers

        /// <summary>
        /// Resolves the logged-in user's display name (used as the "البائع / Seller" line on
        /// the server Print-1 replica). Null-safe so unit tests (no HttpContext/factory) work.
        /// </summary>
        private async Task<string?> ResolveSellerNameAsync()
        {
            string? sellerName = null;
            var userIdClaim = HttpContext?.User?.FindFirst(ClaimTypes.Name);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId) && _repositoriesFactory != null)
            {
                var userRepository = _repositoriesFactory.GetRepository<User>();
                var userQuery = await userRepository.Filter(u => u.Id == userId);
                var user = await userQuery.Data?.FirstOrDefaultAsync();
                if (user != null)
                {
                    var firstName = user.FirstName ?? "";
                    var lastName = user.LastName ?? "";
                    sellerName = $"{firstName} {lastName}".Trim();
                    if (string.IsNullOrWhiteSpace(sellerName))
                    {
                        sellerName = user.UserName;
                    }
                }
            }
            return sellerName;
        }

        private static SmartAndPro.ERP.Application.Services.Invoice.Sale.Print1ServerOptions BuildPrint1Options(
            string language,
            string? sellerName,
            bool showStamp,
            bool showCompanySignature,
            bool showCustomerSignature,
            string? uncheckedPolicies,
            string? uncheckedBanks,
            string? orientation = null)
        {
            return new SmartAndPro.ERP.Application.Services.Invoice.Sale.Print1ServerOptions(
                language,
                sellerName,
                showStamp,
                showCompanySignature,
                showCustomerSignature,
                ParseGuidCsv(uncheckedPolicies),
                ParseGuidCsv(uncheckedBanks),
                Orientation: orientation);
        }

        private static SmartAndPro.ERP.Application.Services.Invoice.Sale.Print2ServerOptions BuildPrint2Options(
            bool showTermsInPrint2,
            bool showBanksInPrint2,
            string? uncheckedPolicies,
            string? uncheckedBanks)
        {
            return new SmartAndPro.ERP.Application.Services.Invoice.Sale.Print2ServerOptions(
                ShowTerms: showTermsInPrint2,
                ShowBanks: showBanksInPrint2,
                UncheckedPolicyIds: ParseGuidCsv(uncheckedPolicies),
                UncheckedBankIds: ParseGuidCsv(uncheckedBanks));
        }

        /// <summary>
        /// The frontend sends the language as <c>lang</c> (query param) while the controller
        /// parameters are named <c>language</c> — accept both so English mode actually binds.
        /// </summary>
        private static string ResolveLanguage(string language, string? lang) =>
            string.IsNullOrWhiteSpace(lang) ? language : lang;

        /// <summary>Parses a comma-separated list of Guids; invalid/empty entries are skipped.</summary>
        private static Guid[]? ParseGuidCsv(string? csv)
        {
            if (string.IsNullOrWhiteSpace(csv)) return null;
            var list = new List<Guid>();
            foreach (var part in csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (Guid.TryParse(part, out var guid)) list.Add(guid);
            }
            return list.Count > 0 ? list.ToArray() : null;
        }

        #endregion

        #region APIs
        [HttpGet("validateZatca/{id}")]
        public async Task<IActionResult> ValidateZatcaInvoice(int id)
        {
            try
            {
                var result = await _salesService.ValidateZatcaInvoiceAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating invoice for Zatca.");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("clearInvoice")]
        public async Task<IActionResult> ClearInvoice([FromBody] IdObjectInt idObject)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var invoiceRepository = _repositoriesFactory.GetRepository<SaleInvoice>();
                var invQuery = await invoiceRepository.Filter(i => i.Id == idObject.Id);
                var existingInvoice = invQuery.Data?.FirstOrDefault();

                if (existingInvoice != null && existingInvoice.IsCleared == true)
                {
                    return Ok(new
                    {
                        success = true,
                        isCleared = true,
                        status = "ALREADY_CLEARED",
                        invoiceNo = existingInvoice.InvoiceNo,
                        message = "Invoice is already sent and cleared with ZATCA."
                    });
                }

                await _salesService.ZatcaClearInvoice(idObject.Id);

                invQuery = await invoiceRepository.Filter(i => i.Id == idObject.Id);
                var updatedInvoice = invQuery.Data?.FirstOrDefault();
                bool isCleared = updatedInvoice?.IsCleared ?? false;

                return Ok(new
                {
                    success = isCleared,
                    isCleared = isCleared,
                    status = isCleared ? "CLEARED" : "NOT_CLEARED",
                    invoiceNo = updatedInvoice?.InvoiceNo,
                    invoiceHash = updatedInvoice?.InvoiceHash,
                    message = isCleared ? "Invoice cleared successfully." : "Invoice submission incomplete."
                });
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);

                var msg = ex.Message ?? string.Empty;

                if (msg.Contains("ZatcaIntegrated is disabled"))
                    return BadRequest(new { success = false, isCleared = false, status = "DISABLED", message = "ZATCA integration is disabled in company settings." });

                if (msg.Contains("Transaction creation failed"))
                    return Ok(new { success = true, isCleared = true, status = "CLEARED_TRANSACTION_FAILED", message = "Invoice cleared, but accounting transaction creation failed. Please contact support." });

                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, isCleared = false, status = "ERROR", message = ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetAllInvoices([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] bool? isSentToZatca = null, [FromQuery] string? keyword = null, [FromQuery] bool? hasTransaction = null)
       {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            IActionResult result = NoContent();
            try
            {
                var responseDate = await _salesService.GetAllAsync(pageNumber, pageSize, isSentToZatca, keyword, hasTransaction);
                result = responseDate.HandleResult();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
            return result;
        }

        [HttpGet("with-reservations")]
        
        public async Task<IActionResult> GetAllReservationInvoices([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] int? type = null, [FromQuery] string keyword = null, [FromQuery] string sortField = null, [FromQuery] int? sortOrder = null, [FromQuery] bool? isSentToZatca = null, [FromQuery] bool? hasTransaction = null)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            IActionResult result = NoContent();
            try
            {
                var responseDate = await _salesService.GetAllReservationInvoicesAsync(pageNumber, pageSize, type, keyword, sortField, sortOrder, isSentToZatca, hasTransaction);
                result = responseDate.HandleResult();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
            return result;
        }

        #region GetAll

        //[HttpGet]
        //public async Task<IActionResult> GetAllInvoices([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
        //{
        //    var result = await _salesService.GetAllAsync(pageNumber, pageSize);

        //    if (result != null && result.Data != null && result.Data.Any())
        //    {
        //        var invoiceDtos = _mapper.Map<IEnumerable<SaleInvoiceDto>>(result.Data);

        //        return Ok(new
        //        {
        //            TotalCount = result.TotalCount,
        //            Data = invoiceDtos
        //        });
        //    }

        //    return NoContent();
        //}


        //[HttpGet("frontal")]
        //public async Task<IActionResult> GetAllFrontalInvoices([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
        //{
        //    var result = await _salesService.GetAllFrontalAsync(pageNumber, pageSize);

        //    if (result?.Data != null && result.Data.Any())
        //    {
        //        var invoiceDtos = _mapper.Map<IEnumerable<SaleInvoiceDto>>(result.Data);

        //        return Ok(new
        //        {
        //            TotalCount = result.TotalCount,
        //            Data = invoiceDtos
        //        });
        //    }

        //    return NoContent();
        //}



        // Make hotel frontal
        #endregion


        [HttpGet("frontal")]
        public async Task<IActionResult> GetAllFrontalInvoices([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] bool? isSentToZatca = null, [FromQuery] string? keyword = null)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            IActionResult result = NoContent();
            try
            {
                var responseDate = await _salesService.GetAllFrontalAsync(pageNumber, pageSize, isSentToZatca, keyword);
                result = responseDate.HandleResult();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
            return result;
        }

        [HttpPost("frontal/{id}")]
        public async Task<IActionResult> MakeFrontalAsync(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            IActionResult result = NoContent();
            try
            {
                var responseData = await _salesService.MakeFrontalAsync(id);
                return Ok(responseData);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
            return result;
        }
        
        
        [HttpGet("quotation")]
        [PermissionAuthorize(Permissions.SalesQuotation.Read)]
        public async Task<IActionResult> GetAllQuotationInvoices()
        {
            var result = await _salesService.GetAllQuotationsAsync();
            if (result != null && result.Any())
            {
                var invoiceDtos = _mapper.Map<IEnumerable<SaleInvoiceDto>>(result);
                return Ok(invoiceDtos);
            }
            return NoContent();
        }

    
        [HttpGet("{id}/payment-summary")]
        public async Task<IActionResult> GetInvoicePaymentSummary(int id)
        {
            if (id <= 0) return BadRequest("Invalid invoice ID.");
            try
            {
                var summary = await _salesService.GetPaymentSummaryAsync(id);
                if (summary == null) return NotFound();
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment summary for invoice id {InvoiceId}", id);
                return StatusCode(500, "Internal server error while fetching invoice payment summary.");
            }
        }

        [HttpGet("copy-template/{id}")]
        public async Task<IActionResult> GetCopyTemplate(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid invoice ID for copy template: {Id}", id);
                return BadRequest("Invalid invoice ID.");
            }

            try
            {
                var templateDto = await _salesService.GetCopyTemplateAsync(id);
                if (templateDto == null)
                {
                    return NotFound("Invoice not found.");
                }

                templateDto.InvoiceNo = await GetInvoiceNumber();
                return Ok(templateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating copy template for invoice ID: {Id}", id);
                return StatusCode(500, "An error occurred while generating copy template.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoiceById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid invoice ID: {Id}", id);
                return BadRequest("Invalid invoice ID.");
            }

            try
            {
                var invoice = await _salesService.GetByIdAsync(id);
                if (invoice != null)
                {
                    var invoiceDto = _mapper.Map<SaleInvoiceDto>(invoice);
                    
                    if (invoice.SaleInvoiceType == SmartAndPro.ERP.Domains.Enums.SaleInvoiceType.ManualReservationInvoice || 
                        invoice.SaleInvoiceType == SmartAndPro.ERP.Domains.Enums.SaleInvoiceType.ComeFromReservation)
                    {
                        var reservationRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Reservation.Reservation>();
                        var reservationQuery = await reservationRepo.Filter(r => r.SaleInvoiceId == invoice.Id);
                        invoiceDto.ReservationIds = await reservationQuery.Data.Select(r => r.Id).ToListAsync();
                    }
                    
                    // Set original quotation number if this invoice was created from a quotation
                    invoiceDto.OriginalQuotationNo = await _salesService.GetOriginalQuotationNoAsync(id);

                    try
                    {
                        var dbContext = _repositoriesFactory.GetRepository<SaleInvoice>().GetDbContext();
                        var projectQuote = await dbContext.Set<SmartAndPro.ERP.Domains.Entities.Projects.ProjectPriceQuote>()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(pq => pq.SaleInvoiceId == id);
                        if (projectQuote != null)
                        {
                            invoiceDto.ProjectId = projectQuote.ProjectId;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error fetching project ID for quotation {Id}", id);
                    }

                    try
                    {
                        var dbContext = _repositoriesFactory.GetRepository<SaleInvoice>().GetDbContext();
                        var refunds = await dbContext.Set<SmartAndPro.ERP.Domains.Entities.Cashier.CashierRefund>()
                            .AsNoTracking()
                            .Include(r => r.Items)
                            .Where(r => r.InvoiceId == id && r.Status == "completed")
                            .ToListAsync();

                        if (refunds.Any())
                        {
                            var refundedAmount = refunds.Sum(r => r.RefundTotalAmount);
                            invoiceDto.RefundedAmount = refundedAmount;
                            invoiceDto.RefundStatus = refundedAmount >= invoice.NetPrice ? "refunded" : "partial_refund";

                            var refundedQtyBySaleItemId = refunds
                                .SelectMany(r => r.Items ?? new List<SmartAndPro.ERP.Domains.Entities.Cashier.CashierRefundItem>())
                                .GroupBy(ri => ri.SaleItemId)
                                .ToDictionary(g => g.Key, g => g.Sum(ri => ri.ReturnQty));

                            foreach (var itemDto in invoiceDto.SaleItems)
                            {
                                if (itemDto.Id.HasValue && refundedQtyBySaleItemId.TryGetValue(itemDto.Id.Value, out var rq))
                                {
                                    itemDto.ReturnedQuantity = rq;
                                }
                            }
                        }
                        else
                        {
                            invoiceDto.RefundStatus = "none";
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error mapping refund details to invoice DTO.");
                    }

                    // Resolve seller name
                    try
                    {
                        var sellerName = "";
                        var paymentRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Cashier.CashierPayment>();
                        var paymentQuery = await paymentRepo.Filter(p => p.InvoiceId == id);
                        var payment = await paymentQuery.Data?.OrderByDescending(p => p.Id).FirstOrDefaultAsync();

                        Guid? userGuid = null;
                        if (payment != null && payment.CreatedBy != Guid.Empty)
                        {
                            userGuid = payment.CreatedBy;
                        }
                        else if (!string.IsNullOrWhiteSpace(invoice.CreatedBy) && Guid.TryParse(invoice.CreatedBy, out var parsedGuid))
                        {
                            userGuid = parsedGuid;
                        }

                        if (userGuid.HasValue && userGuid.Value != Guid.Empty)
                        {
                            var userRepo = _repositoriesFactory.GetRepository<User>();
                            var user = (await userRepo.Filter(u => u.Id == userGuid.Value)).Data?.FirstOrDefault();
                            if (user != null)
                            {
                                var fullName = $"{user.FirstName} {user.LastName}".Trim();
                                sellerName = !string.IsNullOrWhiteSpace(fullName) ? fullName : user.UserName;
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(invoice.CreatedBy) && !Guid.TryParse(invoice.CreatedBy, out _))
                        {
                            sellerName = invoice.CreatedBy;
                        }

                        if (string.IsNullOrWhiteSpace(sellerName) || Guid.TryParse(sellerName, out _))
                        {
                            var userRepo = _repositoriesFactory.GetRepository<User>();
                            var defaultUser = (await userRepo.Filter(u => u.IsActive)).Data?.FirstOrDefault();
                            if (defaultUser != null)
                            {
                                var fullName = $"{defaultUser.FirstName} {defaultUser.LastName}".Trim();
                                sellerName = !string.IsNullOrWhiteSpace(fullName) ? fullName : defaultUser.UserName;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(sellerName))
                        {
                            invoiceDto.SellerName = sellerName;
                            invoiceDto.CashierName = sellerName;
                            invoiceDto.CreatedBy = sellerName;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error resolving seller name for invoice {Id}", id);
                    }
                    
                    return Ok(invoiceDto);
                }

                _logger.LogWarning("Invoice with ID: {Id} not found", id);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice by id: {Id}", id);
                try { System.IO.File.WriteAllText(@"I:\SmartAndPro\Backend\SmartAndPro.ERP.Inventory\api_error.txt", ex.ToString()); } catch {}
                return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
            }
        }

        [HttpGet("product/barcode/{barcode}")]
        public async Task<IActionResult> GetProductByBarcode(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                return BadRequest("Barcode is required.");
            }

            var product = await _salesService.GetProductByBarcodeAsync(barcode);
            if (product == null)
            {
                return NotFound(new { message = "Product not found for the provided barcode." });
            }

            return Ok(product);
        }

        [HttpGet("by-reservation/{reservationId}")]
        public async Task<IActionResult> GetInvoiceByReservationId(Guid reservationId)
        {
            var invoice = await _salesService.GetByReservationIdAsync(reservationId);
            if (invoice != null)
            {
                var invoiceDto = _mapper.Map<SaleInvoiceDto>(invoice);
                return Ok(invoiceDto);
            }

            return Ok(null);
        }

        [HttpGet("by-reservations")]
        public async Task<IActionResult> GetInvoiceByReservationIds([FromQuery] List<Guid> reservationIds)
        {
      
            var invoices = await _salesService.GetByReservationIdsAsync(reservationIds);
            if (invoices == null || !invoices.Any())
            {
                return Ok(new List<SaleInvoiceDto>());
            }

            var invoiceDtos = _mapper.Map<List<SaleInvoiceDto>>(invoices);
            return Ok(invoiceDtos);
        }

        [HttpPost("by-reservation-ids")]
        public async Task<IActionResult> GetInvoiceByReservationIdsBatch([FromBody] SaleInvoiceByReservationIdsRequestDto request)
        {
            var uniqueReservationIds = request?.ReservationIds?
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList() ?? new List<Guid>();

            if (uniqueReservationIds.Count > 100)
            {
                return BadRequest("Maximum allowed reservationIds is 100.");
            }

            if (!uniqueReservationIds.Any())
            {
                return Ok(new SaleInvoiceByReservationIdsResponseDto());
            }

            var cacheKey = $"sale-invoices-by-reservation-ids:{string.Join(",", uniqueReservationIds.OrderBy(x => x))}";
            if (_memoryCache.TryGetValue(cacheKey, out SaleInvoiceByReservationIdsResponseDto? cachedResponse) && cachedResponse != null)
            {
                return Ok(cachedResponse);
            }

            var reservationRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Reservation.Reservation>();
            var reservationsQuery = await reservationRepo.Filter(r =>
                uniqueReservationIds.Contains(r.Id) && r.SaleInvoiceId.HasValue);

            var reservations = reservationsQuery.Data?.ToList() ?? new List<SmartAndPro.ERP.Domains.Entities.Reservation.Reservation>();
            if (!reservations.Any())
            {
                var emptyResponse = new SaleInvoiceByReservationIdsResponseDto();
                _memoryCache.Set(cacheKey, emptyResponse, ReservationInvoicesCacheDuration);
                return Ok(emptyResponse);
            }

            var reservationInvoiceMap = reservations
                .Where(r => r.SaleInvoiceId.HasValue)
                .GroupBy(r => r.Id)
                .ToDictionary(g => g.Key, g => g.First().SaleInvoiceId!.Value);

            var invoices = (await _salesService.GetByReservationIdsAsync(uniqueReservationIds))
                .Where(i => i != null)
                .Select(i => i!)
                .GroupBy(i => i.Id)
                .Select(g => g.First())
                .ToList();

            var invoiceDtos = _mapper.Map<List<SaleInvoiceDto>>(invoices);
            var invoiceDtoById = invoiceDtos.ToDictionary(i => i.Id, i => i);

            var response = new SaleInvoiceByReservationIdsResponseDto();
            foreach (var reservationPair in reservationInvoiceMap)
            {
                if (invoiceDtoById.TryGetValue(reservationPair.Value, out var invoiceDto))
                {
                    response.Invoices[reservationPair.Key] = invoiceDto;
                }
            }

            _memoryCache.Set(cacheKey, response, ReservationInvoicesCacheDuration);
            return Ok(response);
        }

        /// <summary>
        /// Upload a .xlsx file and import all sales invoices in a single server-side transaction.
        /// Returns structured counts: imported, updated, and per-row failures.
        /// </summary>
        /// <param name="file">Excel file (.xlsx or .xls) containing invoice data</param>
        /// <returns>Import result with counts of imported, updated, and failed rows</returns>
        [HttpPost("import")]
        [PermissionAuthorize(Permissions.SalesInvoice.Import)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(SaleInvoiceImportResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ImportFromExcel([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            const long maxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
            if (file.Length > maxFileSizeBytes)
                return BadRequest(new { message = "File is too large. Maximum allowed size is 10 MB." });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
                return BadRequest(new { message = "Only .xlsx and .xls files are supported." });

            var stopwatch = Stopwatch.StartNew();
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? HttpContext.User.FindFirstValue(ClaimTypes.Name)
                         ?? "anonymous";
            try
            {
                _logger.LogInformation("Sales invoice import started. User: {UserId}, File: {FileName}, Size: {FileSize}", userId, file.FileName, file.Length);
                await using var stream = file.OpenReadStream();
                var importResult = await _salesInvoiceImportService.ImportFromExcelAsync(stream);
                stopwatch.Stop();

                _logger.LogInformation(
                    "Sales invoice import completed. User: {UserId}, File: {FileName}, Imported: {Imported}, Updated: {Updated}, Failed: {Failed}, DurationMs: {DurationMs}",
                    userId,
                    file.FileName,
                    importResult.Imported,
                    importResult.Updated,
                    importResult.Failed?.Count ?? 0,
                    stopwatch.ElapsedMilliseconds);

                return Ok(importResult);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Sales invoice import failed. User: {UserId}, File: {FileName}, DurationMs: {DurationMs}", userId, file.FileName, stopwatch.ElapsedMilliseconds);
                LogService.LogError(ex);
                return StatusCode(500, new { message = "Import failed unexpectedly.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [PermissionAuthorize(Permissions.SalesInvoice.Delete)]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            var invoice = await _salesService.GetByIdAsync(id);
            if (invoice == null)
            {
                _logger.LogWarning("Invoice with ID: {Id} not found", id);
                return NotFound();
            }

            if (invoice.IsCleared == true)
            {
                return BadRequest("لا يمكن حذف الفاتورة؛ تم إرسالها واعتمادها لدى هيئة الزكاة والضريبة والجمارك.");
            }

            var result = await _salesService.RemoveAsync(invoice);
            if (result == 1)
            {
                _logger.LogInformation("Invoice with ID: {Id} deleted successfully", id);
                return NoContent();
            }

            _logger.LogError("Failed to delete the invoice with ID: {Id}", id);
            return BadRequest("Failed to delete the invoice.");
        }

        [HttpPost]
        [PermissionAuthorize(Permissions.SalesInvoice.Create)]
        public async Task<IActionResult> CreateInvoice([FromBody] SaleInvoicePostDto invoiceDto)
        {
            if (invoiceDto == null)
            {
                _logger.LogWarning("InvoiceDto object sent from client is null.");
                return BadRequest("InvoiceDto object is null.");
            }

            if (invoiceDto.SaleItems == null || invoiceDto.SaleItems.Count < 1)
            {
                _logger.LogWarning("SaleDto object sent from client is null.");
                return BadRequest("SaleDto object is null.");
            }

            try
            {
                // Force IsQuotation to false for sales invoices
                invoiceDto.IsQuotation = false;
                
                _logger.LogInformation($"Creating SALES INVOICE - IsQuotation: {invoiceDto.IsQuotation}, Customer: {invoiceDto.CustomerProfileName}, Items count: {invoiceDto.SaleItems.Count}");
                var result = await _salesService.AddAsync(invoiceDto);
                _logger.LogInformation($"Sales Invoice created successfully with ID: {result.Id}, InvoiceNo: {result.InvoiceNo}, IsQuotation: {result.IsQuotation}");
                return CreatedAtAction(nameof(GetInvoiceById), new { id = result.Id }, invoiceDto);
            }
            catch (Exception e)
            {
                LogService.LogError(e);
                _logger.LogError($"Failed to create the sales invoice. Error: {e.Message}");
                if (e is BusinessRuleViolationException brvEx)
                {
                    var businessRuleMessage = brvEx.Errors?.FirstOrDefault()?.ErrorMessage ?? e.Message;
                    return BadRequest(new
                    {
                        message = businessRuleMessage,
                        errors = brvEx.Errors?.Select(err => new
                        {
                            err.RuleId,
                            err.ErrorMessage,
                            err.ErrorCode,
                            Severity = err.Severity.ToString()
                        })
                    });
                }
                return BadRequest($"Failed to create the sales invoice: {e.Message}");
            }
        }
        
        
        [HttpPost("quotation")]
        [PermissionAuthorize(Permissions.SalesQuotation.Create)]
        public async Task<IActionResult> CreateQuotation([FromBody] SaleInvoicePostDto invoiceDto)
        {
            if (invoiceDto == null)
            {
                _logger.LogWarning("QuotationDto object sent from client is null.");
                return BadRequest("QuotationDto object is null.");
            }

            if (invoiceDto.SaleItems == null || invoiceDto.SaleItems.Count < 1)
            {
                _logger.LogWarning("SaleDto object sent from client is null.");
                return BadRequest("SaleDto object is null.");
            }

            try
            {
                // Force IsQuotation to true for quotations
                invoiceDto.IsQuotation = true;
                
                _logger.LogInformation($"Creating QUOTATION - IsQuotation: {invoiceDto.IsQuotation}, Customer: {invoiceDto.CustomerProfileName}, Items count: {invoiceDto.SaleItems.Count}");
                var result = await _salesService.AddAsync(invoiceDto);
                _logger.LogInformation($"Quotation created successfully with ID: {result.Id}, InvoiceNo: {result.InvoiceNo}, IsQuotation: {result.IsQuotation}");
                return CreatedAtAction(nameof(GetInvoiceById), new { id = result.Id }, invoiceDto);
            }
            catch (Exception e)
            {
                LogService.LogError(e);
                _logger.LogError($"Failed to create the quotation. Error: {e.Message}");
                if (e is BusinessRuleViolationException brvEx)
                {
                    var businessRuleMessage = brvEx.Errors?.FirstOrDefault()?.ErrorMessage ?? e.Message;
                    return BadRequest(new
                    {
                        message = businessRuleMessage,
                        errors = brvEx.Errors?.Select(err => new
                        {
                            err.RuleId,
                            err.ErrorMessage,
                            err.ErrorCode,
                            Severity = err.Severity.ToString()
                        })
                    });
                }
                return BadRequest($"Failed to create the quotation: {e.Message}");
            }
        }
        
        [HttpPost("convert-quotation/{quotationId}")]
        [PermissionAuthorize(Permissions.SalesInvoice.Create)]
        public async Task<IActionResult> ConvertQuotationToInvoice(int quotationId)
        {
            if (quotationId <= 0)
            {
                _logger.LogWarning("Invalid quotation ID: {Id}", quotationId);
                return BadRequest("Invalid quotation ID.");
            }

            try
            {
                _logger.LogInformation($"Converting quotation ID: {quotationId} to invoice");
                var result = await _salesService.ConvertQuotationToInvoiceAsync(quotationId);
                _logger.LogInformation($"Quotation converted successfully. New invoice ID: {result.Id}, InvoiceNo: {result.InvoiceNo}");
                
                var invoiceDto = _mapper.Map<SaleInvoiceDto>(result);
                
                // Set original quotation number
                invoiceDto.OriginalQuotationNo = await _salesService.GetQuotationNoByIdAsync(quotationId);
                
                return CreatedAtAction(nameof(GetInvoiceById), new { id = result.Id }, invoiceDto);
            }
            catch (Exception e)
            {
                LogService.LogError(e);
                _logger.LogError($"Failed to convert quotation. Error: {e.Message}");
                return BadRequest($"Failed to convert quotation: {e.Message}");
            }
        }
        
        [HttpPost("frontal")]
        [PermissionAuthorize(Permissions.SalesInvoice.Create)]
        public async Task<IActionResult> CreateAndClearInvoice([FromBody] SaleInvoicePostDto invoiceDto)
        {
            if (invoiceDto == null)
            {
                _logger.LogWarning("InvoiceDto object sent from client is null.");
                return BadRequest("InvoiceDto object is null.");
            }

            if (invoiceDto.SaleItems == null || invoiceDto.SaleItems.Count < 1)
            {
                _logger.LogWarning("SaleDto object sent from client is null.");
                return BadRequest("SaleDto object is null.");
            }

            try
            {
                var result = await _salesService.AddAsync(invoiceDto);
                if (result == null)
                {
                    _logger.LogError("Failed to create the invoice.");
                    return BadRequest("Failed to create the invoice.");
                }
                await _salesService.ZatcaClearInvoice(result.Id);
                _logger.LogInformation("Invoice created successfully.");
                return CreatedAtAction(nameof(GetInvoiceById), new { id = result.Id }, invoiceDto);
            }
            catch (Exception e)
            {
                LogService.LogError(e);
                _logger.LogError("Failed to create the invoice.");
                if (e is BusinessRuleViolationException brvEx)
                {
                    var businessRuleMessage = brvEx.Errors?.FirstOrDefault()?.ErrorMessage ?? e.Message;
                    return BadRequest(new
                    {
                        message = businessRuleMessage,
                        errors = brvEx.Errors?.Select(err => new
                        {
                            err.RuleId,
                            err.ErrorMessage,
                            err.ErrorCode,
                            Severity = err.Severity.ToString()
                        })
                    });
                }
                return BadRequest($"Failed to create the invoice: {e.Message}");
            }
        }

        [HttpPost("createTransaction")]
        public async Task<IActionResult> CreateTransaction([FromQuery] int invoiceId)
        {

            var result = await _salesService.CreateTransaction(invoiceId);
            if (result == 1)
            {
                _logger.LogInformation("transaction created successfully.");
                return Ok();
            }

            _logger.LogError("Failed to create the transaction.");
            return BadRequest("Failed to create the transaction.");
        }

       
        [HttpPost("bulkCreateTransactionsOptimized")]
        public async Task<IActionResult> BulkCreateTransactionsOptimized([FromBody] List<int> invoiceIds)
        {
            var result = await _salesService.CreateBulkTransactionsOptimized(invoiceIds);
            if (result == 1)
            {
                _logger.LogInformation("Bulk transactions (Optimized) created successfully.");
                return Ok();
            }

            _logger.LogError("Failed to create bulk transactions (Optimized).");
            return BadRequest("Failed to create bulk transactions (Optimized).");
        }

        [HttpPut("{id}")]
        [PermissionAuthorize(Permissions.SalesInvoice.Update)]
        public async Task<IActionResult> UpdateInvoice(int id, [FromBody] SaleInvoicePutDto invoiceDto)
        {
            if (invoiceDto == null)
            {
                _logger.LogWarning("Invoice data is null");
                return BadRequest("Invoice data is null");
            }

            if (invoiceDto.Id == 0 || invoiceDto.Id != id)
            {
                invoiceDto.Id = id;
            }

            var existingInvoice = await _salesService.GetByIdAsync(id);
            if (existingInvoice == null)
            {
                _logger.LogWarning("Invoice with ID: {Id} not found", id);
                return NotFound();
            }

            if (existingInvoice.IsCleared == true)
            {
                return BadRequest("لا يمكن تعديل الفاتورة؛ تم إرسالها واعتمادها لدى هيئة الزكاة والضريبة والجمارك.");
            }

            if (existingInvoice.SaleInvoiceType == SmartAndPro.ERP.Domains.Enums.SaleInvoiceType.ComeFromReservation ||
                existingInvoice.SaleInvoiceType == SmartAndPro.ERP.Domains.Enums.SaleInvoiceType.ManualReservationInvoice)
            {
                bool isAdmin = User.IsInRole("Admin") || User.IsInRole("System.Admin");
                if (!isAdmin)
                {
                    return BadRequest("غير مصرح لك بتعديل فواتير الحجوزات. هذه الصلاحية لمدير النظام فقط.");
                }
            }

            List<SaleItem>? oldItems = existingInvoice.SaleItems.ToList();
            existingInvoice.SaleItems.Clear();
            foreach (var item in invoiceDto.SaleItems)
            {
                existingInvoice.SaleItems.Add(_mapper.Map<SaleItem>(item));
            }

            var originalIsFrontal = existingInvoice.IsFrontal;
            var invoice = _mapper.Map(invoiceDto, existingInvoice);
            if (!invoiceDto.IsFrontal.HasValue)
            {
                invoice.IsFrontal = originalIsFrontal;
            }

            var result = await _salesService.EditAsync(invoice, oldItems);

            if (result == 1)
            {
                _logger.LogInformation("Invoice updated successfully");
                return NoContent();
            }

            _logger.LogError("Failed to update invoice");
            return BadRequest("Failed to update invoice");
        }

        [HttpGet("InvoiceNo")]
        public async Task<IActionResult> GetInvoiceNo()
        {
            var newInvoiceNum = await GetInvoiceNumber();

            return Ok(new { InvoiceNo = newInvoiceNum });
        }

        [HttpGet("quotationNo")]
        public async Task<IActionResult> GetQuotationNo()
        {
            var newQuotationNum = await GetQuotationNumber();

            return Ok(new { QuotationNo = newQuotationNum });
        }

        [HttpPost("Report/Details")]
        public ActionResult GetSalesInvoicesDetailsReport(GetSalesInvoiceDetailsRequest invoiceDetailsRequest)
        {
            try
            {

                return Ok(_salesService.GetSalesInvoiceDetails(invoiceDetailsRequest));
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return BadRequest();
            }
        }

        [HttpPost("Report/Details/Export")]
        public IActionResult ExportSalesInvoiceToExcel(GetSalesInvoiceDetailsRequest getSalesInvoiceDetails)
        {
            try
            {
                var excelData = _salesService.ExportSalesInvoiceDetails(getSalesInvoiceDetails);

                if (excelData != null)
                {
                    return File(
                        excelData,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "SalesReport.xlsx"
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "An error occurred while exporting the sales report.");
            }

            return NotFound("No data found to export.");
        }

        [HttpGet("Report/Details/document")]
        public async Task<IActionResult> GetDocument([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] string language = "ar")
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            IActionResult result = NoContent();
            try
            {
                var document = await _salesService.GetDocument(startDate, endDate, language);
                
                if (document != null && document.Length > 0)
                {
                    var fileName = language == "ar" 
                        ? $"تقرير_القيمة_المضافة_لأصناف_المبيعات_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.pdf"
                        : $"sales-vat-items-report-{startDate:yyyyMMdd}-{endDate:yyyyMMdd}.pdf";
                    result = File(document, "application/pdf", fileName);
                }
            }
            catch (Exception ex)
            {
                LogService.LogError($"GetDocument controller error: {ex.Message}");
                LogService.LogError($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    LogService.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                result = StatusCode(500, new { message = ex.Message, details = ex.StackTrace });
            }
            return result;
        }

        [HttpGet("Report/Details/summary-document")]
        public async Task<IActionResult> GetSummaryDocument([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] string language = "ar")
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            IActionResult result = NoContent();
            try
            {
                var document = await _salesService.GetSummaryDocument(startDate, endDate, language);
                
                if (document != null && document.Length > 0)
                {
                    var fileName = $"sales-invoice-summary-report-{startDate:yyyyMMdd}-{endDate:yyyyMMdd}.pdf";
                    result = File(document, "application/pdf", fileName);
                }
            }
            catch (Exception ex)
            {
                LogService.LogError($"GetSummaryDocument controller error: {ex.Message}");
                LogService.LogError($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    LogService.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                result = StatusCode(500, new { message = ex.Message, details = ex.StackTrace });
            }
            return result;
        }

        [HttpPost("Report/Details/share")]
        public async Task<IActionResult> ShareSalesInvoiceReport([FromBody] SalesInvoiceReportShareRequest shareRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                LogService.LogError($"Model validation failed: {errors}");
                return BadRequest(new { message = "Invalid request data", errors = ModelState });
            }

            try
            {
                LogService.LogInformation($"Share request received - StartDate: {shareRequest.StartDate}, EndDate: {shareRequest.EndDate}, Method: {shareRequest.Method}, Language: {shareRequest.Language ?? "ar"}");
                
                var document = await _salesService.GetDocument(shareRequest.StartDate, shareRequest.EndDate, shareRequest.Language ?? "ar");
                
                if (document == null || document.Length == 0)
                {
                    LogService.LogError("Sales invoice report document is null or empty");
                    return NotFound("Sales invoice report document not found or could not be generated.");
                }

                LogService.LogInformation($"Document generated successfully, size: {document.Length} bytes");

                await _documentShareService.ShareDocumentAsync(document, shareRequest.Method, shareRequest.Receiver);
                
                LogService.LogInformation($"Document shared successfully via {shareRequest.Method} to {shareRequest.Receiver}");
                
                var response = new DocumentShareResponseDto<string>
                {
                    Id = $"{shareRequest.StartDate}_{shareRequest.EndDate}",
                    Method = shareRequest.Method,
                    Receiver = shareRequest.Receiver,
                    Message = "Sales invoice report shared successfully",
                    IsSuccess = true
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                LogService.LogError($"Error details: {ex.Message}\nStackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    LogService.LogError($"Inner exception: {ex.InnerException.Message}\nInner StackTrace: {ex.InnerException.StackTrace}");
                }
                return StatusCode(500, new { message = "An unexpected error occurred while sharing the sales invoice report.", error = ex.Message });
            }
        }

        [HttpPost("Report/Details/whatsappShare")]
        public async Task<IActionResult> ShareSalesInvoiceReportOnWhatsApp([FromBody] SalesInvoiceReportShareRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var handler = new WhatsAppShareHandler<string>();
                var baseUrl = "https://erp.smartapro.com/sales-invoice-report";
                var reportId = $"{dto.StartDate}_{dto.EndDate}";
                var response = handler.BuildLinks(new[] { reportId }, dto.Receiver, baseUrl);
                return Ok(new { response, reportId });
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, "An unexpected error occurred while sharing the sales invoice report on WhatsApp.");
            }
        }

        // VAT Report Endpoints
        [HttpPost("Report/VAT/Details")]
        public ActionResult GetSalesVATDetailsReport(GetSalesVATDetailsRequest vatDetailsRequest)
        {
            try
            {
                return Ok(_salesService.GetSalesVATDetails(vatDetailsRequest));
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return BadRequest();
            }
        }

        [HttpPost("Report/VAT/Export")]
        public IActionResult ExportSalesVATToExcel(GetSalesVATDetailsRequest getVATDetails)
        {
            try
            {
                var excelData = _salesService.ExportSalesVATDetails(getVATDetails);

                if (excelData != null)
                {
                    return File(
                        excelData,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "SalesVATReport.xlsx"
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "An error occurred while exporting the sales VAT report.");
            }

            return NotFound("No data found to export.");
        }

        [HttpGet("Report/VAT/document")]
        public async Task<IActionResult> GetVATDocument([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] string language = "ar")
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            IActionResult result = NoContent();
            try
            {
                var document = await _salesService.GetVATDocument(startDate, endDate, language);
                
                if (document != null && document.Length > 0)
                {
                    var fileName = $"sales-vat-report-{startDate:yyyyMMdd}-{endDate:yyyyMMdd}.pdf";
                    result = File(document, "application/pdf", fileName);
                }
            }
            catch (Exception ex)
            {
                LogService.LogError($"GetVATDocument controller error: {ex.Message}");
                LogService.LogError($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    LogService.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                result = StatusCode(500, new { message = ex.Message, details = ex.StackTrace });
            }
            return result;
        }

        [HttpPost("Report/VAT/share")]
        public async Task<IActionResult> ShareSalesVATReport([FromBody] SalesVATReportShareRequest shareRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                LogService.LogError($"Model validation failed: {errors}");
                return BadRequest(new { message = "Invalid request data", errors = ModelState });
            }

            try
            {
                LogService.LogInformation($"VAT Share request received - StartDate: {shareRequest.StartDate}, EndDate: {shareRequest.EndDate}, Method: {shareRequest.Method}, Language: {shareRequest.Language ?? "ar"}");
                
                var document = await _salesService.GetVATDocument(shareRequest.StartDate, shareRequest.EndDate, shareRequest.Language ?? "ar");
                
                if (document == null || document.Length == 0)
                {
                    LogService.LogError("Sales VAT report document is null or empty");
                    return NotFound("Sales VAT report document not found or could not be generated.");
                }

                LogService.LogInformation($"VAT Document generated successfully, size: {document.Length} bytes");

                // Handle WhatsApp sharing differently
                if (shareRequest.Method.Equals("WhatsApp", StringComparison.OrdinalIgnoreCase))
                {
                    var cleanReceiver = shareRequest.Receiver.Replace("+", "").Replace(" ", "").TrimStart('0');
                    
                    // Ensure phone number has country code (only add 966 if no country code detected)
                    // Common country codes: 1, 20, 91, 44, 966, 971, etc.
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
                        cleanReceiver = "966" + cleanReceiver; // Default to Saudi Arabia
                    }

                    // Save PDF to wwwroot/downloads folder for public access
                    var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "downloads");
                    Directory.CreateDirectory(webRootPath);

                    var fileName = $"VAT_Report_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.pdf";
                    var filePath = Path.Combine(webRootPath, fileName);

                    await System.IO.File.WriteAllBytesAsync(filePath, document);

                    LogService.LogInformation($"[WhatsApp] PDF saved to: {filePath}");

                    // Generate public download link using current request base URL
                    var baseUrl = $"{Request.Scheme}://{Request.Host}";
                    var downloadUrl = $"{baseUrl}/downloads/{fileName}";
                    
                    // Create WhatsApp message with download link
                    var message = $"مرحباً،%0A%0Aتم إنشاء تقرير القيمة المضافة بنجاح.%0A%0Aلتحميل التقرير:%0A{downloadUrl}%0A%0Aحجم الملف: {document.Length / 1024} كيلوبايت";
                    var whatsappLink = $"https://wa.me/{cleanReceiver}?text={message}";

                    LogService.LogInformation($"[WhatsApp] Link: {whatsappLink}");
                    
                    var response = new DocumentShareResponseDto<string>
                    {
                        Id = $"{shareRequest.StartDate}_{shareRequest.EndDate}",
                        Method = shareRequest.Method,
                        Receiver = cleanReceiver,
                        Message = "Sales VAT report WhatsApp link generated successfully",
                        IsSuccess = true,
                        WhatsAppLink = whatsappLink,
                        DownloadLink = downloadUrl
                    };
                    return Ok(response);
                }
                else
                {
                    // Handle Email and other methods
                    try
                    {
                        await _documentShareService.ShareDocumentAsync(document, shareRequest.Method, shareRequest.Receiver);
                        
                        LogService.LogInformation($"VAT Document shared successfully via {shareRequest.Method} to {shareRequest.Receiver}");
                        
                        var response = new DocumentShareResponseDto<string>
                        {
                            Id = $"{shareRequest.StartDate}_{shareRequest.EndDate}",
                            Method = shareRequest.Method,
                            Receiver = shareRequest.Receiver,
                            Message = shareRequest.Method.Equals("Email", StringComparison.OrdinalIgnoreCase) 
                                ? "تم إرسال تقرير القيمة المضافة عبر البريد الإلكتروني بنجاح" 
                                : "Sales VAT report shared successfully",
                            IsSuccess = true
                        };
                        return Ok(response);
                    }
                    catch (Exception shareEx)
                    {
                        LogService.LogError($"Error sharing document via {shareRequest.Method}: {shareEx.Message}");
                        return BadRequest(new 
                        { 
                            message = shareRequest.Method.Equals("Email", StringComparison.OrdinalIgnoreCase)
                                ? "فشل إرسال البريد الإلكتروني. يرجى التحقق من عنوان البريد الإلكتروني والمحاولة مرة أخرى."
                                : $"Failed to share document via {shareRequest.Method}",
                            error = shareEx.Message,
                            isSuccess = false
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                LogService.LogError($"Error details: {ex.Message}\nStackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    LogService.LogError($"Inner exception: {ex.InnerException.Message}\nInner StackTrace: {ex.InnerException.StackTrace}");
                }
                return StatusCode(500, new { message = "An unexpected error occurred while sharing the sales VAT report.", error = ex.Message });
            }
        }

        [HttpPost("Report/VAT/whatsappShare")]
        public async Task<IActionResult> ShareSalesVATReportOnWhatsApp([FromBody] SalesVATReportShareRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var handler = new WhatsAppShareHandler<string>();
                var baseUrl = "https://erp.smartapro.com/sales-vat-report";
                var reportId = $"{dto.StartDate}_{dto.EndDate}";
                var response = handler.BuildLinks(new[] { reportId }, dto.Receiver, baseUrl);
                return Ok(new { response, reportId });
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, "An unexpected error occurred while sharing the sales VAT report on WhatsApp.");
            }
        }

        [HttpGet("document/{invoiceId}")]
        public async Task<IActionResult> GetDocument(
            int invoiceId, 
            [FromQuery] SmartAndPro.ERP.Domains.Enums.InvoicePrintType printType = SmartAndPro.ERP.Domains.Enums.InvoicePrintType.A4Portrait,
            [FromQuery] bool isLandscape = false, // Keep for backward compatibility
            [FromQuery] string language = "ar",
            [FromQuery] string? lang = null,
            [FromQuery] bool showStamp = true,
            [FromQuery] bool showCompanySignature = true,
            [FromQuery] bool showCustomerSignature = true,
            [FromQuery] bool showTermsInPrint2 = false,
            [FromQuery] bool showBanksInPrint2 = false,
            [FromQuery] string? uncheckedPolicies = null,
            [FromQuery] string? uncheckedBanks = null, // Print-1/Print-2 server parity params
            [FromQuery] string? orientation = null) // User-forced orientation for Format1Server: portrait | landscape (omitted → auto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            IActionResult result = NoContent();
            try
            {
                // Backward compatibility: if isLandscape is true and printType is default, use A4Landscape
                if (isLandscape && printType == SmartAndPro.ERP.Domains.Enums.InvoicePrintType.A4Portrait)
                {
                    printType = SmartAndPro.ERP.Domains.Enums.InvoicePrintType.A4Landscape;
                }
                var sellerName = await ResolveSellerNameAsync();

                var print1 = (printType == SmartAndPro.ERP.Domains.Enums.InvoicePrintType.Format1Server)
                    ? BuildPrint1Options(ResolveLanguage(language, lang), sellerName, showStamp, showCompanySignature, showCustomerSignature, uncheckedPolicies, uncheckedBanks, orientation)
                    : null;

                var print2 = (showTermsInPrint2 || showBanksInPrint2)
                    ? BuildPrint2Options(showTermsInPrint2, showBanksInPrint2, uncheckedPolicies, uncheckedBanks)
                    : null;

                var document = await _salesService.GetDocument(invoiceId, printType, sellerName, print1, print2);
                if (document != null)
                {
                    result = File(document, "application/pdf", $"invoice-{invoiceId}.pdf");
                }
            }
            catch (Exception ex)
            {
                result = StatusCode(500, ex.Message);
            }
            return result;
        }

        [HttpGet("documents")]
        [PermissionAuthorize(Permissions.SalesInvoice.DownloadSelectedPdf)]
        public async Task<IActionResult> GetDocuments(
            [FromQuery] int[] ids,
            [FromQuery] SmartAndPro.ERP.Domains.Enums.InvoicePrintType printType = SmartAndPro.ERP.Domains.Enums.InvoicePrintType.A4Portrait,
            [FromQuery] string language = "ar",
            [FromQuery] string? lang = null,
            [FromQuery] bool showStamp = true,
            [FromQuery] bool showCompanySignature = true,
            [FromQuery] bool showCustomerSignature = true,
            [FromQuery] bool showTermsInPrint2 = false,
            [FromQuery] bool showBanksInPrint2 = false,
            [FromQuery] string? uncheckedPolicies = null,
            [FromQuery] string? uncheckedBanks = null,
            [FromQuery] string? orientation = null)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                SmartAndPro.ERP.Application.Services.Invoice.Sale.Print1ServerOptions? print1 = null;
                if (printType == SmartAndPro.ERP.Domains.Enums.InvoicePrintType.Format1Server)
                {
                    print1 = BuildPrint1Options(ResolveLanguage(language, lang), await ResolveSellerNameAsync(), showStamp, showCompanySignature, showCustomerSignature, uncheckedPolicies, uncheckedBanks, orientation);
                }
                var print2 = (showTermsInPrint2 || showBanksInPrint2)
                    ? BuildPrint2Options(showTermsInPrint2, showBanksInPrint2, uncheckedPolicies, uncheckedBanks)
                    : null;
                var documents = await _salesService.GetDocuments(ids, printType, print1, print2);
                using var zipStream = new MemoryStream();
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    for (int i = 0; i < documents.Length; i++)
                    {
                        var entry = archive.CreateEntry($"invoice-{ids[i]}.pdf");
                        using var entryStream = entry.Open();
                        entryStream.Write(documents[i], 0, documents[i].Length);
                    }
                }
                zipStream.Position = 0;
                return File(zipStream.ToArray(), "application/zip", "invoices.zip");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

[Authorize]
[PermissionAuthorize("SalesInvoice.Read")]
        public class MergeRequest
        {
            public int[] ids { get; set; } = Array.Empty<int>();

            public SmartAndPro.ERP.Domains.Enums.InvoicePrintType printType { get; set; } = SmartAndPro.ERP.Domains.Enums.InvoicePrintType.A4Portrait;

            // Print-1 / Print-2 server parity params
            public string Language { get; set; } = "ar";
            public bool ShowStamp { get; set; } = true;
            public bool ShowCompanySignature { get; set; } = true;
            public bool ShowCustomerSignature { get; set; } = true;
            public bool ShowTermsInPrint2 { get; set; } = false;
            public bool ShowBanksInPrint2 { get; set; } = false;
            public string? UncheckedPolicies { get; set; }
            public string? UncheckedBanks { get; set; }
            public string? Orientation { get; set; }
        }

        [HttpPost("documents/merged")]
        [PermissionAuthorize(Permissions.SalesInvoice.DownloadSelectedMergedPdf)]
        public async Task<IActionResult> GetMerged([FromBody] MergeRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                if (request == null || request.ids == null || request.ids.Length == 0)
                    return BadRequest("No invoice ids supplied");
                if (request.printType is SmartAndPro.ERP.Domains.Enums.InvoicePrintType.Thermal80mm or SmartAndPro.ERP.Domains.Enums.InvoicePrintType.Thermal58mm)
                    return BadRequest("Thermal print types are not supported for merged documents");
                SmartAndPro.ERP.Application.Services.Invoice.Sale.Print1ServerOptions? print1 = null;
                if (request.printType == SmartAndPro.ERP.Domains.Enums.InvoicePrintType.Format1Server)
                {
                    print1 = BuildPrint1Options(request.Language, await ResolveSellerNameAsync(), request.ShowStamp, request.ShowCompanySignature, request.ShowCustomerSignature, request.UncheckedPolicies, request.UncheckedBanks, request.Orientation);
                }
                var print2 = (request.ShowTermsInPrint2 || request.ShowBanksInPrint2)
                    ? BuildPrint2Options(request.ShowTermsInPrint2, request.ShowBanksInPrint2, request.UncheckedPolicies, request.UncheckedBanks)
                    : null;
                var merged = await _salesService.GetMergedDocument(request.ids, request.printType, print1, print2);
                if (merged == null || merged.Length == 0) return NoContent();
                return File(merged, "application/pdf", "invoices-merged.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



      

        [HttpGet("documents/merged/all")]
        [PermissionAuthorize(Permissions.SalesInvoice.ExportPdfMerged)]
        public async Task<IActionResult> GetMergedAll(
            [FromQuery] SmartAndPro.ERP.Domains.Enums.InvoicePrintType printType = SmartAndPro.ERP.Domains.Enums.InvoicePrintType.A4Portrait,
            [FromQuery] string language = "ar",
            [FromQuery] string? lang = null,
            [FromQuery] bool showStamp = true,
            [FromQuery] bool showCompanySignature = true,
            [FromQuery] bool showCustomerSignature = true,
            [FromQuery] bool showTermsInPrint2 = false,
            [FromQuery] bool showBanksInPrint2 = false,
            [FromQuery] string? uncheckedPolicies = null,
            [FromQuery] string? uncheckedBanks = null,
            [FromQuery] string? orientation = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = await _salesService.GetAllAsync();

                if (response.ErrorCode != ErrorCodes.Success)
                    return StatusCode(500, new { message = response.Message ?? "An error occurred while retrieving invoices." });

                IEnumerable<SaleInvoiceDto> invoices;

                if (response.Data is PaginatedResult<IEnumerable<SaleInvoiceDto>> paginated)
                {
                    invoices = paginated.Data;
                }
                else if (response.Data is IEnumerable<SaleInvoiceDto> list)
                {
                    invoices = list;
                }
                else
                {
                    return NoContent(); 
                }

                if (invoices == null || !invoices.Any())
                    return NoContent();

                if (printType is SmartAndPro.ERP.Domains.Enums.InvoicePrintType.Thermal80mm or SmartAndPro.ERP.Domains.Enums.InvoicePrintType.Thermal58mm)
                    return BadRequest("Thermal print types are not supported for merged documents");

                var ids = invoices.Select(i => i.Id).ToArray();
                SmartAndPro.ERP.Application.Services.Invoice.Sale.Print1ServerOptions? print1 = null;
                if (printType == SmartAndPro.ERP.Domains.Enums.InvoicePrintType.Format1Server)
                {
                    print1 = BuildPrint1Options(ResolveLanguage(language, lang), await ResolveSellerNameAsync(), showStamp, showCompanySignature, showCustomerSignature, uncheckedPolicies, uncheckedBanks, orientation);
                }
                var print2 = (showTermsInPrint2 || showBanksInPrint2)
                    ? BuildPrint2Options(showTermsInPrint2, showBanksInPrint2, uncheckedPolicies, uncheckedBanks)
                    : null;
                var merged = await _salesService.GetMergedDocument(ids, printType, print1, print2);

                if (merged == null || merged.Length == 0)
                    return NoContent();

                return File(merged, "application/pdf", "invoices-merged-all.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        #region GetMergedAll
        //[HttpGet("documents/merged/all")]
        //public async Task<IActionResult> GetMergedAll()
        //{
        //    if (!ModelState.IsValid) return BadRequest(ModelState);
        //    try
        //    {
        //        var invoices = await _salesService.GetAllAsync();
        //        if (invoices == null || !invoices.Any()) return NoContent();
        //        var ids = invoices.Select(i => i.Id).ToArray();
        //        var merged = await _salesService.GetMergedDocument(ids);
        //        if (merged == null || merged.Length == 0) return NoContent();
        //        return File(merged, "application/pdf", "invoices-merged-all.pdf");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }
        //}

        #endregion

        [HttpGet("documents/all")]
        [PermissionAuthorize(Permissions.SalesInvoice.ExportPdfZip)]
        public async Task<IActionResult> GetAllDocuments()
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var documents = await _salesService.GetAllDocuments();
                if (documents.Length == 0) return NoContent();
                using var zipStream = new MemoryStream();
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    for (int i = 0; i < documents.Length; i++)
                    {
                        var entry = archive.CreateEntry($"invoice-{i + 1}.pdf");
                        using var entryStream = entry.Open();
                        entryStream.Write(documents[i], 0, documents[i].Length);
                    }
                }
                zipStream.Position = 0;
                return File(zipStream.ToArray(), "application/zip", "all-invoices.zip");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{invoiceId}/share")]
        public async Task<IActionResult> ShareInvoice(int invoiceId, [FromBody] DocumentSharePostDto documentShareDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var document = await _salesService.GetDocument(invoiceId);
                if (document == null)
                    return NotFound("Invoice document not found.");

                if (documentShareDto.Method.Equals("WhatsApp", StringComparison.OrdinalIgnoreCase))
                {
                    var invoiceRepo = _repositoriesFactory.GetRepository<SaleInvoice>();
                    var invoiceResult = await invoiceRepo.Filter(x => x.Id == invoiceId);
                    var invoice = invoiceResult.Data.FirstOrDefault();
                    if (invoice == null)
                        return NotFound("Invoice record not found.");

                    var isQuotation = invoice.IsQuotation;
                    var invoiceNo = invoice.InvoiceNo;

                    var cleanReceiver = documentShareDto.Receiver.Replace("+", "").Replace(" ", "").TrimStart('0');
                    if (!cleanReceiver.StartsWith("1") && 
                        !cleanReceiver.StartsWith("20") && 
                        !cleanReceiver.StartsWith("44") && 
                        !cleanReceiver.StartsWith("91") &&
                        !cleanReceiver.StartsWith("966") && 
                        !cleanReceiver.StartsWith("971") &&
                        !cleanReceiver.StartsWith("974") &&
                        !cleanReceiver.StartsWith("965") &&
                        cleanReceiver.Length >= 9)
                    {
                        cleanReceiver = "966" + cleanReceiver;
                    }

                    // Save PDF to wwwroot/downloads folder for public access
                    var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "downloads");
                    Directory.CreateDirectory(webRootPath);

                    var prefix = isQuotation ? "Quotation" : "Invoice";
                    var fileName = $"{prefix}_{invoiceNo}_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.pdf";
                    var filePath = Path.Combine(webRootPath, fileName);

                    await System.IO.File.WriteAllBytesAsync(filePath, document);

                    LogService.LogInformation($"[WhatsApp] {prefix} PDF saved to: {filePath}");

                    var baseUrl = $"{Request.Scheme}://{Request.Host}";
                    var downloadUrl = $"{baseUrl}/downloads/{fileName}";
                    
                    string arabicMessage;
                    if (isQuotation)
                    {
                        arabicMessage = $"مرحباً،%0A%0Aتم إنشاء عرض السعر رقم {invoiceNo} بنجاح.%0A%0Aلتحميل عرض السعر:%0A{downloadUrl}%0A%0Aحجم الملف: {document.Length / 1024} كيلوبايت";
                    }
                    else
                    {
                        arabicMessage = $"مرحباً،%0A%0Aتم إنشاء الفاتورة رقم {invoiceNo} بنجاح.%0A%0Aلتحميل الفاتورة:%0A{downloadUrl}%0A%0Aحجم الملف: {document.Length / 1024} كيلوبايت";
                    }
                    
                    var whatsappLink = $"https://wa.me/{cleanReceiver}?text={arabicMessage}";

                    LogService.LogInformation($"[WhatsApp] Link: {whatsappLink}");

                    var response = new DocumentShareResponseDto<int>
                    {
                        Id = invoiceId,
                        Method = documentShareDto.Method,
                        Receiver = cleanReceiver,
                        Message = isQuotation ? "Quotation shared on WhatsApp successfully" : "Invoice shared on WhatsApp successfully",
                        IsSuccess = true,
                        WhatsAppLink = whatsappLink,
                        DownloadLink = downloadUrl
                    };
                    return Ok(response);
                }
                else
                {
                    var invoiceRepo = _repositoriesFactory.GetRepository<SaleInvoice>();
                    var invoiceResult = await invoiceRepo.Filter(x => x.Id == invoiceId);
                    var invoice = invoiceResult.Data.FirstOrDefault();

                    var isQuotation = invoice?.IsQuotation == true;
                    var isReservation = invoice?.SaleInvoiceType == SmartAndPro.ERP.Domains.Enums.SaleInvoiceType.ComeFromReservation 
                                     || invoice?.SaleInvoiceType == SmartAndPro.ERP.Domains.Enums.SaleInvoiceType.ManualReservationInvoice 
                                     || (int)(invoice?.SaleInvoiceType ?? 0) == 2;
                    var invoiceNo = !string.IsNullOrWhiteSpace(invoice?.InvoiceNo) ? invoice.InvoiceNo : invoiceId.ToString();

                    string emailSubject = isQuotation 
                        ? $"عرض سعر {invoiceNo}" 
                        : (isReservation ? $"فاتورة حجز {invoiceNo}" : $"فاتورة مبيعات {invoiceNo}");
                    string attachmentName = $"{invoiceNo}.pdf";
                    string emailBody = BuildInvoiceEmailBody(invoiceNo, isQuotation, isReservation);

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

                    try
                    {
                        await _documentShareService.ShareDocumentAsync(
                            document, 
                            documentShareDto.Method, 
                            documentShareDto.Receiver, 
                            emailSubject, 
                            emailBody, 
                            attachmentName,
                            senderDisplayName
                        );
                        var response = new DocumentShareResponseDto<int>
                        {
                            Id = invoiceId,
                            Method = documentShareDto.Method,
                            Receiver = documentShareDto.Receiver,
                            Message = "Invoice shared successfully",
                            IsSuccess = true
                        };
                        return Ok(response);
                    }
                    catch (Exception smtpEx)
                    {
                        LogService.LogError(smtpEx);

                        var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "downloads");
                        Directory.CreateDirectory(webRootPath);

                        var prefix = isQuotation ? "Quotation" : "Invoice";
                        var fileName = $"{prefix}_{invoiceNo}_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.pdf";
                        var filePath = Path.Combine(webRootPath, fileName);

                        await System.IO.File.WriteAllBytesAsync(filePath, document);

                        var baseUrl = $"{Request.Scheme}://{Request.Host}";
                        var downloadUrl = $"{baseUrl}/downloads/{fileName}";

                        var fallbackResponse = new DocumentShareResponseDto<int>
                        {
                            Id = invoiceId,
                            Method = documentShareDto.Method,
                            Receiver = documentShareDto.Receiver,
                            Message = "تم تجهيز رابط التحميل للمستند بنجاح (تعذر الاتصال المباشر بخادم البريد)",
                            IsSuccess = true,
                            DownloadLink = downloadUrl
                        };
                        return Ok(fallbackResponse);
                    }
                }

            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, "An unexpected error occurred while sharing the invoice.");
            }
        }

        [HttpPost("invoices/share")]
        [PermissionAuthorize(Permissions.SalesInvoice.BulkShare)]
        public async Task<IActionResult> ShareInvoices([FromBody] BulkDocumentSharePostDto<int> bulkDocumentSharePostDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var responses = new List<DocumentShareResponseDto<int>>();

            foreach (var invoiceId in bulkDocumentSharePostDto.Ids)
            {
                try
                {
                    var document = await _salesService.GetDocument(invoiceId);
                    if (document == null)
                    {
                        responses.Add(new DocumentShareResponseDto<int>
                        {
                            Id = invoiceId,
                            Method = bulkDocumentSharePostDto.Method,
                            Receiver = bulkDocumentSharePostDto.Receiver,
                            Message = "Invoice document not found",
                            IsSuccess = false
                        });
                    }
                    else
                    {
                        var invoiceRepo = _repositoriesFactory.GetRepository<SaleInvoice>();
                        var invoiceResult = await invoiceRepo.Filter(x => x.Id == invoiceId);
                        var invoice = invoiceResult.Data.FirstOrDefault();

                        var isQuotation = invoice?.IsQuotation == true;
                        var isReservation = invoice?.SaleInvoiceType == SmartAndPro.ERP.Domains.Enums.SaleInvoiceType.ComeFromReservation 
                                         || invoice?.SaleInvoiceType == SmartAndPro.ERP.Domains.Enums.SaleInvoiceType.ManualReservationInvoice 
                                         || (int)(invoice?.SaleInvoiceType ?? 0) == 2;
                        var invoiceNo = !string.IsNullOrWhiteSpace(invoice?.InvoiceNo) ? invoice.InvoiceNo : invoiceId.ToString();

                        string emailSubject = isQuotation 
                            ? $"عرض سعر {invoiceNo}" 
                            : (isReservation ? $"فاتورة حجز {invoiceNo}" : $"فاتورة مبيعات {invoiceNo}");
                        string attachmentName = $"{invoiceNo}.pdf";
                        string emailBody = BuildInvoiceEmailBody(invoiceNo, isQuotation, isReservation);

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

                        responses.Add(new DocumentShareResponseDto<int>
                        {
                            Id = invoiceId,
                            Method = bulkDocumentSharePostDto.Method,
                            Receiver = bulkDocumentSharePostDto.Receiver,
                            Message = "Invoice shared successfully",
                            IsSuccess = true
                        });
                    }
                }
                catch (Exception ex)
                {
                    LogService.LogError(ex);
                    responses.Add(new DocumentShareResponseDto<int>
                    {
                        Id = invoiceId,
                        Method = bulkDocumentSharePostDto.Method,
                        Receiver = bulkDocumentSharePostDto.Receiver,
                        Message = "Failed to share invoice",
                        IsSuccess = false
                    });
                }
            }

            var result = new BulkDocumentShareResponseDto<int>
            {
                Total = responses.Count,
                SuccessCount = responses.Count(r => r.IsSuccess),
                FailedCount = responses.Count(r => !r.IsSuccess),
                Results = responses
            };

            return Ok(result);
        }



        [HttpPost("{invoiceId:int}/whatsappShare")]
        public async Task<IActionResult> ShareInvoiceOnWhatsApp(int invoiceId, [FromBody] DocumentSharePostDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var document = await _salesService.GetDocument(invoiceId);
                if (document == null)
                    return NotFound("Invoice document not found.");

                var invoiceRepo = _repositoriesFactory.GetRepository<SaleInvoice>();
                var invoiceResult = await invoiceRepo.Filter(x => x.Id == invoiceId);
                var invoice = invoiceResult.Data.FirstOrDefault();
                if (invoice == null)
                    return NotFound("Invoice record not found.");

                var isQuotation = invoice.IsQuotation;
                var isReservation = invoice.SaleInvoiceType == SmartAndPro.ERP.Domains.Enums.SaleInvoiceType.ComeFromReservation 
                                 || invoice.SaleInvoiceType == SmartAndPro.ERP.Domains.Enums.SaleInvoiceType.ManualReservationInvoice 
                                 || (int)invoice.SaleInvoiceType == 2;
                var invoiceNo = !string.IsNullOrWhiteSpace(invoice.InvoiceNo) ? invoice.InvoiceNo : invoiceId.ToString();

                var cleanReceiver = dto.Receiver.Replace("+", "").Replace(" ", "").TrimStart('0');
                if (System.Text.RegularExpressions.Regex.IsMatch(cleanReceiver, @"^(10|11|12|15)\d{8}$"))
                {
                    cleanReceiver = "20" + cleanReceiver;
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(cleanReceiver, @"^5\d{8}$"))
                {
                    cleanReceiver = "966" + cleanReceiver;
                }

                // Save PDF to wwwroot/downloads folder for public access
                var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "downloads");
                Directory.CreateDirectory(webRootPath);

                var prefix = isQuotation ? "Quotation" : (isReservation ? "ReservationInvoice" : "Invoice");
                var fileName = $"{invoiceNo}_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.pdf";
                var filePath = Path.Combine(webRootPath, fileName);

                await System.IO.File.WriteAllBytesAsync(filePath, document);

                LogService.LogInformation($"[WhatsApp] {prefix} PDF saved to: {filePath}");

                var clientOrigin = Request.Headers["Origin"].ToString();
                if (string.IsNullOrWhiteSpace(clientOrigin) || clientOrigin.Contains("5001"))
                {
                    clientOrigin = "https://erp.smartapro.com";
                }
                var downloadUrl = isReservation 
                    ? $"{clientOrigin.TrimEnd('/')}/invoices/reservation-details/{invoiceId}" 
                    : $"{clientOrigin.TrimEnd('/')}/invoice?id={invoiceId}";
                
                string arabicMessage;
                if (isQuotation)
                {
                    arabicMessage = $"*عرض سعر ({invoiceNo})*%0A%0Aاضغط على الرابط التالي لتحميل عرض السعر:%0A{downloadUrl}";
                }
                else if (isReservation)
                {
                    arabicMessage = $"*فاتورة حجز ({invoiceNo})*%0A%0Aاضغط على الرابط التالي لتحميل فاتورة الحجز:%0A{downloadUrl}";
                }
                else
                {
                    arabicMessage = $"*فاتورة مبيعات ({invoiceNo})*%0A%0Aاضغط على الرابط التالي لتحميل الفاتورة:%0A{downloadUrl}";
                }
                
                var whatsappLink = $"https://wa.me/{cleanReceiver}?text={arabicMessage}";

                LogService.LogInformation($"[WhatsApp] Link: {whatsappLink}");

                var response = new DocumentShareResponseDto<int>
                {
                    Id = invoiceId,
                    Method = "WhatsApp",
                    Receiver = cleanReceiver,
                    Message = isQuotation ? "Quotation shared on WhatsApp successfully" : "Invoice shared on WhatsApp successfully",
                    IsSuccess = true,
                    WhatsAppLink = whatsappLink,
                    DownloadLink = downloadUrl
                };

                return Ok(new { response, invoiceId });
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, "An unexpected error occurred while sharing the invoice on WhatsApp.");
            }
        }


        [HttpPost("invoices/whatsappShare")]
        [PermissionAuthorize(Permissions.SalesInvoice.BulkShare)]
        public async Task<IActionResult> ShareInvoicesOnWhatsApp([FromBody] BulkDocumentSharePostDto<int> dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {

                var handler = new WhatsAppShareHandler<int>();

                var baseUrl = "https://erp.smartapro.com/invoice";

                var response = handler.BuildLinks(dto.Ids, dto.Receiver, baseUrl);

                return Ok(new { response, InvoiceIds = dto.Ids.ToList() });

            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, "An unexpected error occurred while sharing the invoices on WhatsApp.");
            }
        }



        [HttpGet("NetVATReport")]
        public async Task<IActionResult> GetNetSalesVATReport([FromQuery] string startDate, [FromQuery] string endDate)
        {
            try
            {
                if (string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
                {
                    _logger.LogWarning("GetNetSalesVATReport called with missing date parameters");
                    return BadRequest("Start date and end date are required");
                }

                if (!DateOnly.TryParse(startDate, out var parsedStartDate))
                {
                    _logger.LogWarning($"Invalid start date format: {startDate}");
                    return BadRequest("Invalid start date format. Use yyyy-MM-dd");
                }

                if (!DateOnly.TryParse(endDate, out var parsedEndDate))
                {
                    _logger.LogWarning($"Invalid end date format: {endDate}");
                    return BadRequest("Invalid end date format. Use yyyy-MM-dd");
                }

                _logger.LogInformation($"Generating Net Sales VAT Report from {parsedStartDate} to {parsedEndDate}");
                
                var reportData = await _salesService.GetNetSalesVATReport(parsedStartDate, parsedEndDate);

                _logger.LogInformation($"Net Sales VAT Report generated successfully");
                return Ok(reportData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Net Sales VAT Report");
                return StatusCode(500, "An error occurred while generating the report");
            }
        }

        [HttpGet("NetVATReport/download")]
        public async Task<IActionResult> DownloadNetSalesVATReport([FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] string language = "ar")
        {
            try
            {
                if (string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
                {
                    _logger.LogWarning("DownloadNetSalesVATReport called with missing date parameters");
                    return BadRequest("Start date and end date are required");
                }

                if (!DateOnly.TryParse(startDate, out var parsedStartDate))
                {
                    _logger.LogWarning($"Invalid start date format: {startDate}");
                    return BadRequest("Invalid start date format. Use yyyy-MM-dd");
                }

                if (!DateOnly.TryParse(endDate, out var parsedEndDate))
                {
                    _logger.LogWarning($"Invalid end date format: {endDate}");
                    return BadRequest("Invalid end date format. Use yyyy-MM-dd");
                }

                _logger.LogInformation($"Downloading Net Sales VAT Report PDF from {parsedStartDate} to {parsedEndDate}");
                
                var document = await _salesService.GetNetSalesVATDocument(parsedStartDate, parsedEndDate, language);

                if (document == null || document.Length == 0)
                {
                    _logger.LogWarning("Net Sales VAT Report document is empty");
                    return NotFound("No data found for the specified date range");
                }

                var fileName = $"Net_Sales_VAT_Report_{parsedStartDate:yyyy-MM-dd}_{parsedEndDate:yyyy-MM-dd}.pdf";
                
                _logger.LogInformation($"Returning Net Sales VAT Report PDF: {fileName}, Size: {document.Length} bytes");
                return File(document, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading Net Sales VAT Report");
                return StatusCode(500, "An error occurred while generating the PDF");
            }
        }

        [HttpPost("NetVATReport/share")]
        public async Task<IActionResult> ShareNetSalesVATReport([FromBody] NetSalesVATReportShareRequest shareRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                LogService.LogError($"Model validation failed: {errors}");
                return BadRequest(new { message = "Invalid request data", errors = ModelState });
            }

            try
            {
                LogService.LogInformation($"Net Sales VAT Share request received - StartDate: {shareRequest.StartDate}, EndDate: {shareRequest.EndDate}, Method: {shareRequest.Method}, Language: {shareRequest.Language ?? "ar"}");
                
                var document = await _salesService.GetNetSalesVATDocument(shareRequest.StartDate, shareRequest.EndDate, shareRequest.Language ?? "ar");
                
                if (document == null || document.Length == 0)
                {
                    LogService.LogError("Net Sales VAT report document is null or empty");
                    return NotFound("Net Sales VAT report document not found or could not be generated.");
                }

                LogService.LogInformation($"Net Sales VAT Document generated successfully, size: {document.Length} bytes");

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

                    var fileName = $"Net_Sales_VAT_Report_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.pdf";
                    var filePath = Path.Combine(webRootPath, fileName);

                    await System.IO.File.WriteAllBytesAsync(filePath, document);

                    LogService.LogInformation($"[WhatsApp] PDF saved to: {filePath}");

                    // Generate public download link
                    var baseUrl = $"{Request.Scheme}://{Request.Host}";
                    var downloadUrl = $"{baseUrl}/downloads/{fileName}";
                    
                    var message = $"مرحباً،%0A%0Aتم إنشاء تقرير صافي القيمة المضافة للمبيعات بنجاح.%0A%0Aلتحميل التقرير:%0A{downloadUrl}%0A%0Aحجم الملف: {document.Length / 1024} كيلوبايت";
                    var whatsappLink = $"https://wa.me/{cleanReceiver}?text={message}";

                    LogService.LogInformation($"[WhatsApp] Link: {whatsappLink}");
                    
                    var response = new DocumentShareResponseDto<string>
                    {
                        Id = $"{shareRequest.StartDate}_{shareRequest.EndDate}",
                        Method = shareRequest.Method,
                        Receiver = cleanReceiver,
                        Message = "Net Sales VAT report WhatsApp link generated successfully",
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
                        Message = "Net Sales VAT report sent successfully",
                        IsSuccess = true
                    };
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                LogService.LogError($"ShareNetSalesVATReport error: {ex.Message}");
                LogService.LogError($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Error sharing net sales VAT report", details = ex.Message });
            }
        }

        #endregion

        #region Methods
        private async Task<string> GetInvoiceNumber()
        {
            var lastInvoiceNo = await _salesService.GetLastInvoiceNo();
            return GenerateNewInvoiceNo(lastInvoiceNo);
        }

        private async Task<string> GetQuotationNumber()
        {
            var lastQuotationNo = await _salesService.GetLastQuotationNo();
            return GenerateNewQuotationNo(lastQuotationNo);
        }

        private string GenerateNewInvoiceNo(string lastInvoiceNo)
        {
            if (string.IsNullOrEmpty(lastInvoiceNo)) return inv + 1;

            var digits = new string(lastInvoiceNo.Where(char.IsDigit).ToArray());
            int.TryParse(digits, out int oldNum);

            return inv + (oldNum + 1);
        }

        private string GenerateNewQuotationNo(string lastQuotationNo)
        {
            if (string.IsNullOrEmpty(lastQuotationNo)) return quotation + 1;

            var digits = new string(lastQuotationNo.Where(char.IsDigit).ToArray());
            int.TryParse(digits, out int oldNum);

            return quotation + (oldNum + 1);
        }

        // ===================== Net Due VAT Report Endpoints =====================

        [HttpGet("NetDueVATReport")]
        public async Task<IActionResult> GetNetDueVATReport([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] string language = "ar")
        {
            try
            {
                var reportData = await _salesService.GetNetDueVATReport(startDate, endDate, language);
                return Ok(reportData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating Net Due VAT report", error = ex.Message });
            }
        }

        [HttpGet("NetDueVATReport/document")]
        public async Task<IActionResult> DownloadNetDueVATReport([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] string language = "ar")
        {
            try
            {
                var pdfBytes = await _salesService.GetNetDueVATDocument(startDate, endDate, language);
                
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    return NotFound(new { message = "Failed to generate PDF document" });
                }

                var fileName = $"Net_Due_VAT_Report_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error downloading Net Due VAT report", error = ex.Message });
            }
        }

        [HttpPost("NetDueVATReport/share")]
        public async Task<IActionResult> ShareNetDueVATReport([FromBody] NetDueVATReportShareRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Method) || string.IsNullOrEmpty(request.Receiver))
                {
                    return BadRequest(new { message = "Method and Receiver are required" });
                }

                if (request.Method.ToLower() == "email")
                {
                    // Get PDF bytes
                    var pdfBytes = await _salesService.GetNetDueVATDocument(request.StartDate, request.EndDate, request.Language);
                    
                    if (pdfBytes == null || pdfBytes.Length == 0)
                    {
                        return StatusCode(500, new { message = "Failed to generate PDF for email" });
                    }

                    // Share via email using DocumentShareService
                    await _documentShareService.ShareDocumentAsync(pdfBytes, request.Method, request.Receiver);

                    return Ok(new { isSuccess = true, message = request.Language == "ar" ? "تم إرسال التقرير بنجاح" : "Report sent successfully" });
                }
                else if (request.Method.ToLower() == "whatsapp")
                {
                    // Generate PDF and save to wwwroot
                    var pdfBytes = await _salesService.GetNetDueVATDocument(request.StartDate, request.EndDate, request.Language);
                    
                    if (pdfBytes == null || pdfBytes.Length == 0)
                    {
                        return StatusCode(500, new { message = "Failed to generate PDF for WhatsApp" });
                    }

                    var fileName = $"Net_Due_VAT_Report_{request.StartDate:yyyy-MM-dd}_{request.EndDate:yyyy-MM-dd}_{Guid.NewGuid()}.pdf";
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "reports");
                    
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, fileName);
                    await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

                    // Get base URL from configuration or request
                    var baseUrl = $"{Request.Scheme}://{Request.Host}";
                    var fileUrl = $"{baseUrl}/reports/{fileName}";

                    var message = request.Language == "ar"
                        ? $"تقرير القيمة المضافة المستحقة من {request.StartDate:yyyy-MM-dd} إلى {request.EndDate:yyyy-MM-dd}\n{fileUrl}"
                        : $"Net Due VAT Report from {request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd}\n{fileUrl}";

                    var whatsappUrl = $"https://wa.me/{request.Receiver}?text={Uri.EscapeDataString(message)}";

                    return Ok(new { isSuccess = true, whatsappUrl = whatsappUrl, message = "WhatsApp link generated" });
                }
                else
                {
                    return BadRequest(new { message = "Invalid sharing method. Use 'email' or 'WhatsApp'" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error sharing Net Due VAT report", error = ex.Message });
            }
        }
        #endregion

        #region HTML Email Template Helper
        private string BuildInvoiceEmailBody(string invoiceNo, bool isQuotation, bool isReservation = false)
        {
            var title = isQuotation ? "عرض سعر" : (isReservation ? "فاتورة حجز" : "فاتورة مبيعات");
            var subtitle = isQuotation ? $"Sales Quotation #{invoiceNo}" : (isReservation ? $"Reservation Invoice #{invoiceNo}" : $"Sales Invoice #{invoiceNo}");
            var headerGradient = isQuotation 
                ? "linear-gradient(135deg, #0284c7 0%, #0d9488 100%)" 
                : (isReservation ? "linear-gradient(135deg, #2563eb 0%, #7c3aed 100%)" : "linear-gradient(135deg, #0f172a 0%, #0284c7 100%)");
            var badgeColor = isQuotation ? "#0d9488" : (isReservation ? "#7c3aed" : "#0284c7");

            try
            {
                var basePath = _env?.WebRootPath ?? System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot");
                var templatePath = System.IO.Path.Combine(basePath, "templates", "email", "sale_invoice_template.html");
                if (System.IO.File.Exists(templatePath))
                {
                    var htmlContent = System.IO.File.ReadAllText(templatePath);
                    return htmlContent
                        .Replace("{title}", title)
                        .Replace("{subtitle}", subtitle)
                        .Replace("{headerGradient}", headerGradient)
                        .Replace("{badgeColor}", badgeColor)
                        .Replace("{invoiceNo}", invoiceNo);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to load email template file from disk. Falling back to inline template.");
            }

            // Fallback inline template in case file is missing or unreadable
            return $@"
<!DOCTYPE html>
<html dir=""rtl"" lang=""ar"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{title}</title>
</head>
<body style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f8fafc; margin: 0; padding: 12px; color: #334155;"">
    <div style=""width: 100%; max-width: 660px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; border: 1px solid #e2e8f0; box-shadow: 0 4px 14px rgba(0, 0, 0, 0.05);"">
        <!-- Header -->
        <div style=""background: {headerGradient}; padding: 18px 24px; text-align: center; color: #ffffff;"">
            <h1 style=""margin: 0 0 4px 0; font-size: 20px; font-weight: 800; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; letter-spacing: 1px;"">
                SMART ERP SYSTEM
            </h1>
            <p style=""margin: 0; font-size: 13.5px; font-weight: 600; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; opacity: 0.92; line-height: 1.4;"">
                {title} &nbsp;&bull;&nbsp; <span dir=""ltr"">{invoiceNo}</span>
            </p>
        </div>

        <!-- Clean Body Content -->
        <div style=""padding: 20px 24px; direction: rtl; text-align: right; line-height: 1.6; font-size: 14px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;"">
            <p style=""margin: 0 0 8px 0; font-size: 15px; font-weight: 700; color: #0f172a;"">عزيزي العميل،</p>
            <p style=""margin: 0 0 10px 0; color: #475569;"">
                نشكركم لتعاملكم معنا. تجدون برفقة هذا البريد نسخة رسمية من <strong>{title} رقم ({invoiceNo})</strong> بصيغة PDF.
            </p>
            <p style=""margin: 0; color: #64748b; font-size: 13px;"">
                إذا كان لديكم أي استفسارات أو ملاحظات، يُرجى عدم التردد في التواصل معنا.
            </p>
        </div>

        <!-- Clean Footer -->
        <div style=""background-color: #f8fafc; border-top: 1px solid #f1f5f9; padding: 10px 20px; text-align: center; color: #94a3b8; font-size: 11px;"">
            حقوق النشر &copy; 2026 Smart ERP System. جميع الحقوق محفوظة.
        </div>
    </div>
</body>
</html>";
        }
        #endregion
    }
}
