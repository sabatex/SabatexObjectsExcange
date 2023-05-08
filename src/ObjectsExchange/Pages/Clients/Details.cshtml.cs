using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ObjectsExchange.Data;
using Sabatex.ObjectsExchange.Models;

namespace ObjectsExchange.Pages.Clients
{
    public class DetailsModel : PageModel
    {
        private readonly ObjectsExchangeDbContext _context;

        public DetailsModel(ObjectsExchangeDbContext context)
        {
            _context = context;
        }

      public ClientNodeBase ClientNode { get; set; } = default!;
      public int QuriesCount { get; set; }
      public int ObjectsCount { get; set; }


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
                QuriesCount = _context.QueryObjects.Where(w => w.Destination == id).Count();
                ObjectsCount = _context.ObjectExchanges.Where(w => w.Destination == id).Count();
 
            }
            return Page();
        }
    
        public async Task<IActionResult> OnPostCleanQueriesAsync(Guid id)
        {
            if (ModelState.IsValid)
            {
                string cmd = "DELETE FROM " + _context.QueryObjects.EntityType.GetTableName() + " WHERE destination = {0}";
                await _context.Database.ExecuteSqlRawAsync(cmd, id);
            }
            return await OnGetAsync(id);
        }
        public async Task<IActionResult> OnPostCleanObjectsAsync(Guid id)
        {
            if (ModelState.IsValid)
            {
                string cmd = "DELETE FROM " +_context.ObjectExchanges.EntityType.GetTableName() + " WHERE destination = {0}";
                await _context.Database.ExecuteSqlRawAsync(cmd,id);
            }
            return await OnGetAsync(id);
        }
    }

}
