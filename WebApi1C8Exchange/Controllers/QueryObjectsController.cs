using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi1C8Exchange.Data;
using WebApi1C8Exchange.Models;

namespace WebApi1C8Exchange.Controllers;

[Route("api/[controller]")]
[ApiController]
public class QueryObjectsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<QueryObjectsController> _logger;
    public QueryObjectsController(ApplicationDbContext context, ILogger<QueryObjectsController> logger)
    {
        _context = context;
        _logger = logger;
    }
    [HttpPost]
    public async Task<IActionResult> PostAsync(string senderNode,
                                               string destinationNode,
                                               string apiKey,
                                               ObjectQuery[] queryObject)
    {
            var direction = await _context.GetSendNodesAsync(senderNode, destinationNode, apiKey);
            foreach (var item in queryObject)
            {
                var objectType = await _context.GetClientObjectTypeAsync(direction.destination, item.ObjectType);
                var record = new QueryObject { ObjectId = item.ObjectId, ObjectType = objectType,Destination=direction.destination,Sender=direction.sender }; 
                await _context.AddAsync(record);
            }
            await _context.SaveChangesAsync();
 

        return Ok();
    }


}
