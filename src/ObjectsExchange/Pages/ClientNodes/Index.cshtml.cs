using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ObjectsExchange.Data;

namespace ObjectsExchange.Pages.ClientNodes
{
    public class IndexModel : PageModel
    {
        private readonly ObjectsExchangeDbContext _context;

        public IndexModel(ObjectsExchangeDbContext context)
        {
            _context = context;
        }

        public int ClientId { get; set; }
        public IList<ClientNodes> ClientNode { get;set; } = default!;
        public async Task OnGetAsync(int clientId)
        {
            ClientId = clientId;
            if (_context.ClientNodes != null)
            {
                ClientNode = await _context.ClientNodes.Include(i => i.Objects).Include(j => j.QueryObjects).Where(s => s.ClientId == clientId).Select(s => new ClientNodes { ClientNode = s, QueriesCount = s.QueryObjects.Count(), ObjectsCount = s.Objects.Count() }).ToArrayAsync();

            }
        }
        public class ClientNodes
        {
            public ClientNode? ClientNode { get; set; }
            public int QueriesCount { get; set; }
            public int ObjectsCount { get; set; }
        }


    }
}
