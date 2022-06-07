using System.ComponentModel.DataAnnotations;

namespace WebApiDocumentsExchange.Models;

public class ClientNode
{
    public int Id { get; set; } //key
    [MaxLength(50)]
    public string Name { get; set; } = default!; //index
    public string? Description { get; set; }
    public string Password { get; set; } = default!;
}
