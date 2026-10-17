using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.License;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class LicenseController : ControllerBase
{
    [PermissionAuthorize("LicenseManagement.Read")]
    [HttpGet]
    public IActionResult GetLicenseInfo()
    {
        return Ok(new
        {
            Status = "Active",
            LicenseType = "Enterprise",
            ExpiryDate = DateTime.UtcNow.AddYears(1),
            MaxUsers = 100,
            Features = new[] { "FullERP", "POS", "Pharmacy", "Projects" }
        });
    }

    [PermissionAuthorize("LicenseManagement.Update")]
    [HttpPut]
    public IActionResult UpdateLicense([FromBody] object licenseData)
    {
        return Ok(new { success = true, message = "License updated successfully" });
    }
}
