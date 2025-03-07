﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ObjectsExchange.Data;
using ObjectsExchange.Services;
using Org.BouncyCastle.Asn1.Ocsp;
using Sabatex.ObjectsExchange.Models;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

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
            return new Token(result.AccessToken, result.RefreshToken, apiConfig.TokensLifeTime);

        }

        public async Task<Token> RefreshTokenAsync(Guid clientId, string refreshToken)
        {
            await Task.Delay(1000);
            var clientNode = await _dbContext.ClientNodes.FindAsync(clientId);
            if (clientNode == null)
                throw new LoginIdUknownException($"Login failed: {clientId}");

            var oldAccessToken = await _dbContext.AutenficatedNodes.SingleOrDefaultAsync(s => s.Id == clientId);
            if (oldAccessToken == null)
                throw new TokenNotExistException($"Try refresh unexist token for clientId={clientId} ");

            if (oldAccessToken.RefreshToken != refreshToken)
                throw new TokenNotExistException($"Try refresh utoken for clientId={clientId} fail");

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
            return new Token(result.AccessToken, result.RefreshToken, apiConfig.TokensLifeTime);
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
    }

    public class LoginIdUknownException : Exception
    {
        public LoginIdUknownException(string message) : base(message) { }
    }
    public class TokenNotExistException : Exception
    {
        public TokenNotExistException(string message) : base(message) { }
    }
}
