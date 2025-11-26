using Microsoft.AspNetCore.Components;
using Sabatex.RadzenBlazor;
using System;

namespace ObjectsExchange.Client.Services
{
    public class ApiAdapter:SabatexRadzenBlazorApiDataAdapter,IApiAdapter
    {
        public ApiAdapter(HttpClient httpClient, ILogger<SabatexRadzenBlazorApiDataAdapter> logger, NavigationManager navigationManager):base(httpClient, logger, navigationManager) { }

        public async Task<string> GetDataBaseBackupAsync()
        {
            return await httpClient.GetStringAsync(new Uri(baseUri, "v1/DataBaseBackup"));
        }

        public async Task<string> GetReadmeAsync()
        {

            return await httpClient.GetStringAsync(new Uri(baseUri, "readme"));


        }




    }
}
