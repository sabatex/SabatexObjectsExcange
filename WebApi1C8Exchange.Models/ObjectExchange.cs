using System.ComponentModel.DataAnnotations;

namespace WebApiDocumentsExchange.Models;

public class ObjectExchange
{
    // унікальне Id відправлення клієнта
    public long Id { get; set; }

    [Required]
    public string ObjectId { get; set; } = default!;

    public ObjectType ObjectType { get; set; } = default!;
    public int ObjectTypeId { get; set; }


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
    /// <summary>
    /// внутрішня позначка часу створення обєкта
    /// </summary>
    public DateTime DateStamp { get; set; }
    // приорітет пакета (обробляються з найвищим приорітетом)
    public int Priority { get; set; } = 0;
    public string ObjectJSON { get; set; }
    public bool IsDone { get; set; } = false;

}
