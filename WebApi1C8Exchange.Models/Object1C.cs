namespace WebApi1C8Exchange.Models
{
    public class Object1C
    {
        public Guid Id { get; set; }
        public string Object1CJSON { get; set; }=default!;
        public Node1C Sender { get; set; } = default!;
        public Node1C destination { get; set; } = default!;
        public ObjectType ObjectType { get; set; }
        public DateTime DateStamp { get; set; }=DateTime.Now;
        public ObjectState State { get; set; } = ObjectState.New;
    }
}
