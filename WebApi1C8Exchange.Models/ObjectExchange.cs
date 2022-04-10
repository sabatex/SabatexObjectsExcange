using System.ComponentModel.DataAnnotations;

namespace WebApi1C8Exchange.Models
{
    public class Objectexchange
    {
        public Guid Id { get; set; }
        [MaxLength(255)]
        public string ObjectName { get; set; } = default!;
        public string ObjectJSON { get; set; }=default!;
        public Node1C Sender { get; set; } = default!;
        public Node1C destination { get; set; } = default!;
        public ObjectType ObjectType { get; set; }
        public DateTime DateStamp { get; set; }=DateTime.Now;
    }
}
