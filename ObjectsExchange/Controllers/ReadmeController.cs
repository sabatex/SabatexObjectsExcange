using Microsoft.AspNetCore.Mvc;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ObjectsExchange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReadmeController : ControllerBase
    {
        const string FileName = "Readme.md";
        // GET: api/<ReadmeController>
        [HttpGet]
        public async Task<string> GetAsync()
        {
            if (System.IO.File.Exists(FileName)) 
            {
                return await System.IO.File.ReadAllTextAsync(FileName);  
            }  
            
            return $"The file {FileName} do not exist !";
        }
    }
}
