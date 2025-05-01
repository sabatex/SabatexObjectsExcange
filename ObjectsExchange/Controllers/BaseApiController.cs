using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ObjectsExchange.Data;
using ObjectsExchange.Services;
using Sabatex.ObjectsExchange.Controllers;
using Sabatex.ObjectsExchange.Models;
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
            var refreshToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { ClientId = clientId, AccessToken = result.AccessToken })));
            _logger.LogInformation($"Login success: clientId = {clientId}");
            return Ok(new Token(result.AccessToken, result.AccessToken, _apiConfig.TokensLifeTime));

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


    }
}
