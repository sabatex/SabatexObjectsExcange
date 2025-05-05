using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ObjectsExchange.Data;
using ObjectsExchange.Services;
using Sabatex.ObjectsExchange.Controllers;
using Sabatex.ObjectsExchange.Models;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ObjectsExchange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected readonly ILogger _logger;
        protected readonly ObjectsExchangeDbContext _dbContext;
        protected readonly ApiConfig _apiConfig;
        const string _loginFailed = "Login failed: The password or clientId is wrong!";
        public BaseApiController(ILogger logger, ObjectsExchangeDbContext dbContext, IOptions<ApiConfig> apiConfig)
        {
            _logger = logger;
            _dbContext = dbContext;
            _apiConfig = apiConfig.Value;
        }

        [HttpGet("version")]
        public IActionResult Get()
        {
            return Ok(Assembly.GetExecutingAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty);
        }

        /// <summary>
        /// Autenficate in service
        /// </summary>
        /// <param name="login">The object Login with node Id (register unsensitive) and password</param>
        /// <returns>string access token or empty for fail</returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("login")]
        public async Task<IActionResult> PostLoginAsync(JsonDocument json)
        {
            Guid? clientId = json.RootElement.GetProperty("clientId").GetGuid();
            if (clientId == null)
            {
                _logger.LogError("Login failed: clientId is null");
                return Unauthorized(_loginFailed);
            }

            string? password = json.RootElement.GetProperty("password").GetString();
            if (password == null)
            {
                _logger.LogError("Login failed: password is null");
                return Unauthorized(_loginFailed);
            }

            var clientNode = await _dbContext.ClientNodes.FindAsync(clientId);
            if (clientNode == null)
            {
                _logger.LogError($"Login failed: The wrong clientId = {clientId}");
                return Unauthorized(_loginFailed);
            }

            if (clientNode.Password != GetHashString(password))
            {
                _logger.LogError($"Login failed: The wrong password for clientId = {clientId}");
                return Unauthorized(_loginFailed);
            }

            // success client verified
            // remove old access token
            var oldAccessToken = await _dbContext.AutenficatedNodes.SingleOrDefaultAsync(s => s.Id == clientId);
            if (oldAccessToken != null)
                _dbContext.AutenficatedNodes.Remove(oldAccessToken);

            var result = new AutenficatedNode
            {
                Id = clientNode.Id,
                AccessToken = CreateAccessToken(),
                RefreshToken = CreateAccessToken(),
                ExpiresDate = DateTime.UtcNow + TimeSpan.FromSeconds(_apiConfig.TokensLifeTime)
            };
            await _dbContext.AutenficatedNodes.AddAsync(result);
            await _dbContext.SaveChangesAsync();
            var accessToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { ClientId = clientId, AccessToken = result.AccessToken })));
            var refreshToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { ClientId = clientId, RefreshToken = result.RefreshToken })));
            _logger.LogInformation($"Login success: clientId = {clientId}");
            return Ok(new Token(accessToken, refreshToken, _apiConfig.TokensLifeTime));

        }


        [HttpPost("refresh_token")]
        public  virtual async Task<IActionResult> PostRefreshTokenAsync(JsonDocument json)
        {
            string? refresh_token = json.RootElement.GetProperty("refresh_token").GetString();
            if (refresh_token == null)
            {
                _logger.LogError("Refresh token failed: refresh_token is null");
                return Unauthorized();
            }

            try
            {
                var jsonString = Encoding.UTF8.GetString(Convert.FromBase64String(refresh_token));
                json = JsonDocument.Parse(jsonString);
  
            }
            catch (Exception ex)
            {
                _logger.LogError($"Refresh token failed: {ex.Message}");
                return Unauthorized();
            }
   
            var clientId = json.RootElement.GetProperty("ClientId").GetGuid();
            if (clientId == null)
            {
                _logger.LogError("Refresh token failed: clientId is null");
                return Unauthorized();
            }

            string? refreshToken = json.RootElement.GetProperty("RefreshToken").GetString();
            if (refreshToken == null)
            {
                _logger.LogError("Refresh token failed: refresh_token is null");
                return Unauthorized();
            }



            var clientNode = await _dbContext.ClientNodes.FindAsync(clientId);
            if (clientNode == null)
            {
                _logger.LogError($"Login failed: The wrong clientId = {clientId}");
                return Unauthorized();
            }

            var oldAccessToken = await _dbContext.AutenficatedNodes.SingleOrDefaultAsync(s => s.Id == clientId);
            if (oldAccessToken == null)
            {
                _logger.LogError($"Try refresh unexist token for clientId={clientId} ");
                return Unauthorized();
            }
 
            if (oldAccessToken.RefreshToken != refreshToken)
            {
                _logger.LogError($"Try refresh token for clientId={clientId} fail");
                return Unauthorized();
            }
 
            _dbContext.AutenficatedNodes.Remove(oldAccessToken);
            var result = new AutenficatedNode
            {
                Id = clientNode.Id,
                AccessToken = CreateAccessToken(),
                RefreshToken = CreateAccessToken(),
                ExpiresDate = DateTime.UtcNow + TimeSpan.FromSeconds(_apiConfig.TokensLifeTime)
            };
            await _dbContext.AutenficatedNodes.AddAsync(result);
            await _dbContext.SaveChangesAsync();
             _logger.LogInformation($"Refresh token success: clientId = {clientNode.Id}");
            return Ok(new Token(Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { ClientId = clientNode.Id, AccessToken = result.AccessToken }))),
                                Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { ClientId = clientNode.Id, RefreshToken = result.RefreshToken }))),
                                _apiConfig.TokensLifeTime));
        }



        private string CreateAccessToken()
        {
            var r = new Random();
            byte[] data = new byte[16];
            r.NextBytes(data);
            return new Guid(data).ToString();
        }
        public string GetHashString(string value)
        {
            var hashKey = Encoding.UTF8.GetBytes("ajcewewi%^(F#|}9327nx=-23hdxsa5vcx<>_ d");
            var hmac = new HMACSHA256(hashKey);
            var result = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)));
            return result;
        }

        protected async Task<Guid?> VerifyTokenAsync(string authorization)
        {
            try
            {
                var token = authorization.Split(' ')[1];
                var json = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(token)));
                var clientId = json.RootElement.GetProperty("ClientId").GetGuid();
                var accessToken = json.RootElement.GetProperty("AccessToken").GetString();

                var clientAutenficate = await _dbContext.AutenficatedNodes.FindAsync(clientId);
                if (clientAutenficate != null)
                {
                    if (clientAutenficate.AccessToken != accessToken)
                    {
                        _logger.LogTrace($"{DateTime.Now}: The client {clientId}  try use invalid token {accessToken}");
                        return null;
                    }

                    var existTime = DateTime.UtcNow;
                    if (existTime > clientAutenficate.ExpiresDate)
                    {
                        _logger.LogTrace($"{DateTime.Now}: The client {clientId}  use exprise token {accessToken} by time {existTime}");
                        return null;
                    }
                    return clientAutenficate.Id;
                }
                else
                {

                    _logger.LogTrace($"{DateTime.Now}: Try use service  with nodeId={clientId} and The client {clientId} and token = {accessToken}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Verify token failed: {ex.Message}");
                return null;
            }
        }
    }
}
