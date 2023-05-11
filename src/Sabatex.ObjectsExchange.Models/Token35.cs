namespace Sabatex.ObjectsExchange.Models
{
#if NET3_5 || NETSTANDARD2_0
    public class Token
    {
        public string access_token { get; set; } = string.Empty;
        public string token_type { get; set; } = "Bearer";
        public string refresh_token { get; set; } = string.Empty;
        public int expires_in { get; set; }

    }
#endif
}
