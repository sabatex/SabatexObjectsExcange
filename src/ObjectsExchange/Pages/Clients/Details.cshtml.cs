using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ObjectsExchange.Data;

namespace ObjectsExchange.Pages.Clients
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly ObjectsExchange.Data.ObjectsExchangeDbContext _context;

        public DetailsModel(ObjectsExchange.Data.ObjectsExchangeDbContext context)
        {
            _context = context;
        }

        public Client Client { get; set; } = default!;
        public ClientNodes[] ClientNode { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Clients == null)
            {
                return NotFound();
            }

            var client = await _context.Clients.FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }
            else 
            {
                Client = client;
            }
            ClientNode = await _context.ClientNodes.Include(i => i.Objects).Include(j => j.QueryObjects).Where(s => s.ClientId == client.Id).Select(s => new ClientNodes { ClientNode = s, QueriesCount = s.QueryObjects.Count(), ObjectsCount = s.Objects.Count() }).ToArrayAsync();
            return Page();
        }


        public class ClientNodes
        {
            public ClientNode? ClientNode { get; set; }
            public int QueriesCount { get; set; }
            public int ObjectsCount { get; set; }
        }

    }
}
