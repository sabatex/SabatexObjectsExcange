using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ObjectsExchange.Data;
using Sabatex.ObjectsExchange.Models;

namespace ObjectsExchange.Pages.Clients
{
    public class EditModel : PageModel
    {
        private readonly ObjectsExchangeDbContext _context;

        public EditModel(ObjectsExchangeDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ClientNodeBase ClientNode { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null || _context.ClientNodes == null)
            {
                return NotFound();
            }

            var clientnode =  await _context.ClientNodes.FirstOrDefaultAsync(m => m.Id == id);
            if (clientnode == null)
            {
                return NotFound();
            }
            ClientNode = clientnode;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var clientNode = await _context.ClientNodes.FindAsync(ClientNode.Id);
            if (clientNode == null)
                return NotFound();

            clientNode.FillFromBase(ClientNode);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }

        private bool ClientNodeExists(Guid id)
        {
          return (_context.ClientNodes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
