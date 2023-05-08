namespace Sabatex.ObjectsExchange.Models
{
#if NET6_0_OR_GREATER
#nullable enable
    using System.Text.Json.Serialization;
    public class Token
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = "Bearer";
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        public Token(string accessToken, string refreshToken, int expiresIn, string tokenType = "Bearer")
        {
            AccessToken = accessToken ?? string.Empty;
            RefreshToken = refreshToken ?? string.Empty;
            TokenType = tokenType;
            ExpiresIn = expiresIn;
        }

    }
#endif
}
