using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web.Resource;
using Sabatex.ApiObjectsExchange.Controllers;
using Sabatex.ObjectsExchange.Models;
using Sabatex.ObjectsExchange.Server.Data;
using Sabatex.ObjectsExchange.Server.Services;
using Sabatex.ObjectsExchange.Services;

namespace Sabatex.ObjectsExchange.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    //[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]

    public class ClientsController : ControllerBase
    {
        private readonly ILogger<ClientsController> _logger;
        protected readonly ObjectsExchangeDbContext _dbContext;

        public ClientsController(ObjectsExchangeDbContext dbContext, ILogger<ClientsController> logger, IOptions<ApiConfig> apiConfig)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync(int skip = 0, int take = 10)
        {
            var result = new ResultCollection<ClientNodeBase>
            {
                Items = await _dbContext.ClientNodes.Skip(skip).Take(take).Select(s => new ClientNodeBase
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    ClientAccess = s.ClientAccess,
                    IsDemo = s.IsDemo,
                    Counter = s.Counter,
                    MaxOperationPerMounth = s.MaxOperationPerMounth

                }).ToArrayAsync(),
                Count = await _dbContext.ClientNodes.CountAsync()
        };
            return Ok(result);
        }

    }
}
