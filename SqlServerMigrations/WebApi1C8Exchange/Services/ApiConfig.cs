namespace WebApiDocumentsExchange.Services
{
    public class ApiConfig
    {
        public int TokensLifeTime { get; set; } = 15; //default 15 minutes
        public TimeSpan TokensLifeTimeMinutes =>TimeSpan.FromMinutes(TokensLifeTime);
    }
}
