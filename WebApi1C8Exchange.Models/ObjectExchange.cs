using System.ComponentModel.DataAnnotations;

namespace WebApiDocumentsExchange.Models;

public class ObjectExchange
{
    public long Id { get; set; } //"Id":10

    [Required]
    public string ObjectId { get; set; } = default!; //must lovercase"objectId":"2138431-324u324i-32u4w234"
    [MaxLength(50)]
    public string ObjectType { get; set; } = default!; // lovercase"ObjectType":"Справочник.НоменклатураЭ"

    [MaxLength(50)]
    public string Sender { get; set; } = default!; // "sender":
    [MaxLength(50)]
    public string Destination { get; set; } = default!;
    /// <summary>
    /// внутрішня позначка часу створення обєкта
    /// </summary>
    public DateTime DateStamp { get; set; } = DateTime.Now;
    public string ObjectJSON { get; set; }=default!;
 
}
