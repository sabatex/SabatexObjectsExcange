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
    public class DeleteModel : PageModel
    {
        private readonly Sabatex.ObjectsExchange.Server.Data.ObjectsExchangeDbContext _context;

        public DeleteModel(Sabatex.ObjectsExchange.Server.Data.ObjectsExchangeDbContext context)
        {
            _context = context;
        }

        [BindProperty]
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

        public async Task<IActionResult> OnPostAsync(Guid? id)
        {
            if (id == null || _context.ClientNodes == null)
            {
                return NotFound();
            }
            var clientnode = await _context.ClientNodes.FindAsync(id);

            if (clientnode != null)
            {
                ClientNode = clientnode;
                _context.ClientNodes.Remove(ClientNode);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
