using Microsoft.AspNetCore.Identity;
using ObjectsExchange.Client.Models;
using ObjectsExchange.Models;

namespace ObjectsExchange.Data
{
    public class DataBaseBackup
    {
        public IEnumerable<ClientNode> ClientNodes { get; set; }
        public IEnumerable<Client.Models.Client> Clients { get; set; }
        public IEnumerable<ClientUser> ClientUsers { get; set; }
        public IEnumerable<IdentityRole> Roles { get; set; }
        public IEnumerable<ApplicationUser> Users { get; set; }
        public IEnumerable<IdentityUserRole<string>> UserRoles { get; set; }
    }
}
