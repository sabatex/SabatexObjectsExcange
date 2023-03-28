using Sabatex.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sabatex.ObjectsExchange.Models
{
    public class ResultCollection<T> where T : IEntityBase
    {
        public IEnumerable<T> Items { get; set; } = new T[] { };
        public int Count { get; set; }
    }
}
