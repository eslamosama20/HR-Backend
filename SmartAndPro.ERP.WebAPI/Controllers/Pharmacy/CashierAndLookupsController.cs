using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.DTOs.Pharmacy;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Pharmacy
{
    // NOTE: Ready-prescriptions endpoint is handled by CashierPosController in PharmacyInventoryController.cs
    //       [Route("api/v1/cashier")] GET ready-prescriptions → IPharmacyDispenseService

    /// <summary>
    /// Lookup endpoints for stores and insurance companies
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/v1/pharmacy/lookups")]
[Authorize]
[PermissionAuthorize("Pharmacy.Read")]
    public class PharmacyLookupController : ControllerBase
    {
        private readonly IPharmacyLookupService _lookupService;
        private readonly ILogger<PharmacyLookupController> _logger;

        public PharmacyLookupController(
            IPharmacyLookupService lookupService,
            ILogger<PharmacyLookupController> logger)
        {
            _lookupService = lookupService;
            _logger = logger;
        }

        /// <summary>
        /// Get stores by branch
        /// </summary>
        /// <returns>List of stores</returns>
        [HttpGet("stores")]
        [ProducesResponseType(typeof(List<StoreLookupDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetStores([FromQuery] StoresQueryDto query)
        {
            try
            {
                var result = await _lookupService.GetStoresAsync(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stores");
                return BadRequest(new { message = "Error retrieving stores", error = ex.Message });
            }
        }

        /// <summary>
        /// Search insurance companies
        /// </summary>
        /// <returns>List of insurance companies</returns>
        [HttpGet("insurance-companies")]
        [ProducesResponseType(typeof(List<InsuranceCompanyLookupDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> SearchInsuranceCompanies([FromQuery] InsuranceCompaniesQueryDto query)
        {
            try
            {
                var result = await _lookupService.SearchInsuranceCompaniesAsync(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching insurance companies");
                return BadRequest(new { message = "Error searching insurance companies", error = ex.Message });
            }
        }
    }
}
