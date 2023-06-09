// (c)Serhiy Lakas(https://sabatex.github.io)
namespace Sabatex.ObjectsExchange.ApiConnector
{
    using System;
    
 
    
    using Sabatex.ObjectsExchange.Models;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
   
    using System.Text;
    
#if NET35
#else
    //using System.Net.Http.Json;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

#endif
    public class ExchangeApiConnector : IDisposable
    {
        private string accessToken;
        private readonly Guid clientId;
        private string refreshToken;
        private Token userToken = null;
        private DateTime expired_token;
        private readonly Func<Task<string>> passwordGeter;
        private readonly Func<Token, Task> tokenUpdate;

        private readonly HttpClient httpClient;
        private readonly HttpClientHandler httpClientHandler;



        #region constructors
        public ExchangeApiConnector(ExchangeApiSettings settings, bool acceptFailCertificates, Func<Task<string>> passwordGetter=null,Func<Token,Task> tokenUpdate=null) : base()
        {
            accessToken = settings.AccessToken;
            clientId = new Guid(settings.ClientId);
            refreshToken = settings.RefreshToken;
            this.passwordGeter = passwordGetter;
            this.tokenUpdate = tokenUpdate;
            if (acceptFailCertificates )
            {
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                httpClient = new HttpClient(httpClientHandler);
            }
            else
            {
                httpClient = new HttpClient();
            }
            httpClient.BaseAddress = new Uri(settings.BaseUri);

        }
        #endregion

        #region Autorization
        private async Task UpdateAccessTokenAsync(Token token)
        {
            if (tokenUpdate != null)
                await tokenUpdate.Invoke(token);
            accessToken = token.AccessToken;
            refreshToken = token.RefreshToken;
            expired_token = DateTime.UtcNow + TimeSpan.FromSeconds(token.ExpiresIn);
            httpClient.DefaultRequestHeaders.Add("apiToken", token.AccessToken);
        }

        private async Task AutorizeAsync()
        {
            var login = new Login
            {
                ClientId = clientId,
                Password = await passwordGeter.Invoke()
            };
            var response = await httpClient.PostAsJsonAsync("api/v0/login", login);
            if (response == null) 
                throw new Exception();
            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadFromJsonAsync<Token>();
                if (token == null) return false;
                await UpdateAccessTokenAsync(token);
            }
           throw new Exception();
        }

        private async Task<bool> RefreshTokenAsync()
        {
            if (refreshToken == null) return false;
            var response = await httpClient.PostAsJsonAsync("api/v0/refresh_token", new
            Login
            {
                ClientId = clientId,
                Password = refreshToken
            });
            if (response.IsSuccessStatusCode)
            {
                var rt = await response.Content.ReadFromJsonAsync<Token>();
                if (rt == null) return false;
                await UpdateAccessTokenAsync(rt);
                return true;
            }
            return false;
        }

        /// <summary>
        /// speed check valid token and update invalid
        /// </summary>
        /// <returns></returns>
        private async Task CheckAutorized()
        {
            if (accessToken != null)
            {
                if (expired_token > DateTime.UtcNow)
                {
                    return; // token is valid
                }
                else
                {
                    if (refreshToken != null)
                    {
                        if (await RefreshTokenAsync()) return; // update access token
                    }
                }
            }
            await AutorizeAsync(); // login with password
        }
        #endregion
       
        #region IDisposable
        public void Dispose()
        {
            httpClient?.Dispose();
            httpClientHandler?.Dispose();
        }

        #endregion
        /// <summary>
        /// renev access_token by refresh token or login by password
        /// </summary>
        /// <returns></returns>

