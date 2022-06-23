using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiDocumentsExchange.Data;

public class MSSQLDbContext : ExchangeDbContext
{
    public MSSQLDbContext(DbContextOptions<MSSQLDbContext> options) : base(options)
    {
    }
}
