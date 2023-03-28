using Sabatex.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sabatex.ObjectsExchange.Models
{
    public class ClientNodeBase:IEntityBase
    {
        /// <summary>
        /// Client ID - UUID string
        /// </summary>
        public Guid Id { get; set; }
        string IEntityBase.KeyAsString() => Id.ToString();
        /// <summary>
        /// Frendly client name (not indexed)
        /// </summary>
        [MaxLength(100)]
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? ClientAccess { get; set; }
        public void SetClientAccess(Guid[] nodesList) => ClientAccess = string.Join(",", nodesList);
        public IEnumerable<Guid> GetClientAccess() => ClientAccess?.Split(',').Select(s => new Guid(s)) ?? new Guid[] { };

        public bool IsDemo { get; set; } = true;

        public uint Counter { get; set; }
        public uint MaxOperationPerMounth { get; set; } = 1000;



    }
}
