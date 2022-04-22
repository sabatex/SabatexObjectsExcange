using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApi1C8Exchange.Models;

namespace WebApi1C8Exchange.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public DbSet<ClientNode> ClientNodes { get; set; } = default!;
    public DbSet<ObjectExchange> ObjectExchanges { get; set; } = default!;
 
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ObjectExchange>().HasKey(k => new { k.Id, k.SenderId, k.DestinationId });
        //builder.Entity<Object1C>(en =>
        //{
        //    en.Property(p => p.DateStamp).HasColumnType("timestamp without time zone");
        //});
        //builder.Entity<UnresolvedObject>(en =>
        //{
        //    en.Property(p => p.DateStamp).HasColumnType("timestamp without time zone");
        //});
    }
}
