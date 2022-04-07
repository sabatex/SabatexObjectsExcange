namespace WebApi1C8Excange.Models
{
    public class Object1C
    {
        public Guid Id { get; set; }
        public string Object1CJSON { get; set; }=default!;
        public Node1C Node1C { get; set; } = default!;
        public ObjectDirection Direction { get; set; }
        public ObjectType ObjectType { get; set; }


    }
}
