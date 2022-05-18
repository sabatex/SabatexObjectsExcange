using System.ComponentModel.DataAnnotations;

namespace WebApiDocumentsExchange.Models;

public class ClientNode
{
    public int Id { get; set; }
    [MaxLength(50)]
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string Password { get; set; } = default!;
}
