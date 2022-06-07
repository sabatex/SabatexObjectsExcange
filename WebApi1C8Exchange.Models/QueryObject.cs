using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiDocumentsExchange.Models;
/// <summary>
/// обєкти які потрібно отримати з нода
/// </summary>
public class QueryObject
{
    public long Id { get; set; }
    /// <summary>
    /// відправник
    /// </summary>
    public ClientNode? Sender { get; set; }
    public int SenderId { get; set; }

    /// <summary>
    /// отримувач
    /// </summary>
    public ClientNode? Destination { get; set; }
    public int DestinationId { get; set; }
    public ObjectType? ObjectType { get; set; } = default!;
    public int ObjectTypeId { get; set; }
    public string ObjectId { get; set; } = default!;

}
