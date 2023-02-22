using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using sabatex.ObjectsExchange.Models;
using Sabatex.ObjectsExchange.Server.Data;
using Sabatex.ObjectsExchange.Server.Services;
using Sabatex.ObjectsExchange.Services;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Sabatex.ApiObjectsExchange.Controllers;

[Route("api/[controller]")]
[ApiController]
public class v0Controller : ControllerBase
{
    private readonly ILogger<v0Controller> _logger;
    protected readonly ObjectsExchangeDbContext _dbContext;
    protected readonly ApiConfig _apiConfig;
    private readonly ClientManager _clientManager;
    public static int maxTake = 50;
    readonly byte[] hashKey;
    
    public v0Controller(ObjectsExchangeDbContext dbContext, ILogger<v0Controller> logger, IOptions<ApiConfig> apiConfig,ClientManager clientManager)
    {
        _logger = logger;
        _dbContext = dbContext;
        _apiConfig = apiConfig.Value;
        _clientManager = clientManager;
        hashKey = Encoding.UTF8.GetBytes("ajcewewi%^(F#|}9327nx=-23hdxsa5vcx<>_ d");
    }
    
    /// <summary>
    /// Random GUID access token generate
    /// </summary>
    /// <param name="nodeName"></param>
    /// <returns></returns>
    private string CreateAccessToken()
    {
        var r = new Random();
        byte[] data = new byte[16];
        r.NextBytes(data);
        return new Guid(data).ToString();
    }
    private async Task<ClientNode?> GetClientNodeByTokenAsync(Guid nodeId,string apiToken)
    {
        var clientAutenficate = await _dbContext.AutenficatedNodes.FindAsync(nodeId);
        if (clientAutenficate != null)
        {
            if (apiToken == clientAutenficate.Token)
            {
                _logger.LogTrace($"{DateTime.Now}: The client {nodeId}  try use invalid token {apiToken}");
                return null;
            }
            
            var existTime = DateTime.UtcNow - clientAutenficate.DateStamp;
            if (existTime > _apiConfig.TokensLifeTimeMinutes)
            {
                _logger.LogTrace($"{DateTime.Now}: The client {nodeId}  use exprise token {apiToken} by time {existTime}");
                return null;
            }
            var result = await _dbContext.ClientNodes.FindAsync(nodeId);
            if (result == null)
                _logger.LogError($"{DateTime.Now}: The client {nodeId} do not Exist, but exist valid token by same id ");
            return result;
        }
        else
        {

            var ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            _logger.LogTrace($"{DateTime.Now}: Try use service from ip={ip} with nodeId={nodeId} and The client {nodeId} and token = {apiToken}");
            return null;
        }
    }
    /// <summary>
    /// Autenficate in service
    /// </summary>
    /// <param name="login">The object Login with node Id (register unsensitive) and password</param>
    /// <returns>string access token or empty for fail</returns>
    /// <exception cref="Exception"></exception>
    [HttpPost("login")]
    public async Task<IActionResult> PostLoginAsync(Login login)
    {
        var clientNode = await _dbContext.ClientNodes.FindAsync(login.NodeId);
        if (clientNode != null)
        {
            var hmac = new HMACSHA256(hashKey);
            var passwordBase64 =  Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(login.Password)));

