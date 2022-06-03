using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiDocumentsExchange.Models
{
    public class ObjectType
    {
        public int Id { get; set; }
        /// <summary>
        /// тип обєкта  "Довідник.Номенклатура"
        /// </summary>
        public string Name { get; set; } = default!;
        public ClientNode? Node { get; set; }
        public int NodeId { get; set; }
    }
}
