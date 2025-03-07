using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ObjectsExchange.Data;
using ObjectsExchange.Models;
using ObjectsExchange.Services;
using Radzen;
using Sabatex.Identity.UI;
using Sabatex.ObjectsExchange.Models;
using Sabatex.RadzenBlazor;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace ObjectsExchange.Controllers;
[Authorize()]
public class ClientNodeController : BaseController<ClientNode>
{
    private readonly ClientManager clientManager;

    public ClientNodeController(ObjectsExchangeDbContext context,ILogger<ClientNodeController> logger,ClientManager clientManager) : base(context,logger)
    {
        this.clientManager = clientManager;
    }

    public override Task<ODataServiceResult<ClientNode>> Get([FromQuery] string json)
    {
        return base.Get(json);
    }

    [HttpDelete("clean/{id}")]
    public virtual async Task<IActionResult> CleanNode([FromRoute] Guid id)
    {
        var node = await context.Set<ClientNode>().Include(c=>c.Client).SingleOrDefaultAsync(s=>s.Id ==id);
        if (node == null) { return NotFound(); }

        if (User.IsInRole("Administrator") || (node.Client !=null && node.Client.OwnerUser == User.Identity.Name))
        {
            context.Set<ObjectExchange>().Where(s=>s.Destination == id).ExecuteDelete();
            return new NoContentResult();
        }
        else 
        { 
            return Unauthorized();
        }
        


    }


    protected override IQueryable<ClientNode> OnAfterIncludeInGet(IQueryable<ClientNode> query, QueryParams queryParams)
    {
        var r = base.OnAfterIncludeInGet(query, queryParams);
        //if (queryParams.ForeginKey != null)
        //{
        //    r = r.Where($"it => it.{queryParams.ForeginKey.Name}.ToString() == \"{queryParams.ForeginKey.Id}\"");
        //}

        //queryParams.ForeginKey = null;
        //if (!User.IsInRole("Administrator"))
        //    r = r.Where(s => s.Client != null && s.Client.OwnerUser == User.Identity.Name);

        return r;
    }

    protected override IQueryable<ClientNode> OnAfterWhereInGet(IQueryable<ClientNode> query, QueryParams queryParams)
    {
        var q = base.OnAfterWhereInGet(query, queryParams);
        if (!User.IsInRole("Administrator"))
            q = q.Where(s => s.Client != null && s.Client.OwnerUser == User.Identity.Name);

        q = q.Select(x =>new ClientNode
        {
            Id = x.Id,
            ClientId = x.ClientId,
            Name = x.Name,
            Counter = x.Counter,
            ObjectsCount = context.Set<ObjectExchange>().Where(n => n.Destination == x.Id).Count()
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

    public override Task<IActionResult> GetById([FromRoute] Guid id)
    {
        return base.GetById(id);
    }
    protected override async Task<bool> CheckAccess(ClientNode item, ClientNode? update)
    {
        if (update != null)
        {
            if (item.ClientId != update.ClientId) return false;
        }
        if (User.IsInRole(ApplicationClaim.AdministratorRole)) return true;

        var client = await context.Set<Sabatex.ObjectsExchange.Models.Client>().SingleAsync(s=>s.Id == item.ClientId);
        return   client.OwnerUser == User.Identity.Name;
    }
}


