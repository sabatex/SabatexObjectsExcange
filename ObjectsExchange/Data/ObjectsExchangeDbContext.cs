using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ObjectsExchange.Client.Models;
using ObjectsExchange.Models;
using Sabatex.ObjectsExchange.Models;

namespace ObjectsExchange.Data;

public class ObjectsExchangeDbContext(DbContextOptions<ObjectsExchangeDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Sabatex.ObjectsExchange.Models.ClientNode> ClientNodes { get; set; }
    public DbSet<Sabatex.ObjectsExchange.Models.Client> Clients { get; set; }
    public DbSet<AutenficatedNode> AutenficatedNodes { get; set; }
    public DbSet<ObjectExchange> ObjectExchanges { get; set; }
    public DbSet<Sabatex.ObjectsExchange.Models.ClientUser> ClientUsers { get; set; }
    public DbSet<MessageCounter> MessageCounters { get; set; }
  
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ObjectExchange>().HasOne<Sabatex.ObjectsExchange.Models.ClientNode>().WithMany().HasForeignKey(p => p.Sender).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ObjectExchange>().HasOne<Sabatex.ObjectsExchange.Models.ClientNode>().WithMany().HasForeignKey(p => p.Destination).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ClientNode>().HasOne<MessageCounter>().WithOne(e => e.ClientNode).HasForeignKey<MessageCounter>(e => e.Id);
        

    }
}
