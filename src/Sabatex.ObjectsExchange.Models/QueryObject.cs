using Sabatex.Core;
#if NET6_0_OR_GREATER
using System.ComponentModel.DataAnnotations;
#endif
using System;
namespace Sabatex.ObjectsExchange.Models
{
        /// <summary>
        /// обєкти які потрібно отримати з нода
        /// </summary>
        public class QueryObject : IEntityBase
        {
        /// <summary>
        /// primary key
        /// </summary>
            public long Id { get; set; }
        /// <summary>
        /// Sender client id
        /// </summary>
            public Guid Sender { get; set; }
        /// <summary>
        /// Destination client id
        /// </summary>
            public Guid Destination { get; set; }
        /// <summary>
        /// Client object unique id
        /// </summary>
#if NET6_0_OR_GREATER
        [MaxLength(50)]
#endif
        public string ObjectId { get; set; } = default!; //must lovercase
        /// <summary>
        /// client object type
        /// </summary>
#if NET6_0_OR_GREATER
        [MaxLength(50)]
#endif
        public string ObjectType { get; set; } = default!; // lovercase
            string IEntityBase.KeyAsString() => Id.ToString();
        }

}