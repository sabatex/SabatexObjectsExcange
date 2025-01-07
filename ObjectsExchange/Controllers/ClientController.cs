using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using Microsoft.Extensions.Logging;
using ObjectsExchange.Data;
using ObjectsExchange.Services;
using Sabatex.RadzenBlazor;
using System.Linq.Dynamic.Core;
using System.Security.Claims;

namespace ObjectsExchange.Controllers;

[Authorize()]
public class ClientController : BaseController<Sabatex.ObjectsExchange.Models.Client>
{
    public ClientController(ObjectsExchangeDbContext context,ILogger<ClientController> logger,ClientManager clientManager) : base(context,logger,clientManager)
    {

    }
    protected override IQueryable<Sabatex.ObjectsExchange.Models.Client> OnBeforeGet(IQueryable<Sabatex.ObjectsExchange.Models.Client> query, QueryParams queryParams)
    {
        if (User.IsInRole("Administrator"))
            return base.OnBeforeGet(query, queryParams);
        else
            return base.OnBeforeGet(query, queryParams).Where(s => s.OwnerUser == User.Identity.Name);

    }
    protected override async Task OnBeforeAddItemToDatabase(Sabatex.ObjectsExchange.Models.Client item)
    {
        await base.OnBeforeAddItemToDatabase(item);
        if (item.OwnerUser != User.Identity.Name)
            throw new Exception("The access denied");
        if (await context.Clients.Where(s=>s.OwnerUser == User.Identity.Name).CountAsync() >= 5)
            throw new Exception("The try add client over limit 5!");
  
    }

    protected override async Task<bool> CheckAccess(Sabatex.ObjectsExchange.Models.Client item, Sabatex.ObjectsExchange.Models.Client? update)
    {
        await Task.Yield();
        return item.OwnerUser == User.Identity.Name;
    }
}