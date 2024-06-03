using Microsoft.EntityFrameworkCore;
using ObjectsExchange.Client.Services;
using ObjectsExchange.Data;
using Radzen;
using Sabatex.Core;
using Sabatex.RadzenBlazor;
using System.Net.Http;
using System.Linq.Dynamic.Core;


namespace ObjectsExchange.Services
{
    public class ApiAdapter :IApiAdapter
    {
        private readonly ObjectsExchangeDbContext context;
        public ApiAdapter(ObjectsExchangeDbContext context)
        {
            this.context = context;
        }
        public Task DeleteAsync<TItem>(Guid id) where TItem : class,Sabatex.Core.IEntityBase<Guid>
        {
            throw new NotImplementedException();
        }

        public async Task<ODataServiceResult<TItem>> GetAsync<TItem>(string? filter, string? orderby, string? expand, int? top, int? skip, bool? count, string? format = null, string? select = null, string? ee = null) where TItem : class, IEntityBase<Guid>
        {
            var result = context.Set<TItem>().AsQueryable();
            return new ODataServiceResult<TItem> { Count = 0,Value = await result.ToArrayAsync() };
        }


        protected  async Task<bool> CheckAccess<TItem>(TItem item, TItem? updated)
        {
            await Task.Yield();
            return true;
        }

        public async Task<TItem> GetByIdAsync<TItem>(Guid id, string? expand = null) where TItem : class, Sabatex.Core.IEntityBase<Guid>
        {
            var query = context.Set<TItem>().AsQueryable<TItem>();
            //query = OnBeforeGetById(query, id);
            var result = await query.Where(s => s.Id == id).SingleAsync();
            if (await CheckAccess(result, null))
            {
                return result;
            }
            return null;
         }

        public async Task<TItem> GetByIdAsync<TItem>(string id, string? expand = null) where TItem : class, Sabatex.Core.IEntityBase<Guid>
        {
            var query = context.Set<TItem>().AsQueryable<TItem>();
            var idGuid = Guid.Parse(id);
            //query = OnBeforeGetById(query, id);
            var result = await query.Where(s => s.Id == idGuid).SingleAsync();
            if (await CheckAccess(result, null))
            {
                return result;
            }
            return null;

        }

        public Task<SabatexValidationModel<TItem>> PostAsync<TItem>(TItem? item) where TItem : class, Sabatex.Core.IEntityBase<Guid>
        {
            throw new NotImplementedException();
        }

        public Task<SabatexValidationModel<TItem>> UpdateAsync<TItem>(TItem item) where TItem : class, Sabatex.Core.IEntityBase<Guid>
        {
            throw new NotImplementedException();
        }

        async Task<ODataServiceResult<TItem>> ISabatexRadzenBlazorDataAdapter<Guid>.GetAsync<TItem>(QueryParams queryParams)
        {
            return new ODataServiceResult<TItem> { };
        }
        const string FileName = "Readme.md";
        public async Task<string> GetReadmeAsync()
        {
            await Task.Yield();
            return $"Please wait ...";
        }

        public Task<string> GetDataBaseBackupAsync()
        {
            throw new NotImplementedException();
        }
    }
}
