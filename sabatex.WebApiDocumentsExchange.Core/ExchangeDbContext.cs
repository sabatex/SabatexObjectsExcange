using Microsoft.EntityFrameworkCore;
using WebApiDocumentsExchange.Models;

namespace sabatex.WebApiDocumentsExchange.Core;

public class ExchangeDbContext : DbContext
{
    public DbSet<ClientNode> ClientNodes { get; set; }
    public DbSet<ObjectExchange> ObjectExchanges { get; set; }
    public DbSet<QueryObject> QueryObjects { get; set; }

    public DbSet<AutenficatedNode> AutenficatedNodes { get; set; }


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public ExchangeDbContext(DbContextOptions options)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<QueryObject>().HasOne<ClientNode>().WithMany().HasForeignKey(p=>p.Sender).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<QueryObject>().HasOne<ClientNode>().WithMany().HasForeignKey(p=>p.Destination).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ObjectExchange>().HasOne<ClientNode>().WithMany().HasForeignKey(p => p.Sender).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ObjectExchange>().HasOne<ClientNode>().WithMany().HasForeignKey(p => p.Destination).OnDelete(DeleteBehavior.Cascade);
    }


}
