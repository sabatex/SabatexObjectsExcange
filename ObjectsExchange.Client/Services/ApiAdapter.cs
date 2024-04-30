using Microsoft.AspNetCore.Components;
using Sabatex.RadzenBlazor;
using System;

namespace ObjectsExchange.Client.Services
{
    public class ApiAdapter:SabatexRadzenBlazorApiDataAdapter<Guid>,IApiAdapter
    {
        public ApiAdapter(HttpClient httpClient, ILogger<SabatexRadzenBlazorODataAdapter<Guid>> logger, NavigationManager navigationManager):base(httpClient, logger, navigationManager) { }
 
        public async Task<string> GetReadmeAsync()
        {

            return await httpClient.GetStringAsync(new Uri(baseUri, "readme"));


        }




    }
}
