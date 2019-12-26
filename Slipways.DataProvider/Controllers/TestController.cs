using Microsoft.AspNetCore.Mvc;

namespace Slipways.DataProvider.Controllers
{
    [Route("api/[controller]")]
    public class TestController : Controller
    {
        // GET: api/values
        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}
