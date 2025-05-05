using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ObjectsExchange.Controllers;
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
public class v0Controller :BaseApiController
{
    public static int maxTake = 50;
    public const int MessageSizeLimit = 50000;
    private const string _tokenType = "BEARER";
    public v0Controller(ObjectsExchangeDbContext dbContext, ILogger<v0Controller> logger, IOptions<ApiConfig> apiConfig): base(logger, dbContext, apiConfig)
    {
    }

    public override async Task<IActionResult> PostRefreshTokenAsync(JsonDocument json)
    {
        string? refresh_token = json.RootElement.GetProperty("password").GetString();
        if (refresh_token == null)
        {
            _logger.LogError("Refresh token failed: refresh_token is null");
            return Unauthorized();
        }
        return await base.PostRefreshTokenAsync(JsonDocument.Parse("{\"refresh_token\":\"" + refresh_token + "\"}"));
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
                                                     //[FromHeader] Guid clientId,
                                                     [FromHeader] Guid destinationId,
                                                     [FromQuery] int take = 10)
    {
        var clientNodeId = await VerifyTokenAsync( apiToken);
        if (clientNodeId == null)
            return Unauthorized();
        
        var result = await _dbContext.ObjectExchanges
                        .Where(s => s.Destination == clientNodeId)
                        .Where(s => s.Sender == destinationId)
                        .Where(s => s.Message != null)
                        .OrderBy(d => d.Id) // priority
                        .Take(take)
                        .Select(d => new ObjectExchange_v0(d))
                        .ToArrayAsync();
        return Ok(result);
    }

    [HttpPost("objects")]

    public async Task<IActionResult> PostAsync([FromHeader] string apiToken,
                                               //[FromHeader] Guid clientId,
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
        if (text.Length > MessageSizeLimit)
            return BadRequest($"The message size {text.Length} is overflow limit {MessageSizeLimit} per message.");

        DateTime? dateStamp = null;
        if (json.RootElement.GetProperty("dateStamp").TryGetDateTime(out DateTime tdateStamp))
            dateStamp = tdateStamp;
        var clientNodeId = await VerifyTokenAsync(apiToken);
        if (clientNodeId == null)
            return Unauthorized();
        var clientNode = await _dbContext.ClientNodes.FindAsync(clientNodeId);
        if (clientNode == null)
        {
            _logger.LogError($"The client {clientNodeId} do not exist");
            return Unauthorized();
        }

        if (clientNode.CounterReseted.Day != DateTime.UtcNow.Day)
        {
            clientNode.CounterReseted = DateTime.UtcNow;
            clientNode.Counter = 0;

        }

        if (clientNode.ObjectsCount >= clientNode.MaxOperationPerDay)
        {
            return BadRequest("The limit operations per day is overflow");
        }



        var validNodes = clientNode.GetClientAccess();
        if (validNodes.Contains(destinationId))
        {
            var doc = new ObjectExchange_v0
            {
                Sender = clientNodeId.Value,
                Destination = destinationId,
                ObjectId = objectId,
                ObjectType = objectType,
                ObjectAsText = text,
                DateStamp = DateTime.UtcNow,
                SenderDateStamp = dateStamp
            };
            var oe = doc.GetObjectExchange();
            await _dbContext.ObjectExchanges.AddAsync(oe);
            clientNode.Counter++;
            await _dbContext.SaveChangesAsync();
        }
        else
        {
            string error = $"The node {clientNodeId} try send message to invalid node {destinationId}";
            _logger.LogError(error);
            return BadRequest(error);
        }

        return Ok();
    }

    [HttpDelete("objects/{id:long}")]
    public async Task<IActionResult> DeleteAsync([FromHeader] string apiToken, /*[FromHeader] Guid clientId,*/ long id)
    {
        var clientNodeId = await VerifyTokenAsync(apiToken);
        if (clientNodeId == null)
            return Unauthorized();
        var obj = await _dbContext.ObjectExchanges.FindAsync(id);
        if (obj == null)
            return NotFound();
        if (obj.Destination != clientNodeId)
        {
            var error = $"Нод {clientNodeId} не може видалятидані чужих нодів {obj.Destination}";
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
                                                     //[FromHeader] Guid clientId,
                                                     [FromHeader] Guid destinationId,
                                                     [FromQuery] int take = 10)
    {

        var clientNodeId = await VerifyTokenAsync(apiToken);
        if (clientNodeId == null)
            return Unauthorized();

        var result = await _dbContext.ObjectExchanges
                .Where(s => s.Destination == clientNodeId)
                .Where(s => s.Sender == destinationId)
                .Where(s=>s.Message==null)
                .OrderBy(d => d.Id) // priority
                .Take(take)
                .Select(s=>new QueryObject(s))
                .ToArrayAsync();
        return Ok(result);
    }

    [HttpPost("queries")]
    public async Task<IActionResult> PostQueryAsync([FromHeader] string apiToken,
                                                    //[FromHeader] Guid clientId,
                                                    [FromHeader] Guid destinationId,
                                                    JsonDocument json)
    {
        string? objectId = json.RootElement.GetProperty("objectId").GetString();
        if (objectId == null)
            return BadRequest("The not defined objectId");
        string? objectType = json.RootElement.GetProperty("objectType").GetString();
        if (objectType == null)
            return BadRequest("The not defined objectType");

        var clientNodeId = await VerifyTokenAsync(apiToken);
        if (clientNodeId == null)
            return Unauthorized();

        var clientNode = await _dbContext.ClientNodes.FindAsync(clientNodeId);
        if (clientNode == null)
        {
            _logger.LogError($"The client {clientNodeId} do not exist");
            return Unauthorized();
        }
        var validNodes = clientNode.GetClientAccess();
        if (!validNodes.Contains(destinationId))
            return BadRequest("The node not accept post");
        // check exist same query 
        var obj = new QueryObject
        {
            Sender = clientNodeId.Value,
            Destination = destinationId,
            ObjectId = objectId,
            ObjectType = objectType
        };
        await _dbContext.ObjectExchanges.AddAsync(obj.GetObjectExchange());

        await _dbContext.SaveChangesAsync();
        return Ok();
    }



    [HttpDelete("queries/{id:long}")]
    public async Task<IActionResult> DeleteQueryAsync([FromHeader] string apiToken,
                                                      //[FromHeader] Guid? clientId,
                                                      long id)
    {
        var clientNodeId = await VerifyTokenAsync(apiToken);
        if (clientNodeId == null)
            return Unauthorized();

        var obj = await _dbContext.ObjectExchanges.FindAsync(id);
        if (obj == null)
            return NotFound();
        if (obj.Destination != clientNodeId)
        {
            var error = $"The node {clientNodeId} cannot delete  data from other nodes {obj.Destination}";
            _logger.LogError($"{DateTime.Now}: {error}");
            return Conflict(error);

        }
        _dbContext.ObjectExchanges.Remove(obj);

        await _dbContext.SaveChangesAsync();
        return Ok();

    }


    #endregion

}
