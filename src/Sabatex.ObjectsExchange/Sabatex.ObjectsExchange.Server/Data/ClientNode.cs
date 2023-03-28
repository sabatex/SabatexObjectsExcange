using Sabatex.Core;
using Sabatex.ObjectsExchange.Models;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sabatex.ObjectsExchange.Server.Data;
/// <summary>
/// Client node definition
/// </summary>
public class ClientNode : ClientNodeBase
{
   
    [MaxLength(100)]
    public string NormalizedName { get; set; } = default!;
    public string Password { get; set; } = default!;


}
