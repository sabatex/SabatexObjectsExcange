using System.ComponentModel.DataAnnotations;

namespace WebApiDocumentsExchange.Models;

public class ObjectExchange
{
    // унікальне Id відправлення клієнта
    public Guid Id { get; set; }

    [Required]
    public Guid ObjectId { get; set; }

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
    // приорітет пакета (обробляються з найвищим приорітетом)
    public int Priority { get; set; } = 0;
    public string ObjectJSON { get; set; }
    
}
