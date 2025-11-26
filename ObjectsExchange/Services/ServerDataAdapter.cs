using Sabatex.Core.RadzenBlazor;

namespace ObjectsExchange.Services;

public class ServerDataAdapter : ISabatexRadzenBlazorDataAdapter
{
    Task ISabatexRadzenBlazorDataAdapter.DeleteAsync<TItem, TKey>(TKey id)
    {
        throw new NotImplementedException();
    }

    Task<QueryResult<TItem>> ISabatexRadzenBlazorDataAdapter.GetAsync<TItem, TKey>(QueryParams queryParams)
    {
        throw new NotImplementedException();
    }

    Task<TItem?> ISabatexRadzenBlazorDataAdapter.GetByIdAsync<TItem, TKey>(TKey id, string? expand) where TItem : class
    {
        throw new NotImplementedException();
    }

    Task<TItem?> ISabatexRadzenBlazorDataAdapter.GetByIdAsync<TItem, TKey>(string id, string? expand) where TItem : class
    {
        throw new NotImplementedException();
    }

    Task<SabatexValidationModel<TItem>> ISabatexRadzenBlazorDataAdapter.PostAsync<TItem, TKey>(TItem? item) where TItem : class
    {
        throw new NotImplementedException();
    }

    Task<SabatexValidationModel<TItem>> ISabatexRadzenBlazorDataAdapter.UpdateAsync<TItem, TKey>(TItem item)
    {
        throw new NotImplementedException();
    }
}
