using Sabatex.Core;

namespace ObjectsExchange.Data
{
    public class Client : IEntityBase
    {
        public int Id { get; set; }
        public string Dascription { get; set; } = string.Empty;
        public IEnumerable<ClientNode>? ClientNodes { get; set; }
        public string KeyAsString()=>Id.ToString();
    }
}
