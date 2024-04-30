using Sabatex.RadzenBlazor;

namespace ObjectsExchange.Client.Services
{
    public interface IApiAdapter: ISabatexRadzenBlazorDataAdapter<Guid>
    {
        public Task<string> GetReadmeAsync();
    }
}
