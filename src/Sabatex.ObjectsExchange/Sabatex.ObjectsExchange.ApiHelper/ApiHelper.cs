namespace Sabatex.ObjectsExchange.ApiHelper
{
#if NET6_0_OR_GREATER
    using System.Net.Http;
    using System.Net.Http.Json;
    using System;
    using System.Threading.Tasks;
    public static class ApiHelper
    {

        /// <summary>
        /// login with api 0
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="password"></param>
        /// <returns>token</returns>
        public static async Task<bool> LoginAsync(this HttpClient client, Guid nodeId, string password)
        {
            var login = new Login { NodeId= nodeId, Password = password };
            var response = await client.PostAsJsonAsync("api/v0/login", login);
            if (!response.IsSuccessStatusCode)
                return false;
            var token = await response.Content.ReadAsStringAsync();
            client.DefaultRequestHeaders.Add("apiToken", token);
            return true;
        } 
    }
#else
#endif

 

   
}