using Sabatex.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sabatex.ObjectsExchange.Server.Data;

public class AutenficatedNode:IEntityBase
{
    /// <summary>
    /// the same as ClientNode Id
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Access token
    /// </summary>
    public string Token { get; set; } = String.Empty;
    /// <summary>
    /// the date when taked token
    /// </summary>
    public DateTime DateStamp { get; set; }

    string IEntityBase.KeyAsString() => Id.ToString();

}
