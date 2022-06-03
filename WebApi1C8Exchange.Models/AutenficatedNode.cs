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
    public string Id { get; set; }
    public ClientNode Node { get; set; }
    public int NodeId { get; set; }
    public DateTime DateStamp { get; set; }

}