            if (clientNode.Password == passwordBase64)
            {
                // remove old autenficated token
                var result = await _dbContext.AutenficatedNodes.SingleOrDefaultAsync(s => s.Id == login.NodeId);
                if (result != null)
                {
                    _dbContext.AutenficatedNodes.Remove(result);
                }
                result = new AutenficatedNode 
                {
                    Id = clientNode.Id,
                    DateStamp = DateTime.UtcNow,
                    Token = CreateAccessToken()
                };
                await _dbContext.AutenficatedNodes.AddAsync(result);
                await _dbContext.SaveChangesAsync();
                return Ok(result.Token);
            }
        }
        return Ok(string.Empty);
    }

    /// <summary>
    /// Get current API version
    /// </summary>
    /// <returns>string API version or empty</returns>
    [HttpGet("version")]
    public IActionResult Get()
    {
        return Ok(Assembly.GetExecutingAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty);
    }

    #region ObjectExchange
    /// <summary>
    /// Get incoming objects
    /// </summary>
    /// <param name="apiToken">access token</param>
    /// <param name="nodeId">client node Id</param>
    /// <param name="incomingNodesId">list separated ',' incoming nodes for query </param>
    /// <param name="take">take objects by query </param>
    /// <returns>incoming objects </returns>
    [HttpGet("objects")]
    public async Task<IActionResult> GetObjectsAsync([FromHeader] string apiToken,
                                                     [FromHeader] Guid nodeId,
                                                     [FromQuery]string incomingNodesId,
                                                     [FromQuery]int take = 10)
    {
        var clientNode = await GetClientNodeByTokenAsync(nodeId, apiToken);
        if (clientNode == null)
            return Unauthorized();
        IEnumerable<Guid> nodes;
        try
        {
            nodes = incomingNodesId.Split(',').Select(s=>new Guid(s));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        var result = await _dbContext.ObjectExchanges
                        .Where(s => s.Destination==nodeId &&  nodes.Contains(s.Sender))
                        .OrderBy(d => d.DateStamp) // priority
                        .Take(take).ToArrayAsync();
        return Ok(result);
    }
    
    [HttpPost("objects")]
    
    public async Task<IActionResult> PostAsync([FromHeader] string apiToken,
                                               [FromHeader] Guid nodeId,
                                               [FromBody] PostObject postObject)
    {
        var clientNode = await GetClientNodeByTokenAsync (nodeId, apiToken);
        if (clientNode == null)
            return Unauthorized();
 
        var validNodes = clientNode.GetClientAccess();
        if (validNodes.Contains(postObject.Destination))
        {
            var doc = new ObjectExchange
            {
                Sender = nodeId,
                Destination = postObject.Destination,
                ObjectId = postObject.ObjectId,
                ObjectType = postObject.ObjectType,
                ObjectAsText = postObject.ObjectAsText,
                DateStamp = postObject.DateStamp
            };
            await _dbContext.ObjectExchanges.AddAsync(doc);
        }
        else
        {
            _logger.LogError($"The node {nodeId} try send message to invalid node {postObject.Destination}");
        }
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("objects/{id}")]
    public async Task<IActionResult> DeleteAsync([FromHeader] string apiToken, [FromHeader] Guid nodeId, long id)
    {
        var clientNode = await GetClientNodeByTokenAsync(nodeId, apiToken);
        if (clientNode == null)
            return Unauthorized();
        var obj = await _dbContext.ObjectExchanges.FindAsync(id);
        if (obj == null)
            return NotFound();
        if (obj.Destination != nodeId)
        {
            var error = $"Нод {nodeId} не може видалятидані чужих нодів {obj.Destination}";
            _logger.LogError(error);
            return BadRequest(error);
        }
        _dbContext.ObjectExchanges.Remove(obj);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
   
 
    #endregion

    #region queries
    /// <summary>
    /// Get 
    /// </summary>
    /// <param name="apiToken"></param>
    /// <param name="nodeName"></param>
    /// <param name="take"></param>
    /// <returns></returns>
    [HttpGet("queries")]
    public async Task<IActionResult> GetQueryesAsync([FromHeader] string apiToken,
                                                     [FromHeader] Guid nodeId,
                                                     [FromQuery] int take = 10)
    {
        var clientNode = await GetClientNodeByTokenAsync(nodeId, apiToken);
        if (clientNode == null)
            return Unauthorized();
        var result = await _dbContext.QueryObjects
                .Where(s => s.Destination == nodeId)
                .Take(take)
                .ToArrayAsync();
            return Ok(result);
    }

    [HttpPost("queries")]
    public async Task<IActionResult> PostQueryAsync([FromHeader] string apiToken,
                                                    [FromHeader] Guid nodeId,
                                                    [FromBody] PostObject postObject)
    {
        var clientNode = await GetClientNodeByTokenAsync(nodeId, apiToken);
        if (clientNode == null)
            return Unauthorized();
        
        var validNodes = clientNode.GetClientAccess();
           // check exist same query 
            var obj = new QueryObject
            {
                Sender = nodeId,
                Destination = postObject.Destination,
                ObjectId = postObject.ObjectId,
                ObjectType = postObject.ObjectType
            };
            await _dbContext.QueryObjects.AddAsync(obj);

        await _dbContext.SaveChangesAsync();
        return Ok();
    }



    [HttpDelete("queries/{id}")]
    public async Task<IActionResult> DeleteQueryAsync([FromHeader] string apiToken, [FromHeader] Guid nodeId, long id)
    {
        var clientNode = await GetClientNodeByTokenAsync(nodeId, apiToken);
        if (clientNode == null)
            return Unauthorized();

             var obj = await _dbContext.QueryObjects.FindAsync(id);
            if (obj ==null)
                return NotFound();
            if (obj.Destination !=nodeId)
        {
            var error = $"Нод {clientNode.Name} з id={nodeId} не може видалятидані чужих нодів {obj.Destination}";
            _logger.LogError($"{DateTime.Now}: {error}");
            return Conflict(error);

        }
             _dbContext.QueryObjects.Remove(obj);

        await _dbContext.SaveChangesAsync();
        return Ok();
 
    }


    #endregion


    #region client manager
#if DEBUG
    [HttpPost("client")]
    public async Task<IActionResult> AddClientAsync(string clientName,
                                                    string description,
                                                    string? accessNodes,
                                                    string password)
    {
        var clientNode = await _clientManager.CreateClientAsync(clientName, password, description, accessNodes);
        if (clientNode == null)
            return BadRequest();
        else
            return Ok();
    }
    #endif
    #endregion

}
