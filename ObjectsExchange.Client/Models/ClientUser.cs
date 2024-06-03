using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ObjectsExchange.Client.Models
{
    [PrimaryKey(nameof(ApplicationUserId),nameof(ClientId))]
    public class ClientUser
    {
        public Guid ApplicationUserId { get; set; }
        public Guid ClientId { get; set; }
    }
}
