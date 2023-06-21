// (c)Serhiy Lakas(https://sabatex.github.io)
namespace Sabatex.ObjectsExchange.ApiConnector
{
    using System;
    using Sabatex.ObjectsExchange.Models;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Sabatex.ObjectsExchange.ApiConnector.Common;

    public delegate Task<string> PasswordGetterDelegateAsync();
    public delegate Task<string> RefreshTokenGetterDelegateAsync();
    public delegate Task UpdateTokenDelegateAsync(Token token,DateTime expiredToken);



    public  class ExchangeApiConnector : ExchangeApiConnectorBase,IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly HttpClientHandler? httpClientHandler;
        private readonly PasswordGetterDelegateAsync passwordGetterAsync;
        private readonly UpdateTokenDelegateAsync tokenUpdateAsync;
        private readonly RefreshTokenGetterDelegateAsync refreshTokenGetter;
        protected AuthenticationHeaderValue GetAuthenticationHeaderValue => new AuthenticationHeaderValue($"Bearer {accessToken}");

        #region constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="acceptFailCertificates"></param>
        /// <param name="passwordGetter"></param>
        /// <param name="tokenUpdate"></param>
        public ExchangeApiConnector(string baseUri, Guid clientId,Guid destinationId, string accessToken, DateTime expired_token, bool acceptFailCertificates, PasswordGetterDelegateAsync passwordGetter,RefreshTokenGetterDelegateAsync refreshTokenGetter, UpdateTokenDelegateAsync tokenUpdate) : base(baseUri,clientId,destinationId,accessToken,expired_token)
        {

            this.passwordGetterAsync = passwordGetter;
            this.tokenUpdateAsync = tokenUpdate;
            this.refreshTokenGetter = refreshTokenGetter;
            if (acceptFailCertificates )
            {

                httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                httpClient = new HttpClient(httpClientHandler);
            }
            else
            {
                httpClient = new HttpClient();
            }
            httpClient.BaseAddress = new Uri(this.BaseUri);
            httpClient.DefaultRequestHeaders.Add("apiToken", accessToken);
            httpClient.DefaultRequestHeaders.Add("clientId", this.clientId.ToString());
            httpClient.DefaultRequestHeaders.Add("destinationId", this.destinationId.ToString());
       }

#endregion

        #region Autorization

        private async Task UpdateAccessTokenAsync(Token? token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));
            
            accessToken = token.AccessToken;
            expired_token = DateTime.UtcNow + TimeSpan.FromSeconds(token.ExpiresIn);

            if (tokenUpdateAsync != null)
                await tokenUpdateAsync.Invoke(token,expired_token);
            httpClient.DefaultRequestHeaders.Remove("apiToken");
            httpClient.DefaultRequestHeaders.Add("apiToken",accessToken);
        }

        private async Task AutorizeAsync()
        {

            var login = new Login
            {
                ClientId = clientId,
                Password = await passwordGetterAsync.Invoke()
            };
            var response = await httpClient.PostAsJsonAsync("api/v0/login", login);
            if (response == null) 
                throw new Exception();
            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadFromJsonAsync<Token>();
                if (token == null)
                    throw new Exception();
                await UpdateAccessTokenAsync(token);
                return;
            }
           throw new Exception();
        }
        private async Task<bool> RefreshTokenAsync()
        {
            if (refreshTokenGetter == null)
                return false;
            var refreshToken = await refreshTokenGetter.Invoke();
            if (refreshToken == null) return false;
            var response = await httpClient.PostAsJsonAsync(refreshTokenUrl, new
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
        private async Task CheckAutorizedAsync()
        {
            if (accessToken != null)
            {
                if (expired_token > DateTime.UtcNow)
                {
                    return; // token is valid
                }
                else
                {
                    if (await RefreshTokenAsync())
                        return; // update access token
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
            await CheckAutorizedAsync(); // exception if unautorized
            var response = await httpClient.PostAsJsonAsync(uriString, value);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await AutorizeAsync();
                response = await httpClient.PostAsJsonAsync(uriString, value);
            }
            if (response.IsSuccessStatusCode) return response;
            throw new Exception($"The error post object:{value}");
        }

        private async Task<HttpResponseMessage> GetAsync(string? uriString, int take = 10)
        {
            await CheckAutorizedAsync(); // exception if unautorized
            var response = await httpClient.GetAsync($"{uriString}?take={take}");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await AutorizeAsync();
                response = await httpClient.GetAsync(uriString);
            }
            if (response.IsSuccessStatusCode) return response;
            throw new Exception($"The error get {uriString}");
        }

        private async Task DeleteAsync(string uriString, long id)
        {
            await CheckAutorizedAsync(); // exception if unautorized
            var response = await httpClient.DeleteAsync($"{uriString}/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await AutorizeAsync();
                response = await httpClient.DeleteAsync($"{uriString}/{id}"); 
            }
            if (response.IsSuccessStatusCode)
                return;
            throw new Exception();

        }

        #region Objects API
        /// <summary>
        /// 
        /// </summary>
        /// <param name="take"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<IEnumerable<ObjectExchange>> GetObjectsAsync(int take=10)
        {
            var responce = await GetAsync(objectsUrl,take);
            if (responce != null)
            {
                if (responce.IsSuccessStatusCode)
                {
                    var result = await responce.Content.ReadFromJsonAsync<IEnumerable<ObjectExchange>>();
                    if (result != null)
                        return result;
                }
            }
            throw new Exception();
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="objectId"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public async Task PostObjectAsync(string objectType, string objectId, string text) =>
        await PostAsync(objectsUrl,
                                             new {
                                                 objectType = objectType,
                                                 objectId = objectId,
                                                 text = text,
                                                 dateStamp = DateTime.UtcNow
                                             });
        public async Task DeleteObjectAsync(long id)=>await DeleteAsync(objectsUrl, id);
        #endregion

        #region Query
        public async Task PostQueryObjectAsync(string objectType, string objectId) =>
            await PostAsync(queryUrl, new { objectType, objectId });

        public async Task<IEnumerable<QueryObject>?> GetQueryObjectsAsync(int take)
        {
            var responce = await GetAsync(queryUrl, take);
            if (responce != null)
            {
                if (responce.IsSuccessStatusCode)
                {
                    var result = await responce.Content.ReadFromJsonAsync<IEnumerable<QueryObject>>();
                    if (result != null)
                        return result;
                }
            }
            throw new Exception();

        }

        public async Task DeleteQueryObjectAsync(long id) => await DeleteAsync(queryUrl, id);

#endregion







 




    }
}
