using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ObjectsExchange.Models;
using Sabatex.ObjectsExchange.Models;

namespace ObjectsExchange.Data;

public class ObjectsExchangeDbContext : IdentityDbContext
{
    public DbSet<Sabatex.ObjectsExchange.Models.ClientNode> ClientNodes { get; set; }
    public DbSet<Sabatex.ObjectsExchange.Models.Client> Clients { get; set; }
    public DbSet<AutenficatedNode> AutenficatedNodes { get; set; }
    public DbSet<ObjectExchange> ObjectExchanges { get; set; }
    public DbSet<Sabatex.ObjectsExchange.Models.ClientUser> ClientUsers { get; set; }
    //public DbSet<QueryObject> QueryObjects { get; set; }
    public ObjectsExchangeDbContext(DbContextOptions<ObjectsExchangeDbContext> options) : base(options) { }

    IEnumerable<Sabatex.ObjectsExchange.Models.ClientNode> GetDefaultClientNodes()
    {
        yield return new Sabatex.ObjectsExchange.Models.ClientNode
        {
            Id = new Guid("EF1A359F-9F43-40E6-B702-A56DF87432D6"),
            Name = "Demo",
            ClientId = new Guid("8F830B0C-BF60-4A4D-B9C7-9D86C60DB75D")
        };
    }
    IEnumerable<Sabatex.ObjectsExchange.Models.Client> GetDefaultClients()
    {
        yield return new Sabatex.ObjectsExchange.Models.Client
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
        builder.Entity<ObjectExchange>().HasOne<Sabatex.ObjectsExchange.Models.ClientNode>().WithMany().HasForeignKey(p => p.Sender).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ObjectExchange>().HasOne<Sabatex.ObjectsExchange.Models.ClientNode>().WithMany().HasForeignKey(p => p.Destination).OnDelete(DeleteBehavior.Cascade);
        

    }
}
