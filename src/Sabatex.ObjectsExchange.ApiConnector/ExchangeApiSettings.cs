#if NET6_0_OR_GREATER
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Sabatex.ObjectsExchange.ApiConnector
{
    public class ExchangeApiSettings
    {
        /// <summary>
        /// Base api uri https://host{:port}
        /// </summary>
        public string BaseUri { get; set; }
        public string? AccessToken { get; set; }
        public string ClientId { get; set; }
        public string? RefreshToken { get; set; }
        public string Password { get; set; }
    }
}
#endif