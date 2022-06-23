using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiDocumentsExchange.Models;

namespace WebApiDocumentsExchange.Data;

public class PostgresDbContext : ExchangeDbContext
{
    public PostgresDbContext(DbContextOptions<PostgresDbContext> options) : base(options)
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
