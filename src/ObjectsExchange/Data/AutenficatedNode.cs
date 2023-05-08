using Sabatex.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectsExchange.Data;

public class AutenficatedNode:IEntityBase
{
    /// <summary>
    /// the same as ClientNode Id
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Access token
    /// </summary>
    public string AccessToken { get; set; } = String.Empty;
    /// <summary>
    /// Refresh token
    /// </summary>
    public string RefreshToken { get; set; } = String.Empty;
    /// <summary>
    /// the date when taked token
    /// </summary>
    public DateTime ExpiresDate { get; set; }

    string IEntityBase.KeyAsString() => Id.ToString();

}
