//(c) Serhiy Lakas (https://sabatex.github.io)
namespace Sabatex.ObjectsExchange.ApiConnector
{
#if NET3_5
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using System;
    using System.Collections;
    using System.Net;
    using System.Text;
    using System.Web;
    using static System.Net.Mime.MediaTypeNames;
    using Sabatex.ObjectsExchange.Models;
    public class ExchangeApiConnector
    {
        static private Token _token = null;

        private readonly Guid clientId;
        private readonly string token;
        private readonly Uri host;

        private ExchangeApiConnector(Uri host,Guid clientId,string token)
        {
            this.host = host;
            this.clientId = clientId;
            this.token = token;
        }



        /// <summary>
        /// Login to service
        /// </summary>
        /// <param name="host"></param>
        /// <param name="clientId"></param>
        /// <param name="password"></param>
        /// <returns>return API instance or null is fail</returns>
        public static ExchangeApiConnector Login(Uri host,Guid clientId, string password)
        {
            var login = new { clientId = clientId, password = password };
            var url = new Uri(host, "api/v0/login");
            var webClient = new WebClient();
            webClient.Headers.Add("Content-Type", "application/json; charset=utf-8");
            webClient.Headers.Add("accept", "*/*");
            var s = JsonConvert.SerializeObject(login);
            try
            {
                var token = webClient.UploadString(url, "POST", s);
                _token = JsonConvert.DeserializeObject<Token>(token);
                return new ExchangeApiConnector(host, clientId, _token.access_token);

            }
            catch (Exception ex)
            {
                throw new Exception($"Login failed: {ex.Message}");
            }
        }
 
        
        /// <summary>
        ///  POST object to exchange
        /// </summary>
        /// <param name="destinationId"></param>
        /// <param name="objectType"></param>
        /// <param name="objectId"></param>
        /// <param name="text"></param>
        /// <exception cref="Exception">Failed login to service</exception> 
        public void PostObjects(Guid destinationId, string objectType, string objectId, string text)
        {
            var url = new Uri(host, "api/v0/objects");
            var webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
            webClient.Headers.Add("Content-Type", "application/json; charset=utf-8");
            webClient.Headers.Add("accept", "*/*");
            webClient.Headers.Add("clientId", clientId.ToString());
            webClient.Headers.Add("destinationId", destinationId.ToString());
            webClient.Headers.Add("apiToken", token);
            try
            {
                var obj = new { objectType = objectType, objectId = objectId, text = text ,dateStamp=DateTime.UtcNow};
                string t = JsonConvert.SerializeObject(obj);
                webClient.UploadString(url, "POST", t);
            }
            catch (Exception ex)
            {
                throw new Exception($"POST object to service : {ex.ToString()}");
            }
        }
        /// <summary>
        /// Get objects from service
        /// </summary>
        /// <param name="take"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ObjectExchange[] GetObjects(int take = 10)
        {           
            var url = new Uri(host, $"api/v0/objects?take={take}");
            var webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
            webClient.Headers.Add("Content-Type", "application/json; charset=utf-8");
            webClient.Headers.Add("accept", "*/*");
            webClient.Headers.Add("clientId", clientId.ToString());
            webClient.Headers.Add("apiToken", token);
            try
            {
                string s = webClient.DownloadString(url);
                //var result = JsonConvert.DeserializeObject<ObjectExchange>(s,new JsonSerializerSettings {ContractResolver =new DefaultContractResolver { NamingStrategy=new CamelCaseNamingStrategy()} });
                return JsonConvert.DeserializeObject<ObjectExchange[]>(s);


            }
            catch (Exception ex)
            {
                throw new Exception($"GET objects to service : {ex.ToString()}");
            }
        }

        public void DeleteObject(long id)
        {
            var url = new Uri(host, $"api/v0/objects/{id}");
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "DELETE";
            request.Accept="*/*";
            request.Headers["clientId"]= clientId.ToString();
            request.Headers["apiToken"] = token;
            try
            {
                var responce = request.GetResponse();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error delete object from service : {ex.ToString()}");
            }

        }

        public void PostQuery(Guid destinationId, string objectType, string objectId)
        {
            var url = new Uri(host, "api/v0/queries");
            var webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
            webClient.Headers.Add("Content-Type", "application/json; charset=utf-8");
            webClient.Headers.Add("accept", "*/*");
            webClient.Headers.Add("clientId", clientId.ToString());
            webClient.Headers.Add("destinationId", destinationId.ToString());
            webClient.Headers.Add("apiToken", token);
            try
            {
                var obj = new { objectType = objectType, objectId = objectId};
                string t = JsonConvert.SerializeObject(obj);
                webClient.UploadString(url, "POST", t);
            }
            catch (Exception ex)
            {
                throw new Exception($"POST object to service : {ex.ToString()}");
            }
        }

        public QueryObject[] GetQueries(int take = 10)
        {
            var url = new Uri(host, $"api/v0/queries?take={take}");
            var webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
            webClient.Headers.Add("Content-Type", "application/json; charset=utf-8");
            webClient.Headers.Add("accept", "*/*");
            webClient.Headers.Add("clientId", clientId.ToString());
            webClient.Headers.Add("apiToken", token);
            try
            {
                string s = webClient.DownloadString(url);
                //var result = JsonConvert.DeserializeObject<ObjectExchange>(s,new JsonSerializerSettings {ContractResolver =new DefaultContractResolver { NamingStrategy=new CamelCaseNamingStrategy()} });
                return JsonConvert.DeserializeObject<QueryObject[]>(s);


            }
            catch (Exception ex)
            {
                throw new Exception($"GET queries from service : {ex.ToString()}");
            }
        }

        public void DeleteQuery(long id)
        {
            var url = new Uri(host, $"api/v0/queries/{id}");
            //var request = HttpWebRequest.Create(url);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "DELETE";
            request.Accept = "*/*";
            request.Headers["clientId"] = clientId.ToString();
            request.Headers["apiToken"] = token;

            try
            {
                var responce = request.GetResponse();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error delete query from service : {ex.ToString()}");
            }

        }

    }
#endif
}
