using System.ComponentModel.DataAnnotations;

namespace WebApi1C8Exchange.Models;

public record struct StoreObject(string Id, string ObjectName, string ObjectJSON);
public record struct PostObject(string SenderNode,
                                string[] DestinationNode,
                                string Password,
                                string ObjectName,
                                string ObjectId,
                                string ObjectJson);

public class ObjectExchange
{
    public string Id { get; set; }= default!;
    [MaxLength(255)]
    public string ObjectName { get; set; } = default!;
    public string ObjectJSON { get; set; }=default!;
    public ClientNode Sender { get; set; } = default!;
    public string SenderId { get; set; } = default!;
    public ClientNode Destination { get; set; } = default!;
    public string DestinationId { get; set; } = default!;

    public ObjectType ObjectType { get; set; }
    public string DateStamp { get; set; }=DateTime.Now.ToString();
}
