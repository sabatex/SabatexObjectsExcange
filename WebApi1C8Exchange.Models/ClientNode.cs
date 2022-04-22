namespace WebApi1C8Exchange.Models
{
    public class ClientNode
    {
        public string Id { get; set; }=Guid.NewGuid().ToString();
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
