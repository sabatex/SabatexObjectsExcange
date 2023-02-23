using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sabatex.ObjectsExchange.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Login
    {
        public Guid ClientId { get; set; }
        public string Password { get; set; }= default!;
    }
}
