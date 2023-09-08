using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ObjectsExchange.Data;

namespace ObjectsExchange.Pages.ClientNodes
{
    [Authorize]
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
                ClientNode = await _context.ClientNodes.Where(s => s.ClientId == clientId)
                    .Select(s => new ClientNodes { ClientNode = s,
                                                   QueriesCount = _context.QueryObjects.Where(n=>n.Destination == s.Id).Count(),
                                                   ObjectsCount = _context.ObjectExchanges.Where(n=>n.Destination == s.Id).Count() }).ToArrayAsync();

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
