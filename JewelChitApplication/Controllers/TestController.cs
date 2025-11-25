using Microsoft.AspNetCore.Mvc;

namespace JewelChitApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("ping")]
        public string Ping()
        {
            return "pong";
        }
    }
}
