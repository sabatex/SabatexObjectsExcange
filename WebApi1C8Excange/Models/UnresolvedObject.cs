namespace WebApi1C8Excange.Models
{
    public class UnresolvedObject
    {
        public Guid Id { get; set; }
        public Node1C Node { get; set; } = default!;
        public Guid Owner { get; set; }
    }
}
