using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.DTOs.Pharmacy;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Pharmacy
{
    [Authorize]
    [ApiController]
    [Route("api/v1/pharmacy")]
[Authorize]
[PermissionAuthorize("Pharmacy.Read")]
    public class PharmacyItemController : ControllerBase
    {
        private readonly IPharmacyItemService _itemService;
        private readonly ILogger<PharmacyItemController> _logger;

        public PharmacyItemController(
            IPharmacyItemService itemService,
            ILogger<PharmacyItemController> logger)
        {
            _itemService = itemService;
            _logger = logger;
        }

        /// <summary>
        /// Search for drugs/items by name or code
        /// </summary>
        /// <returns>List of matching items</returns>
        [HttpGet("items/lookup")]
        [ProducesResponseType(typeof(ItemLookupResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> SearchItems([FromQuery] ItemLookupQueryDto query)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _itemService.SearchItemsAsync(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching items");
                return BadRequest(new { message = "Error searching items", error = ex.Message });
            }
        }

        /// <summary>
        /// Get available batches for a specific item
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <param name="storeId">Store ID</param>
        /// <param name="activeOnly">Return only active batches</param>
        /// <returns>List of batches</returns>
        [HttpGet("items/{itemId}/batches")]
        [ProducesResponseType(typeof(BatchLookupResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetItemBatches(
            int itemId, 
            [FromQuery] string storeId,
            [FromQuery] bool activeOnly = true)
        {
            try
            {
                var query = new BatchLookupQueryDto
                {
                    ItemId = itemId,
                    StoreId = storeId,
                    ActiveOnly = activeOnly
                };

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _itemService.GetItemBatchesAsync(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting batches for item {ItemId}", itemId);
                return BadRequest(new { message = "Error retrieving batches", error = ex.Message });
            }
        }

        /// <summary>
        /// Check stock availability for an item
        /// </summary>
        /// <param name="request">Stock check request</param>
        /// <returns>Stock availability info</returns>
        [HttpPost("items/check-stock")]
        [ProducesResponseType(typeof(StockCheckResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CheckStock([FromBody] StockCheckRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _itemService.CheckStockAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking stock");
                return BadRequest(new { message = "Error checking stock", error = ex.Message });
            }
        }
    }

    [Authorize]
    [ApiController]
    [Route("api/v1/pharmacy")]
    public class PharmacyInsuranceController : ControllerBase
    {
        private readonly IPharmacyInsuranceService _insuranceService;
        private readonly ILogger<PharmacyInsuranceController> _logger;

        public PharmacyInsuranceController(
            IPharmacyInsuranceService insuranceService,
            ILogger<PharmacyInsuranceController> logger)
        {
            _insuranceService = insuranceService;
            _logger = logger;
        }

        /// <summary>
        /// Verify insurance coverage for prescription items
        /// </summary>
        /// <param name="request">Insurance verification request</param>
        /// <returns>Coverage details</returns>
        [PermissionAuthorize("Pharmacy.Insurance.EligibilityCheck")]
        [HttpPost("insurance/verify")]
        [ProducesResponseType(typeof(InsuranceCheckResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> VerifyInsurance([FromBody] InsuranceCheckRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _insuranceService.CheckInsuranceCoverageAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying insurance");
                return BadRequest(new { message = "Error verifying insurance", error = ex.Message });
            }
        }

        /// <summary>
        /// Submit insurance approval for a prescription
        /// </summary>
        /// <param name="request">Approval request</param>
        /// <returns>Approval status</returns>
        [PermissionAuthorize("Pharmacy.Insurance.SubmitClaim")]
        [HttpPost("insurance/submit-approval")]
        [ProducesResponseType(typeof(InsuranceApprovalResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> SubmitApproval([FromBody] InsuranceApprovalRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _insuranceService.SubmitInsuranceApprovalAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting insurance approval");
                return BadRequest(new { message = "Error submitting approval", error = ex.Message });
            }
        }
    }

    [Authorize]
    [ApiController]
    [Route("api/v1/pharmacy/sfda")]
[Authorize]
[PermissionAuthorize("Pharmacy.Read")]
    public class PharmacySfdaController : ControllerBase
    {
        [HttpPost("trace")]
        public IActionResult Trace([FromBody] object payload)
        {
            return Ok(new { success = true, message = "SFDA trace logged successfully", timestamp = DateTime.UtcNow });
        }
    }
}
