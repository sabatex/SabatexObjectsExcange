using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ObjectsExchange.Data;

namespace ObjectsExchange.Pages.Clients
{
    public class IndexModel : PageModel
    {
        private readonly ObjectsExchangeDbContext _context;

        public IndexModel(ObjectsExchangeDbContext context)
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
