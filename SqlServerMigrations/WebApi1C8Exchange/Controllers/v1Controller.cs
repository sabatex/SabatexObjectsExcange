using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApiDocumentsExchange.Data;
using WebApiDocumentsExchange.Models;
using WebApiDocumentsExchange.Services;

namespace WebApiDocumentsExchange.Controllers;

[Route("api/[controller]")]
[ApiController]
public class v1Controller : BaseController
{
    public v1Controller(ApplicationDbContext dbContext, IOptions<ApiConfig> apiConfig) : base(dbContext,apiConfig)
    {
    }

    [HttpGet("nodes")]
    public async Task<IActionResult> GetNodesAsync([FromHeader] string apiToken)
    {
        var clientNode = await GetSecureNodeAsync(apiToken);

        var result = await _dbContext.ClientNodes.Select(s => new { Id = s.Id, Name = s.Name, Description = s.Description }).ToArrayAsync();

        return Ok(result);
    }
    #region ObjectExchange
    [HttpPost("objects")]
    public async Task<IActionResult> PostAsync([FromHeader] string apiToken,
                                               [FromHeader] int destinationId,
                                               [FromBody] ShortObjectDescriptorWithBody objectDescriptor)
    {
        var sender = await GetSecureNodeAsync(apiToken);

        var doc = new ObjectExchange
        {
            ObjectId = objectDescriptor.ObjectId,
            ObjectTypeId = objectDescriptor.ObjectTypeId,
            ObjectJSON = objectDescriptor.ObjectJSON,
            SenderId = sender,
            DestinationId = destinationId
        };
        await _dbContext.ObjectExchanges.AddAsync(doc);
        await _dbContext.SaveChangesAsync();

        return Ok(doc);
    }

    #endregion

}
