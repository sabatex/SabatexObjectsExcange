using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ObjectsExchange.Data;
using ObjectsExchange.Services;
using Sabatex.Core.RadzenBlazor;
using Sabatex.ObjectsExchange.Models;
using Sabatex.RadzenBlazor;
using Sabatex.RadzenBlazor.Server;
using System.Linq.Dynamic.Core;
using System.Security.Claims;

namespace ObjectsExchange.Controllers;

[Authorize()]
public class ClientController : BaseController<Sabatex.ObjectsExchange.Models.Client>
{
    public ClientController(ObjectsExchangeDbContext context,ILogger<ClientController> logger) : base(context,logger)
    {

    }
    protected override IQueryable<Sabatex.ObjectsExchange.Models.Client> OnAfterWhereInGet(IQueryable<Sabatex.ObjectsExchange.Models.Client> query, QueryParams queryParams)
    {
        if (User.IsInRole("Administrator"))
            return base.OnAfterWhereInGet(query, queryParams);
        else
            return base.OnAfterWhereInGet(query, queryParams).Where(s => s.OwnerUser == User.Identity.Name);

    }

    protected override IQueryable<Sabatex.ObjectsExchange.Models.Client> OnBeforeGetById(IQueryable<Sabatex.ObjectsExchange.Models.Client> query, Guid id)
    {
        return base.OnBeforeGetById(query, id);
    }
    protected override async Task OnBeforeAddItemToDatabase(Sabatex.ObjectsExchange.Models.Client item)
    {
        await base.OnBeforeAddItemToDatabase(item);
        if (item.OwnerUser != User.Identity?.Name)
            throw new Exception("The access denied");
        if (await context.Set<Sabatex.ObjectsExchange.Models.Client>().Where(s=>s.OwnerUser == User.Identity.Name).CountAsync() >= 5)
            throw new Exception("The try add client over limit 5!");
  
    }

    protected override async Task<bool> CheckAccess(Sabatex.ObjectsExchange.Models.Client item, Sabatex.ObjectsExchange.Models.Client? update)
    {
        await Task.Yield();
        return (item.OwnerUser == User.Identity?.Name) || User.IsInRole("Administrator");
    }
}