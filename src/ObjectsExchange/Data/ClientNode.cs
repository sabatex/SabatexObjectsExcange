using Sabatex.Core;
using Sabatex.ObjectsExchange.Models;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ObjectsExchange.Data;
/// <summary>
/// Client node definition
/// </summary>
public class ClientNode : ClientNodeBase
{
   
    [MaxLength(100)]
    public string NormalizedName { get; set; } = default!;
    public string Password { get; set; } = default!;
    public Client? Client { get; set; }
    public int ClientId { get; set; }
    public void FillFromBase(ClientNodeBase baseNode){
        Name = baseNode.Name;
        Description = baseNode.Description;
        ClientAccess = baseNode.ClientAccess;
        NormalizedName = baseNode.Name.ToUpper();
        IsDemo = baseNode.IsDemo;
        MaxOperationPerMounth = baseNode.MaxOperationPerMounth;
    }

}
