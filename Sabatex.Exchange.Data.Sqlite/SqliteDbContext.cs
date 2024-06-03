using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sabatex.ObjectsExchange.Models;

namespace Sabatex.Exchange.Data.Sqlite;

public class SqliteDbContext : ObjectsExchangeDbContext
{
    public SqliteDbContext(DbContextOptions<SqliteDbContext> options) : base(options) { }

 
}
