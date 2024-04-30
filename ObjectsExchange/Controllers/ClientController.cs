using Markdig.Syntax;
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
public class ClientController : BaseController<ObjectsExchange.Client.Models.Client>
{
    public ClientController(ObjectsExchangeDbContext context,ILogger<ClientController> logger,ClientManager clientManager) : base(context,logger,clientManager)
    {

    }
    protected override IQueryable<Client.Models.Client> OnBeforeGet(IQueryable<Client.Models.Client> query, QueryParams queryParams)
    {
        if (User.IsInRole("Administrator"))
            return base.OnBeforeGet(query, queryParams);
        else
            return base.OnBeforeGet(query, queryParams).Where(s => s.UserId == UserId);

    }
    protected override async Task OnBeforeAddItemToDatabase(Client.Models.Client item)
    {
        await base.OnBeforeAddItemToDatabase(item);
        if (item.UserId != UserId)
            throw new Exception("The access denied");
        if (await context.Clients.Where(s=>s.UserId == UserId).CountAsync() >= 5)
            throw new Exception("The try add client over limit 5!");
  
    }

    protected override async Task<bool> CheckAccess(Client.Models.Client item, Client.Models.Client? update)
    {
        await Task.Yield();
        return item.UserId == UserId;
    }
}