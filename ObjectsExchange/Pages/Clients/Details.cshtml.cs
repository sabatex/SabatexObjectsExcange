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
    public class DetailsModel : PageModel
    {
        private readonly ObjectsExchangeDbContext _context;

        public DetailsModel(ObjectsExchangeDbContext context)
        {
            _context = context;
        }

      public ClientNode ClientNode { get; set; } = default!; 

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null || _context.ClientNodes == null)
            {
                return NotFound();
            }

            var clientnode = await _context.ClientNodes.FirstOrDefaultAsync(m => m.Id == id);
            if (clientnode == null)
            {
                return NotFound();
            }
            else 
            {
                ClientNode = clientnode;
            }
            return Page();
        }
    }
}
