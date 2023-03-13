namespace sabatex.ObjectsExchange.Models
{
    public class Login
    {
        public Guid ClientId { get; set; }
        public string Password { get; set; } = default!;
    }
}
