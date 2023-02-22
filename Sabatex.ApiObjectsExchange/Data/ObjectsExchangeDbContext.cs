using Microsoft.EntityFrameworkCore;
using sabatex.ObjectsExchange.Models;

namespace Sabatex.ObjectsExchange.Server.Data;

public class ObjectsExchangeDbContext:DbContext
{
#pragma warning disable CS8618
    public DbSet<ClientNode> ClientNodes { get; set; }
    public DbSet<AutenficatedNode> AutenficatedNodes { get; set; }
    public DbSet<ObjectExchange> ObjectExchanges { get; set; }
    public DbSet<QueryObject> QueryObjects { get; set; }
    public ObjectsExchangeDbContext(DbContextOptions options) : base(options){}
#pragma warning restore CS8618
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<QueryObject>().HasOne<ClientNode>().WithMany().HasForeignKey(p => p.Sender).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<QueryObject>().HasOne<ClientNode>().WithMany().HasForeignKey(p => p.Destination).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ObjectExchange>().HasOne<ClientNode>().WithMany().HasForeignKey(p => p.Sender).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ObjectExchange>().HasOne<ClientNode>().WithMany().HasForeignKey(p => p.Destination).OnDelete(DeleteBehavior.Cascade);
    }
}
