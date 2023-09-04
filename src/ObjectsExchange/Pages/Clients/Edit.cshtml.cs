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
        private readonly ObjectsExchange.Data.ObjectsExchangeDbContext _context;

        public EditModel(ObjectsExchange.Data.ObjectsExchangeDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Client Client { get; set; } = default!;
        public ClientNodes[] ClientNode { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Clients == null)
            {
                return NotFound();
            }

            var client =  await _context.Clients.FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }
            Client = client;
            ClientNode = await _context.ClientNodes.Include(i=>i.Objects).Include(j=>j.QueryObjects).Where(s=>s.ClientId==client.Id).Select(s=> new ClientNodes {ClientNode=s,QueriesCount = s.QueryObjects.Count(), ObjectsCount = s.Objects.Count() }).ToArrayAsync();
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

            _context.Attach(Client).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(Client.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool ClientExists(int id)
        {
          return (_context.Clients?.Any(e => e.Id == id)).GetValueOrDefault();
        }


        public class ClientNodes
        {
            public ClientNode? ClientNode { get; set; }
            public int QueriesCount { get; set; }
            public int ObjectsCount { get; set; }
        }

    }
}
