using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiDocumentsExchange.Models;

public class AutenficatedNode
{
    /// <summary>
    /// key and access token
    /// </summary>
    public string Id { get; set; }=String.Empty;
    public string Node { get; set; } = String.Empty;
    public DateTime DateStamp { get; set; }

}
