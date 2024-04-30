using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ObjectsExchange.Client.Models;
using ObjectsExchange.Data;
using ObjectsExchange.Services;
using Radzen;
using Sabatex.ObjectsExchange.Models;
using Sabatex.RadzenBlazor;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace ObjectsExchange.Controllers;
[Authorize()]
public class ClientNodeController : BaseController<ObjectsExchange.Client.Models.ClientNode>
{

    public ClientNodeController(ObjectsExchangeDbContext context,ILogger<ClientNodeController> logger,ClientManager clientManager) : base(context,logger, clientManager)
    {

    }

    [HttpDelete("clean/{id}")]
    public virtual async Task<IActionResult> CleanNode([FromRoute] Guid id)
    {
        var node = await context.ClientNodes.Include(c=>c.Client).SingleOrDefaultAsync(s=>s.Id ==id);
        if (node == null) { return NotFound(); }

        if (User.IsInRole("Administrator") || (node.Client !=null && node.Client.UserId == UserId))
        {
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
            q = q.Where(s => s.Client!=null &&  s.Client.UserId == UserId);
        
        q=q.Select(x =>new ClientNode
        {
            Id = x.Id,
            ClientId = x.ClientId,
            Name = x.Name,
            Counter = x.Counter,
            ObjectsCount = context.ObjectExchanges.Where(n => n.Destination == x.Id).Count()
        });

        return q;
    }

    protected override async Task OnBeforeAddItemToDatabase(ClientNode item)
    {
        await base.OnBeforeAddItemToDatabase(item);
        item.Password = clientManager.GetHashString(item.Password);
    }

    protected override async Task OnBeforeUpdateAsync(ClientNode item, ClientNode update)
    {
        await base.OnBeforeUpdateAsync(item,update);
        if (string.IsNullOrWhiteSpace(update.Password))
            update.Password = item.Password;
        else
            update.Password = clientManager.GetHashString(update.Password);
    } 

    protected override async Task OnAfterGetById(ClientNode item, Guid id)
    {
        item.Password = string.Empty;
        await Task.Yield();
    }

    protected override async Task<bool> CheckAccess(ClientNode item, ClientNode? update)
    {
        if (update != null)
        {
            if (item.ClientId != update.ClientId) return false;
        }
        var client = await context.Clients.SingleAsync(s=>s.Id == item.ClientId);
        return client.UserId == UserId;
    }
}


