using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Invoice
{
    [Route("api/invoice/[controller]")]
    [ApiController]
    [Authorize]
    [PermissionAuthorize("Account.Read")]
    public class CostCenterController : ControllerBase
    {
    }
}
