using Sabatex.Core;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sabatex.ObjectsExchange.Server.Data;
/// <summary>
/// Client node definition
/// </summary>
public class ClientNode : IEntityBase
{
   
    /// <summary>
    /// Client ID - UUID string
    /// </summary>
     public Guid Id { get; set; }
    /// <summary>
    /// Frendly client name (not indexed)
    /// </summary>
    [MaxLength(100)]
    public string Name { get; set; } = default!; //index
    [MaxLength(100)]
    public string NormalizedName { get; set; } = default!;
    public string? Description { get; set; }
    public string Password { get; set; } = default!;
    public string? ClientAccess { get; set; }
    public void SetClientAccess(Guid[] nodesList)=>ClientAccess=string.Join(",", nodesList);
    public IEnumerable<Guid> GetClientAccess()=> ClientAccess?.Split(',').Select(s=>new Guid(s)) ?? new Guid[] { };

    public bool IsDemo { get; set; } = true;

    public uint Counter { get; set; }
    public uint MaxOperationPerMounth { get; set; } = 1000;




    string IEntityBase.KeyAsString()=>Id.ToString();

}
