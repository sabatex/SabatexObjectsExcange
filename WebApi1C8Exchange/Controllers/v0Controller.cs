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
public class v0Controller : BaseController
{
    private readonly ILogger<v0Controller> _logger;
    public v0Controller(ApplicationDbContext context, ILogger<v0Controller> logger, IOptions<ApiConfig> apiConfig) :base(context,apiConfig)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get([FromHeader] string apiToken)
    {
        return Ok(AppVersion);
    }
   
    [HttpPost("login")]
    public async Task<IActionResult> PostLoginAsync([FromForm] string nodeName, [FromForm] string password)
    {
        return Ok(await LoginAsync(nodeName, password));
    }

    #region ObjectTypes
    /// <summary>
    /// Отримуємо перелік підтримуємих типів отримувача або поточного нода 
    /// </summary>
    /// <param name="apiToken"></param>
    /// <param name="nodeName"></param>
    /// <param name="nodeName"></param>
    /// <returns></returns>
    [HttpGet("objecttypes")]
    public async Task<IActionResult> GetObjectTypesAsync([FromHeader] string apiToken,string node)
    {

        var clientNode = await GetSecureNodeAsync(apiToken);

        int nodeId = clientNode;
        if (node != null)
        {
            if (!int.TryParse(node, out nodeId))
            {
                nodeId = await GetNodeAsync(node);
            }
        }
        
        return Ok(await _dbContext.ObjectTypes.Where(s => s.NodeId == nodeId).ToArrayAsync());
    }

    /// <summary>
    /// Отримуємо тип для поточного нода  
    /// </summary>
    /// <param name="apiToken"></param>
    /// <param name="nodeName"></param>
    /// <returns></returns>
    [HttpGet("objecttypes/{typeName}")]
    public async Task<IActionResult> GetObjectTypeAsync([FromHeader] string apiToken, [FromRoute] string typeName)
    {
        var clientNode = await GetSecureNodeAsync(apiToken);
        var result = await _dbContext.ObjectTypes.SingleOrDefaultAsync(s=>
                            s.NodeId==clientNode && s.Name==typeName);

        if (result == null)
            return NotFound();
        return Ok(result);
    }


    [HttpPost("objecttypes")]
    public async Task<IActionResult> PostObjectTypesAsync([FromHeader] string apiToken,[FromForm] string objectTypeName)
    {
        if (objectTypeName == null)
            return BadRequest();
        var clientNode = await GetSecureNodeAsync(apiToken);
        var result = await _dbContext.ObjectTypes.SingleOrDefaultAsync(s => s.Name == objectTypeName && s.NodeId == clientNode);
        if (result == null)
        {
            result = new ObjectType
            {
                NodeId = clientNode,
                Name = objectTypeName
            };
            await _dbContext.ObjectTypes.AddAsync(result);
            await _dbContext.SaveChangesAsync();
            
        }
        return Ok(result);
    }


    #endregion

    #region ObjectExchange
    [HttpGet("objects")]
    public async Task<IActionResult> GetObjectsAsync([FromHeader]string apiToken, int take = 10)
    {
        var clientNode = await GetSecureNodeAsync(apiToken);

        var result = await _dbContext.ObjectExchanges.Where(s => s.DestinationId == clientNode && !s.IsDone)
                                     .OrderBy(d=>d.Priority) // priority 0,1,2..x
                                     .ThenBy(d=>d.DateStamp).Take(take).ToArrayAsync();

        return Ok(result);
    }
    
    [HttpPost("objects")]
    public async Task<IActionResult> PostAsync([FromHeader] string apiToken,
                                               [FromBody] PostObject postObject)
    {
        string objectId = postObject.ObjectId.ToUpper();

        var sender = await GetSecureNodeAsync(apiToken);

        int destinationId = await GetObjectDestinationAsync(postObject);
        int objectTypeId = await GetObjectTypeIdAsync(postObject,sender);
 
        var exist = await _dbContext.ObjectExchanges.Where(s => s.DestinationId == destinationId && s.ObjectTypeId == objectTypeId && s.ObjectId == objectId).OrderByDescending(o=>o.DateStamp).FirstOrDefaultAsync();
        int priority = exist?.Priority + 1 ?? 0;
        
        
        var doc = new ObjectExchange
        {
            ObjectId = objectId,
            ObjectTypeId = objectTypeId,
            ObjectJSON = postObject.ObjectJSON,
            SenderId = sender,
            DestinationId = destinationId,
            Priority = priority
        };
        await _dbContext.ObjectExchanges.AddAsync(doc);
        await _dbContext.SaveChangesAsync();

        return Ok(doc.Id);
    }

