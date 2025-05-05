using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ObjectsExchange.Controllers;
using ObjectsExchange.Data;
using ObjectsExchange.Services;
using Radzen.Blazor.Rendering;
using Sabatex.ObjectsExchange.Models;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Sabatex.ObjectsExchange.Controllers;

[Route("api/[controller]")]
[ApiController]
public class v1Controller : BaseApiController
{
    public static int maxTake = 50;
    public const int MessageSizeLimit = 1000000;
    private const string _tokenType = "BEARER";
    public v1Controller(ObjectsExchangeDbContext dbContext, ILogger<v1Controller> logger, IOptions<ApiConfig> apiConfig) : base(logger, dbContext, apiConfig)
    {
    }


    public override async Task<IActionResult> PostRefreshTokenAsync(JsonDocument json)
    {
        try
        {
            string? refresh_token = json.RootElement.GetProperty("password").GetString();
            if (refresh_token == null)
            {
                _logger.LogError("Refresh token failed: refresh_token is null");
                return Unauthorized();
            }
            return await base.PostRefreshTokenAsync(JsonDocument.Parse("{\"refresh_token\":\"" + refresh_token + "\"}"));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Refresh token  {json} fail error:{ex.Message}");
            return Unauthorized();
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
        var clientNodeId = await VerifyTokenAsync(apiToken);
        if (clientNodeId == null)
            return Unauthorized();

        var result = await _dbContext.ObjectExchanges
                        .Where(s => s.Destination == clientNodeId)
                        .Where(s => s.Sender == destinationId)
                        .OrderBy(d => d.Id) // priority
                        .Take(take).ToArrayAsync();
        return Ok(result);
    }

    [HttpPost("objects")]

    public async Task<IActionResult> PostAsync([FromHeader] string apiToken,
                                               //[FromHeader] Guid clientId,
                                               [FromHeader] Guid destinationId,
                                               JsonDocument json)
    {
        string? messageHeader = json.RootElement.GetProperty("messageHeader").GetString();
        if (messageHeader == null)
            return BadRequest("The not defined messageHeader");
        
        string? message = json.RootElement.GetProperty("message").GetString();
        if (message != null)
           if (message.Length > MessageSizeLimit)
                return BadRequest($"The message size {message.Length} is overflow limit {MessageSizeLimit} per message.");

        DateTime? dateStamp = null;
        if (json.RootElement.GetProperty("dateStamp").TryGetDateTime(out DateTime tdateStamp))
            dateStamp = tdateStamp;
        
        var clientNodeId = await VerifyTokenAsync(apiToken);
        if (clientNodeId == null)
        {
            return Unauthorized();
        }

        var clientNode = await _dbContext.ClientNodes.FindAsync(clientNodeId);
        if (clientNode == null)
        {
            _logger.LogError($"The client {clientNodeId} do not Exist, but exist valid token by same id ");
            return Unauthorized();
        }

        if (clientNode.CounterReseted.Day != DateTime.UtcNow.Day)
        {
            clientNode.CounterReseted = DateTime.UtcNow;
            clientNode.Counter = 0;
        }

        if (clientNode.Counter >= clientNode.MaxOperationPerDay)
        {
            return BadRequest("The limit operations per day is overflow");
        }

        var validNodes = clientNode.GetClientAccess();
        if (validNodes.Contains(destinationId))
        {
            var doc = new ObjectExchange
            {
                Sender = clientNodeId.Value,
                Destination = destinationId,
                MessageHeader = messageHeader,
                Message = message,
                DateStamp = DateTime.UtcNow,
                SenderDateStamp = DateTime.SpecifyKind(dateStamp ?? DateTime.UtcNow, DateTimeKind.Utc)
            };
            await _dbContext.ObjectExchanges.AddAsync(doc);
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



}
