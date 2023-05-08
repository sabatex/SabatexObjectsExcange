#if NET6_0_OR_GREATER
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Sabatex.ObjectsExchange.ApiConnector
{
    public class ExchangeApiConnector:IDisposable
    {
        private string? accessToken;
        private readonly string clientId;
        private string? refreshToken;
        private readonly Func<string> passwordGeter;
        private readonly DateTime expired_token;

        private readonly HttpClient httpClient;
        private readonly HttpClientHandler httpClientHandler;

        public static ExchangeApiConnector GetApiConnector(string baseUri, ExchangeApiSettings settings,bool acceptFailCertificates,Func<string> getPassword)
        {
            return new ExchangeApiConnector(settings,acceptFailCertificates,getPassword);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private ExchangeApiConnector(ExchangeApiSettings settings, bool acceptFailCertificates,Func<string> passwordGetter) :base()
        {
            accessToken = settings.AccessToken;
            clientId = settings.ClientId;
            refreshToken = settings.RefreshToken;
            this.passwordGeter = passwordGeter;
            httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
            httpClient = new HttpClient(httpClientHandler);
            httpClient.BaseAddress = new Uri(settings.BaseUri);

        }

        private async Task<bool> AutorizeAsync()
        {
            return true;
        }

        private async Task<bool> RefreshTokenAsync()
        {
            if (refreshToken == null) return false;
            var response = await httpClient.PostAsJsonAsync("api/v0/refresh_token",new
            {
                clientId = clientId,
                password = refreshToken
            });
            if (response.IsSuccessStatusCode)
            {
                //var rt = await response.Content.ReadFromJsonAsync<Token>();
                return true;
            }
            return true;

        }

        /// <summary>
        /// check exist access valid access token
        /// </summary>
        /// <returns></returns>
        private async Task CheckAutorized()
        {
            if (accessToken != null && expired_token > DateTime.UtcNow) return;
            if (accessToken != null)
            {
                if (expired_token > DateTime.UtcNow)
                    return;
                else
                return;

            } 



            if (accessToken == null) 
            {
                await AutorizeAsync();
            }
            
        }
        /// <summary>
        /// renev access_token by refresh token or login by password
        /// </summary>
        /// <returns></returns>
 
        private async Task<HttpResponseMessage> PostAsync<T>(string? uriString,T value)
        {
            await CheckAutorized();
            var response = await httpClient.PostAsJsonAsync(uriString,value);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await AutorizeAsync();
                response = await httpClient.PostAsJsonAsync(uriString, value);
            }
            if (response.IsSuccessStatusCode) return response;
            throw new Exception($"The error post object:{value}");
        }

        public  async Task PostObjectAsync(string objectType,string objectId,string text)=>
            await PostAsync("api/v0/objects",
                                             new {
                                                 objectType = objectType,
                                                 objectId = objectId,
                                                 text = text,
                                                 dateStamp = DateTime.UtcNow 
                                             });


    }
}
#endif