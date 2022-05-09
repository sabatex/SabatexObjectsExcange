using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiDocumentsExchange.Data;
using WebApiDocumentsExchange.Models;

namespace WebApiDocumentsExchange.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExchangeController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public ExchangeController(ApplicationDbContext context)
    {
        _context = context;
    }



    //[HttpPost]
    //public async Task<IActionResult> PostObject([FromBody] PostObject postObject)
    //{
    //    // find source node
    //    var sender = await _context.ClientNodes.FindAsync(postObject.SenderNode);
    //    if (sender == null)
    //        return NotFound();
    //    // check password
    //    if (sender.ApiKey != postObject.ApiKey)
    //        return BadRequest();
    //    foreach (var dest in postObject.DestinationNode)
    //    {
    //        // find destination
    //        var destination = await _context.ClientNodes.FindAsync(dest);
    //        if (destination == null)
    //        {
    //            continue;
    //            //return BadRequest();
    //        }

    //        var doc = await _context.ObjectExchanges.FindAsync(postObject.ObjectId, postObject.SenderNode, dest);
    //        if (doc == null)
    //        {
    //            doc = new ObjectExchange
    //            {
    //                Id = postObject.ObjectId,
    //                ObjectTypeName = postObject.ObjectType,
    //                ObjectJSON = postObject.ObjectJson,
    //                SenderId = postObject.SenderNode,
    //                DestinationId = dest,
    //                ObjectType = ObjectType.Object
    //            };
    //            await _context.ObjectExchanges.AddAsync(doc);
    //        }
    //        else
    //        {

    //            doc.ObjectJSON = postObject.ObjectJson;
    //            doc.DateStamp = DateTime.Now.ToString();
    //        }
    //        await _context.SaveChangesAsync();
    //     }
    //    return Ok();
    //}
    
    //[HttpGet]
    //public async Task<IActionResult> GetObject(string objectId,string clientId,string Password)
    //{
    //    // find source node
    //    var sender = await _context.ClientNodes.FindAsync(clientId);
    //    if (sender == null)
    //        return NotFound();
    //    // check password
    //    if (sender.ApiKey != Password)
    //        return BadRequest();
    //    // get object
    //    var result = await _context.ObjectExchanges.SingleOrDefaultAsync(s=>s.Id ==objectId && s.DestinationId ==clientId);
    //    if (result == null)
    //        return NotFound();
    //    return Ok(result.ObjectJSON);
    //}


    //[HttpPost("objects")]
    //public async Task<IActionResult> PostObjects(string senderNode,
    //                                              string destinationNode,
    //                                              string password,
    //                                              string objectName,
    //                                              StoreObject[] objects)
    //{
    //    var direction = await CheckDirection(senderNode, destinationNode, password);
    //    if (direction.sender == null || direction.destination == null)
    //        return BadRequest();

    //    foreach (var obj in objects)
    //    {
    //        var doc = new ObjectExchange
    //        {
    //            Id = obj.Id,
    //            ObjectTypeName = objectName,
    //            ObjectJSON = obj.ObjectJSON,
    //            Sender = direction.sender,
    //            Destination = direction.destination,
    //            ObjectType = ObjectType.Object
    //        };
    //        await _context.ObjectExchanges.AddAsync(doc);
    //        try
    //    {
    //        await _context.SaveChangesAsync();
    //    }
    //    catch (DbUpdateException)
    //    {
    //        if (ObjectExchangeExists(doc.Id))
    //        {
    //            return Conflict();
    //        }
    //        else
    //        {
    //            throw;
    //        }
    //    }

    //    }
    //    return Ok();
    //}

    //private async Task<(ClientNode? sender,ClientNode? destination)> CheckDirection(string senderNode,
    //                                      string destinationNode,
    //                                      string password)
    //{
    //    // find source node
    //    var sender = await _context.ClientNodes.FindAsync(senderNode);
    //    if (sender == null)
    //        return (null,null);
    //    // check password
    //    if (sender.ApiKey != password)
    //        return (null, null);

    //    // find destination
    //    var destination = await _context.ClientNodes.FindAsync(destinationNode);
    //    if (destination == null)
    //        return (null, null); 
    //    return (sender, destination);
    //}

    //private bool ObjectExchangeExists(string id)
    //{
    //    return _context.ObjectExchanges.Any(e => e.Id == id);
    //}


}
