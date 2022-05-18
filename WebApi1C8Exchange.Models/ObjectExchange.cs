using System.ComponentModel.DataAnnotations;

namespace WebApiDocumentsExchange.Models;

public record struct StoreObject(string Id, string ObjectName, string ObjectJSON);



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

    /// <summary>
    /// унікальне ід обєкта клієнта
    /// </summary>
    [MaxLength(255)]
    [Required]
    public string ObjectId { get; set; }

    /// <summary>
    /// тип обєкта  "Довідник.Номенклатура"
    /// </summary>
    [MaxLength(255)]
    public string ObjectTypeName { get; set; }
    /// <summary>
    /// штам часу обєкта в системі клієнта (для синхронізації повторних вигрузок)
    /// </summary>
    public DateTime ObjectDateStamp { get; set; }
    /// <summary>
    /// отримувач
    /// </summary>
    public ClientNode Sender { get; set; }
    public int SenderId { get; set; }
    /// <summary>
    /// відправник
    /// </summary>
    public ClientNode Destination { get; set; }
    public int DestinationId { get; set; }
    public ExchangeStatus Status { get; set; }
    /// <summary>
    /// внутрішня позначка часу створення обєкта
    /// </summary>
    public DateTime DateStamp { get; set; }
    public string ObjectJSON { get; set; }
    
}
