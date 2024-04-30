using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sabatex.ObjectsExchange.Controllers;

namespace ObjectsExchange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected readonly ILogger _logger;
        public BaseApiController(ILogger logger)
        {
            _logger = logger; 
        }
    }
}
