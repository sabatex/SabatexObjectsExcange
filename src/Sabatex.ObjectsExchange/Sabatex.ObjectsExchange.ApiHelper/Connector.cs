namespace Sabatex.ObjectsExchange.ApiConnector
{
    #if NET5_0_OR_GREATER
    using System.Net.Http;
    using System.Net.Http.Json;
    using System;
    using System.Threading.Tasks;
    using System.Net;
    using System.Text.Json.Serialization;
    using sabatex.ObjectsExchange.Models;
    public static class Connector
    {
         /// <summary>
        /// Login to service
        /// </summary>
        /// <param name="host"></param>
        /// <param name="clientId"></param>
        /// <param name="password"></param>
        /// <returns>return API instance or null is fail</returns>
        public static async Task<bool> Login(this HttpClient httpClient, Guid clientId, string password)
        {
            var login = new { clientId = clientId, password = password };
            var response = await httpClient.PostAsJsonAsync("api/v0/login", login);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<object>();
            }
            return false;
         }

     }
#endif

}