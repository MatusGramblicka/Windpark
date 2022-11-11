using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WindparkAPIAggregation.Interface;

namespace WindparkAPIAggregation.Controllers
{
    [Route("windpark")]
    [ApiController]
    public class WindParkController : ControllerBase
    {
        private readonly IWindparkApiAggregator _windparkApiAggregator;

        public WindParkController(IWindparkApiAggregator windparkApiAggregator)
        {
            _windparkApiAggregator = windparkApiAggregator;
        }

        [HttpGet]
        public async Task<ActionResult> GetData()
        {
            await _windparkApiAggregator.SendDataToRabbitMq();

            return Ok();
        }
    }
}
