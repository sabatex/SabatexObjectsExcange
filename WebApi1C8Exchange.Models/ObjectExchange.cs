using System.ComponentModel.DataAnnotations;

namespace WebApiDocumentsExchange.Models;

public class ObjectExchange
{
    public long Id { get; set; }

    [Required]
    public string ObjectId { get; set; } = default!; //must uppercase

    public ObjectType? ObjectType { get; set; }
    public int ObjectTypeId { get; set; }


    /// <summary>
    /// отримувач
    /// </summary>
    public ClientNode? Sender { get; set; }
    public int SenderId { get; set; }
    /// <summary>
    /// відправник
    /// </summary>
    public ClientNode? Destination { get; set; }
    public int DestinationId { get; set; }
    /// <summary>
    /// внутрішня позначка часу створення обєкта
    /// </summary>
    public DateTime DateStamp { get; set; } = DateTime.Now;
    // приорітет пакета (обробляються з найвищим приорітетом від нуля 0.1.2.3..)
    public int Priority { get; set; } = 0;
    public string ObjectJSON { get; set; }
    public bool IsDone { get; set; } = false;

}
