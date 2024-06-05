using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ObjectsExchange.Data;
using ObjectsExchange.Services;
using Radzen;
using Sabatex.Core;
using Sabatex.RadzenBlazor;
using System.ComponentModel;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Text.Json;

namespace ObjectsExchange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseController<TItem> : ControllerBase where TItem : class, IEntityBase<Guid>, new()
    {
        protected readonly ObjectsExchangeDbContext context;
        protected readonly ILogger logger;
        protected readonly ClientManager clientManager;
        protected BaseController(ObjectsExchangeDbContext context, ILogger logger,ClientManager clientManager)
        {
            this.context = context;
            this.logger = logger;
            this.clientManager = clientManager;
        }


        public string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;


        [HttpGet]
        public virtual async Task<ODataServiceResult<TItem>> Get([FromQuery] string json)
        {
            QueryParams? queryParams = JsonSerializer.Deserialize<QueryParams>(Uri.UnescapeDataString(json));

            if (queryParams == null)
                throw new Exception("Deserialize error");
 
            var query = context.Set<TItem>().AsQueryable<TItem>();
            if (queryParams.Args.Skip != null)
                query = query.Skip(queryParams.Args.Skip.Value); 
            if (queryParams.Args.Top != null)
                query = query.Take(queryParams.Args.Top.Value);
            if (!String.IsNullOrEmpty(queryParams.Args.OrderBy))
            {
                query = query.OrderBy(queryParams.Args.OrderBy);
            }
            if (queryParams.ForeginKey != null)
            {
                query = query.Where($"{queryParams.ForeginKey.Name}==\"{queryParams.ForeginKey.Id}\"");
            }
            if (!String.IsNullOrEmpty(queryParams.Args.Filter))
                query = query.Where(queryParams.Args.Filter); 
            query = OnBeforeGet(query,queryParams);
            var result = new ODataServiceResult<TItem>();
            result.Value = await query.ToArrayAsync();
            if ((queryParams.Args.Skip != null) || (queryParams.Args.Top != null))
                result.Count = await query.CountAsync();
            return result;
        }
        protected virtual IQueryable<TItem> OnBeforeGet(IQueryable<TItem> query, QueryParams queryParams) 
        {
            return query;
        }

        
        protected virtual IQueryable<TItem> OnBeforeGetById(IQueryable<TItem> query,Guid id)
        {
            return query;
        }

        protected virtual async Task OnAfterGetById(TItem item, Guid id)
        {
            await Task.Yield();
        }

        protected abstract Task<bool> CheckAccess(TItem item,TItem? updated);
 

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetById([FromRoute]Guid id)
        {
            var query = context.Set<TItem>().AsQueryable<TItem>();
            query = OnBeforeGetById(query,id);
            var result  = await query.Where(s=>s.Id == id).SingleAsync();
            if (await CheckAccess(result,null))
            {
                await OnAfterGetById(result, id);
                return Ok(result);
            }
            return Unauthorized(); 
        }


        protected virtual async Task OnBeforeAddItemToDatabase(TItem item) => await Task.Yield();

        [HttpPost]
        public virtual async Task<IActionResult> Post([FromBody] TItem value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (value == null)
            {
                ModelState.AddModelError(string.Empty, "The post null value");
                return BadRequest(ModelState);
            }
            try
            {
                await this.OnBeforeAddItemToDatabase(value);
                if (await CheckAccess(value, null))
                {
                    await context.Set<TItem>().AddAsync(value);
                    await context.SaveChangesAsync();
                    return Ok(value);
                }
                return Unauthorized();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return BadRequest(ModelState);
            }
        }

        //[HttpPatch]
        //public virtual async Task<IActionResult> Patch([FromRoute] Guid key, JsonPatchDocument<TItem> delta)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }
        //    var entity = await context.Set<TItem>().FindAsync(key);
        //    if (entity == null)
        //    {
        //        return NotFound();
        //    }
        //    delta.ApplyTo(entity);
        //    try
        //    {
        //        await context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!ValueExists(key))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //    return Ok(entity);
        //}
        
        protected virtual async Task OnBeforeUpdateAsync(TItem item,TItem update)
        {
           await Task.Yield();
        }
        [HttpPut("{id}")]
        public virtual async Task<IActionResult> Put([FromRoute] Guid id, TItem update)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != update.Id)
            {
                return BadRequest();
            }

            var item = await context.Set<TItem>().FindAsync(id);
            if (item == null)
                return NotFound();

            if (!await CheckAccess(item,update))
                return Unauthorized(ModelState);
            
            await OnBeforeUpdateAsync(item,update);
            context.Entry(item).CurrentValues.SetValues(update);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ValueExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Ok(update);
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var product = await context.Set<TItem>().FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            context.Set<TItem>().Remove(product);
            await context.SaveChangesAsync();
            return new NoContentResult();
        }


        private bool ValueExists(Guid key)
        {
            return context.Set<TItem>().Any(p => p.Id == key);
        }



    }
}
