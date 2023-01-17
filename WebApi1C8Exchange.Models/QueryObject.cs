using Sabatex.Core;
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
public class QueryObject : IEntityBase
{
    public long Id { get; set; }
    [MaxLength(50)]
    public string Sender { get; set; } = default!;
    [MaxLength(50)]
    public string Destination { get; set; } = default!;
    
    public string ObjectId { get; set; } = default!; //must lovercase
    [MaxLength(50)]
    public string ObjectType { get; set; } = default!; // lovercase

    public string KeyAsString => Id.ToString();
}
