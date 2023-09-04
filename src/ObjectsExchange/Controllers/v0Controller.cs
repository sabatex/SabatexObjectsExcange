using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web.Resource;
using ObjectsExchange.Data;
using ObjectsExchange.Services;
using Sabatex.ObjectsExchange.Models;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Sabatex.ObjectsExchange.Controllers;

[Route("api/[controller]")]
[ApiController]
public class v0Controller : ControllerBase
{
    private readonly ILogger<v0Controller> _logger;
    private readonly ObjectsExchangeDbContext _dbContext;
    private readonly ApiConfig _apiConfig;
    private readonly ClientManager _clientManager;
    public static int maxTake = 50;
    private const string _tokenType = "BEARER";
    public v0Controller(ObjectsExchangeDbContext dbContext, ILogger<v0Controller> logger, IOptions<ApiConfig> apiConfig, ClientManager clientManager)
    {
        _logger = logger;
        _dbContext = dbContext;
        _apiConfig = apiConfig.Value;
        _clientManager = clientManager;
    }

    private string extractToken(string authorizationToken)
    {
        var r = authorizationToken.Split(' ');
        if (r.Length != 2) return string.Empty;
        if (r[0].ToUpper() != _tokenType) return string.Empty;
        return r[1];
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
    private async Task<ClientNode?> GetClientNodeByTokenAsync(Guid nodeId, string apiToken)
    {
        var clientAutenficate = await _dbContext.AutenficatedNodes.FindAsync(nodeId);
        if (clientAutenficate != null)
        {
            if (apiToken != clientAutenficate.AccessToken)
            {
                _logger.LogTrace($"{DateTime.Now}: The client {nodeId}  try use invalid token {apiToken}");
                await Task.Delay(2000); // no access
                return null;
            }

            var existTime = DateTime.UtcNow;
            if (existTime > clientAutenficate.ExpiresDate)
            {
                _logger.LogTrace($"{DateTime.Now}: The client {nodeId}  use exprise token {apiToken} by time {existTime}");
                await Task.Delay(2000); // no access
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
            await Task.Delay(2000); // no access
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
        try
        {
            return Ok(await _clientManager.LoginAsync(login.ClientId, login.Password));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Login client {login.ClientId} error:{ex.Message}");
            return BadRequest(ex.Message);
        }
    }
    [HttpPost("refresh_token")]
    public async Task<IActionResult> PostRefresTokenAsync(Login login)
    {
        try
        {
            return Ok(await _clientManager.RefreshTokenAsync(login.ClientId, login.Password));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Refresh token fail for client {login.ClientId} error:{ex.Message}");
            return Unauthorized();
        }
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
    /// <param name="destinationId">destination node id (sender) </param>
    /// <param name="take">take objects by query </param>
    /// <returns>incoming objects </returns>
    [HttpGet("objects")]
    public async Task<IActionResult> GetObjectsAsync([FromHeader] string apiToken,
                                                     [FromHeader] Guid clientId,
                                                     [FromHeader] Guid destinationId,
                                                     [FromQuery] int take = 10)
    {
        var clientNode = await GetClientNodeByTokenAsync(clientId, apiToken);
        if (clientNode == null)
            return Unauthorized();

        var result = await _dbContext.ObjectExchanges
                        .Where(s => s.Destination == clientId)
                        .Where(s => s.Sender == destinationId)
                        .OrderBy(d => d.Id) // priority
                        .Take(take).ToArrayAsync();
        return Ok(result);
    }

    [HttpPost("objects")]

    public async Task<IActionResult> PostAsync([FromHeader] string apiToken,
                                               [FromHeader] Guid clientId,
                                               [FromHeader] Guid destinationId,
                                               JsonDocument json)
    {

        string? objectId = json.RootElement.GetProperty("objectId").GetString();
        if (objectId == null)
            return BadRequest("The not defined objectId");
        string? objectType = json.RootElement.GetProperty("objectType").GetString();
        if (objectType == null)
            return BadRequest("The not defined objectType");
        string? text = json.RootElement.GetProperty("text").GetString();
        if (text == null)
            return BadRequest("The not defined text");
        DateTime? dateStamp = null;
        if (json.RootElement.GetProperty("dateStamp").TryGetDateTime(out DateTime tdateStamp))
            dateStamp = tdateStamp;
        var clientNode = await GetClientNodeByTokenAsync(clientId, apiToken);
        if (clientNode == null)
            return Unauthorized();

        var validNodes = clientNode.GetClientAccess();

        if (validNodes.Contains(destinationId))
        {
            var doc = new ObjectExchange
            {
                Sender = clientId,
                Destination = destinationId,
                ObjectId = objectId,
                ObjectType = objectType,
                ObjectAsText = text,
                DateStamp = DateTime.UtcNow,
                SenderDateStamp = dateStamp
            };
            await _dbContext.ObjectExchanges.AddAsync(doc);
            await _dbContext.SaveChangesAsync();
        }
        else
        {
            string error = $"The node {clientId} try send message to invalid node {destinationId}";
            _logger.LogError(error);
            return BadRequest(error);
        }

        return Ok();
    }

    [HttpDelete("objects/{id}")]
    public async Task<IActionResult> DeleteAsync([FromHeader] string apiToken, [FromHeader] Guid clientId, long id)
    {
        var clientNode = await GetClientNodeByTokenAsync(clientId, apiToken);
        if (clientNode == null)
            return Unauthorized();
        var obj = await _dbContext.ObjectExchanges.FindAsync(id);
        if (obj == null)
            return NotFound();
        if (obj.Destination != clientId)
        {
            var error = $"Нод {clientId} не може видалятидані чужих нодів {obj.Destination}";
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
    public async Task<IActionResult> GetQueryesAsync([FromHeader] string? apiToken,
                                                     [FromHeader] Guid clientId,
                                                     [FromHeader] Guid destinationId,
                                                     [FromQuery] int take = 10)
    {
        var clientNode = await GetClientNodeByTokenAsync(clientId, apiToken);
        if (clientNode == null)
            return Unauthorized();
        var result = await _dbContext.QueryObjects
                .Where(s => s.Destination == clientId)
                .Where(s => s.Sender == destinationId)
                .OrderBy(d => d.Id) // priority
                .Take(take)
                .ToArrayAsync();
        return Ok(result);
    }

    [HttpPost("queries")]
    public async Task<IActionResult> PostQueryAsync([FromHeader] string apiToken,
                                                    [FromHeader] Guid clientId,
                                                    [FromHeader] Guid destinationId,
                                                    JsonDocument json)
    {
        string? objectId = json.RootElement.GetProperty("objectId").GetString();
        if (objectId == null)
            return BadRequest("The not defined objectId");
        string? objectType = json.RootElement.GetProperty("objectType").GetString();
        if (objectType == null)
            return BadRequest("The not defined objectType");

        var clientNode = await GetClientNodeByTokenAsync(clientId, apiToken);
        if (clientNode == null)
            return Unauthorized();

        var validNodes = clientNode.GetClientAccess();
        // check exist same query 
        var obj = new QueryObject
        {
            Sender = clientId,
            Destination = destinationId,
            ObjectId = objectId,
            ObjectType = objectType
        };
        await _dbContext.QueryObjects.AddAsync(obj);

        await _dbContext.SaveChangesAsync();
        return Ok();
    }



    [HttpDelete("queries/{id:long}")]
    public async Task<IActionResult> DeleteQueryAsync([FromHeader] string apiToken, [FromHeader] Guid clientId, long id)
    {
        var clientNode = await GetClientNodeByTokenAsync(clientId, apiToken);
        if (clientNode == null)
            return Unauthorized();

        var obj = await _dbContext.QueryObjects.FindAsync(id);
        if (obj == null)
            return NotFound();
        if (obj.Destination != clientId)
        {
            var error = $"Нод {clientNode.Name} з id={clientId} не може видаляти дані чужих нодів {obj.Destination}";
            _logger.LogError($"{DateTime.Now}: {error}");
            return Conflict(error);

        }
        _dbContext.QueryObjects.Remove(obj);

        await _dbContext.SaveChangesAsync();
        return Ok();

    }


    #endregion

}
