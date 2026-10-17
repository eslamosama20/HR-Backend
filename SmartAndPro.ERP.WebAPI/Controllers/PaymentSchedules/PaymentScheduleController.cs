using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Common.Dtos.PaymentSchedules;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.HotelAllotmentInvoices;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.PaymentScheduleServices;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.PaymentSchedules
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentScheduleController : ControllerBase
    {
        private readonly ILogger<PaymentScheduleController> _logger;
        private readonly PaymentScheduleService _service;

        public PaymentScheduleController(PaymentScheduleService service,
                                         ILogger<PaymentScheduleController> logger)
        {
            _service = service;
            _logger = logger;
        }


        [HttpPost]
        public async Task<IActionResult> AddPayment([FromBody] PaymentSchedulePostDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid payment data.");

            var result = await _service.AddAsync(dto);

            if (result.ErrorCode != ErrorCodes.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpGet("{invoiceId}/{invoiceType}")]
        public async Task<IActionResult> GetPayments(string invoiceId, InvoiceTypeEnum invoiceType)
        {
            var payments = await _service.GetAllPaymentSchedulesByInvoiceIdAsync(invoiceId, invoiceType);
            return Ok(payments);
        }

        /// <summary>
        /// ربط سند صرف بدفعة مجدولة - يُستخدم بعد إنشاء سند الصرف يدوياً
        /// PUT /api/PaymentSchedule/{scheduleId}/link-receipt
        /// </summary>
        [HttpPut("{scheduleId}/link-receipt")]
        public async Task<IActionResult> LinkReceipt(
            Guid scheduleId,
            [FromBody] LinkReceiptDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid data.");

            var result = await _service.LinkReceiptToScheduleAsync(
                scheduleId,
                dto.PaycheckReceiptId,
                dto.ReceiptNo ?? string.Empty);

            if (result.ErrorCode != ErrorCodes.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

    }

}
