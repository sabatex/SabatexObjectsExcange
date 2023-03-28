using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sabatex.ObjectsExchange.Server.Data;

namespace Sabatex.ObjectsExchange.Server.Pages.Clients
{
    public class CreateModel : PageModel
    {
        private readonly Sabatex.ObjectsExchange.Server.Data.ObjectsExchangeDbContext _context;

        public CreateModel(Sabatex.ObjectsExchange.Server.Data.ObjectsExchangeDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public ClientNode ClientNode { get; set; } = default!;
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
          if (!ModelState.IsValid || _context.ClientNodes == null || ClientNode == null)
            {
                return Page();
            }

            _context.ClientNodes.Add(ClientNode);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
