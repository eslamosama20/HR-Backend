using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.RequestForQuotations;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationRoomEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationSupplier;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.RequestForQuotations;
using SmartAndPro.ERP.Application.Services.Reservation;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Utils;

namespace SmartAndPro.ERP.WebAPI.Controllers.RequestForQuotations
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseRequestForQuotationController : ControllerBase
    {
        private readonly ServiceBaseFactory _serviceBaseFactory;
        private readonly RequestForQuotationService _service;
        public PurchaseRequestForQuotationController(ServiceBaseFactory serviceBaseFactory)
        {
            _serviceBaseFactory = serviceBaseFactory;
            _service = _serviceBaseFactory.GetService<RequestForQuotationService>();
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPurchaseRequestForQuotation(Guid id)
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

        [HttpGet]
        public async Task<IActionResult> GetPurchaseRequestForQuotations()
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            IActionResult result = NoContent();
            try
            {
                var responseDate = await _service.GetAllAsync(new RequestForQuotationFilterDto());
                result = responseDate.HandleResult();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePurchaseRequestForQuotation([FromBody] RequestForQuotationPostDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            IActionResult result = NoContent();
            try
            {
                var responseDate = await _service.AddAsync(dto);
                result = responseDate.HandleResult();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
            return result;
           
        }
        [HttpPut("{requestForQuotationId}")]
        public async Task<IActionResult> UpdatePurchaseRequestForQuotation(Guid requestForQuotationId , [FromBody] RequestForQuotationPutDto quotationPutDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            IActionResult result = NoContent();
            try
            {
                quotationPutDto.Id = requestForQuotationId;

                var responseDate = await _service.UpdateAsync(quotationPutDto);
                result = responseDate.HandleResult();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
            return result;
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeletePurchaseRequestForQuotation(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            IActionResult result = NoContent();
            try
            {
                var responseDate = await _service.DeleteAsync(id);
                result = responseDate.HandleResult();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }

            return result;
        }


    }
}
