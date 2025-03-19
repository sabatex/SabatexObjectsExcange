using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ObjectsExchange.Data;
using ObjectsExchange.Services;
using Org.BouncyCastle.Asn1.Ocsp;
using Sabatex.ObjectsExchange.Controllers;
using Sabatex.ObjectsExchange.Models;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ObjectsExchange.Services
{
    public class ClientManager
    {
        private readonly ILogger<ClientManager> _logger;
        protected readonly ObjectsExchangeDbContext _dbContext;
        private readonly ApiConfig apiConfig;

        public ClientManager(ObjectsExchangeDbContext dbContext, ILogger<ClientManager> logger, IOptions<ApiConfig> apiConfig)
        {
            _logger = logger;
            _dbContext = dbContext;
            this.apiConfig = apiConfig.Value;
        }
        public string GetHashString(string value)
        {
            var hashKey = Encoding.UTF8.GetBytes("ajcewewi%^(F#|}9327nx=-23hdxsa5vcx<>_ d");
            var hmac = new HMACSHA256(hashKey);
            var result = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)));
            return result;
        }



        private string CreateAccessToken()
        {
            var r = new Random();
            byte[] data = new byte[16];
            r.NextBytes(data);
            return new Guid(data).ToString();
        }

        public async Task<ClientNode> CreateClientAsync(string clientName, string password, string descriptions, string accesNodes)
        {
            var clientNode =  new ClientNode
            {
                Name = clientName,
                Description = descriptions,
                ClientAccess = accesNodes,
                Password = GetHashString(password),
                Id = new Guid()
            };
            await _dbContext.AddAsync(clientNode);
            await _dbContext.SaveChangesAsync();
            return clientNode;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <exception cref="LoginIdUknownException">Failure login to api</exception>
        public async Task<Token> LoginAsync(Guid clientId, string password)
        {
            await Task.Delay(1000);// 
            var clientNode = await _dbContext.ClientNodes.FindAsync(clientId);
            if (clientNode == null)
                throw new LoginIdUknownException($"Login failed: {clientId}");

            if (clientNode.Password != GetHashString(password))
                throw new LoginIdUknownException($"Login failed: {clientId}");

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
                ExpiresDate = DateTime.UtcNow + TimeSpan.FromSeconds(apiConfig.TokensLifeTime)
            };
            await _dbContext.AutenficatedNodes.AddAsync(result);
            await _dbContext.SaveChangesAsync();
            return new Token(result.AccessToken, result.RefreshToken, apiConfig.TokensLifeTime,clientId);

        }

        public async Task<Token> RefreshTokenAsync(string refreshToken)
        {
            var jsonString = Encoding.UTF8.GetString(Convert.FromBase64String(refreshToken));
            var tokenInternal = JsonSerializer.Deserialize<TokenInternal>(jsonString);



            var clientNode = await _dbContext.ClientNodes.FindAsync(tokenInternal.ClientId);
            if (clientNode == null)
                throw new LoginIdUknownException($"Login failed: {tokenInternal.ClientId}");

            var oldAccessToken = await _dbContext.AutenficatedNodes.SingleOrDefaultAsync(s => s.Id == tokenInternal.ClientId);
            if (oldAccessToken == null)
                throw new TokenNotExistException($"Try refresh unexist token for clientId={tokenInternal.ClientId} ");

            if (oldAccessToken.RefreshToken != tokenInternal.AccessToken)
                throw new TokenNotExistException($"Try refresh token for clientId={tokenInternal.ClientId} fail");

            _dbContext.AutenficatedNodes.Remove(oldAccessToken);
            var result = new AutenficatedNode
            {
                Id = clientNode.Id,
                AccessToken = CreateAccessToken(),
                RefreshToken = CreateAccessToken(),
                ExpiresDate = DateTime.UtcNow + TimeSpan.FromSeconds(apiConfig.TokensLifeTime)
            };
            await _dbContext.AutenficatedNodes.AddAsync(result);
            await _dbContext.SaveChangesAsync();
            return new Token(result.AccessToken, result.RefreshToken, apiConfig.TokensLifeTime,tokenInternal.ClientId);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="apiToken"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        private async Task<ClientNode?> GetClientNodeByTokenAsync(Guid nodeId, string apiToken,string? ip)
        {
            var clientAutenficate = await _dbContext.AutenficatedNodes.FindAsync(nodeId);
            if (clientAutenficate != null)
            {
                if (apiToken != clientAutenficate.AccessToken)
                {
                    _logger.LogTrace($"{DateTime.Now}: The client {nodeId}  try use invalid token {apiToken}");
                    return null;
                }

                var existTime = DateTime.UtcNow;
                if (existTime > clientAutenficate.ExpiresDate)
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
                _logger.LogTrace($"{DateTime.Now}: Try use service from ip={ip} with nodeId={nodeId} and The client {nodeId} and token = {apiToken}");
                return null;
            }
        }


        public async Task<IEnumerable<ClientNode>> GetClients(int skip, int take)
        {
            return await _dbContext.ClientNodes.Skip(skip).Take(take).Select(s => new ClientNode
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                ClientAccess = s.ClientAccess,
                IsDemo = s.IsDemo,
                Counter = s.Counter,
                MaxOperationPerDay = s.MaxOperationPerDay

            }).ToArrayAsync();
        }

        public async Task<int> Count()
        {
            return await _dbContext.ClientNodes.CountAsync();
        }

        public async Task<Guid?> VerifyTokenAsync(string authorization)
        {
            var token = TokenInternal.GetFromAuthorization(authorization);
            var clientAutenficate = await _dbContext.AutenficatedNodes.FindAsync(token.ClientId);
            if (clientAutenficate != null)
            {
                if (clientAutenficate.AccessToken != token.AccessToken)
                {
                    _logger.LogTrace($"{DateTime.Now}: The client {token.ClientId}  try use invalid token {token.AccessToken}");
                     return null;
                }

                var existTime = DateTime.UtcNow;
                if (existTime > clientAutenficate.ExpiresDate)
                {
                    _logger.LogTrace($"{DateTime.Now}: The client {token.ClientId}  use exprise token {token.AccessToken} by time {existTime}");
                     return null;
                }
                return clientAutenficate.Id;
            }
            else
            {

                 _logger.LogTrace($"{DateTime.Now}: Try use service  with nodeId={token.ClientId} and The client {token.ClientId} and token = {token.AccessToken}");
                return null;
            }
        }

    }

    public class LoginIdUknownException : Exception
    {
        public LoginIdUknownException(string message) : base(message) { }
    }
    public class TokenNotExistException : Exception
    {
        public TokenNotExistException(string message) : base(message) { }
    }
    public class TokenInternal {
        public Guid ClientId { get; set; }
        public string AccessToken { get; set; }
        public static TokenInternal GetFromAuthorization(string authorization)
        {
            var token = authorization.Split(' ')[1];
            var jsonString = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            return JsonSerializer.Deserialize<TokenInternal>(jsonString);
        }

        public static string GetAuthorization(Guid clientId, string accessToken)
        {
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new TokenInternal { ClientId = clientId, AccessToken = accessToken })));
            return $"Bearer {token}";
        }
    }
}
