using System.ComponentModel.DataAnnotations;

namespace WebApiDocumentsExchange.Models;

public class ClientNode
{
    [MaxLength(50)]
    public string Id { get; set; } = String.Empty;//key
    [MaxLength(50)]
    public string Name { get; set; } = default!; //index
    public string? Description { get; set; }
    public string Password { get; set; } = default!;
}
