namespace WebApiDocumentsExchange.Models;

public class ClientNode
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string Password { get; set; } = default!;
}
