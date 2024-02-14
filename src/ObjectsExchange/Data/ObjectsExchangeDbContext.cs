using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sabatex.ObjectsExchange.Models;

namespace ObjectsExchange.Data;

public class ObjectsExchangeDbContext : IdentityDbContext
{
    public DbSet<ClientNode> ClientNodes { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<AutenficatedNode> AutenficatedNodes { get; set; }
    public DbSet<ObjectExchange> ObjectExchanges { get; set; }
    public DbSet<QueryObject> QueryObjects { get; set; }
    public ObjectsExchangeDbContext(DbContextOptions<ObjectsExchangeDbContext> options) : base(options) { }

    IEnumerable<ClientNode> GetDefaultClientNodes()
    {
        yield return new ClientNode
        {
            Id = new Guid("8F830B0C-BF60-4A4D-B9C7-9D86C60DB75D"),
            Name = "Demo "
        };
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<QueryObject>().HasOne<ClientNode>().WithMany().HasForeignKey(p => p.Sender).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<QueryObject>().HasOne<ClientNode>().WithMany().HasForeignKey(p => p.Destination).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ObjectExchange>().HasOne<ClientNode>().WithMany().HasForeignKey(p => p.Sender).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ObjectExchange>().HasOne<ClientNode>().WithMany().HasForeignKey(p => p.Destination).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Client>().HasData(new Client { Id = 1, Dascription = "Demo" });

    }
}
