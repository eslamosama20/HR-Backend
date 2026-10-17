using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Purchase;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale;
using SmartAndPro.ERP.Application.Common.Dtos.PurchaseQuotations;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Invoice.Purchase;
using SmartAndPro.ERP.Application.Services.PurchaseQuotations;
using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.Domains.Entities.PurchaseQuotations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Utils;

namespace SmartAndPro.ERP.WebAPI.Controllers.PurchaseQuotations
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseQuotationController : ControllerBase
    {
        #region Fields
        private readonly ILogger<PurchaseQuotationController> _logger;
        private readonly IPurchaseQuotationService _service;
        private readonly IMapper _mapper;
        private const string inv = "Q-";
        #endregion

        #region Constructors
        public PurchaseQuotationController(
            IPurchaseQuotationService service,
            IMapper mapper,
            ILogger<PurchaseQuotationController> logger)
        {
            _mapper = mapper;
            _service = service;
            _logger = logger;
        }
        #endregion

        #region APIs

        [HttpGet("frontal")]
        public async Task<IActionResult> GetAllFrontalInvoices([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = await _service.GetAllFrontalAsync(pageNumber, pageSize);

                return response.HandleResult();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, new { message = ex.Message });
            }
        }



        [HttpPost("frontal/{id:int}")]
        public async Task<IActionResult> MakeFrontalAsync(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            IActionResult result = NoContent();
            try
            {
                var responseDate = await _service.MakeFrontalAsync(id);
                if (responseDate != null)
                {
                    var invoiceDto = _mapper.Map<PurchaseQuotationDto>(responseDate);
                    return Ok(invoiceDto);
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
            return result;
        }

        [HttpPost("{id}/convert")]
        public async Task<IActionResult> ConvertToInvoice(int id, [FromQuery] Guid storeDetailsId)
        {
            if (id <= 0) return BadRequest("Invalid ID");
            if (storeDetailsId == Guid.Empty) return BadRequest("StoreDetailsId is required");

            var result = await _service.ConvertToInvoice(id, storeDetailsId);
            return result.HandleResult();
        }

        [HttpGet("item-price/average/{mainPurchaseClassId}")]
        public async Task<IActionResult> GetAverageItemPrice(Guid mainPurchaseClassId, [FromQuery] int days = 30)
        {
             var price = await _service.GetAverageItemPriceAsync(mainPurchaseClassId, days);
             return Ok(new { Price = price });
        }

        [HttpGet("item-price/last/{mainPurchaseClassId}")]
        public async Task<IActionResult> GetLastItemPrice(Guid mainPurchaseClassId)
        {
             var price = await _service.GetLastItemPriceAsync(mainPurchaseClassId);
             return Ok(new { Price = price });
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoiceById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid invoice ID: {Id}", id);
                return BadRequest("Invalid invoice ID.");
            }

            var invoice = await _service.GetByIdAsync(id);
            if (invoice != null)
            {
                var invoiceDto = _mapper.Map<PurchaseQuotationDto>(invoice);
                return Ok(invoiceDto);
            }

            _logger.LogWarning("Invoice with ID: {Id} not found", id);
            return NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            var invoice = await _service.GetByIdAsync(id);
            if (invoice == null)
            {
                _logger.LogWarning("Invoice with ID: {Id} not found", id);
                return NotFound();
            }

            var result = await _service.RemoveAsync(invoice);
            if (result == 1)
            {
                _logger.LogInformation("Invoice with ID: {Id} deleted successfully", id);
                return NoContent();
            }

            _logger.LogError("Failed to delete the invoice with ID: {Id}", id);
            return BadRequest("Failed to delete the invoice.");
        }

        //[HttpPost]
        //public async Task<IActionResult> CreateInvoice([FromBody] PurchaseQuotationPostDto purchaseQuotationPostDto)
        //{
        //    if (purchaseQuotationPostDto == null)
        //    {
        //        _logger.LogWarning("InvoiceDto object sent from client is null.");
        //        return BadRequest("InvoiceDto object is null.");
        //    }
        //    if (purchaseQuotationPostDto.PurchaseItems == null || purchaseQuotationPostDto.PurchaseItems.Count < 1)
        //    {
        //        _logger.LogWarning("PurchaseQuotationItems object sent from client is null.");
        //        return BadRequest("PurchaseQuotationItems object is null.");
        //    }
        //    var purchaseQuotation = _mapper.Map<PurchaseQuotation>(purchaseQuotationPostDto);

        //    var result = await _service.AddAsync(purchaseQuotation);
        //    if (result == 1)
        //    {
        //        _logger.LogInformation("Invoice created successfully.");
        //        return CreatedAtAction(nameof(GetInvoiceById), new { id = purchaseQuotation.Id }, purchaseQuotationPostDto);
        //    }

        //    _logger.LogError("Failed to create the invoice.");
        //    return BadRequest("Failed to create the invoice.");
        //}

        [HttpPost]
        public async Task<IActionResult> CreateInvoice([FromBody] PurchaseQuotationPostDto dto)
        {
            // 1️⃣ سجل أي محاولة POST
            LogService.LogInformation("CreateInvoice called", parameters: new object[] { dto });

            // 2️⃣ Validation بسيطة
            if (dto == null)
            {
                LogService.LogError("InvoiceDto object sent from client is null");
                return BadRequest("InvoiceDto object is null.");
            }

            if (dto.PurchaseQuotationItems == null || !dto.PurchaseQuotationItems.Any())
            {
                LogService.LogError("PurchaseItems object sent from client is null or empty");
                return BadRequest("PurchaseQuotationItems object is null.");
            }

            try
            {
                // 3️⃣ Map DTO للـ Entity
                var entity = _mapper.Map<PurchaseQuotation>(dto);

                // 4️⃣ Call Service
                var result = await _service.AddAsync(entity);

                // 5️⃣ Check result
                if (result == 1)
                {
                    LogService.LogInformation("Invoice created successfully", parameters: new object[] { entity.Id });
                    return CreatedAtAction(nameof(GetInvoiceById), new { id = entity.Id }, dto);
                }
                else
                {
                    LogService.LogError("Failed to create invoice in service");
                    return BadRequest("Failed to create invoice");
                }
            }
            catch (Exception ex)
            {
                // 6️⃣ أي Exception هيتسجل في logs.json
                LogService.LogError(ex);
                return StatusCode(500, "Internal Server Error");
            }
        }


        [HttpGet("InvoiceNo")]
        public async Task<IActionResult> GetInvoiceNo()
        {
            var newInvoiceNum = await GetInvoiceNumber();

            return Ok(new { InvoiceNo = newInvoiceNum });
        }

        //[HttpPost("Report/Details")]
        //public ActionResult GetPurchaseInvoicesDetailsReport(GetPurchaseInvoiceDetailsRequest invoiceDetailsRequest)
        //{
        //    try
        //    {

        //        return Ok(_purchaseInvoiceService.GetPurchaseInvoiceDetails(invoiceDetailsRequest));
        //    }
        //    catch (Exception ex)
        //    {
        //        LogService.LogError(ex);
        //        return BadRequest();
        //    }
        //}

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvoice(int id, [FromBody] PurchaseQuotationPutDto dto)
        {
            if (dto == null || dto.Id != id)
                return BadRequest("Invalid Data or ID mismatch");

            try
            {
                var result = await _service.UpdateAsync(id, dto);
                return result.HandleResult();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, ex.Message);
            }
        }

        #endregion

        #region Methods
        private async Task<string> GetInvoiceNumber()
        {
            var lastInvoiceNo = await _service.GetLastInvoiceNo();
            return GenerateNewInvoiceNo(lastInvoiceNo);
        }

        private string GenerateNewInvoiceNo(string lastInvoiceNo)
        {
            if (string.IsNullOrEmpty(lastInvoiceNo)) return inv + 1;

            var digits = new string(lastInvoiceNo.Where(char.IsDigit).ToArray());
            int.TryParse(digits, out int oldNum);

            return inv + (oldNum + 1);
        }
        #endregion
    }
}
