using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using ObjectsExchange.Data;
using ObjectsExchange.Services;
using Sabatex.ObjectsExchange.Models;

namespace ObjectsExchange.Pages.ClientNodes
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ObjectsExchangeDbContext _context;
        protected readonly ApiConfig _apiConfig;

        public CreateModel(ObjectsExchangeDbContext context,IOptions<ApiConfig> apiConfig)
        {
            _context = context;
            _apiConfig = apiConfig.Value;
        }

        public IActionResult OnGet(int clientId)
        {
            ClientNode = new ClientNode
            {
                Id = Guid.NewGuid(),ClientId = clientId,NormalizedName="NEW"
            };
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

            var clientNode = new ClientNode
            {
                ClientId = ClientNode.ClientId,
                Name = ClientNode.Name,
                ClientAccess = ClientNode.ClientAccess,
                Description = ClientNode.Description,
                MaxOperationPerMounth = ClientNode.MaxOperationPerMounth,
                NormalizedName = ClientNode.Name.ToUpper(),
                Password = _apiConfig.HashPassword(ClientNode.Password)
            };

            _context.ClientNodes.Add(ClientNode);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index",new {clientId=ClientNode.ClientId });
        }
    }
}
