using Microsoft.AspNetCore.Mvc;

namespace SmartAndPro.ERP.WebAPI.Controllers
{
    [Route("api/inventory/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet("live-check")]
        public async Task<IActionResult> Get()
        {

            return Ok("right");
        }

    }
}
