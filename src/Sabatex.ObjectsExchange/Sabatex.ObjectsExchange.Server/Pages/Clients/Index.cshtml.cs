using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sabatex.ObjectsExchange.Server.Data;

namespace Sabatex.ObjectsExchange.Server.Pages.Clients
{
    public class IndexModel : PageModel
    {
        private readonly Sabatex.ObjectsExchange.Server.Data.ObjectsExchangeDbContext _context;

        public IndexModel(Sabatex.ObjectsExchange.Server.Data.ObjectsExchangeDbContext context)
        {
            _context = context;
        }

        public IList<ClientNode> ClientNode { get;set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.ClientNodes != null)
            {
                ClientNode = await _context.ClientNodes.ToListAsync();
            }
        }
    }
}
