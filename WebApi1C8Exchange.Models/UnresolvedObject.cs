namespace WebApi1C8Exchange.Models
{
    public class UnresolvedObject
    {
        public Guid Id { get; set; }
        public Node1C Node { get; set; } = default!;
        public Guid Owner { get; set; }
        public DateTime DateStamp { get; set; }
    }
}
