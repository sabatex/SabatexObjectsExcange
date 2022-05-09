using System.ComponentModel.DataAnnotations;

namespace WebApiDocumentsExchange.Models;

public record struct StoreObject(string Id, string ObjectName, string ObjectJSON);
/// <summary>
/// дані для передачі JSON
/// </summary>
/// <param name="SenderNode">відправник</param>
/// <param name="DestinationNode">отримувачі</param>
/// <param name="ApiKey">ключ від апі</param>
/// <param name="ObjectType">тип обєкта</param>
/// <param name="ObjectId">унікальне id обэкта</param>
/// <param name="ObjectJson">сам object</param>
public record struct PostObject(string[] DestinationNode,
                                string ObjectType,
                                string ObjectId,
                                string ObjectJson);


/// <summary>
/// Список обєктів для нода
/// </summary>
/// <param name="node">назва нода</param>
/// <param name="apiKey">ключ для нода</param>
/// <param name="ObjectsId">ід обєкта</param>
public record struct ExchangeObjectsId(string[] ObjectsId);

public record struct QueryObjects(Guid objectId, string ObjectsJson);

public class ObjectExchange
{
    public Guid Id { get; set; }
    [MaxLength(255)]
    [Required]
    public string ObjectId { get; set; }

    [MaxLength(255)]
    public string ObjectTypeName { get; set; }
   
    public ClientNode Sender { get; set; }
    public int SenderId { get; set; }
    public ClientNode Destination { get; set; }
    public int DestinationId { get; set; }
    public ExchangeStatus Status { get; set; }
    public DateTime DateStamp { get; set; }
    public string ObjectJSON { get; set; }
    
}
