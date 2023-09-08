using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ObjectsExchange.Data;
using ObjectsExchange.Services;
using Sabatex.ObjectsExchange.Models;

namespace ObjectsExchange.Pages.ClientNodes
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ObjectsExchangeDbContext _context;
        protected readonly ApiConfig _apiConfig;
        const string maskPassword = "***********";

        public EditModel(ObjectsExchangeDbContext context, IOptions<ApiConfig> apiConfig)
        {
            _context = context;
            _apiConfig = apiConfig.Value;
        }

        [BindProperty]
        public ClientNode ClientNode { get; set; } = default!;

        [BindProperty]
        public IEnumerable<string> AccessCodes { get; set; }

        public SelectListItem[] AccessItems { get; set; }

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
            ClientNode.Password = maskPassword;
            AccessItems = await _context.ClientNodes.Where(s=>s.ClientId == clientnode.ClientId && s.Id !=clientnode.Id).Select(n=>new SelectListItem { Value=n.Id.ToString(),Text =n.Name}).ToArrayAsync();
            AccessCodes = clientnode.GetClientAccess().Select(s=>s.ToString());
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
            clientNode.Name = ClientNode.Name;
            clientNode.NormalizedName = ClientNode.Name.ToUpper();
            clientNode.Description = ClientNode.Description;
            clientNode.SetClientAccess(AccessCodes);
            clientNode.NormalizedName = ClientNode.Name.ToUpper();
            clientNode.IsDemo = ClientNode.IsDemo;
            clientNode.MaxOperationPerMounth = ClientNode.MaxOperationPerMounth;
            if (ClientNode.Password != maskPassword)
            {
                clientNode.Password = _apiConfig.HashPassword(ClientNode.Password);
            }
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index", new { clientId = ClientNode.ClientId });
        }

        private bool ClientNodeExists(Guid id)
        {
          return (_context.ClientNodes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
