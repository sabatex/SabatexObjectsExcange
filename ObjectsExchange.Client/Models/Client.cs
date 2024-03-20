using ObjectsExchange.Models;
using Sabatex.Core;

namespace ObjectsExchange.Client.Models
{
    public class Client : IEntityBase<Guid>
    {
        public Guid Id{ get; set; }
        public string Description { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; } = default!;
        public Guid UserId { get; set; }
        public IEnumerable<ClientNode>? ClientNodes { get; set; }
        public string KeyAsString() => Id.ToString();
    }
}
