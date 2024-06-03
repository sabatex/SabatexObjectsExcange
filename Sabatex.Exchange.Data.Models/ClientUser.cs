using System.ComponentModel.DataAnnotations;

namespace Sabatex.Exchange.Data.Models;

public class ClientUser
{
    public Guid ApplicationUserId { get; set; }
    public Guid ClientId { get; set; }
}
