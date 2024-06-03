using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sabatex.Exchange.Data.Models;
using Sabatex.ObjectsExchange.Models;

namespace Sabatex.Exchange.Data;

public abstract class ObjectsExchangeDbContext : IdentityDbContext<ApplicationUser,ApplicationRole,Guid>
{
    public DbSet<ClientNode> ClientNodes { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<AutenficatedNode> AutenficatedNodes { get; set; }
    public DbSet<ObjectExchange> ObjectExchanges { get; set; }
    public DbSet<ClientUser> ClientUsers { get; set; }
    //public DbSet<QueryObject> QueryObjects { get; set; }
    public ObjectsExchangeDbContext(DbContextOptions options) : base(options) { }

    IEnumerable<ClientNode> GetDefaultClientNodes()
    {
        yield return new ClientNode
        {
            Id = new Guid("EF1A359F-9F43-40E6-B702-A56DF87432D6"),
            Name = "Demo ",
            ClientId = new Guid("8F830B0C-BF60-4A4D-B9C7-9D86C60DB75D")
        };
    }
    IEnumerable<Client> GetDefaultClients()
    {
        yield return new Client
        {
            Id = new Guid("8F830B0C-BF60-4A4D-B9C7-9D86C60DB75D"),
            Description = "Demo "
        };
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        //builder.Entity<Client.Models.Client>().HasData(GetDefaultClients());
        //builder.Entity<QueryObject>().HasOne<Client.Models.ClientNode>().WithMany().HasForeignKey(p => p.Sender).OnDelete(DeleteBehavior.Cascade);
        //builder.Entity<QueryObject>().HasOne<Client.Models.ClientNode>().WithMany().HasForeignKey(p => p.Destination).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ObjectExchange>().HasOne<ClientNode>().WithMany().HasForeignKey(p => p.Sender).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ObjectExchange>().HasOne<ClientNode>().WithMany().HasForeignKey(p => p.Destination).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ClientUser>().HasKey(nameof(ClientUser.ClientId),nameof(ClientUser.ApplicationUserId));
        

    }
}
