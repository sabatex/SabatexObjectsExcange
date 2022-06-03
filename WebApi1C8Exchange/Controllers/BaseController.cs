using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using WebApiDocumentsExchange.Data;
using WebApiDocumentsExchange.Models;
using WebApiDocumentsExchange.Services;

namespace WebApiDocumentsExchange.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected readonly ApplicationDbContext _dbContext;
        protected readonly ApiConfig _apiConfig;
        public BaseController(ApplicationDbContext dbContext, IOptions<ApiConfig> apiConfig)
        {
            _dbContext = dbContext;
            _apiConfig = apiConfig.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_context"></param>
        /// <param name="node"></param>
        /// <param name="password"></param>
        /// <returns>Api token</returns>
        /// <exception cref="Exception"></exception>
        protected async Task<string> LoginAsync(string node, string password)
        {
            var clientNode = await _dbContext.ClientNodes.SingleOrDefaultAsync(s => s.Name == node);
            if (clientNode != null)
            {
                if (clientNode.Password == password)
                {
                    var result = await _dbContext.AutenficatedNodes.SingleOrDefaultAsync(s => s.NodeId == clientNode.Id);
                    if (result != null)
                    {
                        _dbContext.AutenficatedNodes.Remove(result);
                    }
                    result = new AutenficatedNode { Node = clientNode, DateStamp = DateTime.Now, Id = Guid.NewGuid().ToString() };
                    await _dbContext.AutenficatedNodes.AddAsync(result);
                    await _dbContext.SaveChangesAsync();
                    return result.Id.ToString();
                }
            }
            throw new Exception("The login or password incorect!");
        }

        protected string AppVersion=> Assembly.GetExecutingAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty;

        protected async Task<int> GetSecureNodeAsync(string apiToken)
        {
            var clientAutenficate = await _dbContext.AutenficatedNodes.FindAsync(apiToken);
            if (clientAutenficate != null)
            {
                if ((DateTime.Now - clientAutenficate.DateStamp) < _apiConfig.TokensLifeTimeMinutes)
                {
                    var result = await _dbContext.ClientNodes.FindAsync(clientAutenficate.NodeId);
                    if (result != null)
                    {
                        return result.Id;
                    }
                }

            }
            throw new Exception("Access denied!!!");
        }

        protected async Task<int> GetNodeAsync([NotNull] string node)
        {
            var result = await _dbContext.ClientNodes.SingleOrDefaultAsync(f => f.Name == node);
            if (result == null)
                throw new ArgumentException("Node {0} not exist!", node);

            return result.Id;
        }

        protected async Task<ObjectType?> GetObjectTypeByNameAsync(int nodeId,string name)=>
            await  _dbContext.ObjectTypes.SingleOrDefaultAsync(s => s.Name == name && s.NodeId == nodeId);

    }
}
