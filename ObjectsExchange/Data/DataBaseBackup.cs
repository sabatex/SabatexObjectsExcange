using Microsoft.AspNetCore.Identity;
using ObjectsExchange.Models;
using Sabatex.ObjectsExchange.Models;

namespace ObjectsExchange.Data
{
    public class DataBaseBackup
    {
        public IEnumerable<ClientNode> ClientNodes { get; set; }
        public IEnumerable<Sabatex.ObjectsExchange.Models.Client> Clients { get; set; }
        public IEnumerable<ClientUser> ClientUsers { get; set; }
        public IEnumerable<IdentityRole> Roles { get; set; }
        public IEnumerable<IdentityUser> Users { get; set; }
        public IEnumerable<IdentityUserRole<string>> UserRoles { get; set; }
    }
}
