﻿using Microsoft.AspNetCore.Authorization;
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
public class v2Controller : BaseApiController
{
    public static int maxTake = 50;
    public const int MessageSizeLimit = 1000000;
    private const string _tokenType = "BEARER";
    public v2Controller(ObjectsExchangeDbContext dbContext, ILogger<v1Controller> logger, IOptions<ApiConfig> apiConfig) : base(logger,dbContext,apiConfig)
    {

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


 


    //[HttpPost("refresh_token")]
    //public async Task<IActionResult> PostRefreshTokenAsync(RefreshToken refresh_token)
    //{
    //    try
    //    {
    //         return Ok(await _clientManager.RefreshTokenAsync(refresh_token.refresh_token));
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError($"Refresh token  {refresh_token} fail error:{ex.Message}");
    //        return Unauthorized();
    //    }
    //}

  

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
    public async Task<IActionResult> GetObjectsAsync([FromHeader] string authorization,
                                                     [FromQuery] Guid destinationId,
                                                     [FromQuery] int take = 10)
    {

        var clientId = await VerifyTokenAsync(authorization);
        if (clientId == null)
            return Unauthorized();

        var result = await _dbContext.ObjectExchanges
                        .Where(s => s.Destination == clientId)
                        .Where(s => s.Sender == destinationId)
                        .OrderBy(d => d.Id) // priority
                        .Take(take).ToArrayAsync();
        return Ok(result);
    }

    [HttpPost("objects")]

    public async Task<IActionResult> PostAsync([FromHeader] string authorization,
                                               JsonDocument json)
    {
        var clientId = await VerifyTokenAsync(authorization);

        Guid? destinationId = json.RootElement.GetProperty("destinationId").GetGuid();
        if (destinationId == null)
            return BadRequest("The not defined destinationId");


        string? messageHeader = json.RootElement.GetProperty("messageHeader").GetString();
        if (messageHeader == null)
            return BadRequest("The not defined messageHeader");

        if (messageHeader.Length > 150)
            return BadRequest($"The messageHeader size {messageHeader.Length} is overflow limit 150 per messageHeader.");

        string? message = json.RootElement.GetProperty("message").GetString();
        if (message != null)
           if (message.Length > MessageSizeLimit)
                return BadRequest($"The message size {message.Length} is overflow limit {MessageSizeLimit} per message.");

        DateTime? dateStamp = null;
        if (json.RootElement.GetProperty("dateStamp").TryGetDateTime(out DateTime tdateStamp))
            dateStamp = tdateStamp;
        var clientNode = await _dbContext.ClientNodes.FindAsync(clientId);
        if (clientNode == null)
            return Unauthorized();
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
        if (validNodes == null) return BadRequest("The not defined valid nodes");
        if (validNodes.Contains(destinationId.Value))
        {
            var doc = new ObjectExchange
            {
                Sender = clientId.Value,
                Destination = destinationId.Value,
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
            string error = $"The node {clientId} try send message to invalid node {destinationId}";
            _logger.LogError(error);
            return BadRequest(error);
        }

        return Ok();
    }

    [HttpDelete("objects/{id:long}")]
    public async Task<IActionResult> DeleteAsync([FromHeader] string authorization, long id)
    {
        var clientId = await VerifyTokenAsync(authorization);
        if (clientId == null)
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


    [HttpPost("objects_delete_range")]
    public async Task<IActionResult> PostDeleteAsync([FromHeader] string authorization, JsonDocument json)
    {
        var clientId = await VerifyTokenAsync(authorization);
        if (clientId == null)
            return Unauthorized();
        var ids = json.RootElement.GetProperty("ids").EnumerateArray().Select(s => s.GetInt64()).ToArray();
        if (ids.Length == 0)
            return BadRequest("The not defined ids");
        var objects = await _dbContext.ObjectExchanges
                        .Where(s => s.Destination == clientId)
                        .Where(s => ids.Contains(s.Id))
                        .ToArrayAsync();
        if (objects.Length == 0)
            return NotFound();
        foreach (var obj in objects)
        {
            if (obj.Destination != clientId)
            {
                var error = $"Нод {clientId} не може видалятидані чужих нодів {obj.Destination}";
                _logger.LogError(error);
                return BadRequest(error);
            }
        }
        _dbContext.ObjectExchanges.RemoveRange(objects);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    #endregion


}


public record RefreshToken(string refresh_token);