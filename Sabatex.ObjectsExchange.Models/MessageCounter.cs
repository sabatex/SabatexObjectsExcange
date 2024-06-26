using Sabatex.ObjectsExchange.Models;
using System;
using System.Reflection.PortableExecutable;

namespace ObjectsExchange.Client.Models;

public class MessageCounter
{
    public Guid Id { get; set; }
    public ClientNode ClientNode { get; set; } = null!;
    public int Counter { get; set; }
    public DateTime CounterChange { get; set; }
}
