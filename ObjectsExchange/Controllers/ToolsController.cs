using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ObjectsExchange.Data;

namespace ObjectsExchange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToolsController : ControllerBase
    {
        [HttpGet("DataBaseBackup")]
        [Authorize(Roles ="Administrator")]
        public string GetDataBaseBackup()
        {
            return "new DataBaseBackup()";
        }
    }
}
