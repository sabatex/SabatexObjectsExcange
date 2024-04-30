using Microsoft.EntityFrameworkCore;
using ObjectsExchange.Client.Services;
using ObjectsExchange.Data;
using Radzen;
using Sabatex.Core;
using Sabatex.RadzenBlazor;
using System.Net.Http;


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

        public Task<TItem> GetByIdAsync<TItem>(Guid id, string? expand = null) where TItem : class, Sabatex.Core.IEntityBase<Guid>
        {
            throw new NotImplementedException();
        }

        public Task<TItem> GetByIdAsync<TItem>(string id, string? expand = null) where TItem : class, Sabatex.Core.IEntityBase<Guid>
        {
            throw new NotImplementedException();
        }

        public Task<SabatexValidationModel<TItem>> PostAsync<TItem>(TItem? item) where TItem : class, Sabatex.Core.IEntityBase<Guid>
        {
            throw new NotImplementedException();
        }

        public Task<SabatexValidationModel<TItem>> UpdateAsync<TItem>(TItem item) where TItem : class, Sabatex.Core.IEntityBase<Guid>
        {
            throw new NotImplementedException();
        }

        Task<ODataServiceResult<TItem>> ISabatexRadzenBlazorDataAdapter<Guid>.GetAsync<TItem>(QueryParams queryParams)
        {
            throw new NotImplementedException();
        }
        const string FileName = "Readme.md";
        public async Task<string> GetReadmeAsync()
        {
            await Task.Yield();
            return $"Please wait ...";
        }


    }
}
