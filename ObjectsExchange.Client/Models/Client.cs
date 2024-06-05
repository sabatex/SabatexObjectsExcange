using ObjectsExchange.Models;
using Sabatex.Core;
using System.Text.Json.Serialization;

namespace ObjectsExchange.Client.Models
{
    public class Client : IEntityBase<Guid>
    {
        public Guid Id{ get; set; }
        public string Description { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; } = default!;
        public string UserId { get; set; }
        [JsonIgnore]
        public IEnumerable<ClientNode>? ClientNodes { get; set; }
        public string KeyAsString() => Id.ToString();
    }
}
