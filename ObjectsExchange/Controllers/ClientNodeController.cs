using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObjectsExchange.Client.Models;
using ObjectsExchange.Data;
using Radzen;
using Sabatex.ObjectsExchange.Models;
using Sabatex.RadzenBlazor;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace ObjectsExchange.Controllers;
[Authorize()]
public class ClientNodeController : BaseController<ObjectsExchange.Client.Models.ClientNode>
{

    public ClientNodeController(ObjectsExchangeDbContext context,ILogger<ClientNodeController> logger) : base(context,logger)
    {

    }

    [HttpDelete("clean/{id}")]
    public virtual async Task<IActionResult> CleanNode([FromRoute] Guid id)
    {
        var node = await context.ClientNodes.Include(c=>c.Client).SingleOrDefaultAsync(s=>s.Id ==id);
        if (node == null) { return NotFound(); }

        if (User.IsInRole("Administrator") || node.Client.UserId == UserId)
        {
            context.QueryObjects.Where(s => s.Destination == id).ExecuteDelete();
            context.ObjectExchanges.Where(s=>s.Destination == id).ExecuteDelete();
            return new NoContentResult();
        }
        else 
        { 
            return Unauthorized();
        }
        


    }


    protected override IQueryable<ClientNode> OnBeforeGet(IQueryable<ClientNode> query, QueryParams queryParams)
    {
        var q = base.OnBeforeGet(query, queryParams);
        if (!User.IsInRole("Administrator"))
            q = q.Where(s => s.Client.UserId == UserId);
        
        q=q.Select(x =>new ClientNode
        {
            Id = x.Id,
            ClientId = x.ClientId,
            Name = x.Name,
            Counter = x.Counter,
            QueriesCount = context.QueryObjects.Where(n => n.Destination == x.Id).Count(),
            ObjectsCount = context.ObjectExchanges.Where(n => n.Destination == x.Id).Count()
        });

        return q;
    }


}


