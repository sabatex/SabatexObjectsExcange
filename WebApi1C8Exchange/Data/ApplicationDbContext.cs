using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApiDocumentsExchange.Models;

namespace WebApiDocumentsExchange.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public DbSet<ClientNode> ClientNodes { get; set; }
    public DbSet<ObjectExchange> ObjectExchanges { get; set; }
    public DbSet<QueryObject> QueryObjects { get; set; }

    public DbSet<AutenficatedNode> AutenficatedNodes { get; set; }


    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AutenficatedNode>(en =>
        {
            en.Property(p => p.DateStamp).HasColumnType("timestamp without time zone");
        });
        builder.Entity<ObjectExchange>(en =>
        {
            en.Property(p => p.DateStamp).HasColumnType("timestamp without time zone");
        });

    }
}
