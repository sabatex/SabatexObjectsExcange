using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObjectsExchange.Client.Models;
using ObjectsExchange.Data;
using Radzen;
using Sabatex.RadzenBlazor;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace ObjectsExchange.Controllers;
[Authorize()]
public class ClientNodeController : BaseController<ObjectsExchange.Client.Models.ClientNode>
{
    public ClientNodeController(ObjectsExchangeDbContext context) : base(context)
    {

    }

    public override async Task<ODataServiceResult<ClientNode>> Get([FromBody] QueryParams args)
    {
        
        return await base.Get(args);
    }

}


