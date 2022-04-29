using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApi1C8Exchange.Models
{
    public class ClientObjectType
    {
        public int Id { get; set; }
        public ClientNode Node { get; set; } = default!;
        public string Name { get; set; }=default!;
    }
}
