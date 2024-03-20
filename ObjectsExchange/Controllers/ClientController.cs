using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ObjectsExchange.Data;
using Sabatex.RadzenBlazor;
using System.Security.Claims;

namespace ObjectsExchange.Controllers;

[Authorize()]
public class ClientController : BaseController<ObjectsExchange.Client.Models.Client>
{
    public ClientController(ObjectsExchangeDbContext context) : base(context)
    {

    }
    protected override IQueryable<Client.Models.Client> BeforeGet(IQueryable<Client.Models.Client> query, QueryParams queryParams)
    {
        if (User.IsInRole("Administrator"))
            return base.BeforeGet(query, queryParams);
        else
            return base.BeforeGet(query, queryParams).Where(s => s.UserId == UserId);

    }
    protected override async Task OnBeforeAddItemToDatabase(Client.Models.Client item)
    {
        await base.OnBeforeAddItemToDatabase(item);
        item.UserId = UserId;
    }
}