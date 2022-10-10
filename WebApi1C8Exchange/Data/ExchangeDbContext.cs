using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApiDocumentsExchange.Models;

namespace WebApiDocumentsExchange.Data;

public class ExchangeDbContext : IdentityDbContext
{
    public DbSet<ClientNode> ClientNodes { get; set; }
    public DbSet<ObjectExchange> ObjectExchanges { get; set; }
    public DbSet<QueryObject> QueryObjects { get; set; }

    public DbSet<AutenficatedNode> AutenficatedNodes { get; set; }


    public ExchangeDbContext(DbContextOptions options): base(options)
    {
    }

 
}