    [HttpPost("objects/done/{id:long}")]
    public async Task<IActionResult> PostMarkResivedObjectsAsync([FromHeader] string apiToken,long id)
    {
        var node = await GetSecureNodeAsync(apiToken);
        // find destination
        var obj = await _dbContext.ObjectExchanges.FindAsync(id);
        if (obj == null)
            return NotFound();
        obj.IsDone = true;
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
    [HttpGet("objects/done")]
    public async Task<IActionResult> GetRecivedObjectsAsync([FromHeader] string apiToken,int take=10)
    {
        var node = await GetSecureNodeAsync(apiToken);

        var result = await _dbContext.ObjectExchanges
            .Where(f => f.IsDone && f.SenderId == node)
            .Take(take)
            .Select(s=>s.Id)
            .ToArrayAsync();
        return Ok(result);
    }
    
    [HttpDelete("objects/done/{id:long}")]
    public async Task<IActionResult> DeleteAsync([FromHeader] string apiToken, long id)
    {
        var sender = await GetSecureNodeAsync(apiToken);
        var obj = await _dbContext.ObjectExchanges.FindAsync(id);
        if (obj == null)
            return NotFound();
        if (!obj.IsDone)
            return BadRequest();
        if (obj.SenderId !=sender)
            return BadRequest();
        _dbContext.ObjectExchanges.Remove(obj);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
    [HttpPost("objects/Priority/{id:long}")]
    public async Task<IActionResult> PostPriorityObjectsAsync([FromHeader] string apiToken, long id)
    {
        var node = await GetSecureNodeAsync(apiToken);
        // find destination
        var obj = await _dbContext.ObjectExchanges.FindAsync(id);
        if (obj == null)
            return NotFound();
        obj.Priority = obj.Priority == int.MaxValue? int.MaxValue: obj.Priority+1;
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    #endregion

    #region queries
    [HttpGet("queries")]
    public async Task<IActionResult> GetQueryesAsync([FromHeader] string apiToken, int take = 10)
    {
        var clientNode = await GetSecureNodeAsync(apiToken);

        var result = await _dbContext.QueryObjects
            .Where(s => s.DestinationId == clientNode)
            .Take(take)
            .ToArrayAsync();

        return Ok(result);
    }

    [HttpPost("queries")]
    public async Task<IActionResult> PostQueryAsync([FromHeader] string apiToken,
                                                    [FromBody] QueryedObject queryedObject)
    {
        var sender = await GetSecureNodeAsync(apiToken);

        int destinationId = await GetObjectDestinationAsync(queryedObject);
 
        int objectTypeId = await GetObjectTypeIdAsync(queryedObject,destinationId);
  
        // check exist same query 
        var obj = await _dbContext.QueryObjects.SingleOrDefaultAsync(
            s=>s.DestinationId == destinationId
            && s.SenderId == sender
            && s.ObjectTypeId == objectTypeId
            && s.ObjectId == queryedObject.ObjectId.ToUpper());

        if (obj == null)
        {
            obj = new QueryObject
            {
            DestinationId = destinationId,
            ObjectId = queryedObject.ObjectId.ToUpper(),
            ObjectTypeId = objectTypeId,
            SenderId = sender
            };

            await _dbContext.QueryObjects.AddAsync(obj);
            await _dbContext.SaveChangesAsync();

        }
         return Ok(obj.Id);
    }
    [HttpDelete("queries/{id:long}")]
    public async Task<IActionResult> DeleteQueryAsync([FromHeader] string apiToken,long id)
    {
        var sender = await GetSecureNodeAsync(apiToken);
        var obj = await _dbContext.QueryObjects.FindAsync(id);
        if (obj ==null)
            return NotFound();
        if (obj.DestinationId != sender)
            return BadRequest();

        _dbContext.QueryObjects.Remove(obj);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }


    #endregion

    private async Task<ObjectType?> GetObjectTypeByNameAsync(int nodeId, string name) =>
    await _dbContext.ObjectTypes.SingleOrDefaultAsync(s => s.Name == name && s.NodeId == nodeId);



    private async Task<int> GetObjectDestinationAsync(QueryedObject queryedObject)
    {
        if (int.TryParse(queryedObject.Destination, out int destination))
        {
            return destination; 
        }
        return await GetNodeAsync(queryedObject.Destination); 
    }
    private async Task<int> GetObjectTypeIdAsync(QueryedObject queryedObject,int nodeId)
    {
        if (int.TryParse(queryedObject.ObjectType, out int objectTypeId))
        {
            return objectTypeId;
        }

        var objectType = await GetObjectTypeByNameAsync(nodeId, queryedObject.ObjectType);
        if (objectType == null)
            throw new Exception($"The object type {queryedObject.ObjectType} for node {nodeId} not Exist");
        return objectType.Id;
    }




}
