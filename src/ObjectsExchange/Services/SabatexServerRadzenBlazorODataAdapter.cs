using Microsoft.EntityFrameworkCore;
using ObjectsExchange.Data;
using Radzen;
using Sabatex.Core;
using Sabatex.RadzenBlazor;


namespace ObjectsExchange.Services
{
    public class SabatexServerRadzenBlazorODataAdapter<TKey> : ISabatexRadzenBlazorDataAdapter<TKey>
    {
        private readonly ObjectsExchangeDbContext context;
        public SabatexServerRadzenBlazorODataAdapter(ObjectsExchangeDbContext context)
        {
            this.context = context;
        }
        public Task DeleteAsync<TItem>(TKey id) where TItem : class,Sabatex.Core.IEntityBase<TKey>
        {
            throw new NotImplementedException();
        }

        public async Task<ODataServiceResult<TItem>> GetAsync<TItem>(string? filter, string? orderby, string? expand, int? top, int? skip, bool? count, string? format = null, string? select = null, string? ee = null) where TItem : class, IEntityBase<TKey>
        {
            var result = context.Set<TItem>().AsQueryable();
            return new ODataServiceResult<TItem> { Count = 0,Value = await result.ToArrayAsync() };
        }

        public Task<TItem> GetByIdAsync<TItem>(TKey id, string? expand = null) where TItem : class, Sabatex.Core.IEntityBase<TKey>
        {
            throw new NotImplementedException();
        }

        public Task<TItem> GetByIdAsync<TItem>(string id, string? expand = null) where TItem : class, Sabatex.Core.IEntityBase<TKey>
        {
            throw new NotImplementedException();
        }

        public Task<TItem> PostAsync<TItem>(TItem? item) where TItem : class, Sabatex.Core.IEntityBase<TKey>
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync<TItem>(TItem item) where TItem : class, Sabatex.Core.IEntityBase<TKey>
        {
            throw new NotImplementedException();
        }

        Task<ODataServiceResult<TItem>> ISabatexRadzenBlazorDataAdapter<TKey>.GetAsync<TItem>(QueryParams queryParams)
        {
            throw new NotImplementedException();
        }
    }
}
