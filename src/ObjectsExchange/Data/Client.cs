using Sabatex.Core;

namespace ObjectsExchange.Data
{
    public class Client : IEntityBase<int>
    {
        public int Id { get; set; }
        public string Dascription { get; set; } = string.Empty;
        public IEnumerable<ClientNode>? ClientNodes { get; set; }
        public string KeyAsString() => Id.ToString();
    }
}
