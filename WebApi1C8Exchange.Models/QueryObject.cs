using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApi1C8Exchange.Models;

public class QueryObject
{
    public int Id { get; set; }
    public ClientObjectType ObjectType { get; set; } = default!;
    public string ObjectId { get; set; } = default!;
    public ClientNode Sender { get; set; } = default!;
    public ClientNode Destination { get; set; } = default!;
}
