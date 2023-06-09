namespace Sabatex.ObjectsExchange.Models
{
#if NET6_0_OR_GREATER
    using System.Text.Json.Serialization;
#else
    using Newtonsoft.Json;
#endif
    /// <summary>
    /// The security token
    /// </summary>
    public class Token
    {
        /// <summary>
        /// Access token string 
        /// </summary>
#if NET6_0_OR_GREATER
        [JsonPropertyName("access_token")]
#else
        [JsonProperty("access_token")]
#endif
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Token type, defaut Bearer
        /// </summary>
#if NET6_0_OR_GREATER
        [JsonPropertyName("token_type")]
#else
        [JsonProperty("token_type")]
#endif
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// Refresh token, used for refresh token after expired
        /// </summary>
#if NET6_0_OR_GREATER
        [JsonPropertyName("refresh_token")]
#else
        [JsonProperty("refresh_token")]
#endif
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Expired time by seconds
        /// </summary>
#if NET6_0_OR_GREATER
        [JsonPropertyName("expires_in")]
#else
        [JsonProperty("expires_in")]
#endif
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Connstructor
        /// </summary>
        /// <param name="accessToken">Access token string</param>
        /// <param name="refreshToken">Refresh token, used for refresh token after expired</param>
        /// <param name="expiresIn">Expired time by seconds</param>
        /// <param name="tokenType">Token type, defaut Bearer</param>
        public Token(string accessToken, string refreshToken, int expiresIn, string tokenType = "Bearer")
        {
            AccessToken = accessToken ?? string.Empty;
            RefreshToken = refreshToken ?? string.Empty;
            TokenType = tokenType;
            ExpiresIn = expiresIn;
        }

    }
}
