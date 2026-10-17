using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.SharedKernel.Constants;
using SmartAndPro.ERP.Application.Common.Dtos.Return.Sale;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Services.@base;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Services.Return.Sale;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Domains.Entities.Return.Sale;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;

namespace SmartAndPro.ERP.WebAPI.Controllers.Return
{
    [Route("api/[controller]")]
    [ApiController]
[Authorize]
[PermissionAuthorize("SalesReturn.Read")]
    public class SalesReturnController : ControllerBase
    {
        #region Fields
        private readonly ILogger<SalesReturnController> _logger;
        private readonly ServiceBaseFactory _serviceBaseFactory;
        private readonly SalesReturnService _service;
        private readonly IMapper _mapper;
        private const string inv = "SR-";
        #endregion

        #region Constructors
        public SalesReturnController(ServiceBaseFactory serviceBaseFactory, IMapper mapper, ILogger<SalesReturnController> logger)
        {
            _mapper = mapper;
            _serviceBaseFactory = serviceBaseFactory;
            _service = serviceBaseFactory.GetService<SalesReturnService>();
            _logger = logger;
        }
        #endregion

        #region APIs
        [HttpGet]
        public async Task<IActionResult> GetAllReturns()
        {
            var result = await _service.GetAllAsync();
            if (result != null && result.Any())
            {
                var salesReturnDtos = _mapper.Map<IEnumerable<SalesReturnDto>>(result);
                return Ok(salesReturnDtos);
            }
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReturnById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid salesReturn ID: {Id}", id);
                return BadRequest("Invalid salesReturn ID.");
            }

            var salesReturn = await _service.GetByIdAsync(id);
            if (salesReturn != null)
            {
                var salesReturnDto = _mapper.Map<SalesReturnDto>(salesReturn);
                return Ok(salesReturnDto);
            }

            _logger.LogWarning("Return with ID: {Id} not found", id);
            return NotFound();
        }

        [HttpDelete("{id}")]
        [PermissionAuthorize(Permissions.SalesReturn.Delete)]
        public async Task<IActionResult> DeletesReturn(int id)
        {
            var salesReturn = await _service.GetByIdAsync(id);
            if (salesReturn == null)
            {
                _logger.LogWarning("Return with ID: {Id} not found", id);
                return NotFound();
            }

            var result = await _service.RemoveAsync(salesReturn);
            if (result == 1)
            {
                _logger.LogInformation("Return with ID: {Id} deleted successfully", id);
                return NoContent();
            }

            _logger.LogError("Failed to delete the salesReturn with ID: {Id}", id);
            return BadRequest("Failed to delete the salesReturn.");
        }

        [HttpPost]
        [PermissionAuthorize(Permissions.SalesReturn.Create)]
        public async Task<IActionResult> CreatesReturn([FromBody] SalesReturnPostDto salesReturnDto)
        {
            if (salesReturnDto == null)
            {
                _logger.LogWarning("ReturnDto object sent from client is null.");
                return BadRequest("ReturnDto object is null.");
            }
            if (salesReturnDto.SalesReturnItems == null || salesReturnDto.SalesReturnItems.Count<1)
            {
                _logger.LogWarning("SaleDto object sent from client is null.");
                return BadRequest("SaleDto object is null.");
            }

            var salesReturn = _mapper.Map<SalesReturn>(salesReturnDto);

            var result = await _service.AddAsync(salesReturn);
            if (result == 1)
            {
                _logger.LogInformation("Return created successfully.");
                return CreatedAtAction(nameof(GetReturnById), new { id = salesReturn.Id }, salesReturnDto);
            }

            _logger.LogError("Failed to create the salesReturn.");
            return BadRequest("Failed to create the salesReturn.");
        }

        
        [HttpPost("createTransaction")]
        public async Task<IActionResult> CreateTransaction([FromQuery] int salesReturnId)
        {
            if (salesReturnId == null)
            {
                _logger.LogWarning("Return id sent from client is null.");
                return BadRequest("Return id  is null.");
            }

            var result = await _service.CreateTransaction(salesReturnId);
            if (result == 1)
            {
                _logger.LogInformation("transaction created successfully.");
                return Ok();
            }

            _logger.LogError("Failed to create the transaction.");
            return BadRequest("Failed to create the transaction.");   
        }
        
        [HttpPut("{id}")]
        [PermissionAuthorize(Permissions.SalesReturn.Update)]
        public async Task<IActionResult> UpdatesReturn(int id, [FromBody] SalesReturnPutDto salesReturnDto)
        {
            if (salesReturnDto == null || salesReturnDto.Id != id)
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
            List<SalesReturnItem>? oldItems = existingReturn.SalesReturnItems.ToList();
            existingReturn.SalesReturnItems.Clear();
            foreach (var item in salesReturnDto.SalesReturnItems)
            {
                existingReturn.SalesReturnItems.Add(_mapper.Map<SalesReturnItem>(item));
            }

            var salesReturn = _mapper.Map(salesReturnDto, existingReturn);

            var result = await _service.EditAsync(salesReturn, oldItems);

            if (result == 1)
            {
                _logger.LogInformation("Return updated successfully");
                return NoContent();
            }

            _logger.LogError("Failed to update salesReturn");
            return BadRequest("Failed to update salesReturn");
        }

        [HttpGet("ReturnNo")]
        public async Task<IActionResult> GetReturnNo()
        {
            var newReturnNum = await GetReturnNumber();

            return Ok(new { ReturnNo = newReturnNum });
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
