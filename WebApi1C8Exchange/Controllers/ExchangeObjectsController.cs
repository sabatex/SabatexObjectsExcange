using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi1C8Exchange.Data;
using WebApi1C8Exchange.Models;

namespace WebApi1C8Exchange.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExchangeObjectsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ExchangeObjectsController> _logger;
    public ExchangeObjectsController(ApplicationDbContext context, ILogger<ExchangeObjectsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(string senderNode,
                                               string destinationNode,
                                               string apiKey,
                                               StoreObject[] objects)
    {
        try
        {
            var direction = await _context.GetSendNodesAsync(senderNode, destinationNode, apiKey);
            foreach (var obj in objects)
            {
                var doc = new ObjectExchange
                {
                    Id = obj.Id,
                    ObjectName = obj.ObjectName,
                    ObjectJSON = obj.ObjectJSON,
                    Sender = direction.sender,
                    Destination = direction.destination,
                    ObjectType = ObjectType.Object
                };
                await _context.ObjectExchanges.AddAsync(doc);
            }
 
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync(string node,
                                              string apiKey,
                                              ObjectType exchangeStatus=ObjectType.Object)
    {
        var clientNode = await _context.GetSecureNodeAsync(node, apiKey);
        
        var result = await _context.ObjectExchanges.Where(s=>s.Destination == clientNode && s.ObjectType == exchangeStatus).Take(50).ToArrayAsync();
            
        return Ok(result);
    }


}
