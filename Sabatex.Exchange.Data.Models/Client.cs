using Sabatex.Core;
using System.Text.Json.Serialization;

namespace Sabatex.Exchange.Data.Models;

public class Client : IEntityBase<Guid>
{
    public Guid Id{ get; set; }
    public string Description { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; } = default!;
    public Guid UserId { get; set; }
    [JsonIgnore]
    public IEnumerable<ClientNode>? ClientNodes { get; set; }
    public string KeyAsString() => Id.ToString();
}
