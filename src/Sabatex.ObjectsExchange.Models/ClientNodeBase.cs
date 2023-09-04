using Sabatex.Core;
using System;
using System.Collections.Generic;
#if NET6_0_OR_GREATER
using System.ComponentModel.DataAnnotations;
#else
#endif
using System.Linq;
using System.Text;

namespace Sabatex.ObjectsExchange.Models
{
    /// <summary>
    /// Base class for determine client node
    /// </summary>
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
#if NETCOREAPP2_0_OR_GREATER
        [MaxLength(100)]
#endif
        public string Name { get; set; } = default!;
        /// <summary>
        /// Client description
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Client list id's with delimiters char ; for access node  
        /// </summary>
        public string? ClientAccess { get; set; }
        /// <summary>
        /// Set client access nodes
        /// </summary>
        /// <param name="nodesList">array id's for access nodes</param>
        public void SetClientAccess(Guid[] nodesList)
        {
            //ClientAccess = string.Join(",", nodesList); // not support NET3.5
            bool first = true;
            var result = new StringBuilder();
            foreach (var item in nodesList)
            {
                if (first)
                    first = false;
                else
                    result.Append(',');
                result.Append(item);
            }
            ClientAccess = result.ToString();
        }
        /// <summary>
        /// Set client access nodes
        /// </summary>
        /// <param name="nodesList">array id's for access nodes</param>
        public void SetClientAccess(IEnumerable<string> nodesList)
        {
            //ClientAccess = string.Join(",", nodesList); // not support NET3.5
            bool first = true;
            var result = new StringBuilder();
            foreach (var item in nodesList)
            {
                if (first)
                    first = false;
                else
                    result.Append(',');
                result.Append(item);
            }
            ClientAccess = result.ToString();
        }

        /// <summary>
        /// Get 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Guid> GetClientAccess() => ClientAccess?.Split(',').Select(s => new Guid(s)) ?? new Guid[] { };
        /// <summary>
        /// Demo mode (limit count transactions and trolling)
        /// </summary>
        public bool IsDemo { get; set; } = true;
        /// <summary>
        /// Transactions counter
        /// </summary>
        public uint Counter { get; set; }
        /// <summary>
        /// limit transactions per mounts (drop every mounth)
        /// </summary>
        public uint MaxOperationPerMounth { get; set; } = 1000;
    }
}
