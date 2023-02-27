using Microsoft.EntityFrameworkCore;
using sabatex.ObjectsExchange.Models;
using Sabatex.ApiObjectsExchange.Controllers;
using Sabatex.ObjectsExchange.Server.Data;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Sabatex.ObjectsExchange.Server.Services
{
    public class ClientManager
    {
        private readonly ILogger<ClientManager> _logger;
        protected readonly ObjectsExchangeDbContext _dbContext;

        public ClientManager(ObjectsExchangeDbContext dbContext, ILogger<ClientManager> logger) 
        {
            _logger= logger;
            _dbContext= dbContext;
        }
        private string GetHashString(string value)
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

        public async Task<ClientNode> CreateClientAsync(string clientName,string password,string descriptions,string accesNodes)
        {
            var normalizedName = clientName.ToUpper();
            var clientNode = await _dbContext.ClientNodes.SingleOrDefaultAsync(s => s.NormalizedName == normalizedName);
            if (clientNode != null)
            {
                var errorClientName = $"The client {clientName} is exist";
                _logger.LogError($"{DateTime.Now}: {errorClientName}");
                throw new Exception(errorClientName);
            }
            clientNode = new ClientNode
            {
                NormalizedName = normalizedName,
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
        public async Task<string?> LoginAsync(Guid clientId, string password)
        {
            var clientNode = await _dbContext.ClientNodes.FindAsync(clientId);
            if (clientNode == null)
            {
                _logger.LogError($"Login failed: {clientId}");
                return null;
            }

            if (clientNode.Password != GetHashString(password))
            {
                _logger.LogError($"Login failed: {clientId}");
                return null;
            }
            // remove old autenficated token
            var oldAccessToken = await _dbContext.AutenficatedNodes.SingleOrDefaultAsync(s => s.Id == clientId);
            if (oldAccessToken != null)
                _dbContext.AutenficatedNodes.Remove(oldAccessToken);

            var result = new AutenficatedNode
            {
                Id = clientNode.Id,
                DateStamp = DateTime.UtcNow,
                Token = CreateAccessToken()
            };
            await _dbContext.AutenficatedNodes.AddAsync(result);
            await _dbContext.SaveChangesAsync();
            return result.Token;

        }

    }
}