        private async Task<HttpResponseMessage> PostAsync<T>(string? uriString, T value)
        {
            await CheckAutorized(); // exception if unautorized
            var response = await httpClient.PostAsJsonAsync(uriString, value);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await AutorizeAsync();
                response = await httpClient.PostAsJsonAsync(uriString, value);
            }
            if (response.IsSuccessStatusCode) return response;
            throw new Exception($"The error post object:{value}");
        }

        private async Task<HttpResponseMessage> GetAsync(string? uriString)
        {
            await CheckAutorized(); // exception if unautorized
            var response = await httpClient.GetAsync(uriString);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await AutorizeAsync();
                response = await httpClient.GetAsync(uriString);
            }
            if (response.IsSuccessStatusCode) return response;
            throw new Exception($"The error get {uriString}");
        }

        public async Task<IEnumerable<ObjectExchange>> GetObjects(int take=10)
        {
            var responce = await GetAsync("api/v0/objects");
            return await responce.Content.ReadFromJsonAsync<IEnumerable<ObjectExchange>>();
            
        }


        public async Task PostObjectAsync(string objectType, string objectId, string text) =>
            await PostAsync("api/v0/objects",
                                             new {
                                                 objectType = objectType,
                                                 objectId = objectId,
                                                 text = text,
                                                 dateStamp = DateTime.UtcNow
                                             });



        public event Action<object>? OnTokenExpiration;


        public async Task<long> PostObjectExchangeAsync(PostObject data, Guid destinationId)
        {
            if (!(await IsTokenAlive()))
                throw new Exception("Unable to get object.");
            if (client.DefaultRequestHeaders.Contains("destinationId"))
                client.DefaultRequestHeaders.Remove("destinationId");
            client.DefaultRequestHeaders.Add("destinationId", destinationId.ToString());
            var result = await client.PostAsJsonAsync("api/v0/objects", data);
            if (result is not null && result.IsSuccessStatusCode)
            {
                return long.Parse(await result.Content.ReadAsStringAsync());
            }
            else throw new Exception("Unable to get object.");
        }

        public async Task<long> ApiPostObjectExchangeAsync(string objectType, string objectId, string text, DateTime dateStamp, Guid destinationId)
        {
            if (!(await IsTokenAlive())) throw new Exception("Unable to get object.");
            if (client.DefaultRequestHeaders.Contains("destinationId")) client.DefaultRequestHeaders.Remove("destinationId");
            client.DefaultRequestHeaders.Add("destinationId", destinationId.ToString());
            var result = await client.PostAsJsonAsync("api/v0/objects", new { objectType, objectId, text, dateStamp });
            if (result is not null && result.IsSuccessStatusCode)
            {
                return long.Parse(await result.Content.ReadAsStringAsync());
            }
            else throw new Exception("Unable to get object.");
        }

        public async Task<ObjectExchange[]?> ApiGetObjectExchangeAsync()
        {
            if (!(await IsTokenAlive())) return null;
            return await client.GetFromJsonAsync<ObjectExchange[]>("api/v0/objects");
        }

        public async Task<ObjectExchange[]?> ApiGetObjectExchangeAsync(int take = 10)
        {
            if (!(await IsTokenAlive())) return null;
            return await client.GetFromJsonAsync<ObjectExchange[]>($"api/v0/objects?take={take}");
        }

        public async Task<ObjectExchange[]?> ApiGetObjectExchangeAsync(string nodeName, int take = 10)
        {
            if (!(await IsTokenAlive())) return null;
            return await client.GetFromJsonAsync<ObjectExchange[]>($"api/v0/objects?take={take}&nodeName={nodeName}");
        }

        public async Task<bool> ApiDeleteObjectExchangeAsync(long deletedId)
        {
            if (!(await IsTokenAlive())) return false;
            var result = await client.DeleteAsync($"api/v0/objects/{deletedId}");
            return result is not null && result.IsSuccessStatusCode;
        }

        public async Task<bool> ApiDeleteObjectsExchangeAsync()
        {
            if (!(await IsTokenAlive())) return false;
            var got = await client.GetFromJsonAsync<ObjectExchange[]>("api/v0/objects") ?? Array.Empty<ObjectExchange>();
            foreach (var current in got)
            {
                bool temporary = await ApiDeleteObjectExchangeAsync(current.Id);
                if (temporary == false) return false;
            }
            return true;
        }

        public async Task<long> ApiPostQueryAsync(PostObject data, Guid destinationId)
        {
            if (!(await IsTokenAlive())) throw new Exception("Unable to get object.");
            var result = await client.PostAsJsonAsync("api/v0/queries", data);
            if (result is not null && result.IsSuccessStatusCode)
            {
                return long.Parse(await result.Content.ReadAsStringAsync());
            }
            else throw new Exception("Unable to get object.");
        }

        public async Task<long> ApiPostQueryAsync(string objectType, string objectId, Guid destinationId)
        {
            if (!(await IsTokenAlive())) throw new Exception("Unable to get object.");
            var result = await client.PostAsJsonAsync("api/v0/queries", new { objectType, objectId });
            if (result is not null && result.IsSuccessStatusCode)
            {
                return long.Parse(await result.Content.ReadAsStringAsync());
            }
            else throw new Exception("Unable to get object.");
        }

        public async Task<QueryObject[]?> ApiGetQueryObjectAsync()
        {
            if (!(await IsTokenAlive())) return null;
            // Ask if line lower has any use in this context.
            if (client.DefaultRequestHeaders.Contains("destinationId")) client.DefaultRequestHeaders.Remove("destinationId");
            return await client.GetFromJsonAsync<QueryObject[]>($"api/v0/queries");
        }

        public async Task<QueryObject[]?> ApiGetQueryObjectAsync(int take = 10)
        {
            if (!(await IsTokenAlive())) return null;
            // Ask if line lower has any use in this context.
            if (client.DefaultRequestHeaders.Contains("destinationId")) client.DefaultRequestHeaders.Remove("destinationId");
            return await client.GetFromJsonAsync<QueryObject[]>($"api/v0/queries?take={take}");
        }

        public async Task<QueryObject[]?> ApiGetQueryObjectAsync(string nodeName, int take = 10)
        {
            if (!(await IsTokenAlive())) return null;
            // Ask if line lower has any use in this context.
            if (client.DefaultRequestHeaders.Contains("destinationId")) client.DefaultRequestHeaders.Remove("destinationId");
            return await client.GetFromJsonAsync<QueryObject[]>($"api/v0/queries?take={take}&nodeName={nodeName}");
        }

        public async Task<bool> ApiDeleteQueryObjectAsync(long deletedId)
        {
            if (!(await IsTokenAlive())) return false;
            // Ask if line lower has any use in this context.
            if (client.DefaultRequestHeaders.Contains("destinationId")) client.DefaultRequestHeaders.Remove("destinationId");
            var result = await client.DeleteAsync($"api/v0/queries/{deletedId}");
            return result is not null && result.IsSuccessStatusCode;
        }
    }
}
