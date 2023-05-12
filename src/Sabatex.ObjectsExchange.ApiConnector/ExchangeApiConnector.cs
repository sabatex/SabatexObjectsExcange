//(c) Serhiy Lakas (https://sabatex.github.io)
namespace Sabatex.ObjectsExchange.ApiConnector
{


#if NET6_0_OR_GREATER
#nullable enable
    using System;
    using Sabatex.ObjectsExchange.Models;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text;
    using System.Threading.Tasks;

    public class ExchangeApiConnector : IDisposable
    {
        private string? accessToken;
        private readonly Guid clientId;
        private string? refreshToken;
        private readonly Func<Task<string>> passwordGeter;
        private readonly Func<Token, Task> tokenUpdate;
        private DateTime? expired_token;

        private readonly HttpClient httpClient;
        private readonly HttpClientHandler? httpClientHandler;


        public void Dispose()
        {
            httpClient?.Dispose();
            httpClientHandler?.Dispose();
        }
        #region constructor
        public ExchangeApiConnector(ExchangeApiSettings settings, bool acceptFailCertificates, Func<string> passwordGetter) : base()
        {
            accessToken = settings.AccessToken;
            clientId = new Guid(settings.ClientId);
            refreshToken = settings.RefreshToken;
            this.passwordGeter = passwordGeter;
            httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
            httpClient = new HttpClient(httpClientHandler);
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

        private async Task<bool> AutorizeAsync()
        {
            var login = new Login
            {
                ClientId = clientId,
                Password = await passwordGeter.Invoke()
            };
            var response = await httpClient.PostAsJsonAsync("api/v0/login", login);
            if (response == null) return false;
            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadFromJsonAsync<Token>();
                if (token == null) return false;
                await UpdateAccessTokenAsync(token);
                return true;
            }
            return false;
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


        /// <summary>
        /// renev access_token by refresh token or login by password
        /// </summary>
        /// <returns></returns>

        private async Task<HttpResponseMessage> PostAsync<T>(string? uriString, T value)
        {
            await CheckAutorized();
            var response = await httpClient.PostAsJsonAsync(uriString, value);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await AutorizeAsync();
                response = await httpClient.PostAsJsonAsync(uriString, value);
            }
            if (response.IsSuccessStatusCode) return response;
            throw new Exception($"The error post object:{value}");
        }

        public async Task PostObjectAsync(string objectType, string objectId, string text) =>
            await PostAsync("api/v0/objects",
                                             new {
                                                 objectType = objectType,
                                                 objectId = objectId,
                                                 text = text,
                                                 dateStamp = DateTime.UtcNow
                                             });


    }

#endif
}
