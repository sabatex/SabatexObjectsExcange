using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApiDocumentsExchange.Controllers.v0;

[Route("api/v0/[controller]")]
[ApiController]
public class StateController : ControllerBase
{
    // GET: api/<StateController>
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }

    // GET api/<StateController>/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
        return "value";
    }

    // POST api/<StateController>
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    // PUT api/<StateController>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/<StateController>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}
