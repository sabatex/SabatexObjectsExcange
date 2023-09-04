using Sabatex.Core;
using Sabatex.ObjectsExchange.Models;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ObjectsExchange.Data;
/// <summary>
/// Client node definition
/// </summary>
public class ClientNode : ClientNodeBase
{

    [MaxLength(100)]
    public string NormalizedName { get; set; } = default!;

    [Display(Name = "Password")]
    public string Password { get; set; } = default!;
    public Client? Client { get; set; }
    public int ClientId { get; set; }

    public IEnumerable<ObjectExchange>? Objects { get; set; }
    public IEnumerable<QueryObject>? QueryObjects { get; set; }
}
