using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObjectsExchange.Data;
using Radzen;
using Sabatex.Core;
using Sabatex.RadzenBlazor;
using System.Linq.Dynamic.Core;
using System.Security.Claims;

namespace ObjectsExchange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseController<TItem> : ControllerBase where TItem : class, IEntityBase<Guid>, new()
    {
        protected readonly ObjectsExchangeDbContext context;
        protected readonly ILogger logger;
        protected BaseController(ObjectsExchangeDbContext context, ILogger logger)
        {
            this.context = context;
            this.logger = logger;
        }


        public Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);


        [HttpGet]
        public virtual async Task<ODataServiceResult<TItem>> Get([FromQuery] string json)
        {
            QueryParams queryParams = System.Text.Json.JsonSerializer.Deserialize<QueryParams>(Uri.UnescapeDataString(json));

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

        [HttpGet("{id}")]
        public virtual async Task<TItem> GetById([FromRoute]Guid id)
        {
            var query = context.Set<TItem>().AsQueryable<TItem>();
            query = BeforeGetById(query,id);
            return await query.Where(s=>s.Id == id).SingleAsync();

        }
        protected virtual IQueryable<TItem> BeforeGetById(IQueryable<TItem> query,Guid id)
        {
            return query;
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
                await context.Set<TItem>().AddAsync(value);
                await context.SaveChangesAsync();
                return Ok(value);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return BadRequest(ModelState);
            }
        }

        [HttpPatch]
        public virtual async Task<IActionResult> Patch([FromRoute] Guid key, JsonPatchDocument<TItem> delta)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = await context.Set<TItem>().FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            delta.ApplyTo(entity);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ValueExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Ok(entity);
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
            context.Entry(update).State = EntityState.Modified;
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
