using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using WebApiDocumentsExchange.Data;
using WebApiDocumentsExchange.Models;
using WebApiDocumentsExchange.Services;

namespace WebApiDocumentsExchange.Controllers;

[Route("api/[controller]")]
[ApiController]
public class v0Controller : ControllerBase
{
    private readonly ILogger<v0Controller> _logger;
    protected readonly ApplicationDbContext _dbContext;
    protected readonly ApiConfig _apiConfig;

    public v0Controller(ApplicationDbContext dbContext, ILogger<v0Controller> logger, IOptions<ApiConfig> apiConfig)
    {
        _logger = logger;
        _dbContext = dbContext;
        _apiConfig = apiConfig.Value;

    }
    /// <summary>
    /// Get current API version
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(Assembly.GetExecutingAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty);
    }
   
    [HttpPost("login")]
    public async Task<IActionResult> PostLoginAsync([FromForm] string nodeName, [FromForm] string password)
    {
        return Ok(await LoginAsync(nodeName, password));
    }

    #region ObjectExchange
    [HttpGet("objects")]
    public async Task<IActionResult> GetObjectsAsync([FromHeader]string apiToken, string nodeName, int take = 10)
    {
        var clientNode = await GetSecureNodeAsync(apiToken);
        var destination = await GetNodeAsync(nodeName);

        var result = await _dbContext.ObjectExchanges.Where(s => s.Destination == clientNode && s.Sender == destination)
                                     .OrderBy(d=>d.DateStamp) // priority
                                     .Take(take).ToArrayAsync();
        return Ok(result);
    }
    
    [HttpPost("objects")]
    public async Task<IActionResult> PostAsync([FromHeader] string apiToken,
                                               [FromBody] PostObject postObject)
    {
        string objectId = postObject.ObjectId.ToUpper();

        var sender = await GetSecureNodeAsync(apiToken);

        string destination = await GetNodeAsync(postObject.Destination);
 
        var doc = new ObjectExchange
        {
            ObjectId = objectId.ToLower(),
            ObjectType = postObject.ObjectType.ToLower(),
            ObjectJSON = postObject.ObjectJSON,
            Sender = sender,
            Destination = destination,
            DateStamp = postObject.DateStamp
        };
        await _dbContext.ObjectExchanges.AddAsync(doc);
        await _dbContext.SaveChangesAsync();

        return Ok(doc.Id);
    }

    [HttpDelete("objects/{id:long}")]
    public async Task<IActionResult> DeleteAsync([FromHeader] string apiToken, long id)
    {
        var node = await GetSecureNodeAsync(apiToken);
        var obj = await _dbContext.ObjectExchanges.FindAsync(id);
        if (obj == null)
            return NotFound();
        if (obj.Destination !=node)
            return BadRequest();
        _dbContext.ObjectExchanges.Remove(obj);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
   
 
    #endregion

    #region queries
    [HttpGet("queries")]
    public async Task<IActionResult> GetQueryesAsync([FromHeader] string apiToken, string nodeName, int? take)
    {
        var clientNode = await GetSecureNodeAsync(apiToken);
        var sender = await GetNodeAsync(nodeName);
        
        var result = await _dbContext.QueryObjects
            .Where(s => s.Destination == clientNode && s.Sender == sender)
            .Take(take ?? 10)
            .ToArrayAsync();

        return Ok(result);
    }

    [HttpPost("queries")]
    public async Task<IActionResult> PostQueryAsync([FromHeader] string apiToken,
                                                    [FromBody] QueryedObject queryedObject)
    {
        var sender = await GetSecureNodeAsync(apiToken);

        string destination = await GetNodeAsync(queryedObject.Destination);
 
  
        // check exist same query 
        var obj = await _dbContext.QueryObjects.SingleOrDefaultAsync(
            s=>s.Destination == destination
            && s.Sender == sender
            && s.ObjectType == queryedObject.ObjectType.ToLower()
            && s.ObjectId == queryedObject.ObjectId.ToLower());

        if (obj == null)
        {
            obj = new QueryObject
            {
                Destination = destination,
                ObjectId = queryedObject.ObjectId.ToLower(),
                ObjectType = queryedObject.ObjectType.ToLower(),
                Sender = sender
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
        if (obj.Destination != sender)
            return BadRequest();

        _dbContext.QueryObjects.Remove(obj);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }


    #endregion

 
    private async Task<string> GetNodeAsync([NotNull] string node)
    {
        var result = await _dbContext.ClientNodes.FindAsync(node.ToLower());
        if (result == null)
            throw new ArgumentException("Node {0} not exist!", node);

        return result.Id;
    }
    private async Task<string> GetSecureNodeAsync(string apiToken)
    {
        var clientAutenficate = await _dbContext.AutenficatedNodes.FindAsync(apiToken);
        if (clientAutenficate != null)
        {
            if ((DateTime.Now - clientAutenficate.DateStamp) < _apiConfig.TokensLifeTimeMinutes)
            {
                var result = await _dbContext.ClientNodes.FindAsync(clientAutenficate.Node);
                if (result != null)
                {
                    return result.Id;
                }
            }

        }
        throw new Exception("Access denied!!!");
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="_context"></param>
    /// <param name="node"></param>
    /// <param name="password"></param>
    /// <returns>Api token</returns>
    /// <exception cref="Exception"></exception>
    private async Task<string> LoginAsync(string node, string password)
    {
        var clientNode = await _dbContext.ClientNodes.FindAsync(node.ToLower());
        if (clientNode != null)
        {
            if (clientNode.Password == password)
            {
                var result = await _dbContext.AutenficatedNodes.SingleOrDefaultAsync(s => s.Node == clientNode.Id);
                if (result != null)
                {
                    _dbContext.AutenficatedNodes.Remove(result);
                }
                result = new AutenficatedNode { Node = clientNode.Id, DateStamp = DateTime.Now, Id = Guid.NewGuid().ToString() };
                await _dbContext.AutenficatedNodes.AddAsync(result);
                await _dbContext.SaveChangesAsync();
                return result.Id.ToString();
            }
        }
        throw new Exception("The login or password incorect!");
    }



}
