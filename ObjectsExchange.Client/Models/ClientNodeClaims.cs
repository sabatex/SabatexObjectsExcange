namespace ObjectsExchange.Client.Models
{
    public class ClientNodeClaims
    {
        public Guid Id { get; set; }
        public string PasswordHash { get; set; } = default!;

    }
}
