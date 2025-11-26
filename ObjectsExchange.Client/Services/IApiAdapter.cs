using Sabatex.Core.RadzenBlazor;
using Sabatex.RadzenBlazor;

namespace ObjectsExchange.Client.Services
{
    public interface IApiAdapter: ISabatexRadzenBlazorDataAdapter
    {
        public Task<string> GetReadmeAsync();
        Task<string> GetDataBaseBackupAsync();
    }
}
