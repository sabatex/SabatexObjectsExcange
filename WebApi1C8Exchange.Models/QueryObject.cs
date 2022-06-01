using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiDocumentsExchange.Models;
/// <summary>
/// обєкти які потрібно отримати з нода
/// </summary>
public class QueryObject
{
    public int Id { get; set; }
    /// <summary>
    /// Обєкт для якого не вирішені посилання
    /// </summary>
    public ObjectExchange  Owner { get; set; } = default!;
    public Guid OwnerId { get; set; }
    public string ObjectType { get; set; }
    public Guid ObjectId { get; set; }
    public bool IsResived { get; set; } = false;
}
